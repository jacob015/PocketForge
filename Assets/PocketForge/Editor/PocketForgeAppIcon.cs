using System;
using System.Collections.Generic;
using System.Linq;
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

            // Adaptive takes two layers, background first; the launcher masks them
            // together. The single-layer kinds use the pre-composed art.
            var layersByKind = new Dictionary<string, Texture2D[]>
            {
                ["Adaptive"] = new[] { background, foreground },
                ["Round"] = new[] { Load(Round) },
                ["Legacy"] = new[] { Load(Legacy) }
            };

            var applied = new List<string>();
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
            {
                if (!layersByKind.TryGetValue(kind.ToString(), out var layers))
                {
                    continue;
                }

                var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                if (icons == null || icons.Length == 0)
                {
                    continue;
                }

                foreach (var icon in icons)
                {
                    // Unity downscales each density slot from the source, so one layer
                    // set covers every slot of the kind.
                    icon.SetTextures(layers.Take(Math.Max(icon.maxLayerCount, 1)).ToArray());
                }

                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
                applied.Add($"{kind}({icons.Length})");
            }

            if (applied.Count == 0)
            {
                throw new InvalidOperationException(
                    "No Android icon kinds were assigned; the Android platform module may be missing.");
            }

            Debug.Log($"Pocket Forge Android icon kinds assigned: {string.Join(", ", applied)}.");
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
