using System.Collections.Generic;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Describes one bust outfit to <see cref="Characters.CreateNewBust(BustOutfitDescriptor)"/>.
    /// All <c>*Abs</c> paths are absolute or relative-to-exe — exactly what
    /// <see cref="System.IO.File.ReadAllBytes(string)"/> would accept. The
    /// legacy 9-arg <c>CreateNewBust</c> overload builds one of these by string-
    /// concatenating <c>pathToCG + relativeSubpath</c>, matching its previous
    /// behaviour byte-for-byte.
    /// <para/>
    /// When loaded from a pack (<see cref="BustPacks"/>), the paths are
    /// <c>packRoot</c> joined with the relative paths in <c>bustpack.json</c>.
    /// </summary>
    public class BustOutfitDescriptor
    {
        /// <summary>Name applied to the new GameObject. Mirrors the first arg of the legacy <c>CreateNewBust</c>.</summary>
        public string Name;

        public string BaseSpriteAbs;
        public string MaskSpriteAbs;
        public string BlinkSpriteAbs;

        public bool MouthEnabled = true;
        /// <summary>Prefix; loader appends <c>1.PNG..4.PNG</c>.</summary>
        public string MouthPrefixAbs;

        public bool ExpressionEnabled = true;
        /// <summary>Prefix; loader appends <c>Happy/Angry/Sad/Flirty.PNG</c>.</summary>
        public string ExpressionPrefixAbs;

        /// <summary>
        /// Per-outfit jiggle shader values. <c>null</c> means "use the material
        /// inherited from <c>Core.baseBust</c> as-is" — the legacy code path.
        /// </summary>
        public JiggleParamsValues Jiggle;

        /// <summary>Particle effects to attach as children of <c>MBase1</c>. Default: one Wet preset (cloned from Anna_Towel).</summary>
        public List<ParticleSpec> Particles = new List<ParticleSpec> { new ParticleSpec { Preset = "Wet" } };
    }

    /// <summary>
    /// Plain struct of jiggle uniforms. Matches the property names in
    /// <c>Sprites/JiggleSprite</c> with the leading <c>_</c> stripped.
    /// </summary>
    public class JiggleParamsValues
    {
        public float Speed = 3.0f;
        public float Strength = -0.02f;
        public float Frequency = 4.0f;
        public float NoiseScale = 5.0f;
        public float NoiseSpeed = 0.5f;
        public float NoiseStrength = 0.06f;
        /// <summary>RGBA hex tint string, e.g. <c>"#FFFFFFFF"</c>.</summary>
        public string Tint = "#FFFFFFFF";
        public bool PixelSnap = false;
    }

    public class ParticleSpec
    {
        /// <summary>Built-in: <c>"Wet"</c> (clones <c>Anna_Towel/MBase1/Particle System</c>). Other values reserved for v1.1 custom presets.</summary>
        public string Preset = "Wet";
        /// <summary>Custom preset JSON path (relative to pack root). Only meaningful when <see cref="Preset"/> is <c>"custom"</c>.</summary>
        public string File;
        /// <summary>Optional override for the attached GameObject's name. Defaults to the preset name.</summary>
        public string Name;
    }
}
