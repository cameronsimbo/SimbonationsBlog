import axios from "axios";

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
  answer: string;
  timeTakenSeconds: number;
  isAudioSubmission?: boolean;
}) => api.post("/Exercises/submit", data);

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
