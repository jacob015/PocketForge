using System;
using System.Collections.Generic;
using PocketForge.Content;
using PocketForge.Save;

namespace PocketForge.Progression
{
    public enum ResearchPurchaseStatus
    {
        Success,
        FeatureLocked,
        UnknownNode,
        PrerequisiteMissing,
        MaxLevel,
        InsufficientCores
    }

    public readonly struct ResearchNodeState
    {
        public ResearchNodeState(
            ResearchNodeDefinition definition,
            int currentLevel,
            long cost,
            ResearchPurchaseStatus purchaseStatus)
        {
            Definition = definition;
            CurrentLevel = currentLevel;
            Cost = cost;
            PurchaseStatus = purchaseStatus;
        }

        public ResearchNodeDefinition Definition { get; }
        public int CurrentLevel { get; }
        public long Cost { get; }
        public ResearchPurchaseStatus PurchaseStatus { get; }
        public bool IsMaxLevel => CurrentLevel >= Definition.MaxLevel;
        public bool CanPurchase => PurchaseStatus == ResearchPurchaseStatus.Success;
    }

    public sealed class ResearchService
    {
        private readonly MiningContentCatalog catalog;

        public ResearchService(MiningContentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public IReadOnlyList<ResearchNodeState> GetNodeStates(
            GameSaveData player,
            bool featureUnlocked)
        {
            var states = new List<ResearchNodeState>();
            foreach (var definition in catalog.GetResearchNodes())
            {
                var level = GetLevel(player, definition.NodeId);
                states.Add(new ResearchNodeState(
                    definition,
                    level,
                    definition.GetCost(level),
                    GetPurchaseStatus(player, definition, featureUnlocked)));
            }

            return states;
        }

        public ResearchPurchaseStatus TryPurchase(
            GameSaveData player,
            string nodeId,
            bool featureUnlocked)
        {
            var definition = catalog.GetResearchNode(nodeId);
            if (definition == null)
            {
                return ResearchPurchaseStatus.UnknownNode;
            }

            var status = GetPurchaseStatus(player, definition, featureUnlocked);
            if (status != ResearchPurchaseStatus.Success)
            {
                return status;
            }

            var currentLevel = GetLevel(player, definition.NodeId);
            var cost = definition.GetCost(currentLevel);
            player.blueprintCores -= cost;
            SetLevel(player, definition.NodeId, currentLevel + 1);
            return ResearchPurchaseStatus.Success;
        }

        public float GetPowerMultiplier(GameSaveData player)
        {
            var bonus = 0f;
            foreach (var definition in catalog.GetResearchNodes())
            {
                bonus += GetLevel(player, definition.NodeId) * definition.PowerBonusPerLevel;
            }

            return Math.Max(1f, 1f + bonus);
        }

        public int GetLevel(GameSaveData player, string nodeId)
        {
            if (player?.researchProgress == null || string.IsNullOrWhiteSpace(nodeId))
            {
                return 0;
            }

            var level = 0;
            foreach (var progress in player.researchProgress)
            {
                if (progress != null && progress.nodeId == nodeId)
                {
                    level = Math.Max(level, progress.level);
                }
            }

            var definition = catalog.GetResearchNode(nodeId);
            return definition == null ? Math.Max(0, level) : Math.Clamp(level, 0, definition.MaxLevel);
        }

        private ResearchPurchaseStatus GetPurchaseStatus(
            GameSaveData player,
            ResearchNodeDefinition definition,
            bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return ResearchPurchaseStatus.FeatureLocked;
            }

            var currentLevel = GetLevel(player, definition.NodeId);
            if (currentLevel >= definition.MaxLevel)
            {
                return ResearchPurchaseStatus.MaxLevel;
            }

            if (!string.IsNullOrWhiteSpace(definition.PrerequisiteNodeId) &&
                GetLevel(player, definition.PrerequisiteNodeId) < definition.PrerequisiteLevel)
            {
                return ResearchPurchaseStatus.PrerequisiteMissing;
            }

            return player.blueprintCores >= definition.GetCost(currentLevel)
                ? ResearchPurchaseStatus.Success
                : ResearchPurchaseStatus.InsufficientCores;
        }

        private static void SetLevel(GameSaveData player, string nodeId, int level)
        {
            var normalized = new List<ResearchProgressData>();
            var updated = false;
            if (player.researchProgress != null)
            {
                foreach (var progress in player.researchProgress)
                {
                    if (progress == null || string.IsNullOrWhiteSpace(progress.nodeId))
                    {
                        continue;
                    }

                    if (progress.nodeId == nodeId)
                    {
                        if (updated)
                        {
                            continue;
                        }

                        normalized.Add(new ResearchProgressData
                        {
                            nodeId = nodeId,
                            level = Math.Max(0, level)
                        });
                        updated = true;
                    }
                    else
                    {
                        normalized.Add(progress);
                    }
                }
            }

            if (!updated)
            {
                normalized.Add(new ResearchProgressData
                {
                    nodeId = nodeId,
                    level = Math.Max(0, level)
                });
            }

            player.researchProgress = normalized.ToArray();
        }
    }
}
