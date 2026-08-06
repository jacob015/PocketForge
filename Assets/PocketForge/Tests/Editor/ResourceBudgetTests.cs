using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    /// <summary>
    /// Everything under a Resources folder ships whether or not anything references it,
    /// so these guard the Android download budget: no orphaned art, and every texture
    /// carries an explicit compressed Android format.
    /// </summary>
    public sealed class ResourceBudgetTests
    {
        private const string ResourceRoot = "Assets/PocketForge/Resources";
        private const string ScriptRoot = "Assets/PocketForge/Scripts";

        [Test]
        public void NoResourceTextureShipsWithoutBeingNamedByCode()
        {
            var referenced = CollectStringLiteralsFromScripts();

            var orphans = TextureAssetPaths()
                .Where(path => !referenced.Contains(Path.GetFileNameWithoutExtension(path)))
                .OrderBy(path => path)
                .ToArray();

            Assert.That(
                orphans,
                Is.Empty,
                "These Resources textures are never named by code but still ship:\n" +
                string.Join("\n", orphans));
        }

        [Test]
        public void EveryResourceTextureUsesAnExplicitCompressedAndroidFormat()
        {
            var offenders = new List<string>();

            foreach (var path in TextureAssetPaths())
            {
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                {
                    continue;
                }

                var android = importer.GetPlatformTextureSettings("Android");
                if (!android.overridden)
                {
                    offenders.Add($"{path} (no Android override)");
                    continue;
                }

                if (android.format != TextureImporterFormat.ASTC_6x6)
                {
                    offenders.Add($"{path} ({android.format})");
                }
            }

            Assert.That(
                offenders,
                Is.Empty,
                "Uncompressed or unset Android texture formats inflate the build:\n" +
                string.Join("\n", offenders));
        }

        [Test]
        public void ResourceTextureBudget_StaysWithinTheAndroidDownloadTarget()
        {
            var pixels = TextureAssetPaths()
                .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
                .Where(texture => texture != null)
                .Sum(texture => (long)texture.width * texture.height);

            // ASTC 6x6 packs a 6x6 block into 128 bits.
            var megabytes = pixels * (128d / 36d) / 8d / (1024d * 1024d);

            Assert.That(
                megabytes,
                Is.LessThanOrEqualTo(9d),
                $"Resources UI textures would occupy {megabytes:0.00} MB compressed.");
        }

        private static IEnumerable<string> TextureAssetPaths() =>
            AssetDatabase.FindAssets("t:Texture2D", new[] { ResourceRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct();

        private static HashSet<string> CollectStringLiteralsFromScripts()
        {
            var literals = new HashSet<string>();
            var pattern = new Regex("\"([A-Za-z0-9_]{3,})\"");
            foreach (var file in Directory.GetFiles(ScriptRoot, "*.cs", SearchOption.AllDirectories))
            {
                foreach (Match match in pattern.Matches(File.ReadAllText(file)))
                {
                    literals.Add(match.Groups[1].Value);
                }
            }

            Assert.That(literals, Is.Not.Empty, "Script scan found no literals; the path is probably wrong.");
            return literals;
        }
    }
}
