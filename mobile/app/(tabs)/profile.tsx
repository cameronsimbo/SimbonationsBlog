import { useEffect, useState } from "react";
import { View, Text, StyleSheet, ActivityIndicator } from "react-native";
import { getMyStreak } from "../../lib/api";
import { Colors } from "../../lib/constants";

interface Streak {
  currentStreak: number;
  longestStreak: number;
  lastActivityDate: string | null;
  streakFreezeCount: number;
}

export default function ProfileScreen() {
  const [streak, setStreak] = useState<Streak | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getMyStreak()
      .then((res) => setStreak(res.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>👤 Profile</Text>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>🔥 Streak</Text>
        {loading ? (
          <ActivityIndicator color={Colors.streak} style={{ marginTop: 16 }} />
        ) : streak ? (
          <View style={styles.streakRow}>
            <View style={styles.streakItem}>
              <Text style={styles.streakNumber}>{streak.currentStreak}</Text>
              <Text style={styles.streakLabel}>Current</Text>
            </View>
            <View style={styles.divider} />
            <View style={styles.streakItem}>
              <Text style={styles.streakNumber}>{streak.longestStreak}</Text>
              <Text style={styles.streakLabel}>Longest</Text>
            </View>
            <View style={styles.divider} />
            <View style={styles.streakItem}>
              <Text style={styles.streakNumber}>
                {streak.streakFreezeCount}
              </Text>
              <Text style={styles.streakLabel}>Freezes</Text>
            </View>
          </View>
        ) : (
          <Text style={styles.noData}>
            Sign in to track your streak
          </Text>
        )}
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>📊 Statistics</Text>
        <Text style={styles.noData}>Coming soon...</Text>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>📝 Question Bank</Text>
        <Text style={styles.noData}>
          Your personal questions will appear here
        </Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  header: { paddingHorizontal: 20, paddingTop: 20, paddingBottom: 12 },
  title: { fontSize: 28, fontWeight: "bold", color: Colors.text },
  card: {
    backgroundColor: Colors.surface,
    borderRadius: 16,
    padding: 20,
    marginHorizontal: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  cardTitle: { fontSize: 18, fontWeight: "700", color: Colors.text },
  streakRow: {
    flexDirection: "row",
    justifyContent: "space-around",
    marginTop: 16,
  },
  streakItem: { alignItems: "center" },
  streakNumber: {
    fontSize: 32,
    fontWeight: "bold",
    color: Colors.streak,
  },
  streakLabel: { fontSize: 13, color: Colors.textSecondary, marginTop: 4 },
  divider: {
    width: 1,
    backgroundColor: Colors.border,
    marginVertical: 4,
  },
  noData: {
    color: Colors.textMuted,
    fontSize: 14,
    marginTop: 12,
  },
});
