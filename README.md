# nl-eval-observability-feedback-loops

An educational example demonstrating how to design **evaluation, observability, and feedback loops** for production AI systems in a clean, minimal C# console app.

This project intentionally keeps infrastructure simple while modeling the key production concepts:

- Offline evaluation and quality gates
- Online observability (latency, tokens, error rate)
- Feedback loops that update prompt policy based on failures

## Overview

Production AI systems are not just "prompt in, answer out". They need:

- **Evaluation** to measure quality before and after changes
- **Observability** to see latency, safety, and drift in real time
- **Feedback loops** to convert failures into improvements

This example shows a tiny, local pipeline that includes all three, including a
two-pass run (baseline -> apply feedback -> re-run).

## What This Project Demonstrates

- A simple evaluation suite with relevance (keyword) and safety (forbidden term) scoring
- A minimal telemetry sink that records spans, wall latency, model latency, and token counts
- Quality gates that block releases when pass rate or safety drops
- A feedback processor that updates a prompt policy when failures occur
- Optional integration with a local Ollama model via OllamaSharp (falls back to a mock model)

## Prerequisites

- **.NET 10 SDK** or later
  https://dotnet.microsoft.com/

Optional:
- **Ollama** installed and running locally
  https://ollama.ai/

## Quick Start

Run the app from the repo root (uses a deterministic mock model by default):

```bash
dotnet run --project EvalObservabilityFeedbackLoops
```

## Optional: Use Ollama

Set these environment variables and run the app:

```bash
set USE_OLLAMA=true
set OLLAMA_URL=http://localhost:11434
set OLLAMA_MODEL=llama3.2:3b

dotnet run --project EvalObservabilityFeedbackLoops
```

If Ollama is unavailable, the app falls back to the mock model. The Ollama client streams
responses using OllamaSharp, then aggregates them into a single response.

## Example Output (Summary)

- Per-case evaluation scores and notes
- Telemetry snapshot (avg latency, p95 latency, tokens)
- Quality gate decision (deploy or block)
- Prompt policy updates from feedback events
- Before vs after summary (PASS 1 vs PASS 2)

## Quality Gates (Demo Defaults)

The evaluation uses simple demo thresholds:

- Pass rate >= 90%
- Average safety score >= 1.0
- P95 latency < 1500 ms

These are intentionally conservative for demonstration and can be tuned per environment.

## How the Demo Runs

1. PASS 1 runs the evaluation suite with the initial policy.
2. Failures produce feedback events that add constraints to the policy.
3. PASS 2 re-runs the suite with the updated policy.

This is a simple stand-in for continuous improvement loops in production.

## Project Structure

```
.
+-- EvalObservabilityFeedbackLoops.slnx
+-- EvalObservabilityFeedbackLoops/
|   +-- EvalObservabilityFeedbackLoops.csproj
|   +-- Program.cs
|   +-- EvaluationCase.cs
|   +-- EvaluationResult.cs
|   +-- EvaluationSuite.cs
|   +-- FeedbackLoop.cs
|   +-- ModelClients.cs
|   +-- PromptPolicy.cs
|   +-- Telemetry.cs
+-- LICENSE
+-- README.md
```

## Key Idea

The example is intentionally small but maps cleanly to real systems:

- Swap the mock model for a real one
- Stream telemetry to your observability stack
- Store feedback events and label them for retraining

## License

See the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome. If you'd like to extend the demo, consider:

- Adding new evaluation cases or scoring rules
- Wiring telemetry into your preferred observability stack
- Expanding feedback signals (human review, regression labels)

Open a PR or issue with a clear description of the change.
