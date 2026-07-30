using System;
using System.Collections.Generic;
using PocketForge.Content;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Progression
{
    public readonly struct MinerProgressionResult
    {
        public MinerProgressionResult(
            int experienceGained,
            int previousLevel,
            int currentLevel,
            long rewardCredits,
            long rewardGems,
            IReadOnlyList<ProgressionFeature> unlockedFeatures)
        {
            ExperienceGained = experienceGained;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
            RewardCredits = rewardCredits;
            RewardGems = rewardGems;
            UnlockedFeatures = unlockedFeatures ?? Array.Empty<ProgressionFeature>();
        }

        public int ExperienceGained { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
        public int LevelsGained => Math.Max(0, CurrentLevel - PreviousLevel);
        public long RewardCredits { get; }
        public long RewardGems { get; }
        public IReadOnlyList<ProgressionFeature> UnlockedFeatures { get; }
        public bool DidLevelUp => CurrentLevel > PreviousLevel;
    }

    public sealed class MinerProgressionService
    {
        private readonly MiningContentCatalog catalog;

        public MinerProgressionService(MiningContentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public int GetRequiredExperienceForNextLevel(int currentLevel)
        {
            var level = Math.Max(1, currentLevel);
            var required = (long)catalog.BaseExperienceToLevel +
                           (long)(level - 1) * catalog.ExperienceGrowthPerLevel;
            return (int)Math.Min(int.MaxValue, Math.Max(1L, required));
        }

        public int GetOreExperience(int chapterNumber, bool isBoss)
        {
            var chapter = Math.Max(1, chapterNumber);
            var perChapter = isBoss
                ? catalog.BossExperiencePerChapter
                : catalog.NormalOreExperiencePerChapter;
            return (int)Math.Min(int.MaxValue, (long)chapter * Math.Max(1, perChapter));
        }

        public float GetRankPowerMultiplier(int minerLevel)
        {
            return 1f + Math.Max(0, minerLevel - 1) * catalog.MinerRankPowerBonusPerLevel;
        }

        public bool IsUnlocked(int minerLevel, ProgressionFeature feature)
        {
            foreach (var unlock in catalog.GetFeatureUnlocks())
            {
                if (unlock.Feature == feature)
                {
                    return Math.Max(1, minerLevel) >= unlock.RequiredLevel;
                }
            }

            return false;
        }

        public bool TryGetNextUnlock(int minerLevel, out FeatureUnlockDefinition definition)
        {
            var level = Math.Max(1, minerLevel);
            foreach (var unlock in catalog.GetFeatureUnlocks())
            {
                if (unlock.RequiredLevel > level)
                {
                    definition = unlock;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public MinerProgressionResult GrantExperience(GameSaveData player, int amount)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            player.minerLevel = Math.Max(1, player.minerLevel);
            player.minerExperience = Math.Max(0, player.minerExperience);
            player.highestRewardedMinerLevel = Math.Max(
                1,
                Math.Min(player.minerLevel, player.highestRewardedMinerLevel));
            var previousLevel = player.minerLevel;
            var experienceGained = Math.Max(0, amount);
            var remainingExperience = (long)player.minerExperience + experienceGained;
            var rewardCredits = 0L;
            var rewardGems = 0L;
            var unlockedFeatures = new List<ProgressionFeature>();

            while (player.minerLevel < int.MaxValue)
            {
                var required = GetRequiredExperienceForNextLevel(player.minerLevel);
                if (remainingExperience < required)
                {
                    break;
                }

                remainingExperience -= required;
                player.minerLevel++;
                AddUnlocksAtLevel(player.minerLevel, unlockedFeatures);
                if (player.highestRewardedMinerLevel >= player.minerLevel)
                {
                    continue;
                }

                rewardCredits += (long)player.minerLevel * catalog.LevelRewardCreditsPerLevel;
                if (catalog.MilestoneLevelInterval > 0 &&
                    player.minerLevel % catalog.MilestoneLevelInterval == 0)
                {
                    rewardGems += catalog.MilestoneRewardGems;
                }

                player.highestRewardedMinerLevel = player.minerLevel;
            }

            player.minerExperience = (int)Math.Min(int.MaxValue, Math.Max(0L, remainingExperience));
            var grantedCredits = AddCurrencySafely(ref player.credits, rewardCredits);
            var grantedGems = AddCurrencySafely(ref player.gems, rewardGems);
            return new MinerProgressionResult(
                experienceGained,
                previousLevel,
                player.minerLevel,
                grantedCredits,
                grantedGems,
                unlockedFeatures);
        }

        private void AddUnlocksAtLevel(int level, ICollection<ProgressionFeature> unlockedFeatures)
        {
            foreach (var unlock in catalog.GetFeatureUnlocks())
            {
                if (unlock.RequiredLevel == level)
                {
                    unlockedFeatures.Add(unlock.Feature);
                }
            }
        }

        private static long AddCurrencySafely(ref long currency, long amount)
        {
            currency = Math.Max(0L, currency);
            var capacity = long.MaxValue - currency;
            var granted = Math.Min(capacity, Math.Max(0L, amount));
            currency += granted;
            return granted;
        }
    }
}
