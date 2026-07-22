using System;
using UnityEngine;

namespace PocketForge.Settings
{
    public static class GameSettingsService
    {
        private const string MusicVolumeKey = "PocketForge.Settings.MusicVolume";
        private const string SoundVolumeKey = "PocketForge.Settings.SoundVolume";
        private const string MusicMutedKey = "PocketForge.Settings.MusicMuted";
        private const string SoundMutedKey = "PocketForge.Settings.SoundMuted";
        private const string HapticsKey = "PocketForge.Settings.Haptics";
        private const string ReduceMotionKey = "PocketForge.Settings.ReduceMotion";

        public static event Action Changed;

        private static float musicVolume = 0.65f;
        private static float soundVolume = 0.85f;
        private static bool musicMuted;
        private static bool soundMuted;
        private static bool hapticsEnabled = true;
        private static bool reduceMotion;

        public static float MusicVolume => musicVolume;
        public static float SoundVolume => soundVolume;
        public static bool MusicMuted => musicMuted;
        public static bool SoundMuted => soundMuted;
        public static bool HapticsEnabled => hapticsEnabled;
        public static bool ReduceMotion => reduceMotion;

        public static float EffectiveMusicVolume => MusicMuted ? 0f : MusicVolume;
        public static float EffectiveSoundVolume => SoundMuted ? 0f : SoundVolume;

        public static void Initialize()
        {
            musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.65f));
            soundVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SoundVolumeKey, 0.85f));
            musicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) != 0;
            soundMuted = PlayerPrefs.GetInt(SoundMutedKey, 0) != 0;
            hapticsEnabled = PlayerPrefs.GetInt(HapticsKey, 1) != 0;
            reduceMotion = PlayerPrefs.GetInt(ReduceMotionKey, 0) != 0;
        }

        public static void SetMusicVolume(float value) => SetFloat(MusicVolumeKey, value, ref musicVolume);

        public static void SetSoundVolume(float value) => SetFloat(SoundVolumeKey, value, ref soundVolume);

        public static void SetMusicMuted(bool value) => SetBool(MusicMutedKey, value, ref musicMuted);

        public static void SetSoundMuted(bool value) => SetBool(SoundMutedKey, value, ref soundMuted);

        public static void SetHapticsEnabled(bool value) => SetBool(HapticsKey, value, ref hapticsEnabled);

        public static void SetReduceMotion(bool value) => SetBool(ReduceMotionKey, value, ref reduceMotion);

        public static void Flush() => PlayerPrefs.Save();

        private static void SetFloat(string key, float value, ref float field)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(field, value))
            {
                return;
            }

            field = value;
            PlayerPrefs.SetFloat(key, value);
            Changed?.Invoke();
        }

        private static void SetBool(string key, bool value, ref bool field)
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            Changed?.Invoke();
        }
    }
}
