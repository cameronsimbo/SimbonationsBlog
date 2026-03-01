using Learn.Application.Common.Interfaces;
using MediatR;

namespace Learn.Application.Profile.TestAI;

public record TestAICommand : IRequest<TestConnectionResult>
{
    public string Model { get; init; } = string.Empty; // "claude" | "ollama"
    public string? ApiKey { get; init; }
}
