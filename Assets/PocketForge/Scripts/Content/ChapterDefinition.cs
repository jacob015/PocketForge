using System;
using UnityEngine;

namespace PocketForge.Content
{
    [Serializable]
    public sealed class ChapterDefinition
    {
        [SerializeField] private string contentId = "crystal_cavern";
        [SerializeField, Min(1)] private int chapterNumber = 1;
        [SerializeField, Min(1)] private int startStage = 1;
        [SerializeField, Min(1)] private int stagesPerChapter = 10;
        [SerializeField, Min(0)] private int firstClearCredits = 100;
        [SerializeField, Min(0)] private int firstClearGems = 5;
        [SerializeField, Min(0)] private int firstClearBlueprintCores;
        [SerializeField, Min(0)] private int repeatClearBlueprintCores;
        // Ore payouts only grow linearly with stage while upgrade costs grow
        // geometrically, so each chapter needs its own payout step to keep the
        // credit income in step with the cost curve.
        [SerializeField, Min(1f)] private float rewardMultiplier = 1f;
        [SerializeField, Min(1f)] private float bossDurabilityMultiplier = 3f;
        [SerializeField, Min(1)] private int bossRewardMultiplier = 5;
        [SerializeField, Min(1f)] private float bossVisualScaleMultiplier = 1.2f;
        [SerializeField, Min(1f)] private float bossTimeLimitSeconds = 30f;

        public string ContentId => contentId;
        public int ChapterNumber => chapterNumber;
        public int StartStage => startStage;
        public int StagesPerChapter => stagesPerChapter;
        public int FirstClearCredits => firstClearCredits;
        public int FirstClearGems => firstClearGems;
        public int FirstClearBlueprintCores =>
            firstClearBlueprintCores > 0 ? firstClearBlueprintCores : chapterNumber * 3;
        public int RepeatClearBlueprintCores =>
            repeatClearBlueprintCores > 0 ? repeatClearBlueprintCores : chapterNumber;
        public float RewardMultiplier => Mathf.Max(1f, rewardMultiplier);
        public float BossDurabilityMultiplier => bossDurabilityMultiplier;
        public int BossRewardMultiplier => bossRewardMultiplier;
        public float BossVisualScaleMultiplier => bossVisualScaleMultiplier;
        public float BossTimeLimitSeconds => bossTimeLimitSeconds;
        public int EndStage => startStage + Mathf.Max(1, stagesPerChapter) - 1;

        public int GetStageNumber(int globalStage)
        {
            var relativeStage = Mathf.Max(0, globalStage - startStage);
            return relativeStage % Mathf.Max(1, stagesPerChapter) + 1;
        }

        public bool IsBossStage(int globalStage) => GetStageNumber(globalStage) == Mathf.Max(1, stagesPerChapter);

        public bool ContainsStage(int globalStage) => globalStage >= startStage && globalStage <= EndStage;

        public static ChapterDefinition CreateRuntimeDefault()
        {
            return new ChapterDefinition();
        }
    }
}
