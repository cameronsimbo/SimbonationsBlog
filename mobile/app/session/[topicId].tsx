import { useEffect, useState, useRef } from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ScrollView,
  ActivityIndicator,
  Animated,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { useLocalSearchParams, router } from "expo-router";
import { submitAnswer, completeSession } from "../../lib/api";
import { Colors } from "../../lib/constants";

interface SessionExercise {
  exerciseId: string;
  prompt: string;
  context: string | null;
  hints: string | null;
  exerciseType: number;
  difficultyLevel: number;
  isReview: boolean;
  isInterleaved: boolean;
}

interface SessionData {
  enrollmentId: string;
  topicId: string;
  topicName: string;
  currentLessonId: string;
  currentLessonName: string;
  currentUnitName: string;
  unitIndex: number;
  lessonIndex: number;
  exercises: SessionExercise[];
  newExerciseCount: number;
  reviewExerciseCount: number;
}

interface ExerciseResult {
  exerciseId: string;
  score: number;
  isPassing: boolean;
  xpEarned: number;
  isReview: boolean;
  feedback: string;
  suggestedCorrection: string | null;
}

interface SessionComplete {
  totalXPEarned: number;
  sessionBonus: number;
  exercisesCompleted: number;
  averageScore: number;
  lessonComplete: boolean;
  unitAdvanced: boolean;
  newLevel: number;
  levelTitle: string;
  levelProgress: number;
  currentStreak: number;
  reviewItemsCreated: number;
  weakAreas: string[] | null;
}

type Phase = "exercise" | "feedback" | "complete";

export default function SessionScreen() {
  const { topicId, sessionData: sessionDataStr } = useLocalSearchParams<{
    topicId: string;
    sessionData: string;
  }>();

  const [session, setSession] = useState<SessionData | null>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [answer, setAnswer] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [phase, setPhase] = useState<Phase>("exercise");
  const [results, setResults] = useState<ExerciseResult[]>([]);
  const [currentFeedback, setCurrentFeedback] = useState<ExerciseResult | null>(null);
  const [sessionComplete, setSessionComplete] = useState<SessionComplete | null>(null);
  const [showHints, setShowHints] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [startTime] = useState(Date.now());
  const progressAnim = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    if (sessionDataStr) {
      try {
        setSession(JSON.parse(sessionDataStr));
      } catch {
        setError("Invalid session data");
      }
    }
  }, [sessionDataStr]);

  useEffect(() => {
    if (session) {
      Animated.timing(progressAnim, {
        toValue: (currentIndex + 1) / session.exercises.length,
        duration: 400,
        useNativeDriver: false,
      }).start();
    }
  }, [currentIndex, session]);

  const handleSubmit = async () => {
    if (!session || !answer.trim()) return;
    const exercise = session.exercises[currentIndex];
    setSubmitting(true);
    setError(null);

    try {
      const timeTaken = Math.round((Date.now() - startTime) / 1000);
      const response = await submitAnswer({
        exerciseId: exercise.exerciseId,
        userAnswer: answer.trim(),
        timeTakenSeconds: timeTaken,
      });

      const result: ExerciseResult = {
        exerciseId: exercise.exerciseId,
        score: response.data.score,
        isPassing: response.data.isPassing,
        xpEarned: response.data.xpEarned,
        isReview: exercise.isReview,
        feedback: response.data.feedback,
        suggestedCorrection: response.data.suggestedCorrection ?? null,
      };

      setResults((prev) => [...prev, result]);
      setCurrentFeedback(result);
      setPhase("feedback");
    } catch (err: any) {
      setError(err?.response?.data?.Error || err.message || "Submission failed");
    } finally {
      setSubmitting(false);
    }
  };

  const handleNext = async () => {
    if (!session) return;

    if (currentIndex + 1 >= session.exercises.length) {
      // Complete the session
      try {
        const allResults = [...results];
        const response = await completeSession({
          topicId: session.topicId,
          results: allResults.map((r) => ({
            exerciseId: r.exerciseId,
            score: r.score,
            isPassing: r.isPassing,
            xpEarned: r.xpEarned,
            isReview: r.isReview,
          })),
        });
        setSessionComplete(response.data);
        setPhase("complete");
      } catch (err: any) {
        setError(err.message || "Failed to complete session");
        setPhase("complete");
      }
    } else {
      setCurrentIndex((prev) => prev + 1);
      setAnswer("");
      setShowHints(false);
      setCurrentFeedback(null);
      setPhase("exercise");
    }
  };

  if (!session) {
    return (
      <View style={s.center}>
        {error ? (
          <Text style={s.errorText}>⚠️ {error}</Text>
        ) : (
          <ActivityIndicator size="large" color={Colors.primary} />
        )}
      </View>
    );
  }

  // ─── COMPLETE PHASE ───
  if (phase === "complete") {
    const data = sessionComplete;
    return (
      <ScrollView
        style={s.container}
        contentContainerStyle={s.completeContainer}
      >
        <Text style={s.completeEmoji}>🎉</Text>
        <Text style={s.completeTitle}>Session Complete!</Text>

        {data ? (
          <>
            <View style={s.completeStats}>
              <View style={s.completeStat}>
                <Text style={s.completeStatValue}>⚡ {data.totalXPEarned}</Text>
                <Text style={s.completeStatLabel}>XP Earned</Text>
              </View>
              {data.sessionBonus > 0 ? (
                <View style={s.completeStat}>
                  <Text style={[s.completeStatValue, { color: Colors.gold }]}>
                    +{data.sessionBonus}
                  </Text>
                  <Text style={s.completeStatLabel}>Bonus</Text>
                </View>
              ) : null}
              <View style={s.completeStat}>
                <Text style={s.completeStatValue}>
                  {data.averageScore}%
                </Text>
                <Text style={s.completeStatLabel}>Avg Score</Text>
              </View>
              <View style={s.completeStat}>
                <Text style={[s.completeStatValue, { color: Colors.streak }]}>
                  🔥 {data.currentStreak}
                </Text>
                <Text style={s.completeStatLabel}>Streak</Text>
              </View>
            </View>

            <View style={s.completeLevelCard}>
              <Text style={s.completeLevelTitle}>
                Level {data.newLevel} — {data.levelTitle}
              </Text>
              <View style={s.completeLevelBar}>
                <View
                  style={[
                    s.completeLevelFill,
                    { width: `${Math.round(data.levelProgress * 100)}%` },
                  ]}
                />
              </View>
            </View>

            {data.lessonComplete ? (
              <View style={s.completeAchievement}>
                <Text style={s.completeAchievementText}>
                  ⭐ Lesson Complete!
                  {data.unitAdvanced ? " You advanced to the next unit!" : ""}
                </Text>
              </View>
            ) : null}

            {data.reviewItemsCreated > 0 ? (
              <Text style={s.completeReview}>
                🔄 {data.reviewItemsCreated} exercise
                {data.reviewItemsCreated === 1 ? "" : "s"} added to review
              </Text>
            ) : null}

            {data.unitAdvanced && data.weakAreas && data.weakAreas.length > 0 ? (
              <View style={s.weakAreasCard}>
                <Text style={s.weakAreasTitle}>Areas to revisit next unit</Text>
                {data.weakAreas.map((area, i) => (
                  <Text key={i} style={s.weakAreasItem}>• {area}</Text>
                ))}
              </View>
            ) : null}
          </>
        ) : null}

        <TouchableOpacity
          style={s.completeButton}
          onPress={() => router.back()}
        >
          <Text style={s.completeButtonText}>Back to Path</Text>
        </TouchableOpacity>
      </ScrollView>
    );
  }

  const exercise = session.exercises[currentIndex];

  // ─── FEEDBACK PHASE ───
  if (phase === "feedback" && currentFeedback) {
    return (
      <ScrollView style={s.container} contentContainerStyle={{ padding: 20 }}>
        {/* Progress */}
        <View style={s.progressContainer}>
          <View style={s.progressTrack}>
            <Animated.View
              style={[
                s.progressFill,
                {
                  width: progressAnim.interpolate({
                    inputRange: [0, 1],
                    outputRange: ["0%", "100%"],
                  }),
                },
              ]}
            />
          </View>
          <Text style={s.progressText}>
            {currentIndex + 1}/{session.exercises.length}
          </Text>
        </View>

        <View
          style={[
            s.feedbackBadge,
            {
              backgroundColor: currentFeedback.isPassing
                ? Colors.correct
                : Colors.wrong,
            },
          ]}
        >
          <Text style={s.feedbackBadgeText}>
            {currentFeedback.isPassing ? "✓ Correct" : "✗ Needs Work"} —{" "}
            {currentFeedback.score}%
          </Text>
          <Text style={s.feedbackXP}>+{currentFeedback.xpEarned} XP</Text>
        </View>

        <View style={s.feedbackCard}>
          <Text style={s.feedbackTitle}>Feedback</Text>
          <Text style={s.feedbackText}>{currentFeedback.feedback}</Text>
        </View>

        {currentFeedback.suggestedCorrection ? (
          <View style={s.feedbackCard}>
            <Text style={s.feedbackTitle}>Suggested Correction</Text>
            <Text style={s.feedbackText}>
              {currentFeedback.suggestedCorrection}
            </Text>
          </View>
        ) : null}

        <TouchableOpacity style={s.nextButton} onPress={handleNext}>
          <Text style={s.nextButtonText}>
            {currentIndex + 1 >= session.exercises.length
              ? "Finish Session →"
              : "Next Exercise →"}
          </Text>
        </TouchableOpacity>
      </ScrollView>
    );
  }

  // ─── EXERCISE PHASE ───
  return (
    <KeyboardAvoidingView
      style={s.container}
      behavior={Platform.OS === "ios" ? "padding" : undefined}
    >
      <ScrollView contentContainerStyle={{ padding: 20, flexGrow: 1 }}>
        {/* Progress */}
        <View style={s.progressContainer}>
          <View style={s.progressTrack}>
            <Animated.View
              style={[
                s.progressFill,
                {
                  width: progressAnim.interpolate({
                    inputRange: [0, 1],
                    outputRange: ["0%", "100%"],
                  }),
                },
              ]}
            />
          </View>
          <Text style={s.progressText}>
            {currentIndex + 1}/{session.exercises.length}
          </Text>
        </View>

        {/* Lesson context */}
        <View style={s.contextBar}>
          <Text style={s.contextText}>
            {session.currentUnitName} — {session.currentLessonName}
          </Text>
          {exercise.isReview ? (
            <View style={s.reviewTag}>
              <Text style={s.reviewTagText}>🔄 Review</Text>
            </View>
          ) : exercise.isInterleaved ? (
            <View style={[s.reviewTag, { backgroundColor: Colors.secondary }]}>
              <Text style={s.reviewTagText}>🔀 Practice</Text>
            </View>
          ) : null}
        </View>

        {/* Prompt */}
        <View style={s.promptCard}>
          <Text style={s.prompt}>{exercise.prompt}</Text>
          {exercise.context ? (
            <Text style={s.promptContext}>{exercise.context}</Text>
          ) : null}
        </View>

        {/* Hints */}
        {exercise.hints ? (
          <TouchableOpacity
            style={s.hintsToggle}
            onPress={() => setShowHints(!showHints)}
          >
            <Text style={s.hintsToggleText}>
              {showHints ? "Hide Hints ▲" : "Show Hints ▼"}
            </Text>
            {showHints ? (
              <Text style={s.hintsText}>{exercise.hints}</Text>
            ) : null}
          </TouchableOpacity>
        ) : null}

        {/* Answer Input */}
        <TextInput
          style={s.input}
          value={answer}
          onChangeText={setAnswer}
          placeholder="Type your answer..."
          placeholderTextColor={Colors.textMuted}
          multiline
          editable={!submitting}
        />

        {error ? <Text style={s.errorText}>⚠️ {error}</Text> : null}

        {/* Submit */}
        <TouchableOpacity
          style={[
            s.submitButton,
            (!answer.trim() || submitting) && s.submitButtonDisabled,
          ]}
          onPress={handleSubmit}
          disabled={!answer.trim() || submitting}
        >
          {submitting ? (
            <ActivityIndicator color="#fff" />
          ) : (
            <Text style={s.submitButtonText}>Submit Answer</Text>
          )}
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: Colors.background,
  },
  errorText: {
    color: Colors.wrong,
    fontSize: 14,
    marginBottom: 8,
    textAlign: "center",
  },

  // Progress
  progressContainer: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 16,
  },
  progressTrack: {
    flex: 1,
    height: 10,
    backgroundColor: Colors.surfaceLight,
    borderRadius: 5,
    overflow: "hidden",
  },
  progressFill: {
    height: "100%",
    backgroundColor: Colors.primary,
    borderRadius: 5,
  },
  progressText: {
    color: Colors.textMuted,
    fontSize: 13,
    fontWeight: "700",
    marginLeft: 10,
  },

  // Context
  contextBar: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 12,
  },
  contextText: { color: Colors.textSecondary, fontSize: 13, flex: 1 },
  reviewTag: {
    backgroundColor: Colors.accent,
    paddingHorizontal: 10,
    paddingVertical: 3,
    borderRadius: 10,
  },
  reviewTagText: { color: "#fff", fontSize: 11, fontWeight: "700" },

  // Prompt
  promptCard: {
    backgroundColor: Colors.surface,
    borderRadius: 16,
    padding: 18,
    marginBottom: 14,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  prompt: {
    fontSize: 17,
    color: Colors.text,
    fontWeight: "600",
    lineHeight: 24,
  },
  promptContext: {
    fontSize: 14,
    color: Colors.textSecondary,
    marginTop: 10,
    lineHeight: 20,
    fontStyle: "italic",
  },

  // Hints
  hintsToggle: {
    backgroundColor: Colors.surfaceLight,
    borderRadius: 12,
    padding: 12,
    marginBottom: 14,
  },
  hintsToggleText: { color: Colors.secondary, fontSize: 13, fontWeight: "600" },
  hintsText: {
    color: Colors.textSecondary,
    fontSize: 13,
    marginTop: 8,
    lineHeight: 18,
  },

  // Input
  input: {
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    fontSize: 15,
    color: Colors.text,
    borderWidth: 1,
    borderColor: Colors.border,
    minHeight: 120,
    textAlignVertical: "top",
    marginBottom: 14,
  },

  // Submit
  submitButton: {
    backgroundColor: Colors.primary,
    paddingVertical: 16,
    borderRadius: 14,
    alignItems: "center",
    marginBottom: 20,
  },
  submitButtonDisabled: { opacity: 0.5 },
  submitButtonText: { color: "#fff", fontSize: 16, fontWeight: "800" },

  // Feedback
  feedbackBadge: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    borderRadius: 14,
    padding: 16,
    marginBottom: 14,
  },
  feedbackBadgeText: { color: "#fff", fontSize: 16, fontWeight: "800" },
  feedbackXP: { color: "#fff", fontSize: 14, fontWeight: "700" },
  feedbackCard: {
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  feedbackTitle: {
    fontSize: 14,
    fontWeight: "700",
    color: Colors.textMuted,
    marginBottom: 6,
  },
  feedbackText: {
    fontSize: 15,
    color: Colors.text,
    lineHeight: 22,
  },

  // Next button
  nextButton: {
    backgroundColor: Colors.primary,
    paddingVertical: 16,
    borderRadius: 14,
    alignItems: "center",
    marginTop: 8,
    marginBottom: 20,
  },
  nextButtonText: { color: "#fff", fontSize: 16, fontWeight: "800" },

  // Complete
  completeContainer: {
    padding: 24,
    alignItems: "center",
    paddingTop: 40,
  },
  completeEmoji: { fontSize: 56 },
  completeTitle: {
    fontSize: 28,
    fontWeight: "800",
    color: Colors.text,
    marginTop: 12,
    marginBottom: 20,
  },
  completeStats: {
    flexDirection: "row",
    flexWrap: "wrap",
    justifyContent: "center",
    gap: 16,
    marginBottom: 20,
  },
  completeStat: {
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    alignItems: "center",
    minWidth: 80,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  completeStatValue: {
    fontSize: 18,
    fontWeight: "800",
    color: Colors.xp,
  },
  completeStatLabel: {
    fontSize: 11,
    color: Colors.textMuted,
    marginTop: 4,
  },
  completeLevelCard: {
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    width: "100%",
    marginBottom: 16,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  completeLevelTitle: {
    fontSize: 15,
    fontWeight: "700",
    color: Colors.gold,
    textAlign: "center",
    marginBottom: 8,
  },
  completeLevelBar: {
    height: 10,
    backgroundColor: Colors.surfaceLight,
    borderRadius: 5,
    overflow: "hidden",
  },
  completeLevelFill: {
    height: "100%",
    backgroundColor: Colors.xp,
    borderRadius: 5,
  },
  completeAchievement: {
    backgroundColor: Colors.primaryDark,
    borderRadius: 12,
    padding: 12,
    marginBottom: 12,
    width: "100%",
  },
  completeAchievementText: {
    color: Colors.text,
    fontSize: 14,
    fontWeight: "700",
    textAlign: "center",
  },
  completeReview: {
    color: Colors.accent,
    fontSize: 13,
    marginBottom: 12,
  },
  completeButton: {
    backgroundColor: Colors.primary,
    paddingVertical: 16,
    paddingHorizontal: 32,
    borderRadius: 16,
    marginTop: 12,
  },
  completeButtonText: { color: "#fff", fontSize: 16, fontWeight: "800" },

  weakAreasCard: {
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    marginBottom: 12,
    width: "100%",
    borderWidth: 1,
    borderColor: Colors.border,
  },
  weakAreasTitle: {
    fontSize: 14,
    fontWeight: "700",
    color: Colors.textMuted,
    marginBottom: 8,
  },
  weakAreasItem: {
    fontSize: 14,
    color: Colors.text,
    lineHeight: 22,
  },
});
