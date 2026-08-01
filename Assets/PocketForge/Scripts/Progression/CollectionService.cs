using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Content;
using PocketForge.Save;

namespace PocketForge.Progression
{
    public readonly struct OreCollectionState
    {
        public OreCollectionState(OreDefinition definition, long minedCount, float powerBonus)
        {
            Definition = definition;
            MinedCount = Math.Max(0L, minedCount);
            PowerBonus = Math.Max(0f, powerBonus);
        }

        public OreDefinition Definition { get; }
        public long MinedCount { get; }
        public float PowerBonus { get; }
        public bool IsDiscovered => MinedCount > 0;
    }

    public sealed class CollectionService
    {
        private readonly MiningContentCatalog catalog;
        private readonly OreDefinition[] definitions;
        private readonly int[] milestones;
        private readonly HashSet<string> knownIds;

        public CollectionService(MiningContentCatalog catalog)
        {
            this.catalog = catalog;
            definitions = catalog.GetOreDefinitions().ToArray();
            milestones = catalog.GetCollectionMilestones().ToArray();
            knownIds = new HashSet<string>(
                definitions.Select(definition => definition.ContentId),
                StringComparer.Ordinal);
        }

        public void SanitizeAgainstCatalog(GameSaveData player)
        {
            player.oreCollection = (player.oreCollection ?? Array.Empty<OreCollectionData>())
                .Where(entry => entry != null &&
                                knownIds.Contains(entry.contentId) &&
                                entry.minedCount > 0)
                .GroupBy(entry => entry.contentId, StringComparer.Ordinal)
                .Select(group => new OreCollectionData
                {
                    contentId = group.Key,
                    minedCount = group.Max(entry => entry.minedCount)
                })
                .ToArray();
        }

        public void RecordMinedOre(GameSaveData player, string contentId, long amount = 1L)
        {
            if (player == null || amount <= 0L || !knownIds.Contains(contentId))
            {
                return;
            }

            SanitizeAgainstCatalog(player);
            var entries = player.oreCollection.ToList();
            var entry = entries.FirstOrDefault(candidate => candidate.contentId == contentId);
            if (entry == null)
            {
                entry = new OreCollectionData { contentId = contentId };
                entries.Add(entry);
            }

            entry.minedCount = SaturatingAdd(entry.minedCount, amount);
            player.oreCollection = entries.ToArray();
        }

        public IReadOnlyList<OreCollectionState> GetStates(GameSaveData player)
        {
            return definitions
                .Select(definition =>
                {
                    var count = GetMinedCount(player, definition.ContentId);
                    return new OreCollectionState(definition, count, GetPowerBonus(count));
                })
                .ToArray();
        }

        public long GetTotalMined(GameSaveData player)
        {
            var total = 0L;
            foreach (var entry in player.oreCollection ?? Array.Empty<OreCollectionData>())
            {
                if (entry != null)
                {
                    total = SaturatingAdd(total, entry.minedCount);
                }
            }

            return total;
        }

        public float GetPowerMultiplier(GameSaveData player)
        {
            var bonus = 0f;
            foreach (var definition in definitions)
            {
                bonus += GetPowerBonus(GetMinedCount(player, definition.ContentId));
            }

            return Math.Max(1f, 1f + bonus);
        }

        private long GetMinedCount(GameSaveData player, string contentId)
        {
            var result = 0L;
            foreach (var entry in player.oreCollection ?? Array.Empty<OreCollectionData>())
            {
                if (entry != null && entry.contentId == contentId)
                {
                    result = Math.Max(result, entry.minedCount);
                }
            }

            return result;
        }

        private float GetPowerBonus(long minedCount)
        {
            if (minedCount <= 0L)
            {
                return 0f;
            }

            var bonus = catalog.CollectionDiscoveryPowerBonus;
            foreach (var milestone in milestones)
            {
                if (minedCount >= milestone)
                {
                    bonus += catalog.CollectionMilestonePowerBonus;
                }
            }

            return Math.Max(0f, bonus);
        }

        private static long SaturatingAdd(long left, long right)
        {
            left = Math.Max(0L, left);
            right = Math.Max(0L, right);
            return left > long.MaxValue - right ? long.MaxValue : left + right;
        }
    }
}
