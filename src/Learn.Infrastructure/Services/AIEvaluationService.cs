using Learn.Application.Common.Interfaces;

namespace Learn.Infrastructure.Services;

public class AIEvaluationService : IAIEvaluationService
{
    public async Task<AIEvaluationResult> EvaluateAnswerAsync(EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Azure OpenAI integration
        await Task.CompletedTask;

        return new AIEvaluationResult
        {
            Score = 75,
            IsPassing = true,
            Feedback = "Placeholder evaluation — Azure OpenAI not yet configured.",
            SuggestedCorrection = null,
            DetailedBreakdown = null
        };
    }

    public async Task<List<GeneratedExercise>> GenerateExercisesAsync(ExerciseGenerationRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Azure OpenAI integration for exercise generation
        await Task.CompletedTask;

        return new List<GeneratedExercise>();
    }
}
