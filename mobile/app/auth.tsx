import { useState, useEffect } from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { router } from "expo-router";
import {
  GoogleSignin,
  statusCodes,
} from "@react-native-google-signin/google-signin";
import { register, login, googleLogin } from "../lib/api";
import { useAuth } from "../lib/auth";
import { Colors } from "../lib/constants";

const GOOGLE_WEB_CLIENT_ID =
  "YOUR_GOOGLE_WEB_CLIENT_ID.apps.googleusercontent.com";

export default function AuthScreen() {
  const { signIn } = useAuth();
  const [isRegister, setIsRegister] = useState(true);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [googleLoading, setGoogleLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    GoogleSignin.configure({
      webClientId: GOOGLE_WEB_CLIENT_ID,
      offlineAccess: false,
    });
  }, []);

  const handleSubmit = async () => {
    if (!email.trim() || !password.trim()) {
      setError("Email and password are required.");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = isRegister
        ? await register(email.trim(), password)
        : await login(email.trim(), password);

      await signIn(response.data.token);
      router.replace("/(tabs)");
    } catch (err: any) {
      const msg =
        err.response?.data?.Errors?.[0] ||
        err.response?.data?.Error ||
        err.response?.data?.error ||
        "Something went wrong. Please try again.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleSignIn = async () => {
    setGoogleLoading(true);
    setError(null);

    try {
      await GoogleSignin.hasPlayServices({ showPlayServicesUpdateDialog: true });
      const userInfo = await GoogleSignin.signIn();
      const idToken = userInfo.data?.idToken;

      if (!idToken) {
        setError("Failed to get Google ID token.");
        return;
      }

      const response = await googleLogin(idToken);
      await signIn(response.data.token);
      router.replace("/(tabs)");
    } catch (err: any) {
      if (err.code === statusCodes.SIGN_IN_CANCELLED) {
        // User cancelled — do nothing
        return;
      }
      if (err.code === statusCodes.IN_PROGRESS) {
        return;
      }
      if (err.code === statusCodes.PLAY_SERVICES_NOT_AVAILABLE) {
        setError("Google Play Services not available.");
        return;
      }

      const msg =
        err.response?.data?.Errors?.[0] ||
        err.response?.data?.Error ||
        err.response?.data?.error ||
        err.message ||
        "Google sign-in failed. Please try again.";
      setError(msg);
    } finally {
      setGoogleLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === "ios" ? "padding" : "height"}
    >
      <View style={styles.inner}>
        <Text style={styles.logo}>🎯</Text>
        <Text style={styles.title}>LearnFlow</Text>
        <Text style={styles.subtitle}>
          {isRegister ? "Create your account" : "Welcome back"}
        </Text>

        {error ? (
          <View style={styles.errorBox}>
            <Text style={styles.errorText}>{error}</Text>
          </View>
        ) : null}

        <TextInput
          style={styles.input}
          placeholder="Email"
          placeholderTextColor={Colors.textMuted}
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
          autoComplete="email"
        />

        <TextInput
          style={styles.input}
          placeholder="Password"
          placeholderTextColor={Colors.textMuted}
          value={password}
          onChangeText={setPassword}
          secureTextEntry
          autoComplete={isRegister ? "new-password" : "current-password"}
        />

        <TouchableOpacity
          style={[styles.button, loading && styles.buttonDisabled]}
          onPress={handleSubmit}
          disabled={loading}
          activeOpacity={0.8}
        >
          {loading ? (
            <ActivityIndicator color={Colors.text} />
          ) : (
            <Text style={styles.buttonText}>
              {isRegister ? "Create Account" : "Sign In"}
            </Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity
          onPress={() => {
            setIsRegister(!isRegister);
            setError(null);
          }}
          style={styles.toggle}
        >
          <Text style={styles.toggleText}>
            {isRegister
              ? "Already have an account? Sign in"
              : "Don't have an account? Register"}
          </Text>
        </TouchableOpacity>

        <View style={styles.divider}>
          <View style={styles.dividerLine} />
          <Text style={styles.dividerText}>OR</Text>
          <View style={styles.dividerLine} />
        </View>

        <TouchableOpacity
          style={[styles.googleButton, googleLoading && styles.buttonDisabled]}
          onPress={handleGoogleSignIn}
          disabled={googleLoading}
          activeOpacity={0.8}
        >
          {googleLoading ? (
            <ActivityIndicator color="#1F1F1F" />
          ) : (
            <View style={styles.googleButtonContent}>
              <Text style={styles.googleIcon}>G</Text>
              <Text style={styles.googleButtonText}>Sign in with Google</Text>
            </View>
          )}
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  inner: {
    flex: 1,
    justifyContent: "center",
    paddingHorizontal: 32,
  },
  logo: { fontSize: 64, textAlign: "center", marginBottom: 8 },
  title: {
    fontSize: 32,
    fontWeight: "bold",
    color: Colors.text,
    textAlign: "center",
  },
  subtitle: {
    fontSize: 16,
    color: Colors.textSecondary,
    textAlign: "center",
    marginTop: 6,
    marginBottom: 32,
  },
  errorBox: {
    backgroundColor: "rgba(255,75,75,0.15)",
    borderRadius: 10,
    padding: 12,
    marginBottom: 16,
  },
  errorText: { color: Colors.wrong, fontSize: 14, textAlign: "center" },
  input: {
    backgroundColor: Colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: Colors.border,
    color: Colors.text,
    fontSize: 16,
    paddingHorizontal: 16,
    paddingVertical: 14,
    marginBottom: 14,
  },
  button: {
    backgroundColor: Colors.primary,
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: "center",
    marginTop: 4,
  },
  buttonDisabled: { opacity: 0.6 },
  buttonText: { color: Colors.text, fontSize: 17, fontWeight: "700" },
  toggle: { marginTop: 20, alignItems: "center" },
  toggleText: { color: Colors.secondary, fontSize: 14 },
  divider: {
    flexDirection: "row",
    alignItems: "center",
    marginVertical: 24,
  },
  dividerLine: {
    flex: 1,
    height: 1,
    backgroundColor: Colors.border,
  },
  dividerText: {
    color: Colors.textMuted,
    fontSize: 13,
    fontWeight: "600",
    marginHorizontal: 16,
  },
  googleButton: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    paddingVertical: 14,
    alignItems: "center",
    borderWidth: 1,
    borderColor: "#DADCE0",
  },
  googleButtonContent: {
    flexDirection: "row",
    alignItems: "center",
  },
  googleIcon: {
    fontSize: 20,
    fontWeight: "700",
    color: "#4285F4",
    marginRight: 10,
  },
  googleButtonText: {
    color: "#1F1F1F",
    fontSize: 16,
    fontWeight: "600",
  },
});
