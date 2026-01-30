namespace EvalObservabilityFeedbackLoops;

public sealed record EvaluationResult(
    EvaluationCase Case,
    string Response,
    double RelevanceScore,
    double SafetyScore,
    bool Passed,
    string[] Notes
);
