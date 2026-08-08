using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PocketForge.EditorTools
{
    public static class PocketForgeAndroidBuild
    {
        private const string ApplicationIdentifier = "com.jacob015.pocketforge";
        private const string VersionName = "0.1.0";
        private const int VersionCode = 1;
        private const string KeyAlias = "pocketforge";
        private const string DefaultOutputPath = "Builds/Android/PocketForge-0.1.0.aab";

        [MenuItem("Pocket Forge/Build/Configure Android Release")]
        public static void ConfigureAndroidRelease()
        {
            ApplyPublicPlayerSettings();
            ApplySigningFromEnvironment();
            AssetDatabase.SaveAssets();
            Debug.Log($"Pocket Forge Android release configured: {ApplicationIdentifier} {VersionName} ({VersionCode}).");
        }

        [MenuItem("Pocket Forge/Build/Android Release AAB")]
        public static void BuildReleaseAab()
        {
            ApplyPublicPlayerSettings();
            ApplySigningFromEnvironment();

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new InvalidOperationException("Unable to switch the active build target to Android.");
            }

            EditorUserBuildSettings.buildAppBundle = true;
            AssetDatabase.SaveAssets();

            string outputPath = Environment.GetEnvironmentVariable("POCKETFORGE_ANDROID_OUTPUT");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = DefaultOutputPath;
            }

            outputPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException("Invalid output path."));

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes are present in Build Settings.");
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android release build failed: {report.summary.result}");
            }

            Debug.Log($"Pocket Forge Android release built: {outputPath} ({report.summary.totalSize} bytes)");
        }

        private static void ApplyPublicPlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationIdentifier);
            PlayerSettings.bundleVersion = ResolveVersionName();
            PlayerSettings.Android.bundleVersionCode = ResolveVersionCode();
            // Re-applied on every build so a fresh CI workspace, which has never had the
            // editor UI opened against it, still produces the real launcher icon.
            PocketForgeAppIcon.Apply();
        }

        /// <summary>
        /// Play rejects an upload whose versionCode was already used, so CI feeds its
        /// build number in. Local builds keep the checked-in value.
        /// </summary>
        private static int ResolveVersionCode()
        {
            string raw = Environment.GetEnvironmentVariable("POCKETFORGE_VERSION_CODE");
            return int.TryParse(raw, out int parsed) && parsed > 0 ? parsed : VersionCode;
        }

        private static string ResolveVersionName()
        {
            string raw = Environment.GetEnvironmentVariable("POCKETFORGE_VERSION_NAME");
            return string.IsNullOrWhiteSpace(raw) ? VersionName : raw.Trim();
        }

        private static void ApplySigningFromEnvironment()
        {
            string keystorePath = GetRequiredEnvironmentVariable("POCKETFORGE_KEYSTORE_PATH");
            string keystorePassword = GetRequiredEnvironmentVariable("POCKETFORGE_KEYSTORE_PASS");
            string keyPassword = GetRequiredEnvironmentVariable("POCKETFORGE_KEYALIAS_PASS");

            if (!File.Exists(keystorePath))
            {
                throw new FileNotFoundException("Android signing keystore was not found.", keystorePath);
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = Path.GetFullPath(keystorePath);
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = KeyAlias;
            PlayerSettings.Android.keyaliasPass = keyPassword;
        }

        private static string GetRequiredEnvironmentVariable(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Required environment variable is missing: {name}");
            }

            return value;
        }
    }
}
