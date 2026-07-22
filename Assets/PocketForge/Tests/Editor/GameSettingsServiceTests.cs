using NUnit.Framework;
using PocketForge.Settings;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class GameSettingsServiceTests
    {
        private static readonly string[] Keys =
        {
            "PocketForge.Settings.MusicVolume",
            "PocketForge.Settings.SoundVolume",
            "PocketForge.Settings.MusicMuted",
            "PocketForge.Settings.SoundMuted",
            "PocketForge.Settings.Haptics",
            "PocketForge.Settings.ReduceMotion"
        };

        [SetUp]
        public void SetUp()
        {
            ClearSettings();
            GameSettingsService.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            ClearSettings();
            GameSettingsService.Initialize();
        }

        [Test]
        public void Initialize_UsesMobileFriendlyDefaults()
        {
            Assert.AreEqual(0.65f, GameSettingsService.MusicVolume, 0.001f);
            Assert.AreEqual(0.85f, GameSettingsService.SoundVolume, 0.001f);
            Assert.IsFalse(GameSettingsService.MusicMuted);
            Assert.IsFalse(GameSettingsService.SoundMuted);
            Assert.IsTrue(GameSettingsService.HapticsEnabled);
            Assert.IsFalse(GameSettingsService.ReduceMotion);
        }

        [Test]
        public void VolumeSetters_ClampAndPersistValues()
        {
            GameSettingsService.SetMusicVolume(2f);
            GameSettingsService.SetSoundVolume(-1f);
            GameSettingsService.Flush();
            GameSettingsService.Initialize();

            Assert.AreEqual(1f, GameSettingsService.MusicVolume, 0.001f);
            Assert.AreEqual(0f, GameSettingsService.SoundVolume, 0.001f);
        }

        [Test]
        public void MuteState_ControlsEffectiveVolumeWithoutDestroyingPreference()
        {
            GameSettingsService.SetMusicVolume(0.42f);
            GameSettingsService.SetMusicMuted(true);

            Assert.AreEqual(0f, GameSettingsService.EffectiveMusicVolume, 0.001f);
            Assert.AreEqual(0.42f, GameSettingsService.MusicVolume, 0.001f);

            GameSettingsService.SetMusicMuted(false);
            Assert.AreEqual(0.42f, GameSettingsService.EffectiveMusicVolume, 0.001f);
        }

        private static void ClearSettings()
        {
            foreach (var key in Keys)
            {
                PlayerPrefs.DeleteKey(key);
            }

            PlayerPrefs.Save();
        }
    }
}
