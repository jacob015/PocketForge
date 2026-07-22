using System;
using UnityEngine;

namespace PocketForge.Save
{
    public static class SaveService
    {
        private const string SaveKey = "PocketForge.Save.v1";

        public static GameSaveData Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return GameSaveMigrator.Normalize(new GameSaveData());
            }

            var json = PlayerPrefs.GetString(SaveKey);
            var data = JsonUtility.FromJson<GameSaveData>(json);
            return GameSaveMigrator.Normalize(data);
        }

        public static bool Save(GameSaveData data)
        {
            return Save(data, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public static bool Save(GameSaveData data, long savedAtUnixSeconds)
        {
            try
            {
                data.lastSavedUnixSeconds = Math.Max(0, savedAtUnixSeconds);
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(GameSaveMigrator.Normalize(data)));
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }
    }
}
