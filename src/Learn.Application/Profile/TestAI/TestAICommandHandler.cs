using Learn.Application.Common.Interfaces;
using MediatR;

namespace Learn.Application.Profile.TestAI;

public class TestAICommandHandler : IRequestHandler<TestAICommand, TestConnectionResult>
{
    private readonly IAIConnectionTester _tester;

    public TestAICommandHandler(IAIConnectionTester tester)
    {
        _tester = tester;
    }

    public Task<TestConnectionResult> Handle(TestAICommand command, CancellationToken cancellationToken)
        => _tester.TestAsync(command.Model, command.ApiKey, cancellationToken);
}
