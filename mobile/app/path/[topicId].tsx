import { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
} from "react-native";
import { useLocalSearchParams, router } from "expo-router";
import { getLearningPath, startSession } from "../../lib/api";
import { Colors } from "../../lib/constants";

interface PathLesson {
  lessonId: string;
  name: string;
  orderIndex: number;
  isCurrent: boolean;
  isCompleted: boolean;
  isLocked: boolean;
  crowns: number;
  bestScore: number;
}

interface PathUnit {
  unitId: string;
  name: string;
  description: string;
  orderIndex: number;
  isCurrent: boolean;
  isCompleted: boolean;
  isLocked: boolean;
  lessons: PathLesson[];
}

interface LearningPath {
  topicId: string;
  topicName: string;
  currentUnitIndex: number;
  currentLessonIndex: number;
  totalXPEarned: number;
  units: PathUnit[];
}

const CrownDisplay = ({ count }: { count: number }) => {
  const maxCrowns = 5;
  return (
    <View style={pathStyles.crowns}>
      {Array.from({ length: maxCrowns }).map((_, i) => (
        <Text
          key={i}
          style={[
            pathStyles.crownIcon,
            { opacity: i < count ? 1 : 0.2 },
          ]}
        >
          👑
        </Text>
      ))}
    </View>
  );
};

export default function PathScreen() {
  const { topicId } = useLocalSearchParams<{ topicId: string }>();
  const [path, setPath] = useState<LearningPath | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPath = useCallback(async () => {
    if (!topicId) return;
    try {
      setError(null);
      const response = await getLearningPath(topicId);
      setPath(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load path");
    } finally {
      setLoading(false);
    }
  }, [topicId]);

  useEffect(() => {
    fetchPath();
  }, [fetchPath]);

  const handleStartSession = async () => {
    if (!topicId) return;
    setStarting(true);
    try {
      const response = await startSession(topicId);
      router.push({
        pathname: "/session/[topicId]",
        params: { topicId, sessionData: JSON.stringify(response.data) },
      });
    } catch (err: any) {
      setError(err.message || "Failed to start session");
    } finally {
      setStarting(false);
    }
  };

  if (loading) {
    return (
      <View style={pathStyles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={pathStyles.loadingText}>Loading your path...</Text>
      </View>
    );
  }

  if (error || !path) {
    return (
      <View style={pathStyles.center}>
        <Text style={pathStyles.errorText}>⚠️ {error || "No data"}</Text>
        <TouchableOpacity style={pathStyles.retryBtn} onPress={fetchPath}>
          <Text style={pathStyles.retryBtnText}>Retry</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={pathStyles.container}>
      {/* Header */}
      <View style={pathStyles.header}>
        <Text style={pathStyles.title}>{path.topicName}</Text>
        <View style={pathStyles.xpBadge}>
          <Text style={pathStyles.xpText}>⚡ {path.totalXPEarned} XP</Text>
        </View>
      </View>

      {/* Start Session Button */}
      <TouchableOpacity
        style={[pathStyles.sessionButton, starting && pathStyles.sessionButtonDisabled]}
        onPress={handleStartSession}
        disabled={starting}
        activeOpacity={0.8}
      >
        {starting ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={pathStyles.sessionButtonText}>▶ Start Session</Text>
        )}
      </TouchableOpacity>

      {/* Path */}
      <ScrollView
        contentContainerStyle={pathStyles.pathContainer}
        showsVerticalScrollIndicator={false}
      >
        {path.units.map((unit, unitIdx) => (
          <View key={unit.unitId}>
            {/* Unit Header */}
            <View
              style={[
                pathStyles.unitHeader,
                unit.isCompleted && pathStyles.unitHeaderCompleted,
                unit.isCurrent && pathStyles.unitHeaderCurrent,
                unit.isLocked && pathStyles.unitHeaderLocked,
              ]}
            >
              <Text style={pathStyles.unitIndex}>Unit {unitIdx + 1}</Text>
              <Text style={pathStyles.unitName}>{unit.name}</Text>
              {unit.isCompleted ? (
                <Text style={pathStyles.unitCheck}>✅</Text>
              ) : unit.isLocked ? (
                <Text style={pathStyles.unitLock}>🔒</Text>
              ) : null}
            </View>

            {/* Lesson Nodes */}
            {unit.lessons.map((lesson, lessonIdx) => {
              const isEven = lessonIdx % 2 === 0;
              return (
                <View key={lesson.lessonId}>
                  {/* Connector line */}
                  {lessonIdx > 0 ? (
                    <View style={pathStyles.connector}>
                      <View
                        style={[
                          pathStyles.connectorLine,
                          lesson.isLocked && pathStyles.connectorLocked,
                        ]}
                      />
                    </View>
                  ) : null}

                  <View
                    style={[
                      pathStyles.nodeRow,
                      { justifyContent: isEven ? "flex-start" : "flex-end" },
                    ]}
                  >
                    <View
                      style={[
                        pathStyles.node,
                        lesson.isCompleted && pathStyles.nodeCompleted,
                        lesson.isCurrent && pathStyles.nodeCurrent,
                        lesson.isLocked && pathStyles.nodeLocked,
                      ]}
                    >
                      <Text
                        style={[
                          pathStyles.nodeEmoji,
                          lesson.isLocked && pathStyles.nodeEmojiLocked,
                        ]}
                      >
                        {lesson.isCompleted
                          ? "⭐"
                          : lesson.isCurrent
                          ? "📖"
                          : lesson.isLocked
                          ? "🔒"
                          : "📘"}
                      </Text>
                      <Text
                        style={[
                          pathStyles.nodeLabel,
                          lesson.isLocked && pathStyles.nodeLabelLocked,
                        ]}
                        numberOfLines={2}
                      >
                        {lesson.name}
                      </Text>
                      {lesson.crowns > 0 ? (
                        <CrownDisplay count={lesson.crowns} />
                      ) : null}
                      {lesson.bestScore > 0 && !lesson.isLocked ? (
                        <Text style={pathStyles.nodeScore}>
                          Best: {lesson.bestScore}%
                        </Text>
                      ) : null}
                    </View>
                  </View>
                </View>
              );
            })}

            {/* Separator between units */}
            {unitIdx < path.units.length - 1 ? (
              <View style={pathStyles.unitSeparator}>
                <View style={pathStyles.unitSeparatorLine} />
              </View>
            ) : null}
          </View>
        ))}
      </ScrollView>
    </View>
  );
}

const pathStyles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: Colors.background,
  },
  loadingText: { color: Colors.textSecondary, marginTop: 12, fontSize: 16 },
  errorText: { color: Colors.wrong, fontSize: 15, marginBottom: 12 },
  retryBtn: {
    backgroundColor: Colors.primary,
    paddingHorizontal: 24,
    paddingVertical: 10,
    borderRadius: 12,
  },
  retryBtnText: { color: Colors.text, fontWeight: "700" },

  // Header
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: 20,
    paddingTop: 16,
    paddingBottom: 8,
  },
  title: { fontSize: 22, fontWeight: "800", color: Colors.text, flex: 1 },
  xpBadge: {
    backgroundColor: Colors.surfaceLight,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 14,
  },
  xpText: { color: Colors.xp, fontSize: 14, fontWeight: "700" },

  // Session button
  sessionButton: {
    backgroundColor: Colors.primary,
    marginHorizontal: 20,
    marginVertical: 12,
    paddingVertical: 16,
    borderRadius: 16,
    alignItems: "center",
    shadowColor: Colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 6,
  },
  sessionButtonDisabled: { opacity: 0.6 },
  sessionButtonText: { color: "#fff", fontSize: 18, fontWeight: "800" },

  // Path
  pathContainer: { paddingHorizontal: 20, paddingBottom: 40 },

  // Unit header
  unitHeader: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 14,
    marginTop: 20,
    marginBottom: 12,
    borderWidth: 2,
    borderColor: Colors.border,
  },
  unitHeaderCompleted: { borderColor: Colors.correct },
  unitHeaderCurrent: { borderColor: Colors.primary },
  unitHeaderLocked: { opacity: 0.5 },
  unitIndex: {
    fontSize: 12,
    fontWeight: "700",
    color: Colors.textMuted,
    marginRight: 8,
  },
  unitName: {
    fontSize: 16,
    fontWeight: "700",
    color: Colors.text,
    flex: 1,
  },
  unitCheck: { fontSize: 18 },
  unitLock: { fontSize: 18 },

  // Connector
  connector: { alignItems: "center", height: 24 },
  connectorLine: {
    width: 3,
    height: "100%",
    backgroundColor: Colors.primary,
    borderRadius: 2,
  },
  connectorLocked: { backgroundColor: Colors.border },

  // Node
  nodeRow: {
    flexDirection: "row",
    paddingHorizontal: 20,
  },
  node: {
    width: 130,
    alignItems: "center",
    backgroundColor: Colors.surface,
    borderRadius: 18,
    padding: 14,
    borderWidth: 2,
    borderColor: Colors.border,
  },
  nodeCompleted: {
    borderColor: Colors.correct,
    backgroundColor: Colors.surfaceLight,
  },
  nodeCurrent: {
    borderColor: Colors.primary,
    borderWidth: 3,
    shadowColor: Colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.4,
    shadowRadius: 8,
    elevation: 4,
  },
  nodeLocked: { opacity: 0.4 },
  nodeEmoji: { fontSize: 28, marginBottom: 6 },
  nodeEmojiLocked: { fontSize: 22 },
  nodeLabel: {
    fontSize: 12,
    fontWeight: "600",
    color: Colors.text,
    textAlign: "center",
  },
  nodeLabelLocked: { color: Colors.textMuted },
  nodeScore: {
    fontSize: 10,
    color: Colors.textMuted,
    marginTop: 4,
  },

  // Crowns
  crowns: { flexDirection: "row", marginTop: 4 },
  crownIcon: { fontSize: 10, marginHorizontal: 1 },

  // Unit separator
  unitSeparator: { alignItems: "center", height: 32 },
  unitSeparatorLine: {
    width: 3,
    height: "100%",
    backgroundColor: Colors.border,
    borderRadius: 2,
  },
});
