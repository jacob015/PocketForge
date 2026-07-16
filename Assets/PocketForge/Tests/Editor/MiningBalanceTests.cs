using NUnit.Framework;
using PocketForge.Economy;

namespace PocketForge.Tests.Editor
{
    public sealed class MiningBalanceTests
    {
        [Test]
        public void UpgradeCost_IncreasesForEachLevel()
        {
            Assert.Greater(MiningBalance.GetUpgradeCost(UpgradeType.Pickaxe, 1),
                MiningBalance.GetUpgradeCost(UpgradeType.Pickaxe, 0));
        }

        [Test]
        public void RobotRewardMultiplier_IncreasesOreReward()
        {
            Assert.Greater(MiningBalance.GetOreReward(1, false, 2),
                MiningBalance.GetOreReward(1, false, 0));
        }

        [Test]
        public void DrillLevel_ProvidesAutomaticMiningPower()
        {
            Assert.AreEqual(0f, MiningBalance.GetAutoPowerPerSecond(0));
            Assert.Greater(MiningBalance.GetAutoPowerPerSecond(1), 0f);
        }
    }
}
