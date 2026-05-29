using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SMSAndroidsCore
{
    // ──────────────────────────────────────────────────────────────
    //  Massage Rhythm Minigame
    //
    //  Zone-based, up-and-down strokes, mouse-button-gated.
    //
    //  Flow:
    //    1. A pattern is randomly selected.  It defines a sequence of
    //       "segments", each specifying a zone, a stroke range, a target
    //       stroke speed, and a tolerance.
    //    2. The active zone highlights.  The player moves the mouse
    //       into that zone, HOLDS the mouse button, and strokes
    //       up/down inside the zone repeatedly.
    //    3. Every time the mouse reverses direction (a full stroke)
    //       while the button is held AND the cursor is inside the
    //       active zone, the stroke is scored based on how close its
    //       speed was to the segment's targetSpeed.
    //    4. When enough stroke attempts have been completed in the
    //       segment, the next zone lights up.  When the pattern ends
    //       the next random pattern starts.
    //    5. After maxPatterns patterns total, results are shown.
    //
    //  Patterns are loaded from JSON files on disk.
    //  Visual assets come from an optional minigamebundle.
    //
    //  Call LoadPatterns() once, then StartMinigame() to play.
    // ──────────────────────────────────────────────────────────────

    public enum MinigameState
    {
        Idle,
        StartMenu,
        Countdown,
        Playing,
        Results
    }

    public enum StrokeRating
    {
        Perfect,
        Good,
        OK,
        Miss
    }

    public enum MassageRank
    {
        S, A, B, C, D
    }

    public class MassageMinigame : MonoBehaviour
    {
        // ──────────────────────────────────────────────────────────
        //  Configuration
        // ──────────────────────────────────────────────────────────

        [Header("Gameplay")]
        [Tooltip("Number of horizontal zones in the play area.")]
        public int zoneCount = 7;

        [Tooltip("Number of patterns to play before the results screen.")]
        public int maxPatterns = 3;

        [Tooltip("Seconds the countdown takes (3-2-1-Go).")]
        public float countdownDuration = 3f;

        [Tooltip("Mouse button index (0=Left, 1=Right, 2=Middle).")]
        public int mouseButton = 0;

        [Tooltip("Reference 'slow' speed (normalised units / sec) for ideal traversal time.")]
        public float slowSpeed = 0.3f;

        [Header("Play Area")]
        [Tooltip("Normalized screen rect for the play area (x, y, width, height in 0-1).")]
        public Rect playAreaNormalized = new Rect(0.3f, 0.15f, 0.4f, 0.7f);

        [Header("Stroke Detection")]
        [Tooltip("Minimum distance (normalized play area height) a stroke must cover to count.")]
        [Range(0.02f, 0.5f)]
        public float minStrokeLength = 0.08f;

        [Header("Scoring")]
        [Tooltip("Points for a Perfect stroke (speed within ±10% of target).")]
        public int pointsPerfect = 100;

        [Tooltip("Points for a Good stroke (speed within ±20% of target).")]
        public int pointsGood = 75;

        [Tooltip("Points for an OK stroke (speed within tolerance).")]
        public int pointsOK = 50;

        [Tooltip("Points deducted for stroking with wrong speed or outside the zone.")]
        public int pointsMiss = 0;

        [Header("Rank Thresholds")]
        [Tooltip("Average points-per-stroke needed for each rank (as fraction of pointsPerfect).")]
        public float rankS = 0.90f;
        public float rankA = 0.75f;
        public float rankB = 0.55f;
        public float rankC = 0.35f;

        [Header("Zone Layout")]
        [Tooltip("Custom zone bounds in normalized play area Y (0 = bottom, 1 = top).\n" +
                 "Zones may overlap. Leave empty to use equal strips.\n" +
                 "Array length must match zoneCount.")]
        public ZoneDefinition[] zoneDefs;

        [Header("Colors")]
        public Color zoneIdleColor      = new Color(0.2f, 0.2f, 0.3f, 0.25f);
        public Color zoneActiveColor    = new Color(0.3f, 0.85f, 1f, 0.55f);
        public Color zoneStrokingColor  = new Color(0.2f, 1f, 0.6f, 0.6f);
        public Color zoneMissColor      = new Color(1f, 0.25f, 0.25f, 0.5f);
        public Color cursorColor        = new Color(1f, 1f, 1f, 0.85f);
        public Color cursorDimColor     = new Color(1f, 1f, 1f, 0.3f);
        public Color panelBackground    = new Color(0.05f, 0.05f, 0.1f, 0.7f);

        // ──────────────────────────────────────────────────────────
        //  Internal State
        // ──────────────────────────────────────────────────────────

        private MinigameState state = MinigameState.Idle;
        private float stateTimer;

        // Pattern counting
        private int patternsCompleted;       // how many full patterns have been played this round

        // Patterns
        private List<MassageMovementPattern> patterns = new List<MassageMovementPattern>();
        private MassageMovementPattern activePattern;
        private int activeSegmentIndex;
        private int segmentStrokesRequired;  // randomised target stroke count for the active segment
        private int segmentStrokesCompleted; // stroke attempts made so far in the active segment

        // Stroke tracking
        private float lastNormalizedY;          // mouse Y last frame (0-1 in play area)
        private float strokeStartY;             // where the current stroke began
        private float strokeDistance;            // distance accumulated this stroke
        private float strokeTime;               // time accumulated this stroke
        private int   strokeDirection;           // +1 = up, -1 = down, 0 = undetermined
        private bool  wasButtonHeld;             // button state last frame
        private bool  clickStartedInZone;        // mouse-down occurred within active zone
        private int   currentPlayerZone = -1;
        private int   segmentDirection;          // resolved direction for active segment (+1 up, -1 down)
        private float zoneShakeTimer;            // counts down while bad-click shake is active
        private const float ZONE_SHAKE_TIME = 0.35f;

        // Scoring
        private int totalScore;
        private int totalStrokes;
        private int perfectCount;
        private int goodCount;
        private int okCount;
        private int missCount;
        private int comboCount;
        private int bestCombo;

        // Character / level progression
        // Driven by:
        //   * proxy variable "Minigame_Massage_Character" — which character GO under "Characters" to show
        //   * SaveManager int "Minigame_Massage_<Char>_Level" — index of the next variant to play
        // When level >= variant count we enter sandbox mode (random pick, score doesn't gate progression).
        private string activeCharacterName;
        private GameObject activeCharacterRoot;
        private GameObject activeVariant;
        private int activeVariantIndex;
        private int activeVariantCount;
        private bool sandboxMode;
        private const string PROXY_CHAR_KEY = "Minigame_Massage_Character";
        private const string SAVE_LEVEL_KEY_PREFIX = "Minigame_Massage_";
        private const string SAVE_LEVEL_KEY_SUFFIX = "_Level";
        private const string SAVE_HIGHSCORE_KEY_PREFIX = "Minigame_Massage_";
        private const string SAVE_HIGHSCORE_KEY_SUFFIX = "_Highscore";

        // Speed indicator
        private float currentStrokeSpeedNorm;   // live speed for the UI arrow/bar

        // ──────────────────────────────────────────────────────────
        //  UI References (created at runtime)
        // ──────────────────────────────────────────────────────────

        private GameObject uiRoot;
        private Canvas uiCanvas;
        private RectTransform playAreaPanel;
        private Image[] zoneImages;
        private RectTransform cursorIndicator;
        private Image cursorImage;
        private TextMeshProUGUI patternProgressText;  // "Pattern 2 / 6"
        private TextMeshProUGUI strokeProgressText;   // "3 / 5 strokes"
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI comboText;
        private TextMeshProUGUI patternNameText;
        private TextMeshProUGUI countdownText;
        private TextMeshProUGUI strokeRatingText;
        private TextMeshProUGUI speedHintText;
        private RectTransform speedBar;
        private RectTransform speedBarFill;
        private Image speedBarFillImage;
        private GameObject resultsPanel;
        private TextMeshProUGUI rankText;
        private TextMeshProUGUI finalScoreText;
        private TextMeshProUGUI breakdownText;
        private GameObject startMenuPanel;
        private RectTransform startButtonRect;
        private Image         startButtonImage;
        private Color         startButtonNormal;
        private Color         startButtonHover;
        private RectTransform closeButtonRect;
        private Image         closeButtonImage;
        private Color         closeButtonNormal;
        private Color         closeButtonHover;

        // Rating flash
        private float ratingDisplayTimer;
        private const float RATING_DISPLAY_TIME = 0.5f;

        // Zone flash
        private float zoneFlashTimer;
        private int   zoneFlashIndex = -1;
        private Color zoneFlashColor;
        private const float ZONE_FLASH_TIME = 0.15f;

        // ──────────────────────────────────────────────────────────
        //  Optional AssetBundle visuals
        // ──────────────────────────────────────────────────────────

        // True when UI was built procedurally (so DestroyUI knows to clean up)
        private bool _proceduralUI;

        // Individual sprites (used by the procedural fallback)
        private Sprite zoneHighlightSprite;
        private Sprite cursorSprite;
        private Sprite backgroundSprite;
        private Sprite startMenuSprite;
        private Sprite closeButtonSprite;

        // ══════════════════════════════════════════════════════════
        //  PUBLIC API
        // ══════════════════════════════════════════════════════════

        /// <summary>Begin a new massage minigame round (plays up to <see cref="maxPatterns"/> patterns).</summary>
        public void StartMinigame()
        {
            if (patterns.Count == 0)
            {
                Debug.LogWarning("[MassageMinigame] No patterns loaded — call LoadPatterns() first.");
                return;
            }

            ResetScoring();
            ResetStrokeState();
            CreateUI();
            SetState(MinigameState.Countdown);
        }

        /// <summary>Stops the minigame and destroys the UI.</summary>
        public void StopMinigame()
        {
            SetState(MinigameState.Idle);
            DestroyUI();
        }

        /// <summary>
        /// Loads all pattern JSON files from
        /// <c>BepInEx/plugins/SMSAndroidsCore/Minigame/Patterns/*.json</c>.
        /// Call once during scene load.
        /// </summary>
        public void LoadPatterns()
        {
            patterns.Clear();

            // Normalise the path so any /../ segments are resolved before checking
            string rawFolder = Core.exePath + Core.minigamePath + "Patterns";
            string patternsFolder = System.IO.Path.GetFullPath(rawFolder);

            // Debug.Log($"[MassageMinigame] Looking for patterns in: {patternsFolder}");

            if (!Directory.Exists(patternsFolder))
            {
                Debug.LogWarning($"[MassageMinigame] Patterns folder not found: {patternsFolder}");
                return;
            }

            string[] jsonFiles = Directory.GetFiles(patternsFolder, "*.json");
            // Debug.Log($"[MassageMinigame] Found {jsonFiles.Length} JSON file(s) in folder.");

            foreach (string filePath in jsonFiles)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    MassagePatternCollection collection = JsonConvert.DeserializeObject<MassagePatternCollection>(json);

                    if (collection == null)
                    {
                        Debug.LogError($"[MassageMinigame] JsonConvert returned null for {filePath}. Check the file is valid UTF-8 JSON.");
                        continue;
                    }

                    if (collection.patterns == null || collection.patterns.Length == 0)
                    {
                        Debug.LogWarning($"[MassageMinigame] Parsed {filePath} but found no 'patterns' array. " +
                                         "Check that the top-level key is exactly \"patterns\".");
                        continue;
                    }

                    foreach (var pattern in collection.patterns)
                    {
                        if (pattern.segments == null || pattern.segments.Length == 0)
                        {
                            Debug.LogWarning($"[MassageMinigame] Pattern '{pattern.patternName}' has no segments — skipped. " +
                                             "Check that the key is exactly \"segments\".");
                            continue;
                        }

                        patterns.Add(pattern);
                        // Debug.Log($"[MassageMinigame] Loaded pattern: '{pattern.patternName}' " +
                        //           $"({pattern.segments.Length} segment(s)).");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MassageMinigame] Exception loading {filePath}: {ex}");
                }
            }

            // Debug.Log($"[MassageMinigame] Done — {patterns.Count} pattern(s) ready.");
        }

        /// <summary>
        /// Loads optional sprites from the minigamebundle.
        /// Call this before <see cref="StartMinigame"/>.
        /// The UI itself is wired from named children on this GameObject —
        /// <see cref="LoadVisualAssets"/> is only needed for sprites used by
        /// the procedural fallback renderer.
        /// </summary>
        public void LoadVisualAssets()
        {
            if (Core.minigameBundle == null)
            {
                // Debug.Log("[MassageMinigame] No minigamebundle — using procedural visuals.");
                return;
            }

            zoneHighlightSprite = Core.minigameBundle.LoadAsset<Sprite>("ZoneHighlight");
            cursorSprite        = Core.minigameBundle.LoadAsset<Sprite>("Cursor");
            backgroundSprite    = Core.minigameBundle.LoadAsset<Sprite>("MinigameBackground");
            startMenuSprite     = Core.minigameBundle.LoadAsset<Sprite>("StartMenuBackground");
            closeButtonSprite   = Core.minigameBundle.LoadAsset<Sprite>("CloseButton");

            // Debug.Log("[MassageMinigame] Sprites loaded from minigamebundle.");
        }

        // ══════════════════════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ══════════════════════════════════════════════════════════

        private void OnEnable()
        {
            LoadPatterns();
            LoadVisualAssets();
            ResetScoring();
            ResetStrokeState();
            PickActiveCharacterAndVariant();
            WireHandsToActiveVariant();
            InitTextureProgression();
            CreateUI();
            SetState(MinigameState.StartMenu);
        }

        private void OnDisable()
        {
            CleanupTextureProgression();
            StopMinigame();
        }

        private void Update()
        {
            switch (state)
            {
                case MinigameState.StartMenu:  UpdateStartMenu();  break;
                case MinigameState.Countdown:  UpdateCountdown();  break;
                case MinigameState.Playing:    UpdatePlaying();    break;
                case MinigameState.Results:    UpdateResults();    break;
            }
        }

        // ══════════════════════════════════════════════════════════
        //  STATE MANAGEMENT
        // ══════════════════════════════════════════════════════════

        private void SetState(MinigameState newState)
        {
            state = newState;
            stateTimer = 0f;

            switch (newState)
            {
                case MinigameState.StartMenu:
                    SetGameplayVisible(false);
                    if (startMenuPanel != null) startMenuPanel.SetActive(true);
                    if (resultsPanel   != null) resultsPanel.SetActive(false);
                    if (countdownText  != null) countdownText.gameObject.SetActive(false);
                    break;

                case MinigameState.Countdown:
                    SetGameplayVisible(true);
                    if (startMenuPanel != null) startMenuPanel.SetActive(false);
                    if (countdownText  != null) { countdownText.gameObject.SetActive(true); countdownText.text = "3"; }
                    if (resultsPanel   != null) resultsPanel.SetActive(false);
                    break;

                case MinigameState.Playing:
                    patternsCompleted = 0;
                    if (countdownText != null) countdownText.gameObject.SetActive(false);
                    SelectNextPattern();
                    break;

                case MinigameState.Results:
                    ShowResults();
                    break;
            }
        }

        // ──────────────────────────────────────────────────────────
        //  COUNTDOWN
        // ──────────────────────────────────────────────────────────

        private void UpdateCountdown()
        {
            stateTimer += Time.deltaTime;
            float remaining = countdownDuration - stateTimer;

            if (remaining > 2f)      countdownText.text = "3";
            else if (remaining > 1f) countdownText.text = "2";
            else if (remaining > 0f) countdownText.text = "1";
            else                     countdownText.text = "GO!";

            if (stateTimer >= countdownDuration + 0.5f)
                SetState(MinigameState.Playing);
        }

        // ──────────────────────────────────────────────────────────
        //  PLAYING — main game loop
        // ──────────────────────────────────────────────────────────

        private void UpdatePlaying()
        {
            float dt = Time.deltaTime;
            stateTimer += dt;

            // Mouse → zone + normalizedY
            float normalizedY = UpdatePlayerInput();

            // Stroke detection (only while button is held)
            bool buttonHeld = Input.GetMouseButton(mouseButton);
            ProcessStrokeInput(normalizedY, buttonHeld, dt);
            wasButtonHeld = buttonHeld;

            // Stroke progress display
            if (strokeProgressText != null)
                strokeProgressText.text = $"{segmentStrokesCompleted} / {segmentStrokesRequired}";

            // Visuals
            UpdateCursorVisual(normalizedY, buttonHeld);
            UpdateZoneVisuals(buttonHeld, normalizedY);
            UpdateSpeedBar();
            UpdateRatingDisplay(dt);
            UpdateZoneFlash(dt);
            UpdateTextureProgression();
        }

        /// <summary>
        /// Computes the player's normalizedY (0-1 within play area) and
        /// updates <see cref="currentPlayerZone"/>.
        /// Returns the normalizedY value.
        /// </summary>
        private float UpdatePlayerInput()
        {
            if (playAreaPanel == null) { currentPlayerZone = -1; return 0f; }

            Vector2 mouseScreen = Input.mousePosition;

            // Use the canvas camera (null for ScreenSpaceOverlay)
            Camera cam = null;
            if (uiCanvas != null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = uiCanvas.worldCamera;

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    playAreaPanel, mouseScreen, cam, out localPoint))
            {
                currentPlayerZone = -1;
                // Debug.Log($"[MassageMinigame] ScreenPointToLocal FAILED — cam={cam}, mouseScreen={mouseScreen}");
                return 0f;
            }

            Rect rect = playAreaPanel.rect;
            float normY = (rect.height > 0f)
                ? Mathf.Clamp01((localPoint.y - rect.yMin) / rect.height)
                : 0f;

            // First zone (lowest index) whose bounds contain the cursor
            currentPlayerZone = -1;
            for (int i = 0; i < zoneCount; i++)
                if (IsInZone(normY, i)) { currentPlayerZone = i; break; }

            return normY;
        }

        // ──────────────────────────────────────────────────────────
        //  STROKE DETECTION
        //
        //  A "stroke" is one continuous up or down sweep while:
        //    • The mouse button is held
        //    • The cursor is inside the active zone
        //  When the mouse reverses direction (or the button is
        //  released, or the cursor leaves the zone), the stroke
        //  ends and is scored.
        // ──────────────────────────────────────────────────────────

        private void ProcessStrokeInput(float normalizedY, bool buttonHeld, float dt)
        {
            MassagePatternSegment seg = GetActiveSegment();
            if (seg == null) return;

            bool inCorrectZone = IsInZone(normalizedY, seg.zone);

            // Track whether the mouse button was first pressed inside the active zone
            if (Input.GetMouseButtonDown(mouseButton))
            {
                clickStartedInZone = inCorrectZone;
                // Bad click (pressed outside zone) → trigger shake
                if (!inCorrectZone)
                    zoneShakeTimer = ZONE_SHAKE_TIME;
            }
            if (!buttonHeld)
                clickStartedInZone = false;

            bool validInput = buttonHeld && inCorrectZone && clickStartedInZone;

            // Button just pressed in zone → begin a new stroke
            if (validInput && !wasButtonHeld)
            {
                BeginStroke(normalizedY);
                lastNormalizedY = normalizedY;
                return;
            }

            // Button released or left zone → finalize in-progress stroke
            if (!validInput && strokeDirection != 0)
            {
                FinalizeStroke(seg, normalizedY);
                lastNormalizedY = normalizedY;
                return;
            }

            // Button held and in zone — track movement
            if (validInput)
            {
                float delta = normalizedY - lastNormalizedY;

                if (Mathf.Abs(delta) < 0.0001f)
                {
                    // Mouse didn't move — just accumulate time
                    if (strokeDirection != 0) strokeTime += dt;
                    lastNormalizedY = normalizedY;
                    return;
                }

                int newDir = delta > 0f ? 1 : -1;

                if (strokeDirection == 0)
                {
                    // First real movement — lock in direction and start accumulating
                    strokeDirection = newDir;
                    strokeDistance += Mathf.Abs(delta);
                    strokeTime += dt;
                }
                else if (newDir == strokeDirection)
                {
                    // Same direction — extend stroke
                    strokeDistance += Mathf.Abs(delta);
                    strokeTime += dt;
                }
                else
                {
                    // Direction reversed → finalize old stroke, start new one
                    FinalizeStroke(seg, normalizedY);
                    BeginStroke(normalizedY);
                }
            }

            lastNormalizedY = normalizedY;
        }

        private void BeginStroke(float normalizedY)
        {
            strokeStartY = normalizedY;
            strokeDistance = 0f;
            strokeTime = 0f;
            strokeDirection = 0; // direction determined on first real movement delta
            currentStrokeSpeedNorm = 0f;
        }

        private void FinalizeStroke(MassagePatternSegment seg, float endY)
        {
            if (strokeDistance >= minStrokeLength && strokeTime > 0f)
            {
                ScoreStroke(seg, strokeStartY, endY, strokeTime, strokeDirection);

                // Count as a movement attempt and check if segment is done
                segmentStrokesCompleted++;
                CheckSegmentAdvance();
            }
            // else: stroke too short — silently discard (no penalty, no count)

            ResetStrokeState();
        }

        private void ResetStrokeState()
        {
            strokeDirection = 0;
            strokeDistance = 0f;
            strokeTime = 0f;
            strokeStartY = 0f;
            currentStrokeSpeedNorm = 0f;
        }

        // ──────────────────────────────────────────────────────────
        //  SCORING A STROKE  (region-timer system)
        // ──────────────────────────────────────────────────────────

        private void ScoreStroke(MassagePatternSegment seg, float startY, float endY,
                                 float time, int direction)
        {
            ZoneDefinition zone = GetZoneDef(seg.zone);
            float zoneHeight = zone.yMax - zone.yMin;
            if (zoneHeight <= 0.001f) zoneHeight = 0.001f;

            int expectedDir = segmentDirection;

            // Wrong direction → automatic Miss
            if (direction != expectedDir)
            {
                RecordMiss(seg);
                return;
            }

            // Ideal start / end edges based on direction
            float idealStart = expectedDir == 1 ? zone.yMin : zone.yMax;
            float idealEnd   = expectedDir == 1 ? zone.yMax : zone.yMin;

            // Start accuracy (how close to the correct starting edge)
            float startAcc = Mathf.Clamp01(1f - Mathf.Abs(startY - idealStart) / zoneHeight);

            // End accuracy (how close to the correct ending edge)
            float endAcc = Mathf.Clamp01(1f - Mathf.Abs(endY - idealEnd) / zoneHeight);

            // Time accuracy (how close to ideal traversal time at slow speed)
            float idealTime = zoneHeight / slowSpeed;
            float timeAcc = (idealTime > 0f)
                ? Mathf.Clamp01(1f - Mathf.Abs(time - idealTime) / idealTime)
                : 0f;

            // Combined score (equal weighting)
            float score = (startAcc + endAcc + timeAcc) / 3f;

            StrokeRating rating;
            int points;

            if (score >= 0.85f)
            {
                rating = StrokeRating.Perfect;
                points = pointsPerfect;
                perfectCount++;
                comboCount++;
            }
            else if (score >= 0.70f)
            {
                rating = StrokeRating.Good;
                points = pointsGood;
                goodCount++;
                comboCount++;
            }
            else if (score >= 0.50f)
            {
                rating = StrokeRating.OK;
                points = pointsOK;
                okCount++;
                comboCount++;
            }
            else
            {
                rating = StrokeRating.Miss;
                points = pointsMiss;
                missCount++;
                if (comboCount > bestCombo) bestCombo = comboCount;
                comboCount = 0;
            }

            totalScore += points;
            totalStrokes++;

            // UI updates
            if (scoreText != null) scoreText.text = totalScore.ToString();
            if (comboText != null) comboText.text = comboCount > 1 ? $"x{comboCount}" : "";

            ShowStrokeRating(rating);
            TriggerZoneFlash(seg.zone, rating != StrokeRating.Miss ? zoneStrokingColor : zoneMissColor);
        }

        private void RecordMiss(MassagePatternSegment seg)
        {
            missCount++;
            if (comboCount > bestCombo) bestCombo = comboCount;
            comboCount = 0;

            totalScore += pointsMiss;
            totalStrokes++;

            if (scoreText != null) scoreText.text = totalScore.ToString();
            if (comboText != null) comboText.text = "";

            ShowStrokeRating(StrokeRating.Miss);
            TriggerZoneFlash(seg.zone, zoneMissColor);

            segmentStrokesCompleted++;
            CheckSegmentAdvance();
        }

        // ──────────────────────────────────────────────────────────
        //  SEGMENT / PATTERN TIMING
        // ──────────────────────────────────────────────────────────

        private MassagePatternSegment GetActiveSegment()
        {
            if (activePattern == null || activePattern.segments == null) return null;
            if (activeSegmentIndex >= activePattern.segments.Length) return null;
            return activePattern.segments[activeSegmentIndex];
        }

        /// <summary>
        /// Called after each counted stroke attempt.
        /// Advances to the next segment (or next pattern) when the
        /// required number of stroke attempts have been completed.
        /// </summary>
        private void CheckSegmentAdvance()
        {
            if (segmentStrokesCompleted < segmentStrokesRequired) return;

            MassagePatternSegment current = GetActiveSegment();
            if (current == null) return;

            activeSegmentIndex++;
            segmentStrokesCompleted = 0;

            MassagePatternSegment nextSeg = GetActiveSegment();
            if (nextSeg != null)
            {
                BeginSegment(nextSeg);
            }
            else
            {
                // Pattern exhausted → advance to next pattern
                SelectNextPattern();
            }
        }

        /// <summary>
        /// Initialises state for a newly active segment:
        /// randomises the required stroke count and updates UI hints.
        /// </summary>
        private void BeginSegment(MassagePatternSegment seg)
        {
            int lo = Mathf.Max(1, seg.minStrokes);
            int hi = Mathf.Max(lo, seg.maxStrokes);
            segmentStrokesRequired = UnityEngine.Random.Range(lo, hi + 1);
            segmentStrokesCompleted = 0;

            // Resolve direction: use JSON value if set to "up"/"down", otherwise randomise
            bool hasDir = !string.IsNullOrEmpty(seg.direction) &&
                          (string.Equals(seg.direction, "up",   StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(seg.direction, "down", StringComparison.OrdinalIgnoreCase));
            segmentDirection = hasDir ? seg.DirectionInt
                                      : (UnityEngine.Random.value < 0.5f ? 1 : -1);

            UpdateSpeedHint(seg);
        }

        private void SelectNextPattern()
        {
            if (patterns.Count == 0) return;

            // All patterns for this round played → show results
            if (patternsCompleted >= maxPatterns)
            {
                SetState(MinigameState.Results);
                return;
            }

            patternsCompleted++;
            activePattern = patterns[UnityEngine.Random.Range(0, patterns.Count)];
            activeSegmentIndex = 0;
            segmentStrokesCompleted = 0;
            ResetStrokeState();

            ClearAllZoneHighlights();

            if (patternNameText != null)
                patternNameText.text = activePattern.patternName ?? "";

            if (patternProgressText != null)
                patternProgressText.text = $"Pattern {patternsCompleted} / {maxPatterns}";

            MassagePatternSegment seg = GetActiveSegment();
            if (seg != null)
                BeginSegment(seg);
        }

        // ──────────────────────────────────────────────────────────
        //  RESULTS
        // ──────────────────────────────────────────────────────────

        private void UpdateResults()
        {
            stateTimer += Time.deltaTime;

            // Hover tint
            if (closeButtonImage != null)
                closeButtonImage.color = IsMouseOverRect(closeButtonRect)
                    ? closeButtonHover : closeButtonNormal;

            if (stateTimer > 0.5f && Input.GetMouseButtonDown(0) && IsMouseOverRect(closeButtonRect))
                Minigames.Instance.StopMinigame(gameObject, AverageRatingWord(ComputeAverageStrokeRating()));
        }

        // ──────────────────────────────────────────────────────────
        //  START MENU
        // ──────────────────────────────────────────────────────────

        private void UpdateStartMenu()
        {
            if (startMenuPanel != null && !startMenuPanel.activeSelf)
                startMenuPanel.SetActive(true);

            // Hover tint
            if (startButtonImage != null)
                startButtonImage.color = IsMouseOverRect(startButtonRect)
                    ? startButtonHover : startButtonNormal;

            // Click anywhere on the START button → begin
            if (Input.GetMouseButtonDown(0) && IsMouseOverRect(startButtonRect))
            {
                if (startMenuPanel != null) startMenuPanel.SetActive(false);
                ResetScoring();
                ResetStrokeState();
                SetState(MinigameState.Countdown);
            }
        }

        private bool IsMouseOverRect(RectTransform rt)
        {
            if (rt == null) return false;
            Camera cam = null;
            if (uiCanvas != null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = uiCanvas.worldCamera;
            return RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam);
        }

        /// <summary>Stores normal colour and a darkened hover colour for a button.</summary>
        private static void SetButtonColors(ref Color normal, ref Color hover, Color baseColor)
        {
            normal = baseColor;
            hover  = new Color(baseColor.r * 0.6f, baseColor.g * 0.6f, baseColor.b * 0.6f, baseColor.a);
        }

        private void ShowResults()
        {
            if (comboCount > bestCombo) bestCombo = comboCount;

            MassageRank rank = CalculateRank();
            StrokeRating avgRating = ComputeAverageStrokeRating();

            // Persist progression *before* drawing the panel so that any UI that reads
            // the saved level (e.g. for "Level X complete!" labels) sees the new value.
            ApplyProgression(avgRating);

            if (resultsPanel != null) resultsPanel.SetActive(true);
            if (countdownText != null) countdownText.gameObject.SetActive(false);

            if (rankText != null)
            {
                rankText.text = rank.ToString();
                rankText.color = GetRankColor(rank);
            }

            if (finalScoreText != null)
                finalScoreText.text = $"<size=150>Score:</size>\n{totalScore}";

            if (breakdownText != null)
            {
                string avgWord = AverageRatingWord(avgRating);
                Color avgColor = GetStrokeRatingColor(avgRating);
                string avgHex  = ColorUtility.ToHtmlStringRGB(avgColor);

                breakdownText.text =
                    $"Average: <color=#{avgHex}>{avgWord}</color>\n" +
                    $"Total Strokes: {totalStrokes}\n" +
                    $"Perfect: {perfectCount}\n" +
                    $"Good:    {goodCount}\n" +
                    $"OK:      {okCount}\n" +
                    $"Miss:    {missCount}\n" +
                    $"Best Combo: {bestCombo}" +
                    (sandboxMode ? "\n<i>(sandbox)</i>" : "");
            }

            // Debug.Log($"[MassageMinigame] Round over — Score:{totalScore} Rank:{rank} Avg:{avgRating} " +
            //           $"Strokes:{totalStrokes} P:{perfectCount} G:{goodCount} O:{okCount} M:{missCount} Combo:{bestCombo}");
        }

        private static string AverageRatingWord(StrokeRating r)
        {
            switch (r)
            {
                case StrokeRating.Perfect: return "PERFECT";
                case StrokeRating.Good:    return "GOOD";
                case StrokeRating.OK:      return "OK";
                default:                   return "MISS";
            }
        }

        private static Color GetStrokeRatingColor(StrokeRating r)
        {
            switch (r)
            {
                case StrokeRating.Perfect: return new Color(1f, 0.9f, 0.2f);
                case StrokeRating.Good:    return new Color(0.3f, 1f, 0.5f);
                case StrokeRating.OK:      return new Color(0.5f, 0.8f, 1f);
                default:                   return new Color(1f, 0.3f, 0.3f);
            }
        }

        private MassageRank CalculateRank()
        {
            if (totalStrokes <= 0) return MassageRank.D;

            // Average points per stroke, normalized to perfect
            float avgNorm = (float)totalScore / (totalStrokes * pointsPerfect);

            if (avgNorm >= rankS) return MassageRank.S;
            if (avgNorm >= rankA) return MassageRank.A;
            if (avgNorm >= rankB) return MassageRank.B;
            if (avgNorm >= rankC) return MassageRank.C;
            return MassageRank.D;
        }

        private Color GetRankColor(MassageRank rank)
        {
            switch (rank)
            {
                case MassageRank.S: return new Color(1f, 0.85f, 0.1f);
                case MassageRank.A: return new Color(0.3f, 1f, 0.4f);
                case MassageRank.B: return new Color(0.3f, 0.7f, 1f);
                case MassageRank.C: return new Color(0.8f, 0.5f, 1f);
                default:            return new Color(0.6f, 0.6f, 0.6f);
            }
        }

        private void ResetScoring()
        {
            totalScore = 0;
            totalStrokes = 0;
            perfectCount = 0;
            goodCount = 0;
            okCount = 0;
            missCount = 0;
            comboCount = 0;
            bestCombo = 0;
            activePattern = null;
            activeSegmentIndex = 0;
            segmentStrokesRequired = 0;
            segmentStrokesCompleted = 0;
            patternsCompleted = 0;
        }

        // ══════════════════════════════════════════════════════════
        //  CHARACTER & LEVEL PROGRESSION
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Reads the proxy variable telling us which character to show, then picks a variant
        /// based on per-character SaveManager progression. Disables every other character
        /// (and every other variant under the active character) so only one is visible.
        /// </summary>
        private void PickActiveCharacterAndVariant()
        {
            activeCharacterRoot = null;
            activeVariant       = null;
            activeVariantIndex  = 0;
            activeVariantCount  = 0;
            sandboxMode         = false;

            Transform charactersRoot = FindDeep(transform, "Characters");
            if (charactersRoot == null)
            {
                Debug.LogWarning("[MassageMinigame] No 'Characters' child found — level/progression disabled.");
                return;
            }

            // Resolve target character name.
            string charName = Core.GetProxyVariableString(PROXY_CHAR_KEY, "");
            if (string.IsNullOrEmpty(charName)) charName = "Anis";

            // Activate only the matching character; deactivate the rest.
            for (int i = 0; i < charactersRoot.childCount; i++)
            {
                Transform child = charactersRoot.GetChild(i);
                bool match = string.Equals(child.name, charName, StringComparison.OrdinalIgnoreCase);
                child.gameObject.SetActive(match);
                if (match) activeCharacterRoot = child.gameObject;
            }

            // Fallback: if the requested character doesn't exist, use the first child.
            if (activeCharacterRoot == null && charactersRoot.childCount > 0)
            {
                activeCharacterRoot = charactersRoot.GetChild(0).gameObject;
                activeCharacterRoot.SetActive(true);
                Debug.LogWarning($"[MassageMinigame] Character '{charName}' not found — falling back to '{activeCharacterRoot.name}'.");
            }

            if (activeCharacterRoot == null) return;
            activeCharacterName = activeCharacterRoot.name;

            // Pick a variant based on saved level.
            activeVariantCount = activeCharacterRoot.transform.childCount;
            if (activeVariantCount == 0)
            {
                Debug.LogWarning($"[MassageMinigame] Character '{activeCharacterName}' has no variant children.");
                return;
            }

            int level = SaveManager.GetInt(LevelKey(activeCharacterName), 0);
            sandboxMode = level >= activeVariantCount;
            activeVariantIndex = sandboxMode
                ? UnityEngine.Random.Range(0, activeVariantCount)
                : Mathf.Clamp(level, 0, activeVariantCount - 1);

            for (int i = 0; i < activeVariantCount; i++)
            {
                Transform child = activeCharacterRoot.transform.GetChild(i);
                bool match = i == activeVariantIndex;
                child.gameObject.SetActive(match);
                if (match) activeVariant = child.gameObject;
            }

            // Debug.Log($"[MassageMinigame] {activeCharacterName} — level {level}/{activeVariantCount}, " +
            //           $"sandbox={sandboxMode}, variant='{activeVariant?.name}' (idx {activeVariantIndex})");
        }

        /// <summary>
        /// Ensures all mod-authored scripts exist on the hand and variant GOs (they can't
        /// survive asset bundle serialization since they live in a different assembly at
        /// runtime vs build time), then wires them to the active variant.
        /// </summary>
        private void WireHandsToActiveVariant()
        {
            if (activeVariant == null) return;
            SpriteRenderer targetSR = activeVariant.GetComponent<SpriteRenderer>();
            if (targetSR == null) return;

            Transform handLeft  = FindDeep(transform, "HandLeft");
            Transform handRight = FindDeep(transform, "HandRight");

            ConfigureHand(handLeft,  isLeft: true,  targetSR);
            ConfigureHand(handRight, isLeft: false, targetSR);

            // Ensure SqueezeSpriteController exists on the active variant (drives shader uniforms)
            EnsureSqueezeSpriteController(activeVariant);

            // Wire BreastPhysics on variant children (BreastL → HandLeft, BreastR → HandRight)
            WireBreastPhysics(handLeft, handRight);
        }

        /// <summary>
        /// Ensures a SqueezeContourFollower and LotionTrail exist on the hand GO
        /// (AddComponent if missing due to bundle script stripping), then wires all fields.
        /// </summary>
        private void ConfigureHand(Transform hand, bool isLeft, SpriteRenderer target)
        {
            if (hand == null) return;

            // --- Ensure components exist (mod scripts don't survive bundle serialization) ---
            SqueezeContourFollower scf = hand.GetComponent<SqueezeContourFollower>();
            if (scf == null)
                scf = hand.gameObject.AddComponent<SqueezeContourFollower>();

            LotionTrail lt = hand.GetComponent<LotionTrail>();
            if (lt == null)
                lt = hand.gameObject.AddComponent<LotionTrail>();

            // --- LotionTrail material ---
            if (lt.trailMaterial == null && Core.minigameBundle != null)
                lt.trailMaterial = Core.minigameBundle.LoadAsset<Material>("LotionTrailMAT");

            // --- SCF: sprites from bundle ---
            if (scf.clickSprite == null && Core.minigameBundle != null)
                scf.clickSprite = Core.minigameBundle.LoadAsset<Sprite>("Hand squeeze");
            if (scf.defaultSprite == null && Core.minigameBundle != null)
                scf.defaultSprite = Core.minigameBundle.LoadAsset<Sprite>("Hand float");

            // --- SCF: edge settings ---
            scf.followLeftEdge = isLeft;
            scf.edgeOffset = isLeft ? -1.05f : -1.2f;

            // --- SCF: ownRenderer = direct child's SpriteRenderer (HandSprite) ---
            if (scf.ownRenderer == null && hand.childCount > 0)
                scf.ownRenderer = hand.GetChild(0).GetComponent<SpriteRenderer>();

            // --- SCF: clickActiveObject = grandchild GO (HandSprite (1)), activated on click ---
            if (scf.clickActiveObject == null && hand.childCount > 0)
            {
                Transform handSprite = hand.GetChild(0);
                if (handSprite.childCount > 0)
                    scf.clickActiveObject = handSprite.GetChild(0).gameObject;
            }

            // --- SCF: target + contour mode ---
            scf.targetSprite = target;

            // Determine best contour source: prefer MaskRedChannel if available, else SpriteAlphaEdge
            ContourSource bestSource = ContourSource.SpriteAlphaEdge;
            if (target != null && target.sharedMaterial != null)
            {
                Material mat = target.sharedMaterial;
                if (mat.HasProperty("_MaskTex"))
                {
                    Texture2D mask = mat.GetTexture("_MaskTex") as Texture2D;
                    if (mask != null)
                    {
                        try { mask.GetPixel(0, 0); bestSource = ContourSource.MaskRedChannel; }
                        catch { /* not readable, fall through to alpha edge */ }
                    }
                }
            }
            scf.contourSource = bestSource;

            // Clear cached mask so RefreshMask re-reads from the NEW target's material
            // (maskTexture is public; without clearing, CacheMaskData skips the lookup if it's non-null)
            scf.maskTexture = null;

            // Recache contour data from the new target
            scf.RefreshMask();
            scf.RefreshSpriteData();

            Debug.Log($"[MassageMinigame] ConfigureHand '{hand.name}': target={target?.gameObject.name}, " +
                      $"contourSource={bestSource}, followLeft={isLeft}, " +
                      $"ownRenderer={(scf.ownRenderer != null ? scf.ownRenderer.gameObject.name : "NULL")}, " +
                      $"clickObj={(scf.clickActiveObject != null ? scf.clickActiveObject.name : "NULL")}");
        }

        /// <summary>
        /// Adds SqueezeSpriteController to the active variant if missing.
        /// It auto-detects its own SpriteRenderer in Awake().
        /// </summary>
        private void EnsureSqueezeSpriteController(GameObject variant)
        {
            if (variant.GetComponent<SqueezeSpriteController>() == null)
                variant.AddComponent<SqueezeSpriteController>();
        }

        /// <summary>
        /// Finds or creates BreastPhysics on variant breast children, wires their
        /// contourFollower + handCollider to the matching hand (BreastL → HandLeft, BreastR → HandRight).
        /// </summary>
        private void WireBreastPhysics(Transform handLeft, Transform handRight)
        {
            if (activeVariant == null) return;

            // Look for children named BreastL / BreastR (with PolygonCollider2D = breast shape)
            PolygonCollider2D[] breastColliders = activeVariant.GetComponentsInChildren<PolygonCollider2D>(true);
            if (breastColliders == null || breastColliders.Length == 0) return;

            SqueezeContourFollower scfLeft  = handLeft  != null ? handLeft.GetComponent<SqueezeContourFollower>()  : null;
            SqueezeContourFollower scfRight = handRight != null ? handRight.GetComponent<SqueezeContourFollower>() : null;
            Collider2D colLeft  = handLeft  != null ? handLeft.GetComponent<Collider2D>()  : null;
            Collider2D colRight = handRight != null ? handRight.GetComponent<Collider2D>() : null;

            foreach (PolygonCollider2D polyCol in breastColliders)
            {
                // Only target children that look like breast GOs
                string goName = polyCol.gameObject.name;
                if (!goName.Contains("Breast")) continue;

                // Ensure BreastPhysics component exists
                BreastPhysics bp = polyCol.GetComponent<BreastPhysics>();
                if (bp == null)
                    bp = polyCol.gameObject.AddComponent<BreastPhysics>();

                // Determine side: "BreastL" → HandLeft, "BreastR" → HandRight
                bool isLeftBreast = goName.Contains("L") || goName.Contains("Left") || goName.Contains("left");

                if (isLeftBreast)
                {
                    bp.contourFollower = scfLeft;
                    bp.handCollider = colLeft;
                    bp.tiltInvert = false;
                    bp.tiltYShift = -0.02f;
                }
                else
                {
                    bp.contourFollower = scfRight;
                    bp.handCollider = colRight;
                    bp.tiltInvert = true;
                    bp.tiltYShift = 0.02f;
                }

                // These auto-detect in Start() but set here for re-entry safety
                if (bp.shapeCollider == null)
                    bp.shapeCollider = polyCol;
                if (bp.spriteRenderer == null)
                    bp.spriteRenderer = polyCol.GetComponent<SpriteRenderer>();

                Debug.Log($"[MassageMinigame] BreastPhysics '{goName}': " +
                          $"follower={(bp.contourFollower != null ? bp.contourFollower.gameObject.name : "NULL")}, " +
                          $"handCol={(bp.handCollider != null ? bp.handCollider.gameObject.name : "NULL")}");
            }
        }

        /// <summary>
        /// Computes a single overall stroke-rating from this round's counts.
        /// Maps Perfect=3, Good=2, OK=1, Miss=0; returns the band the average falls into.
        /// </summary>
        private StrokeRating ComputeAverageStrokeRating()
        {
            int total = perfectCount + goodCount + okCount + missCount;
            if (total <= 0) return StrokeRating.Miss;

            float avg = (perfectCount * 3f + goodCount * 2f + okCount * 1f) / total;
            if (avg >= 2.5f) return StrokeRating.Perfect;
            if (avg >= 1.5f) return StrokeRating.Good;
            if (avg >= 0.5f) return StrokeRating.OK;
            return StrokeRating.Miss;
        }

        /// <summary>
        /// Persists progression after a completed round.
        /// Outside sandbox mode, an average rating of Good or higher unlocks the next variant.
        /// </summary>
        private void ApplyProgression(StrokeRating averageRating)
        {
            if (string.IsNullOrEmpty(activeCharacterName)) return;

            // Highscore (per character) — handy for dialogue gating later.
            string hsKey = HighscoreKey(activeCharacterName);
            if (totalScore > SaveManager.GetInt(hsKey, 0))
                SaveManager.SetInt(hsKey, totalScore);

            // Legacy keys kept in sync so older code paths still see something sensible.
            SaveManager.SetBool("Minigame_Massage_Played", true);
            if (totalScore > SaveManager.GetInt("Minigame_Massage_Highscore", 0))
                SaveManager.SetInt("Minigame_Massage_Highscore", totalScore);

            if (sandboxMode) return;

            bool passed = averageRating == StrokeRating.Perfect || averageRating == StrokeRating.Good;
            if (!passed) return;

            // Only ever advance from the *current* level — re-running an already-completed
            // variant for fun shouldn't bump progression. New variants added later will pick
            // up automatically because activeVariantCount is read from sibling count.
            string levelKey = LevelKey(activeCharacterName);
            int storedLevel = SaveManager.GetInt(levelKey, 0);
            if (storedLevel == activeVariantIndex)
                SaveManager.SetInt(levelKey, storedLevel + 1);
        }

        private static string LevelKey(string charName)     => SAVE_LEVEL_KEY_PREFIX + charName + SAVE_LEVEL_KEY_SUFFIX;
        private static string HighscoreKey(string charName) => SAVE_HIGHSCORE_KEY_PREFIX + charName + SAVE_HIGHSCORE_KEY_SUFFIX;

        // ══════════════════════════════════════════════════════════
        //  TEXTURE PROGRESSION
        //
        //  Smoothly blends breast and body textures based on minigame
        //  progress. Per-variant: check variant name to decide which
        //  texture files to load. Add new variants by extending
        //  InitTextureProgression with another name check.
        // ══════════════════════════════════════════════════════════

        private class TextureSlot
        {
            public SpriteRenderer renderer;
            public Sprite originalSprite;
            public Color32[][] sourcePixels;
            public Texture2D mutableTex;
            public Color32[] blendBuffer;
            public int pixelCount;
        }

        private List<TextureSlot> textureSlots;
        private float textureDeadline;
        private float lastBlendProgress = -1f;

        private void InitTextureProgression()
        {
            textureSlots = null;
            lastBlendProgress = -1f;

            if (activeVariant == null) return;

            if (activeVariant.name != "MassageAnisNakedFront") return;

            string texPath = Core.exePath + Core.bustPath + "NIKKE\\Anis\\";
            textureDeadline = (maxPatterns - 0.5f) / maxPatterns;
            textureSlots = new List<TextureSlot>();

            string[] breastFiles = new[]
            {
                "AnisSwimMGMassageNakedFrontBL.png",
                "AnisSwimMGMassageNakedFrontBL1.png",
                "AnisSwimMGMassageNakedFrontBL2.png"
            };

            // Ensure the Blink child's BlinkingSprite component is active
            Transform blinkT = FindDeep(activeVariant.transform, "Blink");
            if (blinkT != null)
            {
                var blinkComp = blinkT.GetComponent<BlinkingSprite>();
                if (blinkComp != null)
                    blinkComp.enabled = true;
                else
                    Characters.AddBlinkingSpriteToBlinkObjects(activeVariant);
            }

            string[] breastRFiles = new[]
            {
                "AnisSwimMGMassageNakedFrontBR.png",
                "AnisSwimMGMassageNakedFrontBR1.png",
                "AnisSwimMGMassageNakedFrontBR2.png"
            };

            foreach (string breastName in new[] { "BreastL", "BreastR" })
            {
                Transform breastT = FindDeep(activeVariant.transform, breastName);
                if (breastT == null) continue;
                SpriteRenderer sr = breastT.GetComponent<SpriteRenderer>();
                if (sr == null || sr.sprite == null) continue;

                string[] files = breastName == "BreastL" ? breastFiles : breastRFiles;
                TextureSlot slot = CreateTextureSlot(sr, texPath, files);
                if (slot != null) textureSlots.Add(slot);
            }

            string[] baseFiles = new[]
            {
                "AnisSwimMGMassageNakedFrontBase.png",
                "AnisSwimMGMassageNakedFrontBase1.png"
            };

            SpriteRenderer baseSR = activeVariant.GetComponent<SpriteRenderer>();
            if (baseSR != null && baseSR.sprite != null)
            {
                TextureSlot baseSlot = CreateTextureSlot(baseSR, texPath, baseFiles);
                if (baseSlot != null) textureSlots.Add(baseSlot);
            }

            if (textureSlots.Count == 0)
                textureSlots = null;
            else
                Debug.Log($"[MassageMinigame] TextureProgression: {textureSlots.Count} slot(s), deadline={textureDeadline:F3}");
        }

        private TextureSlot CreateTextureSlot(SpriteRenderer sr, string basePath, string[] files)
        {
            Color32[][] sourcePixels = new Color32[files.Length][];
            int w = 0, h = 0;

            for (int i = 0; i < files.Length; i++)
            {
                string fullPath = basePath + files[i];
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"[MassageMinigame] Texture progression file not found: {fullPath}");
                    return null;
                }

                byte[] rawData = File.ReadAllBytes(fullPath);
                Texture2D tmp = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tmp.LoadImage(rawData);

                if (i == 0) { w = tmp.width; h = tmp.height; }
                else if (tmp.width != w || tmp.height != h)
                {
                    Debug.LogWarning($"[MassageMinigame] Texture size mismatch: {files[0]} is {w}x{h}, {files[i]} is {tmp.width}x{tmp.height}");
                    Destroy(tmp);
                    return null;
                }

                sourcePixels[i] = tmp.GetPixels32();
                Destroy(tmp);
            }

            Texture2D mutableTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            mutableTex.filterMode = FilterMode.Bilinear;
            Color32[] blendBuffer = new Color32[w * h];
            System.Array.Copy(sourcePixels[0], blendBuffer, blendBuffer.Length);
            mutableTex.SetPixels32(blendBuffer);
            mutableTex.Apply(false);

            Sprite original = sr.sprite;
            float ppu = original.pixelsPerUnit;
            Vector2 pivot = new Vector2(
                original.pivot.x / original.rect.width,
                original.pivot.y / original.rect.height);

            Sprite newSprite = Sprite.Create(mutableTex, new Rect(0, 0, w, h), pivot, ppu);
            sr.sprite = newSprite;

            return new TextureSlot
            {
                renderer = sr,
                originalSprite = original,
                sourcePixels = sourcePixels,
                mutableTex = mutableTex,
                blendBuffer = blendBuffer,
                pixelCount = blendBuffer.Length
            };
        }

        private void UpdateTextureProgression()
        {
            if (textureSlots == null) return;

            float progress = ComputeOverallProgress();
            if (Mathf.Abs(progress - lastBlendProgress) < 0.001f) return;
            lastBlendProgress = progress;

            foreach (TextureSlot slot in textureSlots)
            {
                float normalized = Mathf.Clamp01(progress / textureDeadline);
                BlendSlot(slot, normalized);
            }
        }

        private float ComputeOverallProgress()
        {
            if (maxPatterns <= 0) return 0f;

            float segFraction = 0f;
            if (activePattern != null && activePattern.segments != null && activePattern.segments.Length > 0)
            {
                float strokeFrac = segmentStrokesRequired > 0
                    ? (float)segmentStrokesCompleted / segmentStrokesRequired
                    : 0f;
                segFraction = (activeSegmentIndex + strokeFrac) / activePattern.segments.Length;
            }

            return Mathf.Max(0f, (patternsCompleted - 1 + segFraction) / maxPatterns);
        }

        private static void BlendSlot(TextureSlot slot, float normalizedProgress)
        {
            int stages = slot.sourcePixels.Length;
            if (stages < 2) return;

            Color32[] from, to;
            float t;

            if (stages == 2)
            {
                from = slot.sourcePixels[0];
                to   = slot.sourcePixels[1];
                t    = normalizedProgress;
            }
            else
            {
                float segLen = 1f / (stages - 1);
                int seg = Mathf.Min((int)(normalizedProgress / segLen), stages - 2);
                t    = Mathf.Clamp01((normalizedProgress - seg * segLen) / segLen);
                from = slot.sourcePixels[seg];
                to   = slot.sourcePixels[seg + 1];
            }

            for (int i = 0; i < slot.pixelCount; i++)
                slot.blendBuffer[i] = Color32.Lerp(from[i], to[i], t);

            slot.mutableTex.SetPixels32(slot.blendBuffer);
            slot.mutableTex.Apply(false);
        }

        private void CleanupTextureProgression()
        {
            if (textureSlots == null) return;

            foreach (TextureSlot slot in textureSlots)
            {
                if (slot.renderer != null && slot.originalSprite != null)
                    slot.renderer.sprite = slot.originalSprite;
                if (slot.mutableTex != null) Destroy(slot.mutableTex);
            }

            textureSlots = null;
            lastBlendProgress = -1f;
        }

        // ══════════════════════════════════════════════════════════
        //  ZONE & CURSOR VISUALS
        // ══════════════════════════════════════════════════════════

        private void UpdateZoneVisuals(bool buttonHeld, float normalizedY)
        {
            if (zoneImages == null) return;

            MassagePatternSegment seg = GetActiveSegment();
            int activeZone = seg != null ? seg.zone : -1;

            // Tick the shake timer
            if (zoneShakeTimer > 0f)
                zoneShakeTimer = Mathf.Max(0f, zoneShakeTimer - Time.deltaTime);

            // Compute shake X offset for the active zone
            float shakeX = 0f;
            if (zoneShakeTimer > 0f)
            {
                float phase = (ZONE_SHAKE_TIME - zoneShakeTimer) * 45f;
                shakeX = Mathf.Sin(phase) * 14f * (zoneShakeTimer / ZONE_SHAKE_TIME);
            }

            for (int i = 0; i < zoneImages.Length; i++)
            {
                if (zoneImages[i] == null) continue;

                // Non-active zones are invisible
                if (i != activeZone)
                {
                    // Keep visible during a flash, then hide once it expires
                    if (i == zoneFlashIndex && zoneFlashTimer > 0f) continue;
                    zoneImages[i].enabled = false;
                    continue;
                }

                // Active zone — always visible
                zoneImages[i].enabled = true;

                // Apply shake offset on the active zone's RectTransform
                var rt = zoneImages[i].rectTransform;
                var ap = rt.anchoredPosition;
                ap.x = (i == activeZone) ? shakeX : 0f;
                rt.anchoredPosition = ap;

                if (i == zoneFlashIndex && zoneFlashTimer > 0f) continue;

                bool validStroke = buttonHeld && IsInZone(normalizedY, activeZone) && clickStartedInZone;
                bool badClick    = buttonHeld && !clickStartedInZone;

                if (badClick)
                    zoneImages[i].color = zoneMissColor;
                else if (validStroke)
                    zoneImages[i].color = zoneStrokingColor;
                else
                    zoneImages[i].color = zoneActiveColor;
            }
        }

        /// <summary>
        /// Returns the normalized Y bounds (0–1 within play area) for <paramref name="index"/>.
        /// Uses <see cref="zoneDefs"/> when available; falls back to equal strips.
        /// </summary>
        private ZoneDefinition GetZoneDef(int index)
        {
            if (zoneDefs != null && index >= 0 && index < zoneDefs.Length)
                return zoneDefs[index];

            float strip = 1f / Mathf.Max(1, zoneCount);
            return new ZoneDefinition { yMin = index * strip, yMax = (index + 1) * strip };
        }

        /// <summary>
        /// Returns true if <paramref name="normY"/> (0–1 in play area) is inside
        /// the bounds of <paramref name="zoneIndex"/>. Supports overlapping zones.
        /// </summary>
        private bool IsInZone(float normY, int zoneIndex)
        {
            ZoneDefinition def = GetZoneDef(zoneIndex);
            return normY >= def.yMin && normY <= def.yMax;
        }

        private void TriggerZoneFlash(int zone, Color color)
        {
            zoneFlashIndex = zone;
            zoneFlashColor = color;
            zoneFlashTimer = ZONE_FLASH_TIME;
        }

        private void UpdateZoneFlash(float dt)
        {
            if (zoneFlashTimer <= 0f) return;
            zoneFlashTimer -= dt;

            if (zoneFlashIndex >= 0 && zoneFlashIndex < zoneImages.Length && zoneImages[zoneFlashIndex] != null)
            {
                zoneImages[zoneFlashIndex].enabled = true;
                float t = Mathf.Clamp01(zoneFlashTimer / ZONE_FLASH_TIME);
                zoneImages[zoneFlashIndex].color = Color.Lerp(zoneActiveColor, zoneFlashColor, t);
            }

            if (zoneFlashTimer <= 0f)
                zoneFlashIndex = -1;
        }

        private void SetGameplayVisible(bool visible)
        {
            if (playAreaPanel      != null) playAreaPanel.gameObject.SetActive(visible);
            if (patternProgressText != null) patternProgressText.gameObject.SetActive(visible);
            if (strokeProgressText  != null) strokeProgressText.gameObject.SetActive(visible);
            if (scoreText           != null) scoreText.gameObject.SetActive(visible);
            if (comboText           != null) comboText.gameObject.SetActive(visible);
            if (patternNameText     != null) patternNameText.gameObject.SetActive(visible);
            if (speedHintText       != null) speedHintText.gameObject.SetActive(visible);
            if (speedBar            != null) speedBar.gameObject.SetActive(visible);
            if (strokeRatingText    != null) strokeRatingText.gameObject.SetActive(false); // always off until shown
        }

        private void ClearAllZoneHighlights()
        {
            if (zoneImages == null) return;
            for (int i = 0; i < zoneImages.Length; i++)
                if (zoneImages[i] != null) zoneImages[i].enabled = false;
        }

        private void UpdateCursorVisual(float normalizedY, bool buttonHeld)
        {
            if (cursorIndicator == null || playAreaPanel == null) return;

            // Convert normalizedY back to a local-space Y inside PlayArea
            Rect rect = playAreaPanel.rect;
            float localY = Mathf.Lerp(rect.yMin, rect.yMax, normalizedY);

            // Place the cursor in the PlayArea's local space, then convert to world
            Vector3 worldPos = playAreaPanel.TransformPoint(
                new Vector3(rect.center.x, localY, 0f));

            cursorIndicator.position = worldPos;

            // Dim cursor when button is not held
            if (cursorImage != null)
                cursorImage.color = buttonHeld ? cursorColor : cursorDimColor;
        }

        // ──────────────────────────────────────────────────────────
        //  SPEED INDICATOR
        // ──────────────────────────────────────────────────────────

        private void UpdateSpeedHint(MassagePatternSegment seg)
        {
            if (speedHintText == null) return;
            // Show direction arrow for the resolved (possibly randomised) direction
            speedHintText.text = segmentDirection == 1 ? "\u2191" : "\u2193"; // ↑ or ↓
        }

        private void UpdateSpeedBar()
        {
            if (speedBarFill == null || speedBarFillImage == null) return;

            MassagePatternSegment seg = GetActiveSegment();
            if (seg == null)
            {
                speedBarFill.anchorMin = Vector2.zero;
                speedBarFill.anchorMax = new Vector2(1f, 0f);
                speedBarFill.offsetMin = Vector2.zero;
                speedBarFill.offsetMax = Vector2.zero;
                return;
            }

            // Triangle fill: rises to 1.0 at idealTime, then falls back if too slow.
            // ratio = 1 - |elapsed - idealTime| / idealTime
            ZoneDefinition zone = GetZoneDef(seg.zone);
            float zoneHeight = zone.yMax - zone.yMin;
            float idealTime = (zoneHeight > 0f && slowSpeed > 0f)
                ? zoneHeight / slowSpeed
                : 1f;

            float ratio = 0f;
            if (strokeDirection != 0 && idealTime > 0f)
                ratio = Mathf.Clamp01(1f - Mathf.Abs(strokeTime - idealTime) / idealTime);

            speedBarFill.anchorMin = Vector2.zero;
            speedBarFill.anchorMax = new Vector2(1f, ratio);
            speedBarFill.offsetMin = Vector2.zero;
            speedBarFill.offsetMax = Vector2.zero;

            // Color mirrors scoring thresholds (green = Perfect, yellow = Good, red = Miss)
            if (ratio >= 0.85f)
                speedBarFillImage.color = new Color(0.2f, 1f, 0.4f);   // green  — Perfect
            else if (ratio >= 0.70f)
                speedBarFillImage.color = new Color(1f, 0.9f, 0.2f);   // yellow — Good
            else if (ratio >= 0.50f)
                speedBarFillImage.color = new Color(0.5f, 0.7f, 1f);   // blue   — OK
            else
                speedBarFillImage.color = new Color(1f, 0.35f, 0.25f); // red    — Miss
        }

        // ──────────────────────────────────────────────────────────
        //  STROKE RATING DISPLAY
        // ──────────────────────────────────────────────────────────

        private void ShowStrokeRating(StrokeRating rating)
        {
            if (strokeRatingText == null) return;

            strokeRatingText.gameObject.SetActive(true);
            ratingDisplayTimer = RATING_DISPLAY_TIME;

            switch (rating)
            {
                case StrokeRating.Perfect:
                    strokeRatingText.text = "PERFECT!";
                    strokeRatingText.color = new Color(1f, 0.9f, 0.2f);
                    break;
                case StrokeRating.Good:
                    strokeRatingText.text = "GOOD";
                    strokeRatingText.color = new Color(0.3f, 1f, 0.5f);
                    break;
                case StrokeRating.OK:
                    strokeRatingText.text = "OK";
                    strokeRatingText.color = new Color(0.5f, 0.8f, 1f);
                    break;
                case StrokeRating.Miss:
                    strokeRatingText.text = "MISS";
                    strokeRatingText.color = new Color(1f, 0.3f, 0.3f);
                    break;
            }
        }

        private void UpdateRatingDisplay(float dt)
        {
            if (strokeRatingText == null || !strokeRatingText.gameObject.activeSelf) return;

            ratingDisplayTimer -= dt;
            if (ratingDisplayTimer <= 0f)
            {
                strokeRatingText.gameObject.SetActive(false);
            }
            else
            {
                Color c = strokeRatingText.color;
                c.a = Mathf.Clamp01(ratingDisplayTimer / RATING_DISPLAY_TIME);
                strokeRatingText.color = c;
            }
        }

        // ══════════════════════════════════════════════════════════
        //  UI CREATION
        // ══════════════════════════════════════════════════════════

        private void CreateUI()
        {
            DestroyUI();

            // ── Try wiring from existing children first ────────────────────────
            WireFromChildren();

            // If a PlayArea was found we have enough to run; skip procedural.
            if (playAreaPanel != null)
            {
                _proceduralUI = false;
                // Debug.Log("[MassageMinigame] UI wired from children.");
                return;
            }

            // ── No children found — build the UI procedurally ──────────────
            _proceduralUI = true;
            // Debug.Log("[MassageMinigame] No 'PlayArea' child found — building procedural UI.");
            uiRoot = new GameObject("MassageMinigame_UI");
            uiCanvas = uiRoot.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 100;
            var scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            uiRoot.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = uiRoot.GetComponent<RectTransform>();

            // ── Dim overlay ──────────────────────────────────────
            CreateUIImage(canvasRect, "Overlay",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.4f));

            // ── Play Area Panel ──────────────────────────────────
            var playArea = CreateUIImage(canvasRect, "PlayArea",
                new Vector2(playAreaNormalized.x, playAreaNormalized.y),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width,
                            playAreaNormalized.y + playAreaNormalized.height),
                Vector2.zero, Vector2.zero,
                panelBackground);
            playAreaPanel = playArea.GetComponent<RectTransform>();

            // ── Zone strips ──────────────────────────────────────
            // Bounds come from GetZoneDef so custom zoneDefs (including
            // overlapping ones) are respected automatically.
            zoneImages = new Image[zoneCount];

            for (int i = 0; i < zoneCount; i++)
            {
                ZoneDefinition def = GetZoneDef(i);

                var zoneObj = CreateUIImage(playAreaPanel, $"Zone_{i}",
                    new Vector2(0f, def.yMin), new Vector2(1f, def.yMax),
                    Vector2.zero, Vector2.zero,
                    zoneIdleColor);
                zoneImages[i] = zoneObj.GetComponent<Image>();
                zoneImages[i].enabled = false;

                if (zoneHighlightSprite != null)
                    zoneImages[i].sprite = zoneHighlightSprite;

                // Zone number label
                CreateUIText(zoneObj.GetComponent<RectTransform>(), $"Label_{i}",
                    new Vector2(0f, 0f), new Vector2(0.12f, 1f),
                    Vector2.zero, Vector2.zero,
                    $"{i + 1}", 18, TextAlignmentOptions.Center,
                    new Color(1f, 1f, 1f, 0.3f));
            }

            // ── Cursor indicator bar ─────────────────────────────
            var cursorObj = CreateUIImage(playAreaPanel, "Cursor",
                Vector2.zero, Vector2.zero,
                Vector2.zero, Vector2.zero,
                cursorDimColor);
            cursorIndicator = cursorObj.GetComponent<RectTransform>();
            cursorIndicator.anchorMin = new Vector2(0f, 0f);
            cursorIndicator.anchorMax = new Vector2(1f, 0f);
            cursorIndicator.sizeDelta = new Vector2(0f, 4f);
            cursorImage = cursorObj.GetComponent<Image>();
            if (cursorSprite != null) cursorImage.sprite = cursorSprite;

            // ── HUD — above play area ────────────────────────────
            float hudTop = playAreaNormalized.y + playAreaNormalized.height;

            patternProgressText = CreateUIText(canvasRect, "PatternProgress",
                new Vector2(playAreaNormalized.x, hudTop + 0.01f),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width * 0.5f, hudTop + 0.07f),
                Vector2.zero, Vector2.zero,
                "Pattern 1 / 6", 28, TextAlignmentOptions.Left, Color.white)
                .GetComponent<TextMeshProUGUI>();

            scoreText = CreateUIText(canvasRect, "Score",
                new Vector2(playAreaNormalized.x + playAreaNormalized.width * 0.5f, hudTop + 0.01f),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width, hudTop + 0.07f),
                Vector2.zero, Vector2.zero,
                "0", 32, TextAlignmentOptions.Right, Color.white)
                .GetComponent<TextMeshProUGUI>();

            comboText = CreateUIText(canvasRect, "Combo",
                new Vector2(playAreaNormalized.x + playAreaNormalized.width * 0.5f, hudTop + 0.07f),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width, hudTop + 0.12f),
                Vector2.zero, Vector2.zero,
                "", 24, TextAlignmentOptions.Right, new Color(1f, 0.85f, 0.2f))
                .GetComponent<TextMeshProUGUI>();

            // ── HUD — below play area ────────────────────────────
            float hudBot = playAreaNormalized.y;

            patternNameText = CreateUIText(canvasRect, "PatternName",
                new Vector2(playAreaNormalized.x, hudBot - 0.06f),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width, hudBot - 0.01f),
                Vector2.zero, Vector2.zero,
                "", 22, TextAlignmentOptions.Center, new Color(0.7f, 0.85f, 1f))
                .GetComponent<TextMeshProUGUI>();

            speedHintText = CreateUIText(canvasRect, "SpeedHint",
                new Vector2(playAreaNormalized.x, hudBot - 0.11f),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width * 0.5f, hudBot - 0.06f),
                Vector2.zero, Vector2.zero,
                "", 20, TextAlignmentOptions.Left, new Color(0.9f, 0.75f, 0.4f))
                .GetComponent<TextMeshProUGUI>();

            strokeProgressText = CreateUIText(canvasRect, "StrokeProgress",
                new Vector2(playAreaNormalized.x, hudBot - 0.17f),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width * 0.5f, hudBot - 0.11f),
                Vector2.zero, Vector2.zero,
                "0 / 0 strokes", 18, TextAlignmentOptions.Left, new Color(0.75f, 0.75f, 0.9f))
                .GetComponent<TextMeshProUGUI>();

            // ── Speed bar (below play area, right side) ──────────
            var speedBarBg = CreateUIImage(canvasRect, "SpeedBarBg",
                new Vector2(playAreaNormalized.x + playAreaNormalized.width * 0.55f, hudBot - 0.10f),
                new Vector2(playAreaNormalized.x + playAreaNormalized.width, hudBot - 0.07f),
                Vector2.zero, Vector2.zero,
                new Color(0.15f, 0.15f, 0.2f, 0.8f));
            speedBar = speedBarBg.GetComponent<RectTransform>();

            var fill = CreateUIImage(speedBar, "SpeedBarFill",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                Vector2.zero, Vector2.zero,
                new Color(0.2f, 1f, 0.4f));
            speedBarFill = fill.GetComponent<RectTransform>();
            speedBarFillImage = fill.GetComponent<Image>();

            // Target marker on speed bar (at y = 1.0 = perfect speed)
            CreateUIImage(speedBar, "SpeedTarget",
                new Vector2(0f, 0.99f), new Vector2(1f, 1.01f),
                Vector2.zero, Vector2.zero,
                new Color(1f, 1f, 1f, 0.5f));

            // ── Stroke rating text (center of play area) ─────────
            strokeRatingText = CreateUIText(playAreaPanel, "StrokeRating",
                new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.65f),
                Vector2.zero, Vector2.zero,
                "", 48, TextAlignmentOptions.Center, Color.white)
                .GetComponent<TextMeshProUGUI>();
            strokeRatingText.gameObject.SetActive(false);

            // ── Countdown text (center screen) ───────────────────
            countdownText = CreateUIText(canvasRect, "Countdown",
                new Vector2(0.3f, 0.35f), new Vector2(0.7f, 0.65f),
                Vector2.zero, Vector2.zero,
                "3", 96, TextAlignmentOptions.Center, Color.white)
                .GetComponent<TextMeshProUGUI>();

            // ── Results panel ────────────────────────────────────
            resultsPanel = CreateUIImage(canvasRect, "Results",
                new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.85f),
                Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.05f, 0.12f, 0.92f));
            var rr = resultsPanel.GetComponent<RectTransform>();

            rankText = CreateUIText(rr, "Rank",
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.95f),
                Vector2.zero, Vector2.zero,
                "S", 120, TextAlignmentOptions.Center, Color.yellow)
                .GetComponent<TextMeshProUGUI>();

            finalScoreText = CreateUIText(rr, "FinalScore",
                new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.55f),
                Vector2.zero, Vector2.zero,
                "Score: 0", 36, TextAlignmentOptions.Center, Color.white)
                .GetComponent<TextMeshProUGUI>();

            breakdownText = CreateUIText(rr, "Breakdown",
                new Vector2(0.15f, 0.05f), new Vector2(0.85f, 0.4f),
                Vector2.zero, Vector2.zero,
                "", 22, TextAlignmentOptions.Left, new Color(0.8f, 0.8f, 0.9f))
                .GetComponent<TextMeshProUGUI>();

            var closeBtn = CreateUIImage(rr, "CloseButton",
                new Vector2(0.2f, -0.14f), new Vector2(0.8f, -0.02f),
                Vector2.zero, Vector2.zero,
                new Color(0.7f, 0.15f, 0.15f, 0.9f));
            closeButtonRect = closeBtn.GetComponent<RectTransform>();
            closeButtonImage = closeBtn.GetComponent<Image>();
            if (closeButtonSprite != null) closeBtn.GetComponent<Image>().sprite = closeButtonSprite;
            SetButtonColors(ref closeButtonNormal, ref closeButtonHover, closeButtonImage.color);
            CreateUIText(closeButtonRect, "CloseButtonText",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "CLOSE", 38, TextAlignmentOptions.Center, Color.white);

            resultsPanel.SetActive(false);

            // ── Start menu panel ─────────────────────────────────
            var smGo = CreateUIImage(canvasRect, "StartMenu",
                new Vector2(0.25f, 0.2f), new Vector2(0.75f, 0.8f),
                Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.05f, 0.12f, 0.92f));
            var smRt = smGo.GetComponent<RectTransform>();
            if (startMenuSprite != null) smGo.GetComponent<Image>().sprite = startMenuSprite;

            CreateUIText(smRt, "StartTitle",
                new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.95f),
                Vector2.zero, Vector2.zero,
                "Massage", 72, TextAlignmentOptions.Center, Color.white);

            var startBtn = CreateUIImage(smRt, "StartButton",
                new Vector2(0.25f, 0.12f), new Vector2(0.75f, 0.42f),
                Vector2.zero, Vector2.zero,
                new Color(0.2f, 0.6f, 0.3f, 0.9f));
            startMenuPanel   = smGo;
            startButtonRect  = startBtn.GetComponent<RectTransform>();
            startButtonImage = startBtn.GetComponent<Image>();
            SetButtonColors(ref startButtonNormal, ref startButtonHover, startButtonImage.color);

            CreateUIText(startButtonRect, "StartButtonText",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "START", 48, TextAlignmentOptions.Center, Color.white);

            startMenuPanel.SetActive(false);
        }

        // ──────────────────────────────────────────────────────────
        //  CHILD-BASED UI WIRING
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Searches this GameObject's own hierarchy for children with the
        /// expected names and wires all UI references from whatever it finds.
        /// Missing children are silently skipped here; callers decide whether
        /// to fall back to procedural creation.
        /// <br/><br/>
        /// Expected names: PlayArea, Zone_0…Zone_{zoneCount-1}, Cursor,
        /// PatternProgress, StrokeProgress, Score, Combo, PatternName,
        /// SpeedHint, SpeedBarFill, StrokeRating, Countdown,
        /// Results (containing Rank, FinalScore, Breakdown).
        /// </summary>
        private void WireFromChildren()
        {
            Transform root = transform;

            uiCanvas = GetComponent<Canvas>();
            if (uiCanvas == null)
                uiCanvas = GetComponentInChildren<Canvas>(true);

            // ── Play area ────────────────────────────────────────
            Transform playAreaT = FindDeep(root, "PlayArea");
            if (playAreaT != null)
                playAreaPanel = playAreaT.GetComponent<RectTransform>();

            // ── Zone images ──────────────────────────────────────
            // Auto-detect how many Zone_N children exist and derive
            // their bounds from their actual world-space positions
            // relative to PlayArea.  This overrides zoneCount and
            // zoneDefs so they always match the prefab layout.
            // Works regardless of anchor mode, offsets, or nesting.
            var foundZones = new List<Image>();
            var foundDefs  = new List<ZoneDefinition>();

            // PlayArea world corners for normalisation
            Vector3[] paCorners = new Vector3[4];
            if (playAreaPanel != null)
                playAreaPanel.GetWorldCorners(paCorners);
            float paBottom = paCorners[0].y;
            float paTop    = paCorners[1].y;
            float paHeight = paTop - paBottom;

            for (int i = 0; ; i++)
            {
                Transform zt = FindDeep(root, $"Zone_{i}");
                if (zt == null) break;

                Image img = zt.GetComponent<Image>();
                foundZones.Add(img);

                // Derive zone bounds from world corners
                RectTransform zrt = zt.GetComponent<RectTransform>();
                float yMin = 0f, yMax = 1f;
                if (zrt != null && paHeight > 0f)
                {
                    Vector3[] zCorners = new Vector3[4];
                    zrt.GetWorldCorners(zCorners);
                    // GetWorldCorners: 0=bottomLeft, 1=topLeft
                    yMin = Mathf.Clamp01((zCorners[0].y - paBottom) / paHeight);
                    yMax = Mathf.Clamp01((zCorners[1].y - paBottom) / paHeight);
                }
                foundDefs.Add(new ZoneDefinition { yMin = yMin, yMax = yMax });

                if (img != null) img.enabled = false;
            }

            zoneCount  = foundZones.Count;
            zoneImages = foundZones.ToArray();
            zoneDefs   = foundDefs.ToArray();

            // Debug.Log($"[MassageMinigame] Wired {zoneCount} zone(s) from children.");
            // for (int i = 0; i < zoneCount; i++)
            //     Debug.Log($"[MassageMinigame]   Zone_{i}: yMin={zoneDefs[i].yMin:F3} yMax={zoneDefs[i].yMax:F3}");

            // ── Cursor ───────────────────────────────────────────
            Transform cursorT = FindDeep(root, "Cursor");
            if (cursorT != null)
            {
                cursorIndicator = cursorT.GetComponent<RectTransform>();
                cursorImage     = cursorT.GetComponent<Image>();
            }

            // ── TMP text labels ──────────────────────────────────
            patternProgressText = FindTMP(root, "PatternProgress");
            strokeProgressText  = FindTMP(root, "StrokeProgress");
            scoreText           = FindTMP(root, "Score");
            comboText           = FindTMP(root, "Combo");
            patternNameText     = FindTMP(root, "PatternName");
            countdownText       = FindTMP(root, "Countdown");
            strokeRatingText    = FindTMP(root, "StrokeRating");
            speedHintText       = FindTMP(root, "SpeedHint");

            // ── Speed bar fill ───────────────────────────────────
            Transform fillT = FindDeep(root, "SpeedBarFill");
            if (fillT != null)
            {
                speedBarFill      = fillT.GetComponent<RectTransform>();
                speedBarFillImage = fillT.GetComponent<Image>();
            }

            // ── Results panel ────────────────────────────────────
            Transform resultsT = FindDeep(root, "Results");
            if (resultsT != null)
            {
                resultsPanel   = resultsT.gameObject;
                rankText       = FindTMP(resultsT, "Rank");
                finalScoreText = FindTMP(resultsT, "FinalScore");
                breakdownText  = FindTMP(resultsT, "Breakdown");
                Transform closeBtnT = FindDeep(resultsT, "CloseButton");
                if (closeBtnT != null)
                {
                    closeButtonRect  = closeBtnT.GetComponent<RectTransform>();
                    closeButtonImage = closeBtnT.GetComponent<Image>();
                    if (closeButtonImage != null)
                        SetButtonColors(ref closeButtonNormal, ref closeButtonHover, closeButtonImage.color);
                }
            }

            // ── Start menu panel ─────────────────────────────────
            Transform startMenuT = FindDeep(root, "StartMenu");
            if (startMenuT != null)
            {
                startMenuPanel = startMenuT.gameObject;
                Transform startBtnT = FindDeep(startMenuT, "StartButton");
                if (startBtnT != null)
                {
                    startButtonRect  = startBtnT.GetComponent<RectTransform>();
                    startButtonImage = startBtnT.GetComponent<Image>();
                    if (startButtonImage != null)
                        SetButtonColors(ref startButtonNormal, ref startButtonHover, startButtonImage.color);
                }
            }

            // ── Initial visibility state ────────────────────────────
            if (strokeRatingText != null) strokeRatingText.gameObject.SetActive(false);
            if (resultsPanel     != null) resultsPanel.SetActive(false);
            if (startMenuPanel   != null) startMenuPanel.SetActive(false);
            if (countdownText    != null) countdownText.gameObject.SetActive(false);
        }

        /// <summary>Depth-first search for a child Transform by exact name.</summary>
        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        /// <summary>Finds a child by name and returns its TextMeshProUGUI component.</summary>
        private static TextMeshProUGUI FindTMP(Transform root, string name)
        {
            Transform t = FindDeep(root, name);
            if (t == null) return null;
            return t.GetComponent<TextMeshProUGUI>();
        }

        private void DestroyUI()
        {
            // Only destroy the root GameObject when we created it ourselves.
            // If the UI came from pre-existing children we must not destroy them.
            if (_proceduralUI && uiRoot != null) { Destroy(uiRoot); }
            uiRoot = null;
            _proceduralUI = false;

            zoneImages = null;
            cursorIndicator = null;
            cursorImage = null;
            playAreaPanel = null;
            patternProgressText = null;
            strokeProgressText = null;
            scoreText = null;
            comboText = null;
            patternNameText = null;
            speedHintText = null;
            speedBar = null;
            speedBarFill = null;
            speedBarFillImage = null;
            countdownText = null;
            strokeRatingText = null;
            resultsPanel = null;
            rankText = null;
            finalScoreText = null;
            breakdownText = null;
            startMenuPanel = null;
            startButtonRect = null;
            startButtonImage = null;
            closeButtonRect = null;
            closeButtonImage = null;
        }

        // ══════════════════════════════════════════════════════════
        //  UI HELPERS
        // ══════════════════════════════════════════════════════════

        private GameObject CreateUIImage(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private GameObject CreateUIText(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax,
            string text, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return go;
        }
    }
}
