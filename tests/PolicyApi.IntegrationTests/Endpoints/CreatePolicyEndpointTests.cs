using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;
using NSubstitute;
using Xunit;

public class CreatePolicyEndpointTests : IClassFixture<PolicyApiFactory>
{
    private readonly PolicyApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly CreatePolicyRequest TestRequest = new(
        CustomerNumber: "12345",
        Status: "Active",
        StartDate: new DateTime(2026, 1, 1),
        EndDate: new DateTime(2027, 1, 1),
        Premium: 1200.00m
    );

    private static readonly Policy TestPolicy = new(
        PolicyNumber: "POL-001",
        CustomerNumber: "12345",
        Status: "Active",
        StartDate: new DateTime(2026, 1, 1),
        EndDate: new DateTime(2027, 1, 1),
        Premium: 1200.00m
    );

    public CreatePolicyEndpointTests(PolicyApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePolicy_Returns201Created_WhenRequestIsValid()
    {
        _factory.GuidewireService.PolicyToReturn = TestPolicy;
        var response = await _client.PostAsJsonAsync("api/v1/policies", TestRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePolicy_PublishesEvent_WhenFeatureToggleIsEnabled()
    {
        _factory.GuidewireService.PolicyToReturn = TestPolicy;
        _factory.EventPublisher.PublishedEvents.Clear();

        var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IFeatureManager>();
                services.AddSingleton<IFeatureManager>(CreateFeatureManager(true));
            })
        ).CreateClient();

        await client.PostAsJsonAsync("api/v1/policies", TestRequest, TestContext.Current.CancellationToken);

        _factory.EventPublisher.PublishedEvents.Should().ContainSingle();
        var published = _factory.EventPublisher.PublishedEvents[0];
        published.EventType.Should().Be("PolicyCreated");
        published.Event.Should().BeOfType<PolicyEvent>()
            .Which.PolicyNumber.Should().Be(TestPolicy.PolicyNumber);
    }

    [Fact]
    public async Task CreatePolicy_DoesNotPublishEvent_WhenFeatureToggleIsDisabled()
    {
        _factory.GuidewireService.PolicyToReturn = TestPolicy;
        _factory.EventPublisher.PublishedEvents.Clear();

        var client = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IFeatureManager>();
                services.AddSingleton<IFeatureManager>(CreateFeatureManager(false));
            })
        ).CreateClient();

        await client.PostAsJsonAsync("api/v1/policies", TestRequest, TestContext.Current.CancellationToken);

        _factory.EventPublisher.PublishedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePolicy_Returns500_WhenGuidewireThrows()
    {
        _factory.GuidewireService.ShouldThrow = true;
        var response = await _client.PostAsJsonAsync("api/v1/policies", TestRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        _factory.GuidewireService.ShouldThrow = false;
    }

    private static IFeatureManager CreateFeatureManager(bool enableEventPublish)
    {
        var manager = Substitute.For<IFeatureManager>();
        manager.IsEnabledAsync(FeatureFlags.EnableEventPublish).Returns(enableEventPublish);
        return manager;
    }

    [Fact]
    public async Task CreatePolicy_Returns400_WhenCustomerNumberContainsNonDigits()
    {
        var invalidRequest = TestRequest with { CustomerNumber = "ABC-123" };
        var response = await _client.PostAsJsonAsync("api/v1/policies", invalidRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
