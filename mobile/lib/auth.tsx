import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { loadStoredToken, storeToken, clearToken, setOnUnauthorized, loadStoredAIPreferences } from "./api";

interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  token: string | null;
  signIn: (token: string) => Promise<void>;
  signOut: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType>({
  isAuthenticated: false,
  isLoading: true,
  token: null,
  signIn: async () => {},
  signOut: async () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Register 401 interceptor callback to auto sign-out
    setOnUnauthorized(() => setToken(null));

    loadStoredToken()
      .then((t) => {
        if (t && isTokenExpired(t)) {
          clearToken();
          return null;
        }
        return t;
      })
      .then((t) => setToken(t))
      .finally(() => setIsLoading(false));

    loadStoredAIPreferences().catch(() => {});
  }, []);

  const signIn = async (newToken: string) => {
    await storeToken(newToken);
    setToken(newToken);
  };

  const signOut = async () => {
    await clearToken();
    setToken(null);
  };

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated: token !== null,
        isLoading,
        token,
        signIn,
        signOut,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}

// Decode JWT and check if expired (no library needed)
function isTokenExpired(token: string): boolean {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return true;
    const payload = JSON.parse(atob(parts[1]));
    if (!payload.exp) return false;
    // exp is in seconds, Date.now() in ms — add 30s buffer
    return payload.exp * 1000 < Date.now() + 30000;
  } catch {
    return true; // malformed token → treat as expired
  }
}
