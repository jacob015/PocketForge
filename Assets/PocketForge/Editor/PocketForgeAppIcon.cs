using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace PocketForge.EditorTools
{
    /// <summary>
    /// Assigns the Android launcher icons from checked-in art.
    ///
    /// Every m_Icons entry in ProjectSettings was empty, so builds up to this point
    /// shipped Unity's default icon. Going through the API rather than editing
    /// ProjectSettings.asset by hand keeps Unity's own slot and layer rules, and lets
    /// the release build re-apply them on a machine whose editor UI was never opened.
    ///
    /// The kinds are looked up by name from GetSupportedIconKinds instead of naming
    /// AndroidPlatformIconKind directly, because that type lives in the Android
    /// platform extension assembly while GetSupportedIconKinds is in UnityEditor.
    /// </summary>
    public static class PocketForgeAppIcon
    {
        private const string IconRoot = "Assets/PocketForge/Art/AppIcon/";
        private const string AdaptiveForeground = IconRoot + "AppIconAdaptiveForeground.png";
        private const string AdaptiveBackground = IconRoot + "AppIconAdaptiveBackground.png";
        private const string Legacy = IconRoot + "AppIconLegacy.png";
        private const string Round = IconRoot + "AppIconRound.png";

        [MenuItem("Pocket Forge/Build/Apply Android Icons")]
        public static void ApplyAndSave()
        {
            Apply();
            AssetDatabase.SaveAssets();
            Debug.Log("Pocket Forge Android launcher icons applied.");
        }

        public static void Apply()
        {
            var foreground = Load(AdaptiveForeground);
            var background = Load(AdaptiveBackground);
            var round = Load(Round);
            var legacy = Load(Legacy);

            var applied = new List<string>();
            var kinds = PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android);
            foreach (var kind in kinds)
            {
                var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                if (icons == null || icons.Length == 0)
                {
                    continue;
                }

                var name = kind.ToString();
                foreach (var icon in icons)
                {
                    // Matching kinds by name missed everything but Legacy, so the layer
                    // count decides instead: anything wanting two layers is the adaptive
                    // pair, background first. Only the round variant still needs its name,
                    // and it falls back to the same square art when the name differs.
                    Texture2D[] layers = icon.maxLayerCount >= 2
                        ? new[] { background, foreground }
                        : new[] { name.IndexOf("Round", StringComparison.OrdinalIgnoreCase) >= 0 ? round : legacy };
                    icon.SetTextures(layers);
                }

                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
                applied.Add($"{name}[{icons.Length} slots, {icons[0].maxLayerCount} layers]");
            }

            if (applied.Count == 0)
            {
                throw new InvalidOperationException(
                    "No Android icon kinds were assigned; the Android platform module may be missing.");
            }

            Debug.Log(
                $"Pocket Forge Android icon kinds supported: {kinds.Length}. " +
                $"Assigned: {string.Join(" | ", applied)}.");
        }

        private static Texture2D Load(string path)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                throw new InvalidOperationException($"Launcher icon art is missing: {path}");
            }

            return texture;
        }
    }
}
