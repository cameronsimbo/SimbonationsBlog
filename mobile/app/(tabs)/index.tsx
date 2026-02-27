import { useEffect, useState } from "react";
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  RefreshControl,
} from "react-native";
import { router } from "expo-router";
import { getTopics } from "../../lib/api";
import { Colors, SubjectDomains, DifficultyLevels } from "../../lib/constants";

interface Topic {
  id: string;
  name: string;
  description: string;
  subjectDomain: number;
  difficultyLevel: number;
  totalUnits: number;
  isPublished: boolean;
}

export default function LearnScreen() {
  const [topics, setTopics] = useState<Topic[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchTopics = async () => {
    try {
      setError(null);
      const response = await getTopics();
      setTopics(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load topics");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchTopics();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    fetchTopics();
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={styles.loadingText}>Loading topics...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>🎯 Learn</Text>
        <Text style={styles.subtitle}>Pick a topic to start learning</Text>
      </View>

      {error ? (
        <View style={styles.errorContainer}>
          <Text style={styles.errorText}>⚠️ {error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchTopics}>
            <Text style={styles.retryText}>Retry</Text>
          </TouchableOpacity>
        </View>
      ) : null}

      {topics.length === 0 && !error ? (
        <View style={styles.emptyContainer}>
          <Text style={styles.emptyIcon}>📭</Text>
          <Text style={styles.emptyTitle}>No topics yet</Text>
          <Text style={styles.emptySubtitle}>
            Topics will appear here once they've been created.
          </Text>
        </View>
      ) : (
        <FlatList
          data={topics}
          keyExtractor={(item) => item.id}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={onRefresh}
              tintColor={Colors.primary}
            />
          }
          contentContainerStyle={styles.list}
          renderItem={({ item }) => (
            <TouchableOpacity
              style={styles.card}
              onPress={() => router.push(`/topic/${item.id}`)}
              activeOpacity={0.7}
            >
              <View style={styles.cardHeader}>
                <Text style={styles.cardTitle}>{item.name}</Text>
                <View style={styles.badge}>
                  <Text style={styles.badgeText}>
                    {DifficultyLevels[item.difficultyLevel] ?? "Unknown"}
                  </Text>
                </View>
              </View>
              <Text style={styles.cardDescription} numberOfLines={2}>
                {item.description}
              </Text>
              <View style={styles.cardFooter}>
                <Text style={styles.cardDomain}>
                  {SubjectDomains[item.subjectDomain] ?? "General"}
                </Text>
                <Text style={styles.cardUnits}>
                  {item.totalUnits} {item.totalUnits === 1 ? "unit" : "units"}
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
  loadingText: { color: Colors.textSecondary, marginTop: 12, fontSize: 16 },
  header: { paddingHorizontal: 20, paddingTop: 20, paddingBottom: 12 },
  title: { fontSize: 28, fontWeight: "bold", color: Colors.text },
  subtitle: { fontSize: 15, color: Colors.textSecondary, marginTop: 4 },
  list: { paddingHorizontal: 16, paddingBottom: 20 },
  card: {
    backgroundColor: Colors.surface,
    borderRadius: 16,
    padding: 20,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  cardHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 8,
  },
  cardTitle: { fontSize: 18, fontWeight: "700", color: Colors.text, flex: 1 },
  badge: {
    backgroundColor: Colors.primaryDark,
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
  },
  badgeText: { color: Colors.text, fontSize: 12, fontWeight: "600" },
  cardDescription: {
    fontSize: 14,
    color: Colors.textSecondary,
    lineHeight: 20,
    marginBottom: 12,
  },
  cardFooter: { flexDirection: "row", justifyContent: "space-between" },
  cardDomain: { color: Colors.secondary, fontSize: 13, fontWeight: "600" },
  cardUnits: { color: Colors.textMuted, fontSize: 13 },
  errorContainer: { alignItems: "center", padding: 20 },
  errorText: { color: Colors.wrong, fontSize: 15, marginBottom: 12 },
  retryButton: {
    backgroundColor: Colors.primary,
    paddingHorizontal: 24,
    paddingVertical: 10,
    borderRadius: 12,
  },
  retryText: { color: Colors.text, fontWeight: "700" },
  emptyContainer: { alignItems: "center", paddingTop: 60 },
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
});
