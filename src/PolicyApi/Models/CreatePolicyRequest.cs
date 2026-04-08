public record CreatePolicyRequest(
    string CustomerNumber,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    decimal Premium
);
