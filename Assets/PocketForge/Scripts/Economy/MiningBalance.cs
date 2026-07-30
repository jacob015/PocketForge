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
        public static long GetUpgradeCost(UpgradeType type, int currentLevel)
        {
            var baseCost = type switch
            {
                UpgradeType.Pickaxe => 10,
                UpgradeType.Drill => 25,
                UpgradeType.Robot => 50,
                _ => 10
            };

            var value = baseCost * System.Math.Pow(1.65d, System.Math.Max(0, currentLevel));
            return value >= long.MaxValue
                ? long.MaxValue
                : System.Math.Max(1L, (long)System.Math.Ceiling(value));
        }

        public static float GetTapPower(int pickaxeLevel) => 1f + pickaxeLevel;

        public static float GetAutoPowerPerSecond(int drillLevel) => drillLevel * 0.5f;

        public static float GetRewardMultiplier(int robotLevel) => 1f + robotLevel * 0.1f;

        public static float GetOreDurability(int stage) => 10f + (stage - 1) * 5f;

        public static long GetOreReward(int stage, bool isRare, int robotLevel)
        {
            var baseReward = (long)stage * (isRare ? 5 : 2);
            var value = baseReward * (double)GetRewardMultiplier(robotLevel);
            return value >= long.MaxValue
                ? long.MaxValue
                : System.Math.Max(1L, (long)System.Math.Ceiling(value));
        }
    }
}
