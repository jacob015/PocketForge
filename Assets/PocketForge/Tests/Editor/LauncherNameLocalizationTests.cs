using System.IO;
using NUnit.Framework;
using PocketForge.EditorTools;

namespace PocketForge.Tests.Editor
{
    /// <summary>
    /// The launcher name is written during Gradle export, so it is only observable by
    /// unpacking an AAB. These cover the parts that can be checked without a build.
    /// </summary>
    public sealed class LauncherNameLocalizationTests
    {
        [Test]
        public void EveryGameLanguageExceptTheDefault_HasALauncherName()
        {
            // English stays on productName, which is the untranslated default resource.
            foreach (var qualifier in new[] { "values-ko", "values-ja", "values-zh-rCN" })
            {
                Assert.That(
                    PocketForgeLauncherName.LocalisedNames.ContainsKey(qualifier),
                    Is.True,
                    $"No launcher name for {qualifier}, so that locale falls back to English.");
                Assert.That(
                    PocketForgeLauncherName.LocalisedNames[qualifier].Trim(),
                    Is.Not.Empty,
                    $"{qualifier} would render an empty launcher label.");
            }
        }

        [Test]
        public void GeneratedStringsXml_DeclaresAppNameAndSurvivesEscaping()
        {
            foreach (var name in PocketForgeLauncherName.LocalisedNames.Values)
            {
                var xml = PocketForgeLauncherName.BuildStringsXml(name);
                Assert.That(xml, Does.StartWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
                Assert.That(xml, Does.Contain("<string name=\"app_name\">"));
                Assert.That(xml, Does.Contain(name), $"{name} was mangled into: {xml}");
            }
        }

        [Test]
        public void EscapingCoversXmlAndAndroidsOwnFormattingCharacters()
        {
            // An unescaped apostrophe is the classic one: aapt fails the build on it.
            Assert.That(PocketForgeLauncherName.EscapeAndroidString("Miner's & <Forge>"),
                Is.EqualTo("Miner\\'s &amp; &lt;Forge&gt;"));
        }

        [Test]
        public void ResourceRoot_PrefersTheLauncherModuleWhereAppNameLives()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "launcher", "src", "main"));
                Directory.CreateDirectory(Path.Combine(root, "unityLibrary"));

                var resolved = PocketForgeLauncherName.ResolveLauncherResRoot(
                    Path.Combine(root, "unityLibrary"));

                Assert.That(
                    resolved,
                    Is.EqualTo(Path.Combine(root, "launcher", "src", "main", "res")),
                    "Writing into unityLibrary would not override the launcher's app_name.");
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }

        [Test]
        public void ResourceRoot_FallsBackToTheGivenModuleWhenThereIsNoLauncher()
        {
            var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                var module = Path.Combine(root, "unityLibrary");
                Directory.CreateDirectory(module);

                Assert.That(
                    PocketForgeLauncherName.ResolveLauncherResRoot(module),
                    Is.EqualTo(Path.Combine(module, "src", "main", "res")));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }
    }
}
