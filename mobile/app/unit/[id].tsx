import { useEffect, useState } from "react";
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
} from "react-native";
import { useLocalSearchParams, router } from "expo-router";
import { getLessons } from "../../lib/api";
import { Colors } from "../../lib/constants";

interface LessonItem {
  id: string;
  name: string;
  description: string;
  orderIndex: number;
  exerciseCount: number;
  estimatedMinutes: number;
}

export default function UnitDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const [lessons, setLessons] = useState<LessonItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getLessons(id)
      .then((res) => setLessons(res.data))
      .catch((err) => setError(err.message || "Failed to load lessons"))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
      </View>
    );
  }

  if (error) {
    return (
      <View style={styles.center}>
        <Text style={styles.errorText}>{error}</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.sectionTitle}>
        Lessons ({lessons.length})
      </Text>

      {lessons.length === 0 ? (
        <View style={styles.emptyContainer}>
          <Text style={styles.emptyText}>No lessons available yet</Text>
        </View>
      ) : (
        <FlatList
          data={lessons}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          renderItem={({ item, index }) => (
            <TouchableOpacity
              style={styles.lessonCard}
              activeOpacity={0.7}
              onPress={() => router.push(`/lesson/${item.id}`)}
            >
              <View style={styles.lessonIndex}>
                <Text style={styles.lessonIndexText}>{index + 1}</Text>
              </View>
              <View style={styles.lessonInfo}>
                <Text style={styles.lessonName}>{item.name}</Text>
                <Text style={styles.lessonDescription} numberOfLines={2}>
                  {item.description}
                </Text>
                <View style={styles.lessonMeta}>
                  <Text style={styles.metaText}>
                    ~{item.estimatedMinutes} min
                  </Text>
                  {item.exerciseCount > 0 ? (
                    <Text style={styles.exerciseBadge}>
                      {item.exerciseCount} exercises
                    </Text>
                  ) : (
                    <Text style={styles.generateBadge}>Ready to generate</Text>
                  )}
                </View>
              </View>
              <Text style={styles.chevron}>›</Text>
            </TouchableOpacity>
          )}
        />
      )}
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
  errorText: { color: Colors.wrong, fontSize: 16 },
  sectionTitle: {
    fontSize: 20,
    fontWeight: "700",
    color: Colors.text,
    paddingHorizontal: 20,
    paddingTop: 20,
    paddingBottom: 12,
  },
  list: { paddingHorizontal: 16, paddingBottom: 20 },
  lessonCard: {
    flexDirection: "row",
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: Colors.border,
    alignItems: "center",
  },
  lessonIndex: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: Colors.secondary,
    justifyContent: "center",
    alignItems: "center",
  },
  lessonIndexText: { color: Colors.text, fontSize: 14, fontWeight: "bold" },
  lessonInfo: { flex: 1, marginLeft: 14 },
  lessonName: { fontSize: 16, fontWeight: "700", color: Colors.text },
  lessonDescription: {
    fontSize: 13,
    color: Colors.textSecondary,
    marginTop: 3,
    lineHeight: 18,
  },
  lessonMeta: {
    flexDirection: "row",
    gap: 10,
    marginTop: 6,
    alignItems: "center",
  },
  metaText: { fontSize: 12, color: Colors.textMuted },
  exerciseBadge: {
    fontSize: 11,
    color: Colors.primary,
    fontWeight: "600",
  },
  generateBadge: {
    fontSize: 11,
    color: Colors.accent,
    fontWeight: "600",
  },
  chevron: {
    fontSize: 24,
    color: Colors.textMuted,
    marginLeft: 8,
  },
  emptyContainer: { alignItems: "center", paddingTop: 40 },
  emptyText: { color: Colors.textMuted, fontSize: 15 },
});
