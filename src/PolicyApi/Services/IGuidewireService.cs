public interface IGuidewireService
{
    Task<Policy> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken ct = default);
}
