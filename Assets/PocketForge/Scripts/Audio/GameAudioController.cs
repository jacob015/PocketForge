using PocketForge.Settings;
using UnityEngine;

namespace PocketForge.Audio
{
    public sealed class GameAudioController : MonoBehaviour
    {
        private AudioSource musicSource;
        private AudioSource soundSource;
        private AudioClip clickClip;
        private AudioClip upgradeClip;
        private AudioClip rewardClip;

        public static GameAudioController Instance { get; private set; }

        public static GameAudioController Create(AudioClip music, AudioClip click, AudioClip upgrade, AudioClip reward)
        {
            var audioObject = new GameObject("GameAudio");
            var controller = audioObject.AddComponent<GameAudioController>();
            controller.Configure(music, click, upgrade, reward);
            return controller;
        }

        public void PlayUiClick()
        {
            if (clickClip != null && GameSettingsService.EffectiveSoundVolume > 0f)
            {
                soundSource.PlayOneShot(clickClip);
            }
        }

        public void PlayUpgradeSuccess()
        {
            if (upgradeClip != null && GameSettingsService.EffectiveSoundVolume > 0f)
            {
                soundSource.PlayOneShot(upgradeClip);
            }

            TriggerHaptic();
        }

        public void PlayReward()
        {
            if (rewardClip != null && GameSettingsService.EffectiveSoundVolume > 0f)
            {
                soundSource.PlayOneShot(rewardClip);
            }
        }

        private void Awake()
        {
            Instance = this;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.playOnAwake = false;
            GameSettingsService.Changed += ApplySettings;
        }

        private void Configure(AudioClip music, AudioClip click, AudioClip upgrade, AudioClip reward)
        {
            musicSource.clip = music;
            clickClip = click;
            upgradeClip = upgrade;
            rewardClip = reward;
            ApplySettings();
            if (music != null && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        private void ApplySettings()
        {
            musicSource.volume = GameSettingsService.MusicVolume;
            musicSource.mute = GameSettingsService.MusicMuted;
            soundSource.volume = GameSettingsService.SoundVolume;
            soundSource.mute = GameSettingsService.SoundMuted;
        }

        private void OnDestroy()
        {
            GameSettingsService.Changed -= ApplySettings;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private static void TriggerHaptic()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (GameSettingsService.HapticsEnabled)
            {
                Handheld.Vibrate();
            }
#endif
        }
    }
}
