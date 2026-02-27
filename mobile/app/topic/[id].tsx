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
import { getTopic } from "../../lib/api";
import { Colors, SubjectDomains, DifficultyLevels } from "../../lib/constants";

interface UnitSummary {
  id: string;
  name: string;
  description: string;
  orderIndex: number;
  totalLessons: number;
}

interface TopicDetail {
  id: string;
  name: string;
  description: string;
  subjectDomain: number;
  difficultyLevel: number;
  totalUnits: number;
  units: UnitSummary[];
}

export default function TopicDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const [topic, setTopic] = useState<TopicDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getTopic(id)
      .then((res) => setTopic(res.data))
      .catch((err) => setError(err.message || "Failed to load topic"))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
      </View>
    );
  }

  if (error || !topic) {
    return (
      <View style={styles.center}>
        <Text style={styles.errorText}>⚠️ {error || "Topic not found"}</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>{topic.name}</Text>
        <Text style={styles.subtitle}>{topic.description}</Text>
        <View style={styles.metaRow}>
          <View style={styles.metaBadge}>
            <Text style={styles.metaText}>
              {SubjectDomains[topic.subjectDomain]}
            </Text>
          </View>
          <View style={[styles.metaBadge, { backgroundColor: Colors.accent }]}>
            <Text style={styles.metaText}>
              {DifficultyLevels[topic.difficultyLevel]}
            </Text>
          </View>
        </View>
      </View>

      <Text style={styles.sectionTitle}>
        Units ({topic.units?.length ?? 0})
      </Text>

      {topic.units?.length === 0 ? (
        <View style={styles.emptyContainer}>
          <Text style={styles.emptyText}>No units available yet</Text>
        </View>
      ) : (
        <FlatList
          data={topic.units}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          renderItem={({ item, index }) => (
            <TouchableOpacity
              style={styles.unitCard}
              activeOpacity={0.7}
              onPress={() => router.push(`/unit/${item.id}`)}
            >
              <View style={styles.unitNumber}>
                <Text style={styles.unitNumberText}>{index + 1}</Text>
              </View>
              <View style={styles.unitInfo}>
                <Text style={styles.unitName}>{item.name}</Text>
                <Text style={styles.unitDescription} numberOfLines={1}>
                  {item.description}
                </Text>
                <Text style={styles.unitLessons}>
                  {item.totalLessons}{" "}
                  {item.totalLessons === 1 ? "lesson" : "lessons"}
                </Text>
              </View>
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
  header: { paddingHorizontal: 20, paddingTop: 20, paddingBottom: 8 },
  title: { fontSize: 26, fontWeight: "bold", color: Colors.text },
  subtitle: {
    fontSize: 15,
    color: Colors.textSecondary,
    marginTop: 6,
    lineHeight: 22,
  },
  metaRow: { flexDirection: "row", gap: 8, marginTop: 12 },
  metaBadge: {
    backgroundColor: Colors.secondary,
    paddingHorizontal: 12,
    paddingVertical: 5,
    borderRadius: 12,
  },
  metaText: { color: Colors.text, fontSize: 12, fontWeight: "700" },
  sectionTitle: {
    fontSize: 18,
    fontWeight: "700",
    color: Colors.text,
    paddingHorizontal: 20,
    paddingTop: 20,
    paddingBottom: 8,
  },
  list: { paddingHorizontal: 16, paddingBottom: 20 },
  unitCard: {
    flexDirection: "row",
    backgroundColor: Colors.surface,
    borderRadius: 14,
    padding: 16,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: Colors.border,
    alignItems: "center",
  },
  unitNumber: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: Colors.primary,
    justifyContent: "center",
    alignItems: "center",
  },
  unitNumberText: { color: Colors.text, fontSize: 16, fontWeight: "bold" },
  unitInfo: { flex: 1, marginLeft: 14 },
  unitName: { fontSize: 16, fontWeight: "700", color: Colors.text },
  unitDescription: {
    fontSize: 13,
    color: Colors.textSecondary,
    marginTop: 2,
  },
  unitLessons: {
    fontSize: 12,
    color: Colors.textMuted,
    marginTop: 4,
  },
  emptyContainer: { alignItems: "center", paddingTop: 40 },
  emptyText: { color: Colors.textMuted, fontSize: 15 },
});
