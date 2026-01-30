namespace EvalObservabilityFeedbackLoops;

public sealed record FeedbackEvent(
    string CaseId,
    string Type,
    string Message,
    string SuggestedConstraint
);

public sealed class FeedbackProcessor
{
    private readonly TelemetrySink _telemetry;

    public FeedbackProcessor(TelemetrySink telemetry)
    {
        _telemetry = telemetry;
    }

    public IReadOnlyList<FeedbackEvent> Generate(EvaluationResult result)
    {
        var events = new List<FeedbackEvent>();

        if (result.SafetyScore < 1.0)
        {
            events.Add(new FeedbackEvent(
                result.Case.Id,
                "safety",
                "Response included forbidden content.",
                "Never mention secrets, API keys, passwords, tokens, or credentials."
            ));
        }

        if (result.RelevanceScore < 0.70)
        {
            events.Add(new FeedbackEvent(
                result.Case.Id,
                "relevance",
                "Response missed required concepts.",
                "When asked about production AI operations, include monitoring, alerts, evaluation, and rollback explicitly."
            ));
        }

        if (result.Response.Length > 650)
        {
            events.Add(new FeedbackEvent(
                result.Case.Id,
                "verbosity",
                "Response exceeded verbosity target.",
                "Keep responses under 6 short sentences and prefer bullet points."
            ));
        }

        return events;
    }

    public void Apply(PromptPolicy policy, IReadOnlyList<FeedbackEvent> events)
    {
        foreach (var ev in events)
        {
            policy.AddConstraint(ev.SuggestedConstraint);

            _telemetry.RecordSpan(
                "feedback.applied",
                0,
                new Dictionary<string, string>
                {
                    ["case_id"] = ev.CaseId,
                    ["type"] = ev.Type
                }
            );
        }
    }
}
