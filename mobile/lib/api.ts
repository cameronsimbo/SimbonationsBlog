import axios from "axios";
import AsyncStorage from "@react-native-async-storage/async-storage";

const api = axios.create({
  baseURL: "http://localhost:5000/api",
  timeout: 10000,
  headers: { "Content-Type": "application/json" },
});

let authToken: string | null = null;

export function setAuthToken(token: string | null) {
  authToken = token;
  if (token) {
    api.defaults.headers.common["Authorization"] = `Bearer ${token}`;
  } else {
    delete api.defaults.headers.common["Authorization"];
  }
}

export async function loadStoredToken(): Promise<string | null> {
  try {
    const token = await AsyncStorage.getItem("authToken");
    if (token) {
      setAuthToken(token);
    }
    return token;
  } catch {
    return null;
  }
}

export async function storeToken(token: string): Promise<void> {
  await AsyncStorage.setItem("authToken", token);
  setAuthToken(token);
}

export async function clearToken(): Promise<void> {
  await AsyncStorage.removeItem("authToken");
  setAuthToken(null);
}

// Auth
export const register = (email: string, password: string, displayName?: string) =>
  api.post("/Auth/register", { email, password, displayName });

export const login = (email: string, password: string) =>
  api.post("/Auth/login", { email, password });

export const googleLogin = (idToken: string) =>
  api.post("/Auth/google", { idToken });

// Topics
export const getTopics = (publishedOnly = true) =>
  api.get("/Topics", { params: { publishedOnly } });

export const getTopic = (id: string) => api.get(`/Topics/${id}`);

export const createTopic = (data: {
  name: string;
  description: string;
  subjectDomain: number;
  difficultyLevel: number;
}) => api.post("/Topics", data);

// Lessons
export const getLessons = (unitId: string) =>
  api.get("/Lessons", { params: { unitId } });

// Exercises
export const getExercises = (lessonId: string) =>
  api.get("/Exercises", { params: { lessonId } });

export const submitAnswer = (data: {
  exerciseId: string;
  userAnswer: string;
  timeTakenSeconds: number;
}) => api.post("/Exercises/submit", data);

export const generateExercises = (lessonId: string, count: number = 5) =>
  api.post("/Exercises/generate", { lessonId, count });

export const voteOnExercise = (exerciseId: string, isUpvote: boolean) =>
  api.post(`/Exercises/${exerciseId}/vote`, { isUpvote });

// Streaks
export const getMyStreak = () => api.get("/Streaks/me");

// Leaderboards
export const getWeeklyLeaderboard = (top = 50) =>
  api.get("/Leaderboards/weekly", { params: { top } });

// Question Bank
export const getMyQuestionBank = () => api.get("/QuestionBank/mine");

export const createQuestionBankItem = (data: {
  prompt: string;
  referenceAnswer: string;
  subjectDomain: number;
  exerciseType: number;
  difficultyLevel: number;
  context?: string;
  hints?: string;
}) => api.post("/QuestionBank", data);

export default api;
