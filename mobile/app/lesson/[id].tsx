import { useEffect, useState, useRef } from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  ScrollView,
  Animated,
} from "react-native";
import { useLocalSearchParams, router } from "expo-router";
import {
  getExercises,
  generateExercises,
  submitAnswer,
  voteOnExercise,
} from "../../lib/api";
import { Colors } from "../../lib/constants";

interface Exercise {
  id: string;
  orderIndex: number;
  exerciseType: number;
  difficultyLevel: number;
  prompt: string;
  context: string | null;
  hints: string | null;
  maxScore: number;
  upvoteCount: number;
  downvoteCount: number;
  voteScore: number;
  userVote: boolean | null;
}

interface AttemptResult {
  attemptId: string;
  score: number;
  isPassing: boolean;
  feedback: string;
  suggestedCorrection: string | null;
  detailedBreakdown: string | null;
  xpEarned: number;
  dailySubmissionsRemaining: number;
  isLessonComplete: boolean;
  upvoteCount: number;
  downvoteCount: number;
  userVote: boolean | null;
}

type ScreenState = "loading" | "generate" | "exercise" | "feedback" | "complete";

export default function LessonScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [screenState, setScreenState] = useState<ScreenState>("loading");
  const [answer, setAnswer] = useState("");
  const [result, setResult] = useState<AttemptResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [generating, setGenerating] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [showHints, setShowHints] = useState(false);
  const [startTime, setStartTime] = useState<number>(Date.now());
  const [totalXP, setTotalXP] = useState(0);
  const [currentVote, setCurrentVote] = useState<boolean | null>(null);
  const [voteLoading, setVoteLoading] = useState(false);
  const progressAnim = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    if (!id) return;
    loadExercises();
  }, [id]);

  useEffect(() => {
    if (exercises.length > 0) {
      Animated.timing(progressAnim, {
        toValue: (currentIndex + 1) / exercises.length,
        duration: 300,
        useNativeDriver: false,
      }).start();
    }
  }, [currentIndex, exercises.length]);

  const loadExercises = async () => {
    try {
      const res = await getExercises(id!);
      if (res.data.length > 0) {
        setExercises(res.data);
        setScreenState("exercise");
        setStartTime(Date.now());
      } else {
        setScreenState("generate");
      }
    } catch {
      setScreenState("generate");
    }
  };

  const handleGenerate = async () => {
    setGenerating(true);
    setError(null);
    try {
      const res = await generateExercises(id!);
      setExercises(res.data);
      setScreenState("exercise");
      setStartTime(Date.now());
    } catch (err: any) {
      setError(
        err.response?.data?.title || "Failed to generate exercises. Try again."
      );
    } finally {
      setGenerating(false);
    }
  };

  const handleSubmit = async () => {
    if (!answer.trim()) return;

    const exercise = exercises[currentIndex];
    const timeTaken = Math.round((Date.now() - startTime) / 1000);

    setSubmitting(true);
    setError(null);

    try {
      const res = await submitAnswer({
        exerciseId: exercise.id,
        userAnswer: answer.trim(),
        timeTakenSeconds: timeTaken,
      });
      setResult(res.data);
      setCurrentVote(res.data.userVote);
      setTotalXP((prev) => prev + res.data.xpEarned);
      setScreenState("feedback");
    } catch (err: any) {
      setError(
        err.response?.data?.title || "Failed to submit answer. Try again."
      );
    } finally {
      setSubmitting(false);
    }
  };

  const handleVote = async (isUpvote: boolean) => {
    const exercise = exercises[currentIndex];
    setVoteLoading(true);
    try {
      const res = await voteOnExercise(exercise.id, isUpvote);
      setCurrentVote(res.data.userVote);
      // Update exercise in list
      setExercises((prev) =>
        prev.map((e) =>
          e.id === exercise.id
            ? {
                ...e,
                upvoteCount: res.data.upvoteCount,
                downvoteCount: res.data.downvoteCount,
                voteScore: res.data.voteScore,
                userVote: res.data.userVote,
              }
            : e
        )
      );
    } catch {
      // Silently fail vote
    } finally {
      setVoteLoading(false);
    }
  };

  const handleNext = () => {
    if (result?.isLessonComplete || currentIndex >= exercises.length - 1) {
      setScreenState("complete");
    } else {
      setCurrentIndex((prev) => prev + 1);
      setAnswer("");
      setResult(null);
      setCurrentVote(null);
      setShowHints(false);
      setStartTime(Date.now());
      setScreenState("exercise");
    }
  };

  // ---- GENERATE SCREEN ----
  if (screenState === "loading") {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={styles.loadingText}>Loading exercises...</Text>
      </View>
    );
  }

  if (screenState === "generate") {
    return (
      <View style={styles.center}>
        <Text style={styles.generateIcon}>🧠</Text>
        <Text style={styles.generateTitle}>Ready to Learn</Text>
        <Text style={styles.generateSubtitle}>
          AI will generate exercises tailored to this lesson's content.
        </Text>
        {error ? <Text style={styles.errorText}>{error}</Text> : null}
        <TouchableOpacity
          style={[styles.primaryButton, generating && styles.buttonDisabled]}
          onPress={handleGenerate}
          disabled={generating}
        >
          {generating ? (
            <ActivityIndicator color={Colors.text} />
          ) : (
            <Text style={styles.primaryButtonText}>Generate Exercises</Text>
          )}
        </TouchableOpacity>
      </View>
    );
  }

  // ---- COMPLETE SCREEN ----
  if (screenState === "complete") {
    return (
      <View style={styles.center}>
        <Text style={styles.completeIcon}>🎉</Text>
        <Text style={styles.completeTitle}>Lesson Complete!</Text>
        <Text style={styles.completeXP}>+{totalXP} XP earned</Text>
        <Text style={styles.completeSubtitle}>
          You completed {exercises.length} exercise
          {exercises.length !== 1 ? "s" : ""}
        </Text>
        <TouchableOpacity
          style={styles.primaryButton}
          onPress={() => router.back()}
        >
          <Text style={styles.primaryButtonText}>Back to Lessons</Text>
        </TouchableOpacity>
      </View>
    );
  }

  const exercise = exercises[currentIndex];

  // ---- FEEDBACK SCREEN ----
  if (screenState === "feedback" && result) {
    return (
      <ScrollView
        style={styles.container}
        contentContainerStyle={styles.feedbackContent}
      >
        {/* Progress bar */}
        <View style={styles.progressBar}>
          <Animated.View
            style={[
              styles.progressFill,
              {
                width: progressAnim.interpolate({
                  inputRange: [0, 1],
                  outputRange: ["0%", "100%"],
                }),
              },
            ]}
          />
        </View>

        {/* Score badge */}
        <View
          style={[
            styles.scoreBadge,
            result.isPassing ? styles.scorePassing : styles.scoreFailing,
          ]}
        >
          <Text style={styles.scoreNumber}>{result.score}</Text>
          <Text style={styles.scoreLabel}>
            {result.isPassing ? "PASSED" : "NEEDS WORK"}
          </Text>
        </View>

        {/* XP earned */}
        <View style={styles.xpRow}>
          <Text style={styles.xpText}>+{result.xpEarned} XP</Text>
        </View>

        {/* Feedback */}
        <View style={styles.feedbackCard}>
          <Text style={styles.feedbackLabel}>Feedback</Text>
          <Text style={styles.feedbackText}>{result.feedback}</Text>
        </View>

        {result.suggestedCorrection ? (
          <View style={styles.feedbackCard}>
            <Text style={styles.feedbackLabel}>Suggested Improvement</Text>
            <Text style={styles.feedbackText}>
              {result.suggestedCorrection}
            </Text>
          </View>
        ) : null}

        {result.detailedBreakdown ? (
          <View style={styles.feedbackCard}>
            <Text style={styles.feedbackLabel}>Breakdown</Text>
            <Text style={styles.feedbackText}>{result.detailedBreakdown}</Text>
          </View>
        ) : null}

        {/* Vote buttons */}
        <View style={styles.voteSection}>
          <Text style={styles.voteLabel}>Was this a good question?</Text>
          <View style={styles.voteRow}>
            <TouchableOpacity
              style={[
                styles.voteButton,
                currentVote === true && styles.voteButtonActive,
              ]}
              onPress={() => handleVote(true)}
              disabled={voteLoading}
            >
              <Text style={styles.voteIcon}>👍</Text>
              <Text
                style={[
                  styles.voteCount,
                  currentVote === true && styles.voteCountActive,
                ]}
              >
                {exercises[currentIndex]?.upvoteCount ?? 0}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[
                styles.voteButton,
                currentVote === false && styles.voteButtonActiveDown,
              ]}
              onPress={() => handleVote(false)}
              disabled={voteLoading}
            >
              <Text style={styles.voteIcon}>👎</Text>
              <Text
                style={[
                  styles.voteCount,
                  currentVote === false && styles.voteCountActiveDown,
                ]}
              >
                {exercises[currentIndex]?.downvoteCount ?? 0}
              </Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* Next button */}
        <TouchableOpacity style={styles.primaryButton} onPress={handleNext}>
          <Text style={styles.primaryButtonText}>
            {result.isLessonComplete ||
            currentIndex >= exercises.length - 1
              ? "Finish Lesson"
              : "Next Exercise"}
          </Text>
        </TouchableOpacity>

        <Text style={styles.remainingText}>
          {result.dailySubmissionsRemaining} submissions remaining today
        </Text>
      </ScrollView>
    );
  }

  // ---- EXERCISE SCREEN ----
  return (
    <ScrollView
      style={styles.container}
      contentContainerStyle={styles.exerciseContent}
    >
      {/* Progress bar */}
      <View style={styles.progressBar}>
        <Animated.View
          style={[
            styles.progressFill,
            {
              width: progressAnim.interpolate({
                inputRange: [0, 1],
                outputRange: ["0%", "100%"],
              }),
            },
          ]}
        />
      </View>

      <Text style={styles.exerciseCounter}>
        Exercise {currentIndex + 1} of {exercises.length}
      </Text>

      {/* Prompt */}
      <View style={styles.promptCard}>
        <Text style={styles.promptText}>{exercise.prompt}</Text>
      </View>

      {/* Context */}
      {exercise.context ? (
        <View style={styles.contextCard}>
          <Text style={styles.contextLabel}>Context</Text>
          <Text style={styles.contextText}>{exercise.context}</Text>
        </View>
      ) : null}

      {/* Hints toggle */}
      {exercise.hints ? (
        <TouchableOpacity
          style={styles.hintsToggle}
          onPress={() => setShowHints(!showHints)}
        >
          <Text style={styles.hintsToggleText}>
            {showHints ? "Hide Hints" : "💡 Show Hints"}
          </Text>
          {showHints ? (
            <Text style={styles.hintsText}>{exercise.hints}</Text>
          ) : null}
        </TouchableOpacity>
      ) : null}

      {/* Answer input */}
      <TextInput
        style={styles.answerInput}
        placeholder="Type your answer here..."
        placeholderTextColor={Colors.textMuted}
        value={answer}
        onChangeText={setAnswer}
        multiline
        numberOfLines={6}
        textAlignVertical="top"
      />

      {error ? <Text style={styles.errorText}>{error}</Text> : null}

      {/* Submit button */}
      <TouchableOpacity
        style={[
          styles.primaryButton,
          (!answer.trim() || submitting) && styles.buttonDisabled,
        ]}
        onPress={handleSubmit}
        disabled={!answer.trim() || submitting}
      >
        {submitting ? (
          <ActivityIndicator color={Colors.text} />
        ) : (
          <Text style={styles.primaryButtonText}>Submit Answer</Text>
        )}
      </TouchableOpacity>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: Colors.background,
    paddingHorizontal: 32,
  },
  loadingText: { color: Colors.textSecondary, marginTop: 12, fontSize: 16 },

  // Progress bar
  progressBar: {
    height: 6,
    backgroundColor: Colors.surface,
    borderRadius: 3,
    marginHorizontal: 20,
    marginTop: 12,
    overflow: "hidden",
  },
  progressFill: {
    height: "100%",
    backgroundColor: Colors.primary,
    borderRadius: 3,
  },

  // Generate screen
  generateIcon: { fontSize: 64, marginBottom: 16 },
  generateTitle: {
    fontSize: 24,
    fontWeight: "bold",
    color: Colors.text,
    marginBottom: 8,
  },
  generateSubtitle: {
    fontSize: 15,
    color: Colors.textSecondary,
    textAlign: "center",
    marginBottom: 24,
    lineHeight: 22,
  },

  // Complete screen
  completeIcon: { fontSize: 72, marginBottom: 16 },
  completeTitle: {
    fontSize: 28,
    fontWeight: "bold",
    color: Colors.text,
    marginBottom: 8,
  },
  completeXP: {
    fontSize: 22,
    fontWeight: "700",
    color: Colors.xp,
    marginBottom: 8,
  },
  completeSubtitle: {
    fontSize: 15,
    color: Colors.textSecondary,
    marginBottom: 32,
  },

  // Exercise screen
  exerciseContent: { paddingBottom: 40 },
  exerciseCounter: {
    fontSize: 13,
    color: Colors.textMuted,
    textAlign: "center",
    marginTop: 12,
    marginBottom: 16,
  },
  promptCard: {
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 20,
    marginHorizontal: 16,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  promptText: {
    fontSize: 17,
    color: Colors.text,
    lineHeight: 26,
    fontWeight: "600",
  },
  contextCard: {
    backgroundColor: Colors.surfaceLight,
    borderRadius: 12,
    padding: 16,
    marginHorizontal: 16,
    marginTop: 12,
  },
  contextLabel: {
    fontSize: 12,
    fontWeight: "700",
    color: Colors.secondary,
    marginBottom: 6,
    textTransform: "uppercase",
  },
  contextText: { fontSize: 14, color: Colors.textSecondary, lineHeight: 20 },
  hintsToggle: {
    marginHorizontal: 16,
    marginTop: 12,
    padding: 12,
    backgroundColor: Colors.surface,
    borderRadius: 10,
  },
  hintsToggleText: {
    fontSize: 14,
    color: Colors.accent,
    fontWeight: "600",
  },
  hintsText: {
    fontSize: 14,
    color: Colors.textSecondary,
    marginTop: 8,
    lineHeight: 20,
  },
  answerInput: {
    backgroundColor: Colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: Colors.border,
    color: Colors.text,
    fontSize: 15,
    padding: 16,
    marginHorizontal: 16,
    marginTop: 16,
    minHeight: 120,
    lineHeight: 22,
  },
  errorText: {
    color: Colors.wrong,
    fontSize: 14,
    textAlign: "center",
    marginTop: 8,
  },

  // Feedback screen
  feedbackContent: { paddingBottom: 40 },
  scoreBadge: {
    alignSelf: "center",
    alignItems: "center",
    paddingVertical: 20,
    paddingHorizontal: 32,
    borderRadius: 20,
    marginTop: 20,
  },
  scorePassing: { backgroundColor: "rgba(88,204,2,0.15)" },
  scoreFailing: { backgroundColor: "rgba(255,75,75,0.15)" },
  scoreNumber: { fontSize: 48, fontWeight: "bold", color: Colors.text },
  scoreLabel: {
    fontSize: 14,
    fontWeight: "700",
    color: Colors.textSecondary,
    marginTop: 4,
  },
  xpRow: { alignItems: "center", marginTop: 12 },
  xpText: { fontSize: 20, fontWeight: "700", color: Colors.xp },
  feedbackCard: {
    backgroundColor: Colors.surface,
    borderRadius: 12,
    padding: 16,
    marginHorizontal: 16,
    marginTop: 16,
  },
  feedbackLabel: {
    fontSize: 12,
    fontWeight: "700",
    color: Colors.secondary,
    textTransform: "uppercase",
    marginBottom: 8,
  },
  feedbackText: { fontSize: 15, color: Colors.text, lineHeight: 22 },

  // Vote section
  voteSection: {
    alignItems: "center",
    marginTop: 20,
    paddingHorizontal: 16,
  },
  voteLabel: {
    fontSize: 14,
    color: Colors.textSecondary,
    marginBottom: 10,
  },
  voteRow: { flexDirection: "row", gap: 16 },
  voteButton: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    backgroundColor: Colors.surface,
    borderRadius: 12,
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  voteButtonActive: {
    borderColor: Colors.primary,
    backgroundColor: "rgba(88,204,2,0.1)",
  },
  voteButtonActiveDown: {
    borderColor: Colors.wrong,
    backgroundColor: "rgba(255,75,75,0.1)",
  },
  voteIcon: { fontSize: 20 },
  voteCount: { fontSize: 14, color: Colors.textMuted, fontWeight: "600" },
  voteCountActive: { color: Colors.primary },
  voteCountActiveDown: { color: Colors.wrong },

  // Buttons
  primaryButton: {
    backgroundColor: Colors.primary,
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: "center",
    marginHorizontal: 16,
    marginTop: 20,
  },
  buttonDisabled: { opacity: 0.5 },
  primaryButtonText: { color: Colors.text, fontSize: 17, fontWeight: "700" },
  remainingText: {
    fontSize: 12,
    color: Colors.textMuted,
    textAlign: "center",
    marginTop: 12,
  },
});
