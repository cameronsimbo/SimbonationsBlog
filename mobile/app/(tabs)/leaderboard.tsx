import { useEffect, useState } from "react";
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  RefreshControl,
} from "react-native";
import { getWeeklyLeaderboard } from "../../lib/api";
import { Colors } from "../../lib/constants";

interface LeaderboardEntry {
  userId: string;
  displayName: string;
  weeklyXP: number;
  rank: number;
}

interface LeaderboardResult {
  weekStartDate: string;
  entries: LeaderboardEntry[];
}

export default function LeaderboardScreen() {
  const [data, setData] = useState<LeaderboardResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchLeaderboard = async () => {
    try {
      const response = await getWeeklyLeaderboard(20);
      setData(response.data);
    } catch {
      // silently fail — will show empty state
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchLeaderboard();
  }, []);

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.secondary} />
      </View>
    );
  }

  const entries = data?.entries ?? [];

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>🏆 Weekly Leaderboard</Text>
        <Text style={styles.subtitle}>Top learners this week</Text>
      </View>

      {entries.length === 0 ? (
        <View style={styles.emptyContainer}>
          <Text style={styles.emptyIcon}>🏅</Text>
          <Text style={styles.emptyTitle}>No entries yet</Text>
          <Text style={styles.emptySubtitle}>
            Complete exercises to appear on the leaderboard!
          </Text>
        </View>
      ) : (
        <FlatList
          data={entries}
          keyExtractor={(item) => `${item.userId}-${item.rank}`}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => {
                setRefreshing(true);
                fetchLeaderboard();
              }}
              tintColor={Colors.secondary}
            />
          }
          contentContainerStyle={styles.list}
          renderItem={({ item, index }) => (
            <View
              style={[
                styles.row,
                index === 0 && styles.rowFirst,
                index === 1 && styles.rowSecond,
                index === 2 && styles.rowThird,
              ]}
            >
              <View style={styles.rankContainer}>
                <Text style={styles.rank}>
                  {index === 0
                    ? "🥇"
                    : index === 1
                    ? "🥈"
                    : index === 2
                    ? "🥉"
                    : `${item.rank}`}
                </Text>
              </View>
              <Text style={styles.name} numberOfLines={1}>
                {item.displayName || `User ${item.userId.slice(0, 6)}`}
              </Text>
              <Text style={styles.xp}>{item.weeklyXP} XP</Text>
            </View>
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
  header: { paddingHorizontal: 20, paddingTop: 20, paddingBottom: 12 },
  title: { fontSize: 28, fontWeight: "bold", color: Colors.text },
  subtitle: { fontSize: 15, color: Colors.textSecondary, marginTop: 4 },
  list: { paddingHorizontal: 16, paddingBottom: 20 },
  row: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: Colors.surface,
    borderRadius: 12,
    padding: 16,
    marginBottom: 8,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  rowFirst: { borderColor: Colors.gold, borderWidth: 2 },
  rowSecond: { borderColor: "#C0C0C0", borderWidth: 2 },
  rowThird: { borderColor: "#CD7F32", borderWidth: 2 },
  rankContainer: { width: 40, alignItems: "center" },
  rank: { fontSize: 20, color: Colors.textMuted, fontWeight: "bold" },
  name: {
    flex: 1,
    fontSize: 16,
    fontWeight: "600",
    color: Colors.text,
    marginLeft: 12,
  },
  xp: { fontSize: 16, fontWeight: "bold", color: Colors.xp },
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
