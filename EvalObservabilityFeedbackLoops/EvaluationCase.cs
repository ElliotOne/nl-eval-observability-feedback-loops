namespace EvalObservabilityFeedbackLoops;

public sealed record EvaluationCase(
    string Id,
    string Prompt,
    string[] ExpectedKeywords,
    string[] ForbiddenKeywords,
    string RiskLevel
);
