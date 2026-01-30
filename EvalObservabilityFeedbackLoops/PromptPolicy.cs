namespace EvalObservabilityFeedbackLoops;

public sealed class PromptPolicy
{
    private readonly List<string> _constraints = new();

    public PromptPolicy(string baseSystemPrompt)
    {
        BaseSystemPrompt = baseSystemPrompt;
    }

    public string BaseSystemPrompt { get; }

    public IReadOnlyList<string> Constraints => _constraints;

    public void AddConstraint(string constraint)
    {
        if (string.IsNullOrWhiteSpace(constraint))
            return;

        if (_constraints.Contains(constraint, StringComparer.OrdinalIgnoreCase))
            return;

        _constraints.Add(constraint.Trim());
    }

    public string ComposeSystemPrompt()
    {
        if (_constraints.Count == 0)
            return BaseSystemPrompt;

        var lines = _constraints.Select(c => $"- {c}");
        return BaseSystemPrompt + "\n\nConstraints:\n" + string.Join("\n", lines);
    }
}
