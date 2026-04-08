public sealed class GuidewireService(
    HttpClient client,
    ILogger<GuidewireService> logger
) : IGuidewireService
{
    public async Task<Policy> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken ct = default)
    {
        var response = await client.PostAsJsonAsync("api/v1/policies", request, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<Policy>(ct);
        logger.LogInformation("Policy created in Guidewire: {PolicyNumber}", created!.PolicyNumber);
        return created;
    }
}
