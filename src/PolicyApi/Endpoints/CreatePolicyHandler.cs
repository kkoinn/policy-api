using Microsoft.FeatureManagement;

public static class CreatePolicyHandler
{
    public static async Task<IResult> HandleAsync(
        CreatePolicyRequest request,
        IGuidewireService guidewire,
        IEventPublisher publisher,
        IFeatureManager features,
        ILogger<Program> logger,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(request.CustomerNumber) || !request.CustomerNumber.All(char.IsDigit))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.CustomerNumber)] = ["CustomerNumber must contain only digits."]
            });
        }

        var created = await guidewire.CreatePolicyAsync(request, ct);

        if (await features.IsEnabledAsync(FeatureFlags.EnableEventPublish))
        {
            var policyEvent = new PolicyEvent(
                EventType: "PolicyCreated",
                PolicyNumber: created.PolicyNumber,
                CustomerNumber: created.CustomerNumber,
                Timestamp: DateTime.UtcNow,
                Payload: created
            );
            await publisher.PublishAsync(policyEvent, "PolicyCreated", ct);
        }
        else
        {
            logger.LogInformation("EventPublish is disabled via Feature Toggle.");
        }

        return Results.Created($"api/v1/policies/{created.PolicyNumber}", created);
    }
}
