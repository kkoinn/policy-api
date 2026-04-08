public class FakeGuidewireService : IGuidewireService
{
    public Policy? PolicyToReturn { get; set; }
    public bool ShouldThrow { get; set; }

    public Task<Policy> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken ct = default)
    {
        if (ShouldThrow)
            throw new HttpRequestException("Guidewire unavailable.");

        return Task.FromResult(PolicyToReturn ?? new Policy(
            PolicyNumber: "POL-FAKE",
            CustomerNumber: request.CustomerNumber,
            Status: request.Status,
            StartDate: request.StartDate,
            EndDate: request.EndDate,
            Premium: request.Premium
        ));
    }
}
