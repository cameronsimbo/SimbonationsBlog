import { useState, useCallback, useRef, useEffect } from "react";
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Dimensions,
  Animated,
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
const NODE_SIZE = 76;
const NODE_RADIUS = NODE_SIZE / 2;
const SHADOW_DEPTH = 8;
const VERTICAL_GAP = 100;
const AMPLITUDE = Math.min(SCREEN_WIDTH * 0.22, 90);
const CENTER_X = SCREEN_WIDTH / 2;
const TOP_PADDING = 30;
const TRAIL_WIDTH = 16;
const TRAIL_SHADOW_WIDTH = 22;
const BANNER_HEIGHT = 56;

// Duolingo S-curve: repeating x-offset pattern (values -1..1)
// Creates smooth arcs: center -> right -> center -> left -> center
const X_OFFSETS = [0, 0.58, 0.95, 0.58, 0, -0.58, -0.95, -0.58];

// ─── Path node types ─────────────────────────────────────
interface LessonNode {
  type: "lesson";
  lesson: PathLesson;
  x: number;
  y: number;
  lessonIndex: number;
}

interface BannerNode {
  type: "banner";
  unitIndex: number;
  unitName: string;
  unitCompleted: boolean;
  unitLocked: boolean;
  y: number;
}

type PathNode = LessonNode | BannerNode;

// ─── Position nodes in Duolingo S-curve ──────────────────
function buildPathLayout(units: PathUnit[]): PathNode[] {
  const nodes: PathNode[] = [];
  let yOffset = TOP_PADDING;
  let lessonCounter = 0;

  for (let unitIdx = 0; unitIdx < units.length; unitIdx++) {
    const unit = units[unitIdx];

    // Unit banner
    nodes.push({
      type: "banner",
      unitIndex: unitIdx,
      unitName: unit.name,
      unitCompleted: unit.isCompleted,
      unitLocked: unit.isLocked,
      y: yOffset,
    });
    yOffset += BANNER_HEIGHT + 20;

    // Lesson nodes following S-curve pattern
    for (const lesson of unit.lessons) {
      const patternIdx = lessonCounter % X_OFFSETS.length;
      const x = CENTER_X + X_OFFSETS[patternIdx] * AMPLITUDE;

      nodes.push({
        type: "lesson",
        lesson,
        x,
        y: yOffset,
        lessonIndex: lessonCounter,
      });

      yOffset += VERTICAL_GAP;
      lessonCounter++;
    }

    yOffset += 20;
  }

  return nodes;
}

// ─── Catmull-Rom -> cubic Bezier smooth trail ────────────
function buildTrailPath(points: { x: number; y: number }[]): string {
  if (points.length < 2) return "";
  if (points.length === 2) {
    return `M ${points[0].x} ${points[0].y} L ${points[1].x} ${points[1].y}`;
  }

  let d = `M ${points[0].x} ${points[0].y}`;
  const tension = 0.4;

  for (let i = 0; i < points.length - 1; i++) {
    const p0 = points[Math.max(0, i - 1)];
    const p1 = points[i];
    const p2 = points[i + 1];
    const p3 = points[Math.min(points.length - 1, i + 2)];

    const cp1x = p1.x + ((p2.x - p0.x) * tension) / 3;
    const cp1y = p1.y + ((p2.y - p0.y) * tension) / 3;
    const cp2x = p2.x - ((p3.x - p1.x) * tension) / 3;
    const cp2y = p2.y - ((p3.y - p1.y) * tension) / 3;

    d += ` C ${cp1x} ${cp1y} ${cp2x} ${cp2y} ${p2.x} ${p2.y}`;
  }

  return d;
}

// ─── Build trail segments (active green + inactive gray) ─
function buildTrailPaths(lessonNodes: LessonNode[]) {
  const allPts = lessonNodes.map((n) => ({ x: n.x, y: n.y }));
  const fullPath = buildTrailPath(allPts);

  // Find last active node (completed or current)
  let activeEndIdx = -1;
  for (let i = 0; i < lessonNodes.length; i++) {
    if (
      lessonNodes[i].lesson.isCompleted ||
      lessonNodes[i].lesson.isCurrent
    ) {
      activeEndIdx = i;
    }
  }

  const activePath =
    activeEndIdx >= 1
      ? buildTrailPath(allPts.slice(0, activeEndIdx + 1))
      : "";

  return { fullPath, activePath };
}

// ─── Color helpers ───────────────────────────────────────
function getNodeColors(lesson: PathLesson) {
  if (lesson.isCompleted) {
    return { bg: "#58CC02", shadow: "#449B02", border: "#58CC02" };
  }
  if (lesson.isCurrent) {
    return { bg: "#58CC02", shadow: "#449B02", border: "#7CE830" };
  }
  if (lesson.isLocked) {
    return { bg: "#3C3C3C", shadow: "#2A2A2A", border: "#3C3C3C" };
  }
  return { bg: Colors.surface, shadow: "#152429", border: Colors.border };
}

// ─── Star icon ───────────────────────────────────────────
function StarIcon({ filled, size = 32 }: { filled: boolean; size?: number }) {
  return (
    <Text
      style={{
        fontSize: size,
        color: filled ? "#FFFFFF" : "#6B6B6B",
        textAlign: "center",
        lineHeight: size + 2,
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

// ─── Bouncing animation for current node ─────────────────
function BouncingNode({ children }: { children: React.ReactNode }) {
  const bounce = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    const loop = Animated.loop(
      Animated.sequence([
        Animated.timing(bounce, {
          toValue: -6,
          duration: 800,
          useNativeDriver: true,
        }),
        Animated.timing(bounce, {
          toValue: 0,
          duration: 800,
          useNativeDriver: true,
        }),
      ])
    );
    loop.start();
    return () => loop.stop();
  }, [bounce]);

  return (
    <Animated.View style={{ transform: [{ translateY: bounce }] }}>
      {children}
    </Animated.View>
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

  // ── Loading ──
  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={styles.loadingText}>Loading your path...</Text>
      </View>
    );
  }

  // ── Error ──
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

  // ── Build layout ──
  const nodes = buildPathLayout(path.units);
  const lessonNodes = nodes.filter(
    (n): n is LessonNode => n.type === "lesson"
  );
  const { fullPath, activePath } = buildTrailPaths(lessonNodes);
  const totalHeight =
    nodes.length > 0 ? nodes[nodes.length - 1].y + VERTICAL_GAP + 40 : 400;

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity
          style={styles.backButton}
          onPress={() => {
            if (router.canGoBack()) {
              router.back();
            } else {
              router.replace("/(tabs)");
            }
          }}
        >
          <Text style={styles.backButtonText}>{"\u2190"} Back</Text>
        </TouchableOpacity>
        <Text style={styles.title} numberOfLines={1}>
          {path.topicName}
        </Text>
        <View style={styles.xpBadge}>
          <Text style={styles.xpText}>{path.totalXPEarned} XP</Text>
        </View>
      </View>

      {/* Start Session */}
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
          <Text style={styles.sessionButtonText}>
            {"\u25B6"} Start Session
          </Text>
        )}
      </TouchableOpacity>

      {/* Winding Path */}
      <ScrollView
        contentContainerStyle={{ height: totalHeight }}
        showsVerticalScrollIndicator={false}
      >
        {/* SVG trail layers */}
        <Svg
          width={SCREEN_WIDTH}
          height={totalHeight}
          style={StyleSheet.absoluteFill}
        >
          {/* Layer 1: Full trail shadow (widest, darkest) */}
          {fullPath ? (
            <SvgPath
              d={fullPath}
              stroke="#1A2E35"
              strokeWidth={TRAIL_SHADOW_WIDTH}
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ) : null}
          {/* Layer 2: Full trail (gray) */}
          {fullPath ? (
            <SvgPath
              d={fullPath}
              stroke={Colors.border}
              strokeWidth={TRAIL_WIDTH}
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ) : null}
          {/* Layer 3: Active trail shadow */}
          {activePath ? (
            <SvgPath
              d={activePath}
              stroke="#449B02"
              strokeWidth={TRAIL_SHADOW_WIDTH}
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ) : null}
          {/* Layer 4: Active trail (green) */}
          {activePath ? (
            <SvgPath
              d={activePath}
              stroke="#58CC02"
              strokeWidth={TRAIL_WIDTH}
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ) : null}
        </Svg>

        {/* Render nodes */}
        {nodes.map((node) => {
          // ── Unit banner ──
          if (node.type === "banner") {
            return (
              <View
                key={`banner-${node.unitIndex}`}
                style={[
                  styles.unitBanner,
                  { top: node.y },
                  node.unitLocked && styles.unitBannerLocked,
                ]}
              >
                <View
                  style={[
                    styles.bannerPill,
                    node.unitCompleted && styles.bannerPillCompleted,
                    node.unitLocked && styles.bannerPillLocked,
                  ]}
                >
                  <Text style={styles.bannerIcon}>
                    {node.unitCompleted
                      ? "\uD83C\uDFC6"
                      : node.unitLocked
                      ? "\uD83D\uDD12"
                      : "\uD83D\uDCDA"}
                  </Text>
                  <Text style={styles.bannerText}>
                    Unit {node.unitIndex + 1} {"\u2014"} {node.unitName}
                  </Text>
                </View>
              </View>
            );
          }

          // ── Lesson node (3D button) ──
          const lesson = node.lesson;
          const colors = getNodeColors(lesson);
          const isCurrent = lesson.isCurrent;

          const nodeButton = (
            <View style={styles.nodeButtonOuter}>
              {/* 3D shadow disk (sits behind, offset down) */}
              <View
                style={[
                  styles.nodeShadowDisk,
                  { backgroundColor: colors.shadow },
                ]}
              />
              {/* Main disk */}
              <View
                style={[
                  styles.nodeMainDisk,
                  {
                    backgroundColor: colors.bg,
                    borderColor: colors.border,
                  },
                ]}
              >
                {lesson.isLocked ? (
                  <Text style={styles.lockIcon}>{"\uD83D\uDD12"}</Text>
                ) : (
                  <StarIcon
                    filled={lesson.isCompleted || lesson.isCurrent}
                    size={32}
                  />
                )}
              </View>
            </View>
          );

          return (
            <View
              key={lesson.lessonId}
              style={[
                styles.nodeWrapper,
                {
                  top: node.y - NODE_RADIUS - SHADOW_DEPTH / 2,
                  left: node.x - NODE_RADIUS,
                },
              ]}
            >
              {/* Glow ring for current */}
              {isCurrent && <View style={styles.glowRing} />}

              {/* Node (bouncing if current) */}
              {isCurrent ? (
                <BouncingNode>{nodeButton}</BouncingNode>
              ) : (
                nodeButton
              )}

              {/* Label */}
              {!lesson.isLocked && (
                <Text
                  style={[
                    styles.nodeLabel,
                    isCurrent && styles.nodeLabelCurrent,
                  ]}
                  numberOfLines={2}
                >
                  {lesson.name}
                </Text>
              )}

              {/* Crown dots */}
              {lesson.crowns > 0 && <CrownDots count={lesson.crowns} />}

              {/* Score */}
              {lesson.bestScore > 0 && !lesson.isLocked && (
                <Text style={styles.scoreLabel}>{lesson.bestScore}%</Text>
              )}
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

  // Unit banner
  unitBanner: {
    position: "absolute",
    left: 16,
    right: 16,
    zIndex: 10,
    alignItems: "center",
  },
  unitBannerLocked: { opacity: 0.5 },
  bannerPill: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#58CC02",
    borderRadius: 20,
    paddingVertical: 10,
    paddingHorizontal: 20,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 4,
    elevation: 4,
  },
  bannerPillCompleted: { backgroundColor: Colors.gold },
  bannerPillLocked: { backgroundColor: "#3C3C3C" },
  bannerIcon: { fontSize: 18, marginRight: 8 },
  bannerText: {
    fontSize: 14,
    fontWeight: "800",
    color: "#FFFFFF",
    textTransform: "uppercase",
    letterSpacing: 0.5,
  },

  // Node wrapper (absolute positioned)
  nodeWrapper: {
    position: "absolute",
    width: NODE_SIZE,
    alignItems: "center",
    zIndex: 5,
  },

  // 3D button container
  nodeButtonOuter: {
    width: NODE_SIZE,
    height: NODE_SIZE + SHADOW_DEPTH,
    alignItems: "center",
  },

  // Shadow disk (bottom layer — 3D depth effect)
  nodeShadowDisk: {
    position: "absolute",
    bottom: 0,
    width: NODE_SIZE,
    height: NODE_SIZE,
    borderRadius: NODE_RADIUS,
  },

  // Main disk (top layer)
  nodeMainDisk: {
    position: "absolute",
    top: 0,
    width: NODE_SIZE,
    height: NODE_SIZE,
    borderRadius: NODE_RADIUS,
    borderWidth: 3,
    alignItems: "center",
    justifyContent: "center",
  },

  // Glow ring for current node
  glowRing: {
    position: "absolute",
    width: NODE_SIZE + 20,
    height: NODE_SIZE + 20 + SHADOW_DEPTH,
    borderRadius: (NODE_SIZE + 20) / 2,
    backgroundColor: "rgba(88, 204, 2, 0.15)",
    top: -10,
    left: -10,
    zIndex: -1,
  },

  lockIcon: { fontSize: 26 },

  // Label below node
  nodeLabel: {
    fontSize: 11,
    fontWeight: "600",
    color: Colors.textSecondary,
    textAlign: "center",
    marginTop: 4,
    width: NODE_SIZE + 40,
  },
  nodeLabelCurrent: { color: Colors.text, fontWeight: "800" },

  // Crown dots
  crownDots: { flexDirection: "row", marginTop: 3, gap: 3 },
  crownDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: Colors.gold,
  },

  // Score
  scoreLabel: { fontSize: 10, color: Colors.textMuted, marginTop: 2 },
});
