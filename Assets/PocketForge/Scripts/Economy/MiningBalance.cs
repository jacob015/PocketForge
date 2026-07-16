namespace PocketForge.Economy
{
    public enum UpgradeType
    {
        Pickaxe,
        Drill,
        Robot
    }

    public static class MiningBalance
    {
        public static int GetUpgradeCost(UpgradeType type, int currentLevel)
        {
            var baseCost = type switch
            {
                UpgradeType.Pickaxe => 10,
                UpgradeType.Drill => 25,
                UpgradeType.Robot => 50,
                _ => 10
            };

            return UnityEngine.Mathf.CeilToInt(baseCost * UnityEngine.Mathf.Pow(1.65f, currentLevel));
        }

        public static float GetTapPower(int pickaxeLevel) => 1f + pickaxeLevel;

        public static float GetAutoPowerPerSecond(int drillLevel) => drillLevel * 0.5f;

        public static float GetRewardMultiplier(int robotLevel) => 1f + robotLevel * 0.1f;

        public static float GetOreDurability(int stage) => 10f + (stage - 1) * 5f;

        public static int GetOreReward(int stage, bool isRare, int robotLevel)
        {
            var baseReward = stage * (isRare ? 5 : 2);
            return UnityEngine.Mathf.CeilToInt(baseReward * GetRewardMultiplier(robotLevel));
        }
    }
}
