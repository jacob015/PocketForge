using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Android;
using UnityEngine;

namespace PocketForge.EditorTools
{
    /// <summary>
    /// Localises the name shown under the launcher icon.
    ///
    /// Unity writes productName into the launcher module's default strings.xml as
    /// app_name, so every locale gets the same text. The game itself runs in four
    /// languages, and a Korean player seeing a Latin name on their home screen is the
    /// one piece of the app that never localises.
    ///
    /// The translations are written as extra resource qualifiers after Gradle export
    /// rather than through an .androidlib, which would mean carrying a second Gradle
    /// module and its namespace and compileSdk just to hold four short strings.
    /// Android resolves the most specific qualifier, so values-ko wins on a Korean
    /// device while the untranslated default keeps productName.
    /// </summary>
    public sealed class PocketForgeLauncherName : IPostGenerateGradleAndroidProject
    {
        // Runs late; nothing else here depends on ordering.
        public int callbackOrder => 100;

        /// <summary>Android resource qualifier to launcher label.</summary>
        public static readonly IReadOnlyDictionary<string, string> LocalisedNames =
            new Dictionary<string, string>
            {
                ["values-ko"] = "포켓 포지",
                ["values-ja"] = "ポケットフォージ",
                ["values-zh-rCN"] = "口袋熔炉"
            };

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var resRoot = ResolveLauncherResRoot(path);
            var written = new List<string>();

            foreach (var pair in LocalisedNames)
            {
                var directory = Path.Combine(resRoot, pair.Key);
                Directory.CreateDirectory(directory);
                var file = Path.Combine(directory, "strings.xml");
                File.WriteAllText(file, BuildStringsXml(pair.Value), new UTF8Encoding(false));
                written.Add(pair.Key);
            }

            Debug.Log(
                $"Pocket Forge launcher name localised into {resRoot}: {string.Join(", ", written)}.");
        }

        /// <summary>
        /// The callback receives the unityLibrary module. app_name lives in the sibling
        /// launcher module, which is what the manifest's android:label resolves against.
        /// </summary>
        internal static string ResolveLauncherResRoot(string unityLibraryPath)
        {
            var projectRoot = Path.GetDirectoryName(unityLibraryPath);
            if (projectRoot != null)
            {
                var launcher = Path.Combine(projectRoot, "launcher", "src", "main", "res");
                if (Directory.Exists(Path.GetDirectoryName(launcher)))
                {
                    return launcher;
                }
            }

            // Older export layouts keep everything in the one module.
            return Path.Combine(unityLibraryPath, "src", "main", "res");
        }

        internal static string BuildStringsXml(string appName) =>
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
            "<resources>\n" +
            $"    <string name=\"app_name\">{EscapeAndroidString(appName)}</string>\n" +
            "</resources>\n";

        /// <summary>
        /// Android string resources are XML and additionally treat apostrophes and
        /// quotes as formatting characters, so both layers need escaping.
        /// </summary>
        internal static string EscapeAndroidString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                switch (character)
                {
                    case '&': builder.Append("&amp;"); break;
                    case '<': builder.Append("&lt;"); break;
                    case '>': builder.Append("&gt;"); break;
                    case '\'': builder.Append("\\'"); break;
                    case '"': builder.Append("\\\""); break;
                    default: builder.Append(character); break;
                }
            }

            return builder.ToString();
        }
    }
}
