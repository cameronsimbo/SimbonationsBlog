import { useEffect, useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TextInput,
  TouchableOpacity,
  ScrollView,
} from "react-native";
import {
  getMyStreak,
  loadStoredAIPreferences,
  saveAIPreferences,
  testAIConnection,
  AIModel,
} from "../../lib/api";
import { Colors } from "../../lib/constants";

interface Streak {
  currentStreak: number;
  longestStreak: number;
  lastActivityDate: string | null;
  streakFreezeCount: number;
}

interface TestResult {
  success: boolean;
  responseTimeMs: number;
  message: string;
}

export default function ProfileScreen() {
  const [streak, setStreak] = useState<Streak | null>(null);
  const [loading, setLoading] = useState(true);

  // AI settings state
  const [aiModel, setAiModel] = useState<AIModel>("claude");
  const [apiKey, setApiKey] = useState("");
  const [showKey, setShowKey] = useState(false);
  const [testResult, setTestResult] = useState<TestResult | null>(null);
  const [testing, setTesting] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    getMyStreak()
      .then((res) => setStreak(res.data))
      .catch(() => {})
      .finally(() => setLoading(false));

    loadStoredAIPreferences().then((prefs) => {
      if (prefs) {
        setAiModel(prefs.model);
        setApiKey(prefs.apiKey ?? "");
      }
    });
  }, []);

  async function handleTest() {
    setTesting(true);
    setTestResult(null);
    try {
      const res = await testAIConnection(aiModel, apiKey || undefined);
      setTestResult(res.data);
    } catch {
      setTestResult({ success: false, responseTimeMs: 0, message: "Request failed" });
    } finally {
      setTesting(false);
    }
  }

  async function handleSave() {
    setSaving(true);
    try {
      await saveAIPreferences(aiModel, apiKey || undefined);
    } finally {
      setSaving(false);
    }
  }

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <View style={styles.header}>
        <Text style={styles.title}>Profile</Text>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Streak</Text>
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
          <Text style={styles.noData}>Sign in to track your streak</Text>
        )}
      </View>

      {/* AI Settings */}
      <View style={styles.card}>
        <Text style={styles.cardTitle}>AI Grading</Text>

        {/* Model picker */}
        <Text style={styles.label}>Model</Text>
        <View style={styles.pickerRow}>
          <TouchableOpacity
            style={[styles.pickerBtn, aiModel === "claude" && styles.pickerBtnActive]}
            onPress={() => { setAiModel("claude"); setTestResult(null); }}
          >
            <Text style={[styles.pickerBtnText, aiModel === "claude" && styles.pickerBtnTextActive]}>
              Claude Haiku
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.pickerBtn, aiModel === "ollama" && styles.pickerBtnActive]}
            onPress={() => { setAiModel("ollama"); setTestResult(null); }}
          >
            <Text style={[styles.pickerBtnText, aiModel === "ollama" && styles.pickerBtnTextActive]}>
              Ollama
            </Text>
          </TouchableOpacity>
        </View>

        {/* API Key input — only for Claude */}
        {aiModel === "claude" && (
          <>
            <Text style={styles.label}>Anthropic API Key</Text>
            <View style={styles.keyRow}>
              <TextInput
                style={styles.keyInput}
                value={apiKey}
                onChangeText={setApiKey}
                placeholder="sk-ant-..."
                placeholderTextColor={Colors.textMuted}
                secureTextEntry={!showKey}
                autoCapitalize="none"
                autoCorrect={false}
              />
              <TouchableOpacity
                style={styles.eyeBtn}
                onPress={() => setShowKey((v) => !v)}
              >
                <Text style={styles.eyeIcon}>{showKey ? "Hide" : "Show"}</Text>
              </TouchableOpacity>
            </View>
          </>
        )}

        {/* Test Connection */}
        <TouchableOpacity
          style={[styles.actionBtn, styles.testBtn]}
          onPress={handleTest}
          disabled={testing}
        >
          {testing ? (
            <ActivityIndicator color={Colors.text} size="small" />
          ) : (
            <Text style={styles.actionBtnText}>Test Connection</Text>
          )}
        </TouchableOpacity>

        {/* Test result */}
        {testResult && (
          <View style={[styles.resultRow, testResult.success ? styles.resultSuccess : styles.resultError]}>
            <Text style={styles.resultText}>
              {testResult.success
                ? `Connected · ${testResult.responseTimeMs} ms`
                : `${testResult.message}`}
            </Text>
          </View>
        )}

        {/* Save */}
        <TouchableOpacity
          style={[styles.actionBtn, styles.saveBtn]}
          onPress={handleSave}
          disabled={saving}
        >
          {saving ? (
            <ActivityIndicator color={Colors.text} size="small" />
          ) : (
            <Text style={styles.actionBtnText}>Save Settings</Text>
          )}
        </TouchableOpacity>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Question Bank</Text>
        <Text style={styles.noData}>Your personal questions will appear here</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  content: { paddingBottom: 32 },
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
  cardTitle: { fontSize: 18, fontWeight: "700", color: Colors.text, marginBottom: 4 },
  streakRow: {
    flexDirection: "row",
    justifyContent: "space-around",
    marginTop: 16,
  },
  streakItem: { alignItems: "center" },
  streakNumber: { fontSize: 32, fontWeight: "bold", color: Colors.streak },
  streakLabel: { fontSize: 13, color: Colors.textSecondary, marginTop: 4 },
  divider: { width: 1, backgroundColor: Colors.border, marginVertical: 4 },
  noData: { color: Colors.textMuted, fontSize: 14, marginTop: 12 },
  label: { color: Colors.textSecondary, fontSize: 13, marginTop: 16, marginBottom: 8 },
  pickerRow: { flexDirection: "row", gap: 8 },
  pickerBtn: {
    flex: 1,
    paddingVertical: 10,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: Colors.border,
    alignItems: "center",
  },
  pickerBtnActive: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  pickerBtnText: { color: Colors.textSecondary, fontWeight: "600", fontSize: 14 },
  pickerBtnTextActive: { color: Colors.text },
  keyRow: { flexDirection: "row", alignItems: "center", gap: 8 },
  keyInput: {
    flex: 1,
    backgroundColor: Colors.surfaceLight,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: Colors.border,
    color: Colors.text,
    paddingHorizontal: 14,
    paddingVertical: 10,
    fontSize: 14,
  },
  eyeBtn: {
    paddingHorizontal: 10,
    paddingVertical: 10,
  },
  eyeIcon: { color: Colors.textSecondary, fontSize: 13 },
  actionBtn: {
    marginTop: 16,
    borderRadius: 10,
    paddingVertical: 12,
    alignItems: "center",
    justifyContent: "center",
    minHeight: 44,
  },
  testBtn: { backgroundColor: Colors.surfaceLight, borderWidth: 1, borderColor: Colors.border },
  saveBtn: { backgroundColor: Colors.primary },
  actionBtnText: { color: Colors.text, fontWeight: "700", fontSize: 15 },
  resultRow: {
    marginTop: 10,
    borderRadius: 8,
    paddingHorizontal: 14,
    paddingVertical: 8,
  },
  resultSuccess: { backgroundColor: "#1a3d1a" },
  resultError: { backgroundColor: "#3d1a1a" },
  resultText: { color: Colors.text, fontSize: 13 },
});
