import { useEffect, useState, useCallback } from "react";
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  RefreshControl,
  Modal,
} from "react-native";
import { router } from "expo-router";
import {
  getDashboard,
  getTopics,
  enrollInTopic,
} from "../../lib/api";
import { Colors, SubjectDomains, DifficultyLevels } from "../../lib/constants";
import { useAuth } from "../../lib/auth";

interface EnrolledTopic {
  topicId: string;
  topicName: string;
  topicDescription: string;
  subjectDomain: number;
  difficultyLevel: number;
  iconUrl: string | null;
  currentUnitIndex: number;
  currentLessonIndex: number;
  currentUnitName: string;
  currentLessonName: string;
  totalUnits: number;
  totalXPEarned: number;
  sessionsCompleted: number;
  lastSessionDate: string | null;
}

interface Dashboard {
  totalXP: number;
  level: number;
  levelTitle: string;
  levelProgress: number;
  xpForNextLevel: number;
  currentStreak: number;
  longestStreak: number;
  streakFreezeCount: number;
  enrolledTopics: EnrolledTopic[];
  reviewItemsDue: number;
  dailySubmissionsRemaining: number;
}

interface Topic {
  id: string;
  name: string;
  description: string;
  subjectDomain: number;
  difficultyLevel: number;
  totalUnits: number;
}

export default function DashboardScreen() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showEnroll, setShowEnroll] = useState(false);
  const [availableTopics, setAvailableTopics] = useState<Topic[]>([]);
  const [enrolling, setEnrolling] = useState(false);

  const fetchDashboard = useCallback(async () => {
    try {
      setError(null);
      setLoading(true);
      const response = await getDashboard();
      setDashboard(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load dashboard");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    if (authLoading) return;
    if (!isAuthenticated) {
      router.replace("/login");
      return;
    }
    fetchDashboard();
  }, [authLoading, isAuthenticated, fetchDashboard]);

  const onRefresh = () => {
    setRefreshing(true);
    fetchDashboard();
  };

  const handleEnrollPress = async () => {
    try {
      const response = await getTopics();
      const enrolled = new Set(
        dashboard?.enrolledTopics.map((t) => t.topicId) ?? []
      );
      setAvailableTopics(
        response.data.filter((t: Topic) => !enrolled.has(t.id))
      );
      setShowEnroll(true);
    } catch {
      setError("Failed to load available topics");
    }
  };

  const handleEnroll = async (topicId: string) => {
    setEnrolling(true);
    try {
      await enrollInTopic(topicId);
      setShowEnroll(false);
      fetchDashboard();
    } catch {
      setError("Failed to enroll");
    } finally {
      setEnrolling(false);
    }
  };

  if (authLoading || (loading && !refreshing)) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={styles.loadingText}>Loading your dashboard...</Text>
      </View>
    );
  }

  if (error && !dashboard) {
    return (
      <View style={styles.center}>
        <Text style={styles.errorText}>⚠️ {error}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={fetchDashboard}>
          <Text style={styles.retryText}>Retry</Text>
        </TouchableOpacity>
      </View>
    );
  }

  if (!dashboard) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
      </View>
    );
  }

  const data = dashboard;

  return (
    <View style={styles.container}>
      <FlatList
        data={data.enrolledTopics}
        keyExtractor={(item) => item.topicId}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={onRefresh}
            tintColor={Colors.primary}
          />
        }
        contentContainerStyle={styles.list}
        ListHeaderComponent={
          <>
            {/* Stats Bar */}
            <View style={styles.statsBar}>
              <View style={styles.statItem}>
                <Text style={styles.statIcon}>🔥</Text>
                <Text style={styles.statValue}>{data.currentStreak}</Text>
                <Text style={styles.statLabel}>Streak</Text>
              </View>
              <View style={styles.statDivider} />
              <View style={styles.statItem}>
                <Text style={styles.statIcon}>⚡</Text>
                <Text style={[styles.statValue, { color: Colors.xp }]}>
                  {data.totalXP}
                </Text>
                <Text style={styles.statLabel}>Total XP</Text>
              </View>
              <View style={styles.statDivider} />
              <View style={styles.statItem}>
                <Text style={styles.statIcon}>🏅</Text>
                <Text style={[styles.statValue, { color: Colors.gold }]}>
                  Lv.{data.level}
                </Text>
                <Text style={styles.statLabel}>{data.levelTitle}</Text>
              </View>
            </View>

            {/* Level Progress */}
            <View style={styles.levelBar}>
              <View style={styles.levelBarTrack}>
                <View
                  style={[
                    styles.levelBarFill,
                    { width: `${Math.round(data.levelProgress * 100)}%` },
                  ]}
                />
              </View>
              <Text style={styles.levelBarText}>
                {data.totalXP} / {data.xpForNextLevel} XP
              </Text>
            </View>

            {/* Review Badge */}
            {data.reviewItemsDue > 0 ? (
              <View style={styles.reviewBanner}>
                <Text style={styles.reviewIcon}>🔄</Text>
                <Text style={styles.reviewText}>
                  {data.reviewItemsDue} review
                  {data.reviewItemsDue === 1 ? "" : "s"} due — they'll appear
                  in your next session
                </Text>
              </View>
            ) : null}

            {/* Section header */}
            <View style={styles.sectionHeader}>
              <Text style={styles.sectionTitle}>My Courses</Text>
              <TouchableOpacity
                style={styles.addButton}
                onPress={handleEnrollPress}
              >
                <Text style={styles.addButtonText}>+ Add</Text>
              </TouchableOpacity>
            </View>
          </>
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyIcon}>🎯</Text>
            <Text style={styles.emptyTitle}>No courses yet</Text>
            <Text style={styles.emptySubtitle}>
              Enroll in a topic to start your learning journey!
            </Text>
            <TouchableOpacity
              style={styles.enrollButton}
              onPress={handleEnrollPress}
            >
              <Text style={styles.enrollButtonText}>Browse Topics</Text>
            </TouchableOpacity>
          </View>
        }
        renderItem={({ item }) => (
          <TouchableOpacity
            style={styles.courseCard}
            onPress={() => router.push(`/path/${item.topicId}`)}
            activeOpacity={0.7}
          >
            <View style={styles.courseHeader}>
              <View style={{ flex: 1 }}>
                <Text style={styles.courseName}>{item.topicName}</Text>
                <Text style={styles.courseMeta}>
                  {SubjectDomains[item.subjectDomain] ?? "General"} •{" "}
                  {DifficultyLevels[item.difficultyLevel] ?? "Unknown"}
                </Text>
              </View>
              <View style={styles.courseXP}>
                <Text style={styles.courseXPValue}>{item.totalXPEarned}</Text>
                <Text style={styles.courseXPLabel}>XP</Text>
              </View>
            </View>

            <View style={styles.courseProgress}>
              <Text style={styles.courseCurrentLabel}>Next up:</Text>
              <Text style={styles.courseCurrentValue}>
                {item.currentUnitName} — {item.currentLessonName}
              </Text>
            </View>

            <View style={styles.courseFooter}>
              <Text style={styles.courseStats}>
                {item.sessionsCompleted} session
                {item.sessionsCompleted === 1 ? "" : "s"} • Unit{" "}
                {item.currentUnitIndex + 1}/{item.totalUnits}
              </Text>
              <View style={styles.startButton}>
                <Text style={styles.startButtonText}>Continue →</Text>
              </View>
            </View>
          </TouchableOpacity>
        )}
      />

      {/* Enroll Modal */}
      <Modal visible={showEnroll} animationType="slide" transparent>
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <View style={styles.modalHeader}>
              <Text style={styles.modalTitle}>Choose a Topic</Text>
              <TouchableOpacity onPress={() => setShowEnroll(false)}>
                <Text style={styles.modalClose}>✕</Text>
              </TouchableOpacity>
            </View>

            {availableTopics.length === 0 ? (
              <Text style={styles.modalEmpty}>
                No more topics available to enroll in.
              </Text>
            ) : (
              <FlatList
                data={availableTopics}
                keyExtractor={(t) => t.id}
                renderItem={({ item: t }) => (
                  <TouchableOpacity
                    style={styles.topicOption}
                    onPress={() => handleEnroll(t.id)}
                    disabled={enrolling}
                  >
                    <Text style={styles.topicOptionName}>{t.name}</Text>
                    <Text style={styles.topicOptionDesc} numberOfLines={2}>
                      {t.description}
                    </Text>
                    <View style={styles.topicOptionMeta}>
                      <Text style={styles.topicOptionBadge}>
                        {SubjectDomains[t.subjectDomain] ?? "General"}
                      </Text>
                      <Text style={styles.topicOptionBadge}>
                        {t.totalUnits} units
                      </Text>
                    </View>
                  </TouchableOpacity>
                )}
              />
            )}
          </View>
        </View>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: Colors.background,
  },
  loadingText: { color: Colors.textSecondary, marginTop: 12, fontSize: 16 },
  errorText: { color: Colors.wrong, fontSize: 15, marginBottom: 12 },
  retryButton: {
    backgroundColor: Colors.primary,
    paddingHorizontal: 24,
    paddingVertical: 10,
    borderRadius: 12,
  },
  retryText: { color: Colors.text, fontWeight: "700" },
  list: { paddingHorizontal: 16, paddingBottom: 20 },

  // Stats bar
  statsBar: {
    flexDirection: "row",
    backgroundColor: Colors.surface,
    borderRadius: 16,
    padding: 16,
    marginTop: 16,
    marginBottom: 8,
    borderWidth: 1,
    borderColor: Colors.border,
    alignItems: "center",
    justifyContent: "space-around",
  },
  statItem: { alignItems: "center", flex: 1 },
  statIcon: { fontSize: 22, marginBottom: 4 },
  statValue: { fontSize: 22, fontWeight: "800", color: Colors.streak },
  statLabel: { fontSize: 11, color: Colors.textMuted, marginTop: 2 },
  statDivider: { width: 1, height: 36, backgroundColor: Colors.border },

  // Level bar
  levelBar: { paddingHorizontal: 4, marginBottom: 12 },
  levelBarTrack: {
    height: 8,
    backgroundColor: Colors.surfaceLight,
    borderRadius: 4,
    overflow: "hidden",
  },
  levelBarFill: {
    height: "100%",
    backgroundColor: Colors.xp,
    borderRadius: 4,
  },
  levelBarText: {
    fontSize: 11,
    color: Colors.textMuted,
    textAlign: "right",
    marginTop: 4,
  },

  // Review banner
  reviewBanner: {
    flexDirection: "row",
    backgroundColor: Colors.surfaceLight,
    borderRadius: 12,
    padding: 12,
    marginBottom: 12,
    alignItems: "center",
    borderWidth: 1,
    borderColor: Colors.accent,
  },
  reviewIcon: { fontSize: 20, marginRight: 8 },
  reviewText: { color: Colors.accent, fontSize: 13, flex: 1 },

  // Section header
  sectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 12,
    marginTop: 4,
  },
  sectionTitle: { fontSize: 20, fontWeight: "700", color: Colors.text },
  addButton: {
    backgroundColor: Colors.primaryDark,
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 12,
  },
  addButtonText: { color: Colors.text, fontSize: 13, fontWeight: "700" },

  // Course card
  courseCard: {
    backgroundColor: Colors.surface,
    borderRadius: 16,
    padding: 18,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  courseHeader: {
    flexDirection: "row",
    alignItems: "flex-start",
    marginBottom: 10,
  },
  courseName: { fontSize: 17, fontWeight: "700", color: Colors.text },
  courseMeta: { fontSize: 12, color: Colors.textMuted, marginTop: 2 },
  courseXP: { alignItems: "center", marginLeft: 12 },
  courseXPValue: { fontSize: 18, fontWeight: "800", color: Colors.xp },
  courseXPLabel: { fontSize: 10, color: Colors.textMuted },
  courseProgress: {
    backgroundColor: Colors.surfaceLight,
    borderRadius: 10,
    padding: 10,
    marginBottom: 10,
  },
  courseCurrentLabel: { fontSize: 11, color: Colors.textMuted },
  courseCurrentValue: {
    fontSize: 14,
    color: Colors.text,
    fontWeight: "600",
    marginTop: 2,
  },
  courseFooter: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  courseStats: { fontSize: 12, color: Colors.textMuted },
  startButton: {
    backgroundColor: Colors.primary,
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 12,
  },
  startButtonText: { color: "#fff", fontSize: 13, fontWeight: "700" },

  // Empty
  emptyContainer: { alignItems: "center", paddingTop: 40 },
  emptyIcon: { fontSize: 48 },
  emptyTitle: {
    fontSize: 20,
    fontWeight: "700",
    color: Colors.text,
    marginTop: 16,
  },
  emptySubtitle: {
    fontSize: 15,
    color: Colors.textSecondary,
    marginTop: 8,
    textAlign: "center",
    paddingHorizontal: 40,
  },
  enrollButton: {
    backgroundColor: Colors.primary,
    paddingHorizontal: 28,
    paddingVertical: 12,
    borderRadius: 14,
    marginTop: 20,
  },
  enrollButtonText: { color: "#fff", fontSize: 15, fontWeight: "700" },

  // Modal
  modalOverlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.7)",
    justifyContent: "flex-end",
  },
  modalContent: {
    backgroundColor: Colors.background,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    maxHeight: "80%",
    padding: 20,
  },
  modalHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
  },
  modalTitle: { fontSize: 20, fontWeight: "700", color: Colors.text },
  modalClose: { fontSize: 22, color: Colors.textMuted, padding: 4 },
  modalEmpty: {
    color: Colors.textSecondary,
    fontSize: 15,
    textAlign: "center",
    paddingVertical: 20,
  },
  topicOption: {
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  topicOptionName: { fontSize: 16, fontWeight: "700", color: Colors.text },
  topicOptionDesc: {
    fontSize: 13,
    color: Colors.textSecondary,
    marginTop: 4,
  },
  topicOptionMeta: { flexDirection: "row", marginTop: 8, gap: 8 },
  topicOptionBadge: {
    backgroundColor: Colors.primaryDark,
    paddingHorizontal: 10,
    paddingVertical: 3,
    borderRadius: 10,
    fontSize: 11,
    color: Colors.text,
    fontWeight: "600",
    overflow: "hidden",
  },
});
