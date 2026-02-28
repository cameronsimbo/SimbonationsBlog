import { useState, useCallback } from "react";
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Dimensions,
} from "react-native";
import { useLocalSearchParams, router, useFocusEffect } from "expo-router";
import { getLearningPath, startSession } from "../../lib/api";
import { Colors } from "../../lib/constants";
import Svg, { Path as SvgPath } from "react-native-svg";

// ─── Types ───────────────────────────────────────────────
interface PathLesson {
  lessonId: string;
  name: string;
  orderIndex: number;
  isCurrent: boolean;
  isCompleted: boolean;
  isLocked: boolean;
  crowns: number;
  bestScore: number;
}

interface PathUnit {
  unitId: string;
  name: string;
  description: string;
  orderIndex: number;
  isCurrent: boolean;
  isCompleted: boolean;
  isLocked: boolean;
  lessons: PathLesson[];
}

interface LearningPath {
  topicId: string;
  topicName: string;
  currentUnitIndex: number;
  currentLessonIndex: number;
  totalXPEarned: number;
  units: PathUnit[];
}

// ─── Layout constants ────────────────────────────────────
const SCREEN_WIDTH = Dimensions.get("window").width;
const NODE_SIZE = 68;
const NODE_RADIUS = NODE_SIZE / 2;
const VERTICAL_GAP = 110;
const AMPLITUDE = 70;
const CENTER_X = SCREEN_WIDTH / 2;
const UNIT_BANNER_HEIGHT = 48;
const TOP_PADDING = 20;

// ─── Flattened path item ─────────────────────────────────
type PathNodeType = "lesson" | "unit-banner";

interface PathNode {
  type: PathNodeType;
  lesson?: PathLesson;
  unitIndex?: number;
  unitName?: string;
  unitCompleted?: boolean;
  unitLocked?: boolean;
  x: number;
  y: number;
  globalIndex: number;
}

// ─── Build node positions with sinusoidal winding ────────
function buildPathNodes(units: PathUnit[]): PathNode[] {
  const nodes: PathNode[] = [];
  let globalIndex = 0;

  for (let unitIdx = 0; unitIdx < units.length; unitIdx++) {
    const unit = units[unitIdx];

    // Unit banner
    const bannerY = TOP_PADDING + globalIndex * VERTICAL_GAP;
    nodes.push({
      type: "unit-banner",
      unitIndex: unitIdx,
      unitName: unit.name,
      unitCompleted: unit.isCompleted,
      unitLocked: unit.isLocked,
      x: CENTER_X,
      y: bannerY,
      globalIndex,
    });
    globalIndex++;

    // Lesson nodes — sinusoidal x offset for winding path
    for (let lessonIdx = 0; lessonIdx < unit.lessons.length; lessonIdx++) {
      const lesson = unit.lessons[lessonIdx];
      const y = TOP_PADDING + globalIndex * VERTICAL_GAP;
      const x = CENTER_X + AMPLITUDE * Math.sin(globalIndex * 0.85);

      nodes.push({
        type: "lesson",
        lesson,
        x,
        y,
        globalIndex,
      });
      globalIndex++;
    }
  }

  return nodes;
}

// ─── SVG connector curves between consecutive lessons ────
function buildConnectorPaths(
  nodes: PathNode[]
): { d: string; color: string }[] {
  const paths: { d: string; color: string }[] = [];
  const lessonNodes = nodes.filter((n) => n.type === "lesson");

  for (let i = 0; i < lessonNodes.length - 1; i++) {
    const from = lessonNodes[i];
    const to = lessonNodes[i + 1];

    const fromY = from.y + NODE_RADIUS;
    const toY = to.y - NODE_RADIUS;
    const midY = (fromY + toY) / 2;

    // Quadratic Bézier control point — offset for smooth S-curve
    const cpX = (from.x + to.x) / 2 + (to.x - from.x) * 0.3;

    const d = `M ${from.x} ${fromY} Q ${cpX} ${midY} ${to.x} ${toY}`;

    const isActive =
      from.lesson?.isCompleted === true && to.lesson?.isLocked !== true;
    const color = isActive ? Colors.correct : Colors.border;

    paths.push({ d, color });
  }

  return paths;
}

// ─── Star icon ───────────────────────────────────────────
function StarIcon({ filled, size = 30 }: { filled: boolean; size?: number }) {
  return (
    <Text
      style={{
        fontSize: size,
        color: filled ? "#FFFFFF" : Colors.textMuted,
        textAlign: "center",
        lineHeight: size + 4,
      }}
    >
      {filled ? "\u2605" : "\u2606"}
    </Text>
  );
}

// ─── Crown dots ──────────────────────────────────────────
function CrownDots({ count }: { count: number }) {
  if (count <= 0) return null;
  return (
    <View style={styles.crownDots}>
      {Array.from({ length: Math.min(count, 5) }).map((_, i) => (
        <View key={i} style={styles.crownDot} />
      ))}
    </View>
  );
}

// ─── Main screen ─────────────────────────────────────────
export default function PathScreen() {
  const { topicId } = useLocalSearchParams<{ topicId: string }>();
  const [path, setPath] = useState<LearningPath | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPath = useCallback(async () => {
    if (!topicId) return;
    try {
      setError(null);
      const response = await getLearningPath(topicId);
      setPath(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load path");
    } finally {
      setLoading(false);
    }
  }, [topicId]);

  useFocusEffect(
    useCallback(() => {
      fetchPath();
    }, [fetchPath])
  );

  const handleStartSession = async () => {
    if (!topicId) return;
    setStarting(true);
    try {
      const response = await startSession(topicId);
      router.push({
        pathname: "/session/[topicId]",
        params: { topicId, sessionData: JSON.stringify(response.data) },
      });
    } catch (err: any) {
      setError(err.message || "Failed to start session");
    } finally {
      setStarting(false);
    }
  };

  // ─── Loading ───
  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={styles.loadingText}>Loading your path...</Text>
      </View>
    );
  }

  // ─── Error ───
  if (error || !path) {
    return (
      <View style={styles.center}>
        <Text style={styles.errorText}>{error || "No data"}</Text>
        <TouchableOpacity style={styles.retryBtn} onPress={fetchPath}>
          <Text style={styles.retryBtnText}>Retry</Text>
        </TouchableOpacity>
      </View>
    );
  }

  // ─── Build layout ───
  const nodes = buildPathNodes(path.units);
  const connectors = buildConnectorPaths(nodes);
  const totalHeight =
    nodes.length > 0 ? nodes[nodes.length - 1].y + VERTICAL_GAP : 400;

  return (
    <View style={styles.container}>
      {/* ── Header ── */}
      <View style={styles.header}>
        <TouchableOpacity
          style={styles.backButton}
          onPress={() => router.back()}
        >
          <Text style={styles.backButtonText}>{"\u2190"} Back</Text>
        </TouchableOpacity>
        <Text style={styles.title}>{path.topicName}</Text>
        <View style={styles.xpBadge}>
          <Text style={styles.xpText}>{path.totalXPEarned} XP</Text>
        </View>
      </View>

      {/* ── Start Session ── */}
      <TouchableOpacity
        style={[
          styles.sessionButton,
          starting && styles.sessionButtonDisabled,
        ]}
        onPress={handleStartSession}
        disabled={starting}
        activeOpacity={0.8}
      >
        {starting ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.sessionButtonText}>{"\u25B6"} Start Session</Text>
        )}
      </TouchableOpacity>

      {/* ── Winding Path ── */}
      <ScrollView
        contentContainerStyle={{ height: totalHeight + 60 }}
        showsVerticalScrollIndicator={false}
      >
        {/* SVG curved connectors */}
        <Svg
          width={SCREEN_WIDTH}
          height={totalHeight + 60}
          style={StyleSheet.absoluteFill}
        >
          {connectors.map((conn, i) => (
            <SvgPath
              key={i}
              d={conn.d}
              stroke={conn.color}
              strokeWidth={4}
              fill="none"
              strokeLinecap="round"
            />
          ))}
        </Svg>

        {/* Render each node */}
        {nodes.map((node) => {
          // ── Unit banner ──
          if (node.type === "unit-banner") {
            return (
              <View
                key={`unit-${node.unitIndex}`}
                style={[
                  styles.unitBanner,
                  {
                    top: node.y - UNIT_BANNER_HEIGHT / 2,
                    left: 20,
                    right: 20,
                  },
                  node.unitCompleted === true && styles.unitBannerCompleted,
                  node.unitLocked === true && styles.unitBannerLocked,
                ]}
              >
                <Text style={styles.unitBannerIcon}>
                  {node.unitCompleted
                    ? "\uD83C\uDFC6"
                    : node.unitLocked
                    ? "\uD83D\uDD12"
                    : "\uD83D\uDCDA"}
                </Text>
                <View style={styles.unitBannerTextWrap}>
                  <Text style={styles.unitBannerLabel}>
                    Unit {(node.unitIndex ?? 0) + 1}
                  </Text>
                  <Text style={styles.unitBannerName} numberOfLines={1}>
                    {node.unitName}
                  </Text>
                </View>
              </View>
            );
          }

          // ── Lesson node ──
          const lesson = node.lesson!;
          const isCompleted = lesson.isCompleted;
          const isCurrent = lesson.isCurrent;
          const isLocked = lesson.isLocked;

          const bgColor = isCompleted
            ? Colors.correct
            : isCurrent
            ? Colors.primary
            : isLocked
            ? Colors.surfaceLight
            : Colors.surface;

          const borderColor = isCompleted
            ? Colors.correct
            : isCurrent
            ? "#7CE830"
            : isLocked
            ? Colors.border
            : Colors.border;

          return (
            <View
              key={lesson.lessonId}
              style={[
                styles.nodeWrapper,
                {
                  top: node.y - NODE_RADIUS,
                  left: node.x - NODE_RADIUS,
                },
              ]}
            >
              {/* Glow ring for current */}
              {isCurrent ? <View style={styles.glowRing} /> : null}

              {/* Circle */}
              <View
                style={[
                  styles.nodeCircle,
                  { backgroundColor: bgColor, borderColor: borderColor },
                  isLocked && styles.nodeLocked,
                ]}
              >
                {isLocked ? (
                  <Text style={styles.lockIcon}>{"\uD83D\uDD12"}</Text>
                ) : (
                  <StarIcon filled={isCompleted || isCurrent} size={30} />
                )}
              </View>

              {/* Label */}
              {!isLocked ? (
                <Text
                  style={[
                    styles.nodeLabel,
                    isCurrent && styles.nodeLabelCurrent,
                  ]}
                  numberOfLines={2}
                >
                  {lesson.name}
                </Text>
              ) : null}

              {/* Crown dots */}
              {lesson.crowns > 0 ? (
                <CrownDots count={lesson.crowns} />
              ) : null}

              {/* Best score */}
              {lesson.bestScore > 0 && !isLocked ? (
                <Text style={styles.scoreLabel}>{lesson.bestScore}%</Text>
              ) : null}
            </View>
          );
        })}
      </ScrollView>
    </View>
  );
}

// ─── Styles ──────────────────────────────────────────────
const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
  center: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: Colors.background,
  },
  loadingText: { color: Colors.textSecondary, marginTop: 12, fontSize: 16 },
  errorText: { color: Colors.wrong, fontSize: 15, marginBottom: 12 },
  retryBtn: {
    backgroundColor: Colors.primary,
    paddingHorizontal: 24,
    paddingVertical: 10,
    borderRadius: 12,
  },
  retryBtnText: { color: Colors.text, fontWeight: "700" },

  // Header
  header: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 16,
    paddingTop: 16,
    paddingBottom: 8,
  },
  backButton: { paddingVertical: 6, paddingRight: 12 },
  backButtonText: { color: Colors.primary, fontSize: 16, fontWeight: "700" },
  title: { fontSize: 20, fontWeight: "800", color: Colors.text, flex: 1 },
  xpBadge: {
    backgroundColor: Colors.surfaceLight,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 14,
  },
  xpText: { color: Colors.xp, fontSize: 14, fontWeight: "700" },

  // Session button
  sessionButton: {
    backgroundColor: Colors.primary,
    marginHorizontal: 20,
    marginVertical: 10,
    paddingVertical: 14,
    borderRadius: 16,
    alignItems: "center",
    shadowColor: Colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 6,
  },
  sessionButtonDisabled: { opacity: 0.6 },
  sessionButtonText: { color: "#fff", fontSize: 17, fontWeight: "800" },

  // Unit banner (absolute)
  unitBanner: {
    position: "absolute",
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: Colors.surface,
    borderRadius: 14,
    paddingVertical: 10,
    paddingHorizontal: 16,
    borderWidth: 2,
    borderColor: Colors.primary,
    zIndex: 10,
  },
  unitBannerCompleted: { borderColor: Colors.correct },
  unitBannerLocked: { borderColor: Colors.border, opacity: 0.5 },
  unitBannerIcon: { fontSize: 22, marginRight: 10 },
  unitBannerTextWrap: { flex: 1 },
  unitBannerLabel: {
    fontSize: 11,
    fontWeight: "700",
    color: Colors.textMuted,
    textTransform: "uppercase",
    letterSpacing: 1,
  },
  unitBannerName: {
    fontSize: 15,
    fontWeight: "700",
    color: Colors.text,
    marginTop: 1,
  },

  // Node wrapper (absolute)
  nodeWrapper: {
    position: "absolute",
    width: NODE_SIZE,
    alignItems: "center",
    zIndex: 5,
  },

  // Glow ring behind current node
  glowRing: {
    position: "absolute",
    width: NODE_SIZE + 16,
    height: NODE_SIZE + 16,
    borderRadius: (NODE_SIZE + 16) / 2,
    backgroundColor: "rgba(88, 204, 2, 0.2)",
    top: -8,
    left: -8,
    zIndex: -1,
  },

  // Circle node
  nodeCircle: {
    width: NODE_SIZE,
    height: NODE_SIZE,
    borderRadius: NODE_RADIUS,
    borderWidth: 4,
    alignItems: "center",
    justifyContent: "center",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 4,
    elevation: 4,
  },
  nodeLocked: { opacity: 0.4 },
  lockIcon: { fontSize: 22 },

  // Label below node
  nodeLabel: {
    fontSize: 11,
    fontWeight: "600",
    color: Colors.textSecondary,
    textAlign: "center",
    marginTop: 6,
    width: NODE_SIZE + 30,
  },
  nodeLabelCurrent: { color: Colors.text, fontWeight: "800" },

  // Crown dots
  crownDots: { flexDirection: "row", marginTop: 4, gap: 3 },
  crownDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: Colors.gold,
  },

  // Score
  scoreLabel: { fontSize: 10, color: Colors.textMuted, marginTop: 2 },
});
