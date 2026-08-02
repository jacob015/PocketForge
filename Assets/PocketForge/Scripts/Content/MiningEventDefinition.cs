using System;
using UnityEngine;

namespace PocketForge.Content
{
    public enum EventRewardType
    {
        Credits,
        Gems,
        BlueprintCores
    }

    [Serializable]
    public sealed class EventRewardTierDefinition
    {
        [SerializeField] private string tierId = string.Empty;
        [SerializeField, Min(1)] private long requiredTokens = 1;
        [SerializeField] private EventRewardType rewardType;
        [SerializeField, Min(1)] private long rewardAmount = 1;

        public string TierId => tierId;
        public long RequiredTokens => Math.Max(1L, requiredTokens);
        public EventRewardType RewardType => rewardType;
        public long RewardAmount => Math.Max(1L, rewardAmount);

        internal static EventRewardTierDefinition Create(
            string id,
            long requirement,
            EventRewardType type,
            long amount)
        {
            return new EventRewardTierDefinition
            {
                tierId = id,
                requiredTokens = Math.Max(1L, requirement),
                rewardType = type,
                rewardAmount = Math.Max(1L, amount)
            };
        }
    }

    [Serializable]
    public sealed class MiningEventDefinition
    {
        [SerializeField] private string eventId = string.Empty;
        [SerializeField] private string titleLocalizationKey = string.Empty;
        [SerializeField] private string descriptionLocalizationKey = string.Empty;
        [SerializeField, Min(1)] private int oresPerToken = 5;
        [SerializeField] private EventRewardTierDefinition[] rewardTiers = Array.Empty<EventRewardTierDefinition>();
        [Header("Token Exchange")]
        [SerializeField, Min(1)] private long exchangeCostTokens = 20;
        [SerializeField] private EventRewardType exchangeRewardType = EventRewardType.BlueprintCores;
        [SerializeField, Min(1)] private long exchangeRewardAmount = 4;
        [SerializeField, Min(1)] private int exchangeLimit = 2;

        public string EventId => eventId;
        public string TitleLocalizationKey => titleLocalizationKey;
        public string DescriptionLocalizationKey => descriptionLocalizationKey;
        public int OresPerToken => Math.Max(1, oresPerToken);
        public EventRewardTierDefinition[] RewardTiers => rewardTiers ?? Array.Empty<EventRewardTierDefinition>();
        public long ExchangeCostTokens => Math.Max(1L, exchangeCostTokens);
        public EventRewardType ExchangeRewardType => exchangeRewardType;
        public long ExchangeRewardAmount => Math.Max(1L, exchangeRewardAmount);
        public int ExchangeLimit => Math.Max(1, exchangeLimit);

        public static MiningEventDefinition[] CreateRuntimeDefaults()
        {
            return new[]
            {
                new MiningEventDefinition
                {
                    eventId = "crystal_rush",
                    titleLocalizationKey = "event_crystal_rush",
                    descriptionLocalizationKey = "event_crystal_rush_desc",
                    oresPerToken = 5,
                    rewardTiers = new[]
                    {
                        EventRewardTierDefinition.Create("crystal_rush_10", 10, EventRewardType.Credits, 500),
                        EventRewardTierDefinition.Create("crystal_rush_30", 30, EventRewardType.BlueprintCores, 3),
                        EventRewardTierDefinition.Create("crystal_rush_60", 60, EventRewardType.Gems, 3)
                    },
                    exchangeCostTokens = 20,
                    exchangeRewardType = EventRewardType.BlueprintCores,
                    exchangeRewardAmount = 4,
                    exchangeLimit = 2
                }
            };
        }
    }
}
