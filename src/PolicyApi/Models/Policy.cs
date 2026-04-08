public record Policy(
    string PolicyNumber,
    string CustomerNumber,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    decimal Premium
);
