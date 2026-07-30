using System.Collections.Generic;
using PocketForge.Content;
using PocketForge.Economy;
using PocketForge.Progression;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Mining
{
    public readonly struct ChapterSelectionOption
    {
        public ChapterSelectionOption(
            int chapterNumber,
            string contentId,
            int startStage,
            int endStage,
            bool isCurrent,
            bool isCleared,
            bool isLocked,
            bool isBossChallenge,
            int targetStage)
        {
            ChapterNumber = chapterNumber;
            ContentId = contentId;
            StartStage = startStage;
            EndStage = endStage;
            IsCurrent = isCurrent;
            IsCleared = isCleared;
            IsLocked = isLocked;
            IsBossChallenge = isBossChallenge;
            TargetStage = targetStage;
        }

        public int ChapterNumber { get; }
        public string ContentId { get; }
        public int StartStage { get; }
        public int EndStage { get; }
        public bool IsCurrent { get; }
        public bool IsCleared { get; }
        public bool IsLocked { get; }
        public bool IsBossChallenge { get; }
        public int TargetStage { get; }
    }

    public readonly struct MiningGameResult
    {
        public MiningGameResult(
            bool stateChanged,
            bool oreBroken,
            bool purchaseSucceeded,
            long rewardCredits = 0,
            bool purchaseFailed = false,
            long rewardGems = 0,
            long rewardBlueprintCores = 0,
            bool chapterCompleted = false,
            bool firstChapterClear = false,
            bool bossFailed = false,
            int completedChapterNumber = 0,
            MinerProgressionResult progression = default)
        {
            StateChanged = stateChanged;
            OreBroken = oreBroken;
            PurchaseSucceeded = purchaseSucceeded;
            RewardCredits = rewardCredits;
            PurchaseFailed = purchaseFailed;
            RewardGems = rewardGems;
            RewardBlueprintCores = rewardBlueprintCores;
            ChapterCompleted = chapterCompleted;
            FirstChapterClear = firstChapterClear;
            BossFailed = bossFailed;
            CompletedChapterNumber = completedChapterNumber;
            Progression = progression;
        }

        public bool StateChanged { get; }
        public bool OreBroken { get; }
        public bool PurchaseSucceeded { get; }
        public long RewardCredits { get; }
        public bool PurchaseFailed { get; }
        public long RewardGems { get; }
        public long RewardBlueprintCores { get; }
        public bool ChapterCompleted { get; }
        public bool FirstChapterClear { get; }
        public bool BossFailed { get; }
        public int CompletedChapterNumber { get; }
        public MinerProgressionResult Progression { get; }
    }

    public readonly struct OfflineProgressResult
    {
        public OfflineProgressResult(
            bool checkpointAdvanced,
            long elapsedSeconds,
            long rewardedSeconds,
            int farmStage,
            int processedOres,
            long rewardCredits,
            MinerProgressionResult progression = default)
        {
            CheckpointAdvanced = checkpointAdvanced;
            ElapsedSeconds = elapsedSeconds;
            RewardedSeconds = rewardedSeconds;
            FarmStage = farmStage;
            ProcessedOres = processedOres;
            RewardCredits = rewardCredits;
            Progression = progression;
        }

        public bool CheckpointAdvanced { get; }
        public long ElapsedSeconds { get; }
        public long RewardedSeconds { get; }
        public int FarmStage { get; }
        public int ProcessedOres { get; }
        public long RewardCredits { get; }
        public MinerProgressionResult Progression { get; }
        public bool HasReward => RewardCredits > 0 || Progression.ExperienceGained > 0;
    }

    public sealed class MiningGameService
    {
        private readonly MiningContentCatalog catalog;
        private readonly MiningPowerService powerService;
        private readonly MinerProgressionService progressionService;
        private readonly ResearchService researchService;

        public MiningGameService(MiningContentCatalog catalog)
        {
            this.catalog = catalog;
            powerService = new MiningPowerService(catalog);
            progressionService = new MinerProgressionService(catalog);
            researchService = new ResearchService(catalog);
        }

        public MiningGameState CreateInitialState(GameSaveData saveData, float rareRoll)
        {
            saveData.furthestStage = Mathf.Max(saveData.stage, saveData.furthestStage);
            return new MiningGameState(saveData, CreateOre(saveData.stage, rareRoll));
        }

        public IReadOnlyList<ChapterSelectionOption> GetChapterSelectionOptions(MiningGameState state)
        {
            var currentChapter = catalog.GetChapterForStage(state.Player.stage);
            var furthestStage = Mathf.Max(state.Player.stage, state.Player.furthestStage);
            var options = new List<ChapterSelectionOption>();
            foreach (var chapter in catalog.GetChapters())
            {
                var isCurrent = chapter.ChapterNumber == currentChapter.ChapterNumber;
                var isCleared = state.Player.highestCompletedChapter >= chapter.ChapterNumber;
                var isLocked = chapter.StartStage > furthestStage;
                var targetStage = chapter.ContainsStage(furthestStage)
                    ? furthestStage
                    : chapter.StartStage;
                var isBossChallenge = isCurrent &&
                                      targetStage != state.Player.stage &&
                                      chapter.IsBossStage(targetStage) &&
                                      state.Player.highestCompletedChapter < chapter.ChapterNumber;
                options.Add(new ChapterSelectionOption(
                    chapter.ChapterNumber,
                    chapter.ContentId,
                    chapter.StartStage,
                    chapter.EndStage,
                    isCurrent,
                    isCleared,
                    isLocked,
                    isBossChallenge,
                    targetStage));
            }

            return options;
        }

        public MiningGameResult SelectChapter(MiningGameState state, int chapterNumber, float rareRoll)
        {
            foreach (var option in GetChapterSelectionOptions(state))
            {
                if (option.ChapterNumber != chapterNumber ||
                    option.IsLocked ||
                    option.IsCurrent && !option.IsBossChallenge)
                {
                    continue;
                }

                state.Player.furthestStage = Mathf.Max(state.Player.stage, state.Player.furthestStage);
                state.Player.stage = option.TargetStage;
                state.ReplaceOre(CreateOre(state.Player.stage, rareRoll));
                return new MiningGameResult(true, false, false);
            }

            return new MiningGameResult(false, false, false);
        }

        public MiningGameResult Mine(MiningGameState state, float rareRoll)
        {
            if (!state.Ore.CanTap)
            {
                return new MiningGameResult(false, false, false);
            }

            var power = GetMiningPower(state);
            state.Ore.BeginTapCooldown(power.TapCooldownSeconds);
            return DamageOre(state, power.TapDamage, rareRoll);
        }

        public MiningGameResult Tick(MiningGameState state, float deltaTime, float rareRoll)
        {
            if (deltaTime <= 0f)
            {
                return new MiningGameResult(false, false, false);
            }

            state.Ore.TickTapCooldown(deltaTime);
            var bossTimerDisplayChanged = false;
            var bossExpired = false;
            if (state.Ore.IsBoss)
            {
                var previousDisplaySeconds = Mathf.CeilToInt(state.Ore.BossTimeRemaining);
                state.Ore.BossTimeRemaining = Mathf.Max(0f, state.Ore.BossTimeRemaining - deltaTime);
                bossExpired = state.Ore.BossTimeRemaining <= 0f;
                bossTimerDisplayChanged = previousDisplaySeconds != Mathf.CeilToInt(state.Ore.BossTimeRemaining);
            }

            var autoPower = GetMiningPower(state).AutoPowerPerSecond;
            if (autoPower > 0f)
            {
                var damageResult = DamageOre(state, autoPower * deltaTime, rareRoll);
                if (damageResult.OreBroken)
                {
                    return damageResult;
                }

                return bossExpired
                    ? FailBossAttempt(state, rareRoll)
                    : damageResult;
            }

            if (bossExpired)
            {
                return FailBossAttempt(state, rareRoll);
            }

            return bossTimerDisplayChanged
                ? new MiningGameResult(true, false, false)
                : new MiningGameResult(false, false, false);
        }

        public MiningGameResult TryUpgrade(MiningGameState state, UpgradeType type)
        {
            var level = GetLevel(state.Player, type);
            var cost = GetUpgradeCost(type, level);
            if (state.Player.credits < cost)
            {
                return new MiningGameResult(false, false, false, purchaseFailed: true);
            }

            state.Player.credits -= cost;
            SetLevel(state.Player, type, level + 1);
            return new MiningGameResult(true, false, true);
        }

        public long GetUpgradeCost(UpgradeType type, int currentLevel)
        {
            return catalog.GetUpgrade(type).GetCost(currentLevel);
        }

        public MiningPowerSnapshot GetMiningPower(MiningGameState state) =>
            powerService.Calculate(
                state.Player,
                new MiningPowerModifiers(
                    progressionService.GetRankPowerMultiplier(state.Player.minerLevel),
                    1f,
                    researchService.GetPowerMultiplier(state.Player),
                    1f,
                    1f));

        public int GetRequiredMinerExperience(int minerLevel) =>
            progressionService.GetRequiredExperienceForNextLevel(minerLevel);

        public float GetMinerRankPowerMultiplier(int minerLevel) =>
            progressionService.GetRankPowerMultiplier(minerLevel);

        public bool IsFeatureUnlocked(int minerLevel, ProgressionFeature feature) =>
            progressionService.IsUnlocked(minerLevel, feature);

        public bool TryGetNextFeatureUnlock(
            int minerLevel,
            out FeatureUnlockDefinition definition) =>
            progressionService.TryGetNextUnlock(minerLevel, out definition);

        public IReadOnlyList<ResearchNodeState> GetResearchNodeStates(MiningGameState state) =>
            researchService.GetNodeStates(
                state.Player,
                progressionService.IsUnlocked(state.Player.minerLevel, ProgressionFeature.Research));

        public ResearchPurchaseStatus TryPurchaseResearch(MiningGameState state, string nodeId) =>
            researchService.TryPurchase(
                state.Player,
                nodeId,
                progressionService.IsUnlocked(state.Player.minerLevel, ProgressionFeature.Research));

        public float GetResearchPowerMultiplier(MiningGameState state) =>
            researchService.GetPowerMultiplier(state.Player);

        public float GetTapPower(int pickaxeLevel) =>
            powerService.Calculate(new GameSaveData { pickaxeLevel = pickaxeLevel }).TapDamage;

        public float GetAutoPowerPerSecond(int drillLevel) =>
            powerService.Calculate(new GameSaveData { drillLevel = drillLevel }).AutoPowerPerSecond;

        public float GetBossRecommendedPower(MiningGameState state)
        {
            var currentBossPower = MiningPowerService.GetBossRecommendedPower(state.Ore);
            if (currentBossPower > 0f)
            {
                return currentBossPower;
            }

            foreach (var option in GetChapterSelectionOptions(state))
            {
                if (option.IsBossChallenge)
                {
                    return GetBossRecommendedPowerForStage(option.TargetStage);
                }
            }

            return 0f;
        }

        public bool IsBossChallengeReady(MiningGameState state)
        {
            foreach (var option in GetChapterSelectionOptions(state))
            {
                if (option.IsBossChallenge)
                {
                    return true;
                }
            }

            return false;
        }

        public float GetRewardMultiplier(int robotLevel) => 1f + robotLevel * catalog.GetUpgrade(UpgradeType.Robot).EffectPerLevel;

        public long GetOreReward(int stage, bool isRare, int robotLevel)
        {
            var definition = catalog.GetOreForStage(stage);
            var multiplier = isRare ? definition.RareRewardMultiplier : definition.NormalRewardMultiplier;
            var reward = System.Math.Ceiling(stage * (double)multiplier * GetRewardMultiplier(robotLevel));
            return reward >= long.MaxValue ? long.MaxValue : System.Math.Max(1L, (long)reward);
        }

        public long GetRewardedAdCredits(MiningGameState state)
        {
            return SaturatingMultiply(
                GetOreReward(state.Player.stage, false, state.Player.robotLevel),
                catalog.RewardedAdRewardMultiplier);
        }

        public MiningGameResult GrantRewardedAdCredits(MiningGameState state)
        {
            var reward = GetRewardedAdCredits(state);
            var granted = AddCurrencySafely(ref state.Player.credits, reward);
            return new MiningGameResult(true, false, false, granted);
        }

        public OfflineProgressResult ClaimOfflineProgress(MiningGameState state, long nowUnixSeconds)
        {
            var player = state.Player;
            var farmStage = GetOfflineFarmStage(state);
            var savedAtUnixSeconds = System.Math.Max(0L, player.lastSavedUnixSeconds);
            if (nowUnixSeconds <= 0)
            {
                return new OfflineProgressResult(false, 0, 0, farmStage, 0, 0);
            }

            if (savedAtUnixSeconds <= 0)
            {
                player.lastSavedUnixSeconds = nowUnixSeconds;
                return new OfflineProgressResult(true, 0, 0, farmStage, 0, 0);
            }

            if (nowUnixSeconds <= savedAtUnixSeconds)
            {
                return new OfflineProgressResult(false, 0, 0, farmStage, 0, 0);
            }

            var elapsedSeconds = nowUnixSeconds - savedAtUnixSeconds;
            var rewardedSeconds = System.Math.Min(elapsedSeconds, catalog.MaxOfflineRewardSeconds);
            var power = GetMiningPower(state);
            var ore = catalog.GetOreForStage(farmStage);
            var durability = Mathf.Max(0.01f, ore.GetDurability(farmStage));
            var processedOresLong = (long)System.Math.Floor(
                power.AutoPowerPerSecond * rewardedSeconds / durability);
            var processedOres = (int)System.Math.Min(int.MaxValue, System.Math.Max(0L, processedOresLong));
            var rewardPerOre = GetOreReward(farmStage, false, player.robotLevel);
            var rewardLong = SaturatingMultiply(processedOresLong, rewardPerOre);
            var reward = AddCurrencySafely(ref player.credits, rewardLong);

            var experienceLong = (long)processedOres *
                                 progressionService.GetOreExperience(
                                     catalog.GetChapterForStage(farmStage).ChapterNumber,
                                     false);
            var progression = progressionService.GrantExperience(
                player,
                (int)System.Math.Min(int.MaxValue, System.Math.Max(0L, experienceLong)));
            var totalRewardCredits = SaturatingAdd(reward, progression.RewardCredits);
            player.lastSavedUnixSeconds = nowUnixSeconds;
            return new OfflineProgressResult(
                true,
                elapsedSeconds,
                rewardedSeconds,
                farmStage,
                processedOres,
                totalRewardCredits,
                progression);
        }

        public int GetOfflineFarmStage(MiningGameState state)
        {
            var furthestStage = Mathf.Max(1, state.Player.furthestStage);
            if (furthestStage <= 1)
            {
                return 1;
            }

            var farmStage = furthestStage - 1;
            while (farmStage > 1 &&
                   catalog.GetChapterForStage(farmStage).IsBossStage(farmStage))
            {
                farmStage--;
            }

            return Mathf.Max(1, farmStage);
        }

        private MiningGameResult DamageOre(MiningGameState state, float amount, float rareRoll)
        {
            if (amount <= 0f)
            {
                return new MiningGameResult(false, false, false);
            }

            state.Ore.Health -= amount;
            if (state.Ore.Health > 0f)
            {
                return new MiningGameResult(true, false, false);
            }

            var completedChapter = state.Ore.Chapter;
            var chapterCompleted = state.Ore.IsBoss;
            var reward = GetOreReward(state.Player.stage, state.Ore.IsRare, state.Player.robotLevel);
            if (chapterCompleted)
            {
                reward = SaturatingMultiply(reward, completedChapter.BossRewardMultiplier);
            }

            var rewardGems = 0L;
            var rewardBlueprintCores = 0L;
            var firstChapterClear = false;
            if (chapterCompleted && state.Player.highestCompletedChapter < completedChapter.ChapterNumber)
            {
                firstChapterClear = true;
                state.Player.highestCompletedChapter = completedChapter.ChapterNumber;
                reward = SaturatingAdd(reward, completedChapter.FirstClearCredits);
                rewardGems = completedChapter.FirstClearGems;
                rewardBlueprintCores = completedChapter.FirstClearBlueprintCores;
            }
            else if (chapterCompleted)
            {
                rewardBlueprintCores = completedChapter.RepeatClearBlueprintCores;
            }

            reward = AddCurrencySafely(ref state.Player.credits, reward);
            rewardGems = AddCurrencySafely(ref state.Player.gems, rewardGems);
            rewardBlueprintCores = AddCurrencySafely(
                ref state.Player.blueprintCores,
                rewardBlueprintCores);
            var progression = progressionService.GrantExperience(
                state.Player,
                progressionService.GetOreExperience(
                    completedChapter.ChapterNumber,
                    chapterCompleted));
            var nextStage = state.Player.stage + 1;
            var shouldRemainInFarmStage =
                !state.Ore.IsBoss &&
                completedChapter.IsBossStage(nextStage) &&
                state.Player.furthestStage >= nextStage &&
                state.Player.highestCompletedChapter < completedChapter.ChapterNumber;
            if (!shouldRemainInFarmStage)
            {
                state.Player.stage = nextStage;
                state.Player.furthestStage = Mathf.Max(state.Player.furthestStage, state.Player.stage);
            }

            state.ReplaceOre(CreateOre(state.Player.stage, rareRoll));
            return new MiningGameResult(
                true,
                true,
                false,
                reward,
                rewardGems: rewardGems,
                rewardBlueprintCores: rewardBlueprintCores,
                chapterCompleted: chapterCompleted,
                firstChapterClear: firstChapterClear,
                completedChapterNumber: chapterCompleted ? completedChapter.ChapterNumber : 0,
                progression: progression);
        }

        private OreState CreateOre(int stage, float rareRoll)
        {
            var definition = catalog.GetOreForStage(stage);
            var chapter = catalog.GetChapterForStage(stage);
            var isBoss = chapter.IsBossStage(stage);
            var durability = definition.GetDurability(stage) * (isBoss ? chapter.BossDurabilityMultiplier : 1f);
            return new OreState(definition, chapter, durability, rareRoll < definition.RareChance, isBoss);
        }

        private float GetBossRecommendedPowerForStage(int stage)
        {
            var definition = catalog.GetOreForStage(stage);
            var chapter = catalog.GetChapterForStage(stage);
            if (!chapter.IsBossStage(stage) || chapter.BossTimeLimitSeconds <= 0f)
            {
                return 0f;
            }

            var durability = definition.GetDurability(stage) * chapter.BossDurabilityMultiplier;
            return durability / chapter.BossTimeLimitSeconds;
        }

        private MiningGameResult FailBossAttempt(MiningGameState state, float rareRoll)
        {
            state.Player.stage = Mathf.Max(state.Ore.Chapter.StartStage, state.Player.stage - 1);
            state.ReplaceOre(CreateOre(state.Player.stage, rareRoll));
            return new MiningGameResult(true, false, false, bossFailed: true);
        }

        private static int GetLevel(GameSaveData data, UpgradeType type)
        {
            return type switch
            {
                UpgradeType.Pickaxe => data.pickaxeLevel,
                UpgradeType.Drill => data.drillLevel,
                UpgradeType.Robot => data.robotLevel,
                _ => 0
            };
        }

        private static void SetLevel(GameSaveData data, UpgradeType type, int level)
        {
            switch (type)
            {
                case UpgradeType.Pickaxe: data.pickaxeLevel = level; break;
                case UpgradeType.Drill: data.drillLevel = level; break;
                case UpgradeType.Robot: data.robotLevel = level; break;
            }
        }

        private static long AddCurrencySafely(ref long currency, long amount)
        {
            currency = System.Math.Max(0L, currency);
            var granted = System.Math.Min(long.MaxValue - currency, System.Math.Max(0L, amount));
            currency += granted;
            return granted;
        }

        private static long SaturatingAdd(long left, long right)
        {
            left = System.Math.Max(0L, left);
            right = System.Math.Max(0L, right);
            return left > long.MaxValue - right ? long.MaxValue : left + right;
        }

        private static long SaturatingMultiply(long left, long right)
        {
            left = System.Math.Max(0L, left);
            right = System.Math.Max(0L, right);
            if (left == 0L || right == 0L)
            {
                return 0L;
            }

            return left > long.MaxValue / right ? long.MaxValue : left * right;
        }
    }
}
