using Learn.Application.Common.Exceptions;
using Learn.Application.Common.Interfaces;
using Learn.Application.Exercises.GetByLesson;
using Learn.Application.Exercises.GetByLesson.Models;
using Learn.Domain.Entities;
using Learn.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Exercises.Generate;

public class GenerateExercisesCommandHandler : IRequestHandler<GenerateExercisesCommand, List<ExerciseVm>>
{
    private readonly ILearnDbContext _db;
    private readonly IAIEvaluationService _aiService;

    public GenerateExercisesCommandHandler(ILearnDbContext db, IAIEvaluationService aiService)
    {
        _db = db;
        _aiService = aiService;
    }

    public async Task<List<ExerciseVm>> Handle(GenerateExercisesCommand command, CancellationToken cancellationToken)
    {
        Lesson? lesson = await _db.Lessons
            .Include(l => l.Unit)
                .ThenInclude(u => u.Topic)
            .Include(l => l.Exercises)
            .FirstOrDefaultAsync(l => l.Id == command.LessonId, cancellationToken);

        if (lesson is null)
        {
            throw new NotFoundException(nameof(Lesson), command.LessonId);
        }

        // If exercises already exist and haven't been hidden, return them
        List<Exercise> existingExercises = lesson.Exercises
            .Where(e => !e.IsHidden)
            .OrderBy(e => e.OrderIndex)
            .ToList();

        if (lesson.HasGeneratedExercises && existingExercises.Count > 0)
        {
            return existingExercises.Select(e => new ExerciseVm
            {
                Id = e.Id,
                OrderIndex = e.OrderIndex,
                ExerciseType = e.ExerciseType,
                DifficultyLevel = e.DifficultyLevel,
                Prompt = e.Prompt,
                Context = e.Context,
                AudioUrl = e.AudioUrl,
                Hints = e.Hints,
                MaxScore = e.MaxScore,
                UpvoteCount = e.UpvoteCount,
                DownvoteCount = e.DownvoteCount,
                VoteScore = e.VoteScore,
                UserVote = null
            }).ToList();
        }

        Topic topic = lesson.Unit.Topic;

        // Build generation request with full pedagogical context
        ExerciseGenerationRequest request = new()
        {
            SubjectDomain = topic.SubjectDomain,
            DifficultyLevel = topic.DifficultyLevel,
            ExerciseType = ExerciseType.Explanation,
            Count = command.Count,
            TopicName = topic.Name,
            UnitName = lesson.Unit.Name,
            LessonName = lesson.Name,
            KeyConcepts = topic.KeyConcepts,
            GenerationGuidance = topic.GenerationGuidance,
            LessonContext = lesson.GenerationContext
        };

        List<GeneratedExercise> generated = await _aiService.GenerateExercisesAsync(request, cancellationToken);

        int startIndex = existingExercises.Count;
        List<Exercise> newExercises = new();

        for (int i = 0; i < generated.Count; i++)
        {
            GeneratedExercise gen = generated[i];
            Exercise exercise = Exercise.Create(
                lessonId: lesson.Id,
                orderIndex: startIndex + i,
                exerciseType: gen.ExerciseType,
                difficultyLevel: topic.DifficultyLevel,
                prompt: gen.Prompt,
                referenceAnswer: gen.ReferenceAnswer,
                context: gen.Context,
                hints: gen.Hints);

            _db.Exercises.Add(exercise);
            newExercises.Add(exercise);
        }

        lesson.HasGeneratedExercises = true;
        lesson.ExerciseCount = startIndex + newExercises.Count;

        await _db.SaveChangesAsync(cancellationToken);

        return newExercises.Select(e => new ExerciseVm
        {
            Id = e.Id,
            OrderIndex = e.OrderIndex,
            ExerciseType = e.ExerciseType,
            DifficultyLevel = e.DifficultyLevel,
            Prompt = e.Prompt,
            Context = e.Context,
            AudioUrl = e.AudioUrl,
            Hints = e.Hints,
            MaxScore = e.MaxScore,
            UpvoteCount = 0,
            DownvoteCount = 0,
            VoteScore = 0,
            UserVote = null
        }).ToList();
    }
}
