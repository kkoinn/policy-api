using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

public class CreatePolicyHandlerTests
{
    private readonly IGuidewireService _guidewire = Substitute.For<IGuidewireService>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly IFeatureManager _features = Substitute.For<IFeatureManager>();
    private readonly ILogger<Program> _logger = Substitute.For<ILogger<Program>>();

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

    private Task<IResult> CallHandler(CreatePolicyRequest? request = null) =>
        CreatePolicyHandler.HandleAsync(
            request ?? TestRequest,
            _guidewire,
            _publisher,
            _features,
            _logger,
            CancellationToken.None
        );

    [Fact]
    public async Task CreatePolicy_ReturnsCreated_WhenPolicyIsValid()
    {
        _guidewire.CreatePolicyAsync(TestRequest, Arg.Any<CancellationToken>()).Returns(TestPolicy);
        var result = await CallHandler();
        result.Should().BeOfType<Created<Policy>>().Which.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task CreatePolicy_PublishesEvent_WhenFeatureToggleIsEnabled()
    {
        _guidewire.CreatePolicyAsync(TestRequest, Arg.Any<CancellationToken>()).Returns(TestPolicy);
        _features.IsEnabledAsync(FeatureFlags.EnableEventPublish).Returns(true);

        await CallHandler();

        await _publisher.Received(1).PublishAsync(
            Arg.Is<PolicyEvent>(e => e.EventType == "PolicyCreated" && e.PolicyNumber == TestPolicy.PolicyNumber && e.CustomerNumber == TestPolicy.CustomerNumber),
            "PolicyCreated",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePolicy_DoesNotPublishEvent_WhenFeatureToggleIsDisabled()
    {
        _guidewire.CreatePolicyAsync(TestRequest, Arg.Any<CancellationToken>()).Returns(TestPolicy);
        _features.IsEnabledAsync(FeatureFlags.EnableEventPublish).Returns(false);

        await CallHandler();

        await _publisher.DidNotReceive().PublishAsync(Arg.Any<PolicyEvent>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePolicy_ThrowsException_WhenGuidewireFails()
    {
        _guidewire.CreatePolicyAsync(TestRequest, Arg.Any<CancellationToken>())
                  .ThrowsAsync(new HttpRequestException("Guidewire unavailable."));
        var act = () => CallHandler();
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Guidewire unavailable.");
    }

    [Theory]
    [InlineData("ABC-123")]
    [InlineData("CUST-001")]
    [InlineData("12 34")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreatePolicy_ReturnsValidationProblem_WhenCustomerNumberIsNotNumeric(string customerNumber)
    {
        var request = TestRequest with { CustomerNumber = customerNumber };
        var result = await CallHandler(request);
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
