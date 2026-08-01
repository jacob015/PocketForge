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

        private static MineUiSkin shared;

        private readonly Dictionary<string, Texture2D> textures = new();
        private readonly Dictionary<string, Texture2D> v5Textures = new();
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
    }
}
