namespace PocketForge.Save
{
    public static class GameSaveMigrator
    {
        public const int CurrentVersion = 3;

        public static GameSaveData Normalize(GameSaveData data)
        {
            data ??= new GameSaveData();
            data.credits = System.Math.Max(0, data.credits);
            data.stage = System.Math.Max(1, data.stage);
            data.pickaxeLevel = System.Math.Max(0, data.pickaxeLevel);
            data.drillLevel = System.Math.Max(0, data.drillLevel);
            data.robotLevel = System.Math.Max(0, data.robotLevel);
            data.lastSavedUnixSeconds = System.Math.Max(0, data.lastSavedUnixSeconds);
            data.version = CurrentVersion;
            return data;
        }
    }
}
