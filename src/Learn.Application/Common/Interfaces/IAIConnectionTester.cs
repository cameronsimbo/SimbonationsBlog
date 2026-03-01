namespace Learn.Application.Common.Interfaces;

public interface IAIConnectionTester
{
    Task<TestConnectionResult> TestAsync(string model, string? apiKey, CancellationToken ct);
}

public record TestConnectionResult
{
    public bool Success { get; init; }
    public long ResponseTimeMs { get; init; }
    public string Message { get; init; } = string.Empty;
}
