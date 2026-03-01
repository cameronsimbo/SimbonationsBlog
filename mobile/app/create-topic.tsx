import { useState } from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  ScrollView,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { router } from "expo-router";
import { createTopic } from "../lib/api";
import { Colors, SubjectDomains, DifficultyLevels } from "../lib/constants";

type Phase = "form" | "validating" | "success";

export default function CreateTopicScreen() {
  const [phase, setPhase] = useState<Phase>("form");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [subjectDomain, setSubjectDomain] = useState<number>(0);
  const [difficultyLevel, setDifficultyLevel] = useState<number>(0);
  const [rejectionReason, setRejectionReason] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (!name.trim()) return;

    setRejectionReason(null);
    setPhase("validating");

    try {
      await createTopic({
        name: name.trim(),
        description: description.trim(),
        subjectDomain,
        difficultyLevel,
      });
      setPhase("success");
      setTimeout(() => router.back(), 1200);
    } catch (err: any) {
      setPhase("form");
      const errors = err.response?.data?.errors;
      const reason = errors?.Name?.[0] ?? "Failed to create topic. Please try again.";
      setRejectionReason(reason);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === "ios" ? "padding" : undefined}
    >
      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
        <Text style={styles.title}>Create Your Own Topic</Text>
        <Text style={styles.subtitle}>
          Our AI will check that your topic is educationally suitable before saving it.
        </Text>

        {phase === "validating" && (
          <View style={styles.validatingCard}>
            <ActivityIndicator size="small" color={Colors.primary} />
            <Text style={styles.validatingText}>Checking topic suitability…</Text>
          </View>
        )}

        {phase === "success" && (
          <View style={styles.successCard}>
            <Text style={styles.successText}>Topic created! Redirecting…</Text>
          </View>
        )}

        {rejectionReason && (
          <View style={styles.rejectionCard}>
            <Text style={styles.rejectionTitle}>Topic not accepted</Text>
            <Text style={styles.rejectionText}>{rejectionReason}</Text>
          </View>
        )}

        <Text style={styles.label}>Topic Name *</Text>
        <TextInput
          style={styles.input}
          value={name}
          onChangeText={setName}
          placeholder="e.g. Ancient Roman History"
          placeholderTextColor={Colors.textMuted}
          editable={phase === "form"}
        />

        <Text style={styles.label}>Description</Text>
        <TextInput
          style={[styles.input, styles.multiline]}
          value={description}
          onChangeText={setDescription}
          placeholder="What should learners gain from this topic?"
          placeholderTextColor={Colors.textMuted}
          multiline
          numberOfLines={3}
          editable={phase === "form"}
        />

        <Text style={styles.label}>Subject Domain</Text>
        <View style={styles.toggleRow}>
          {Object.entries(SubjectDomains).map(([key, label]) => {
            const val = Number(key);
            return (
              <TouchableOpacity
                key={key}
                style={[styles.toggleButton, subjectDomain === val && styles.toggleButtonActive]}
                onPress={() => setSubjectDomain(val)}
                disabled={phase !== "form"}
              >
                <Text
                  style={[
                    styles.toggleButtonText,
                    subjectDomain === val && styles.toggleButtonTextActive,
                  ]}
                >
                  {label}
                </Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <Text style={styles.label}>Difficulty</Text>
        <View style={styles.toggleRow}>
          {Object.entries(DifficultyLevels).map(([key, label]) => {
            const val = Number(key);
            return (
              <TouchableOpacity
                key={key}
                style={[styles.toggleButton, difficultyLevel === val && styles.toggleButtonActive]}
                onPress={() => setDifficultyLevel(val)}
                disabled={phase !== "form"}
              >
                <Text
                  style={[
                    styles.toggleButtonText,
                    difficultyLevel === val && styles.toggleButtonTextActive,
                  ]}
                >
                  {label}
                </Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <TouchableOpacity
          style={[styles.submitButton, phase !== "form" && styles.submitButtonDisabled]}
          onPress={handleSubmit}
          disabled={phase !== "form" || !name.trim()}
        >
          <Text style={styles.submitButtonText}>Submit Topic</Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.cancelButton} onPress={() => router.back()}>
          <Text style={styles.cancelButtonText}>Cancel</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  scroll: { padding: 20, paddingBottom: 40 },
  title: { fontSize: 24, fontWeight: "800", color: Colors.text, marginBottom: 6 },
  subtitle: { fontSize: 14, color: Colors.textSecondary, marginBottom: 24 },

  validatingCard: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: Colors.surfaceLight,
    borderRadius: 12,
    padding: 14,
    marginBottom: 16,
    gap: 10,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  validatingText: { color: Colors.textSecondary, fontSize: 14 },

  successCard: {
    backgroundColor: Colors.primaryDark,
    borderRadius: 12,
    padding: 14,
    marginBottom: 16,
    alignItems: "center",
  },
  successText: { color: Colors.text, fontWeight: "700", fontSize: 15 },

  rejectionCard: {
    backgroundColor: "#2D1010",
    borderRadius: 12,
    padding: 14,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: Colors.wrong,
  },
  rejectionTitle: { color: Colors.wrong, fontWeight: "700", fontSize: 14, marginBottom: 4 },
  rejectionText: { color: "#FFA0A0", fontSize: 13, lineHeight: 18 },

  label: { fontSize: 13, color: Colors.textSecondary, marginBottom: 6, marginTop: 16 },
  input: {
    backgroundColor: Colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: Colors.border,
    color: Colors.text,
    fontSize: 15,
    paddingHorizontal: 14,
    paddingVertical: 12,
  },
  multiline: { minHeight: 80, textAlignVertical: "top" },

  toggleRow: { flexDirection: "row", flexWrap: "wrap", gap: 8 },
  toggleButton: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: Colors.border,
    backgroundColor: Colors.surface,
  },
  toggleButtonActive: {
    backgroundColor: Colors.primary,
    borderColor: Colors.primary,
  },
  toggleButtonText: { fontSize: 13, color: Colors.textSecondary, fontWeight: "600" },
  toggleButtonTextActive: { color: Colors.text },

  submitButton: {
    backgroundColor: Colors.primary,
    borderRadius: 14,
    paddingVertical: 14,
    alignItems: "center",
    marginTop: 28,
  },
  submitButtonDisabled: { opacity: 0.5 },
  submitButtonText: { color: Colors.text, fontSize: 16, fontWeight: "700" },

  cancelButton: { alignItems: "center", marginTop: 14, paddingVertical: 10 },
  cancelButtonText: { color: Colors.textMuted, fontSize: 14 },
});
