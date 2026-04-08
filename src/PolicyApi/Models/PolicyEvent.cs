public record PolicyEvent(
    string EventType,
    string PolicyNumber,
    string CustomerNumber,
    DateTime Timestamp,
    Policy Payload,
    string Source = "PolicyApi"
);
