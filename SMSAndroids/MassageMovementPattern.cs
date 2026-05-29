using System;
using UnityEngine;

namespace SMSAndroidsCore
{
    // ──────────────────────────────────────────────────────────────
    //  Data structures for massage minigame movement patterns.
    //  Patterns are authored as JSON files and loaded from disk.
    //  Visual assets (sprites, etc.) come from the minigamebundle.
    //
    //  Gameplay flow:
    //    A pattern is a named sequence of "segments".
    //    Each segment activates a zone and requires a random number of
    //    stroke attempts (between minStrokes and maxStrokes) to complete.
    //    While the zone is active the player must:
    //      1. Move the mouse into that zone.
    //      2. Hold the mouse button.
    //      3. Stroke up-and-down within the zone at the target speed.
    //    Every valid stroke earns points based on speed accuracy.
    //    After all segments are done the next pattern starts.  The round
    //    ends after a fixed number of patterns (see MassageMinigame.maxPatterns).
    //
    //  Zone layout:
    //    Zone 0 = bottom of the play area
    //    Zone N-1 = top of the play area
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// One segment of a massage pattern.
    /// A zone is highlighted and the player must complete between
    /// <see cref="minStrokes"/> and <see cref="maxStrokes"/> traversals
    /// (chosen randomly each time the segment starts).
    /// </summary>
    [Serializable]
    public class MassagePatternSegment
    {
        /// <summary>Zone index the player must be in (0 = bottom).</summary>
        public int zone;

        /// <summary>Minimum number of stroke attempts needed to clear this segment.</summary>
        public int minStrokes;

        /// <summary>Maximum number of stroke attempts needed to clear this segment.</summary>
        public int maxStrokes;

        /// <summary>
        /// Stroke direction: "up" (bottom-to-top) or "down" (top-to-bottom).
        /// Defaults to "up" if null or unrecognised.
        /// </summary>
        public string direction;

        /// <summary>
        /// Returns +1 for "up" (bottom-to-top) or −1 for "down" (top-to-bottom).
        /// Defaults to +1.
        /// </summary>
        public int DirectionInt
        {
            get
            {
                if (string.Equals(direction, "down", StringComparison.OrdinalIgnoreCase))
                    return -1;
                return 1;
            }
        }

        /// <summary>
        /// Target stroke speed (legacy — kept for JSON compat; ignored by region-timer scoring).
        /// </summary>
        public float targetSpeed;

        /// <summary>
        /// Speed tolerance (legacy — kept for JSON compat; ignored by region-timer scoring).
        /// </summary>
        public float speedTolerance;
    }

    /// <summary>
    /// One complete movement pattern — a named sequence of zone segments.
    /// Loaded from JSON.
    /// </summary>
    [Serializable]
    public class MassageMovementPattern
    {
        /// <summary>Display name shown during gameplay ("Gentle Wave", etc.).</summary>
        public string patternName;

        /// <summary>Ordered segments the player must complete.</summary>
        public MassagePatternSegment[] segments;
    }

    /// <summary>
    /// Defines the vertical extent of a single zone within the play area.
    /// Both values are normalized (0 = bottom of play area, 1 = top).
    /// Zones are allowed to overlap.
    /// </summary>
    [Serializable]
    public class ZoneDefinition
    {
        [Tooltip("Bottom edge of the zone (0 = bottom of play area).")]
        [Range(0f, 1f)] public float yMin;

        [Tooltip("Top edge of the zone (1 = top of play area).")]
        [Range(0f, 1f)] public float yMax;
    }

    /// <summary>
    /// Wrapper so <see cref="JsonUtility"/> can deserialize an array of
    /// patterns from a single JSON file.
    /// <code>
    /// {
    ///   "patterns": [ { ... }, { ... } ]
    /// }
    /// </code>
    /// </summary>
    [Serializable]
    public class MassagePatternCollection
    {
        public MassageMovementPattern[] patterns;
    }
}
