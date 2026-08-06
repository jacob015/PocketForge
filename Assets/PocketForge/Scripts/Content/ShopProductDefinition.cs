using System;
using UnityEngine;

namespace PocketForge.Content
{
    public enum ShopProductKind
    {
        DailyFree,
        RewardedAd,
        GemExchange,
        Iap
    }

    [Serializable]
    public sealed class ShopProductDefinition
    {
        [SerializeField] private string productId = string.Empty;
        [SerializeField] private string titleLocalizationKey = string.Empty;
        [SerializeField] private string descriptionLocalizationKey = string.Empty;
        [SerializeField] private ShopProductKind kind;
        [SerializeField, Min(0)] private long rewardCredits;
        [SerializeField, Min(0)] private long rewardGems;
        [SerializeField, Min(0)] private long rewardBlueprintCores;
        [SerializeField, Min(0)] private long costGems;
        [SerializeField, Min(0)] private int dailyLimit;
        [SerializeField] private string iapProductId = string.Empty;

        public string ProductId => productId;
        public string TitleLocalizationKey => titleLocalizationKey;
        public string DescriptionLocalizationKey => descriptionLocalizationKey;
        public ShopProductKind Kind => kind;
        public long RewardCredits => Math.Max(0L, rewardCredits);
        public long RewardGems => Math.Max(0L, rewardGems);
        public long RewardBlueprintCores => Math.Max(0L, rewardBlueprintCores);
        public long CostGems => Math.Max(0L, costGems);
        public int DailyLimit => Math.Max(0, dailyLimit);
        public string IapProductId => iapProductId;

        public static ShopProductDefinition[] CreateRuntimeDefaults()
        {
            return new[]
            {
                Create("daily_supply", "shop_daily_supply", "shop_daily_supply_desc",
                    ShopProductKind.DailyFree, 125, 0, 0, 0, 1),
                Create("rewarded_supply", "shop_rewarded_supply", "shop_rewarded_supply_desc",
                    ShopProductKind.RewardedAd, 200, 0, 0, 0, 3),
                Create("gem_credit_crate", "shop_credit_crate", "shop_credit_crate_desc",
                    ShopProductKind.GemExchange, 600, 0, 0, 5, 0),
                Create("gem_core_crate", "shop_core_crate", "shop_core_crate_desc",
                    ShopProductKind.GemExchange, 0, 0, 3, 10, 0),
                Create("remove_ads", "remove_ads", "shop_remove_ads_desc",
                    ShopProductKind.Iap, 0, 0, 0, 0, 0, "remove_ads"),
                Create("starter_pack", "shop_starter_pack", "shop_starter_pack_desc",
                    ShopProductKind.Iap, 2000, 20, 6, 0, 0, "starter_pack")
            };
        }

        private static ShopProductDefinition Create(
            string id,
            string titleKey,
            string descriptionKey,
            ShopProductKind productKind,
            long credits,
            long gems,
            long cores,
            long gemCost,
            int limit,
            string storeProductId = "")
        {
            return new ShopProductDefinition
            {
                productId = id,
                titleLocalizationKey = titleKey,
                descriptionLocalizationKey = descriptionKey,
                kind = productKind,
                rewardCredits = Math.Max(0L, credits),
                rewardGems = Math.Max(0L, gems),
                rewardBlueprintCores = Math.Max(0L, cores),
                costGems = Math.Max(0L, gemCost),
                dailyLimit = Math.Max(0, limit),
                iapProductId = storeProductId ?? string.Empty
            };
        }
    }
}
