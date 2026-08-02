using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketForge.Presentation
{
    /// <summary>
    /// Loads the final, reference-matched UI parts from Resources and creates cached sprites.
    /// MineHudView stays concerned with layout while this class owns asset lookup and slicing.
    /// </summary>
    public sealed class MineUiSkin
    {
        private const string ResourceRoot = "PocketForge/UI/Final/";
        private const string V5ResourceRoot = "PocketForge/UI/V5/";
        private const string Task13ResourceRoot = "PocketForge/UI/Task13/";

        private static MineUiSkin shared;

        // Borders are measured at the first straight pixel after each rounded corner.
        // Keeping them here prevents call sites from guessing values that cut through curves.
        private static readonly IReadOnlyDictionary<string, Vector4> Task13SliceBorders =
            new Dictionary<string, Vector4>(StringComparer.Ordinal)
            {
                ["UiCollectionModalBody"] = new Vector4(39f, 39f, 39f, 39f),
                ["UiEquipmentModalBody"] = new Vector4(40f, 42f, 40f, 42f),
                ["UiEquipmentSlotCardBase"] = new Vector4(38f, 38f, 38f, 38f),
                ["UiEquipmentInventoryCardClean"] = new Vector4(33f, 33f, 33f, 33f),
                ["OverlayEquipmentRarityCommon"] = new Vector4(33f, 34f, 33f, 34f),
                ["OverlayEquipmentRarityRare"] = new Vector4(33f, 34f, 33f, 34f),
                ["OverlayEquipmentRarityEpic"] = new Vector4(33f, 34f, 33f, 34f),
                ["OverlayEquipmentRarityLegendary"] = new Vector4(33f, 34f, 33f, 34f),
                ["OverlayEquipmentSelected"] = new Vector4(33f, 34f, 33f, 34f),
                ["ButtonEquipmentUnequipRuntime"] = new Vector4(27f, 27f, 27f, 27f),
                ["ButtonEquipmentEquipRuntime"] = new Vector4(32f, 31f, 32f, 31f),
                ["ButtonEquipmentMergeRuntime"] = new Vector4(41f, 42f, 41f, 42f),
                ["ButtonEquipmentAutoEquipRuntime"] = new Vector4(38f, 38f, 38f, 38f),
                ["ButtonAchievementClaimRuntime"] = new Vector4(27f, 27f, 27f, 27f),
                ["TabCollectionActive"] = new Vector4(29f, 30f, 29f, 30f),
                ["TabCollectionInactive"] = new Vector4(30f, 31f, 30f, 31f),
                ["UiMuseumExhibitCardClean"] = new Vector4(34f, 34f, 34f, 34f),
                ["UiAchievementInProgressState"] = new Vector4(27f, 27f, 27f, 27f),
                ["UiTask13HorizontalPanelClean"] = new Vector4(35f, 35f, 35f, 35f)
            };

        private readonly Dictionary<string, Texture2D> textures = new();
        private readonly Dictionary<string, Texture2D> v5Textures = new();
        private readonly Dictionary<string, Texture2D> task13Textures = new();
        private readonly Dictionary<string, Sprite> sprites = new();
        private readonly HashSet<string> missingAssets = new();

        private MineUiSkin()
        {
        }

        public static MineUiSkin Load()
        {
            if (shared != null)
            {
                return shared;
            }

            var candidate = new MineUiSkin();
            if (candidate.Texture("HudHeader") == null)
            {
                return null;
            }

            shared = candidate;
            return shared;
        }

        public Texture2D Texture(string assetName)
        {
            if (textures.TryGetValue(assetName, out var cached) && cached != null)
            {
                return cached;
            }

            textures.Remove(assetName);

            var texture = Resources.Load<Texture2D>(ResourceRoot + assetName);
            textures[assetName] = texture;
            if (texture == null && missingAssets.Add(assetName))
            {
                Debug.LogWarning($"Pocket Forge UI asset is missing: {ResourceRoot}{assetName}");
            }

            return texture;
        }

        public Sprite Simple(string assetName)
        {
            return Create(assetName, Vector4.zero, "simple");
        }

        public Texture2D V5Texture(string assetName)
        {
            if (v5Textures.TryGetValue(assetName, out var cached) && cached != null)
            {
                return cached;
            }

            v5Textures.Remove(assetName);

            var texture = Resources.Load<Texture2D>(V5ResourceRoot + assetName);
            v5Textures[assetName] = texture;
            if (texture == null && missingAssets.Add("V5/" + assetName))
            {
                Debug.LogWarning($"Pocket Forge UI asset is missing: {V5ResourceRoot}{assetName}");
            }

            return texture;
        }

        public Sprite V5Simple(string assetName)
        {
            return CreateV5(assetName);
        }

        public Texture2D Task13Texture(string assetName)
        {
            if (task13Textures.TryGetValue(assetName, out var cached) && cached != null)
            {
                return cached;
            }

            task13Textures.Remove(assetName);

            var texture = Resources.Load<Texture2D>(Task13ResourceRoot + assetName);
            task13Textures[assetName] = texture;
            if (texture == null && missingAssets.Add("Task13/" + assetName))
            {
                Debug.LogWarning($"Pocket Forge UI asset is missing: {Task13ResourceRoot}{assetName}");
            }

            return texture;
        }

        public Sprite Task13Simple(string assetName)
        {
            var cacheKey = "Task13/" + assetName + ":simple";
            if (sprites.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            sprites.Remove(cacheKey);

            var texture = Task13Texture(assetName);
            if (texture == null)
            {
                return null;
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprites[cacheKey] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates a Task 13 sprite using the measured border catalog.
        /// Border order is left, bottom, right, top to match Sprite.Create.
        /// </summary>
        public Sprite Task13Sliced(string assetName)
        {
            if (!TryGetTask13SliceBorder(assetName, out var borderPixels))
            {
                if (missingAssets.Add("Task13 border/" + assetName))
                {
                    Debug.LogWarning(
                        $"Pocket Forge Task13 asset has no measured slice border: {assetName}. " +
                        "Falling back to a Simple sprite.");
                }

                return Task13Simple(assetName);
            }

            var cacheKey = $"Task13/{assetName}:sliced:" +
                           $"{borderPixels.x:F1},{borderPixels.y:F1}," +
                           $"{borderPixels.z:F1},{borderPixels.w:F1}";
            if (sprites.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            sprites.Remove(cacheKey);

            var texture = Task13Texture(assetName);
            if (texture == null)
            {
                return null;
            }

            var finite = IsFinite(borderPixels.x) &&
                         IsFinite(borderPixels.y) &&
                         IsFinite(borderPixels.z) &&
                         IsFinite(borderPixels.w);
            var valid = finite &&
                        borderPixels.x >= 0f &&
                        borderPixels.y >= 0f &&
                        borderPixels.z >= 0f &&
                        borderPixels.w >= 0f &&
                        borderPixels.x + borderPixels.z < texture.width &&
                        borderPixels.y + borderPixels.w < texture.height;
            if (!valid)
            {
                Debug.LogWarning(
                    $"Pocket Forge Task13 border is invalid for {assetName} " +
                    $"({texture.width}x{texture.height}): {borderPixels}");
                return Task13Simple(assetName);
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                borderPixels);
            sprites[cacheKey] = sprite;
            return sprite;
        }

        public static bool TryGetTask13SliceBorder(string assetName, out Vector4 borderPixels)
        {
            return Task13SliceBorders.TryGetValue(assetName, out borderPixels);
        }

        public Sprite Sliced(string assetName, Vector4 normalizedBorder)
        {
            var key = $"{normalizedBorder.x:F3},{normalizedBorder.y:F3},{normalizedBorder.z:F3},{normalizedBorder.w:F3}";
            return Create(assetName, normalizedBorder, key);
        }

        private Sprite Create(string assetName, Vector4 normalizedBorder, string variant)
        {
            var cacheKey = assetName + ":" + variant;
            if (sprites.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            sprites.Remove(cacheKey);

            var texture = Texture(assetName);
            if (texture == null)
            {
                return null;
            }

            var rect = new Rect(0f, 0f, texture.width, texture.height);
            var border = new Vector4(
                normalizedBorder.x * rect.width,
                normalizedBorder.y * rect.height,
                normalizedBorder.z * rect.width,
                normalizedBorder.w * rect.height);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            sprites[cacheKey] = sprite;
            return sprite;
        }

        private Sprite CreateV5(string assetName)
        {
            var cacheKey = "V5/" + assetName + ":simple";
            if (sprites.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            sprites.Remove(cacheKey);

            var texture = V5Texture(assetName);
            if (texture == null)
            {
                return null;
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprites[cacheKey] = sprite;
            return sprite;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
