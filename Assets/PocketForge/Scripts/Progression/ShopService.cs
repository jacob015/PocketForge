using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PocketForge.Content;
using PocketForge.Save;

namespace PocketForge.Progression
{
    public enum ShopActionStatus
    {
        Success,
        FeatureLocked,
        UnknownProduct,
        WrongPurchaseMethod,
        DailyLimitReached,
        InsufficientGems,
        AlreadyOwned
    }

    public readonly struct ShopProductState
    {
        public ShopProductState(ShopProductDefinition definition, int claimedCount, bool owned)
        {
            Definition = definition;
            ClaimedCount = Math.Max(0, claimedCount);
            Owned = owned;
        }

        public ShopProductDefinition Definition { get; }
        public int ClaimedCount { get; }
        public bool Owned { get; }
        public int Remaining => Definition.DailyLimit <= 0
            ? int.MaxValue
            : Math.Max(0, Definition.DailyLimit - ClaimedCount);
    }

    public readonly struct ShopBoardState
    {
        public ShopBoardState(
            string periodKey,
            long refreshAtUnixSeconds,
            IReadOnlyList<ShopProductState> products)
        {
            PeriodKey = periodKey ?? string.Empty;
            RefreshAtUnixSeconds = Math.Max(0L, refreshAtUnixSeconds);
            Products = products ?? Array.Empty<ShopProductState>();
        }

        public string PeriodKey { get; }
        public long RefreshAtUnixSeconds { get; }
        public IReadOnlyList<ShopProductState> Products { get; }
    }

    public readonly struct ShopActionResult
    {
        public ShopActionResult(
            ShopActionStatus status,
            ShopProductDefinition definition = null,
            long rewardCredits = 0L,
            long rewardGems = 0L,
            long rewardBlueprintCores = 0L)
        {
            Status = status;
            Definition = definition;
            RewardCredits = Math.Max(0L, rewardCredits);
            RewardGems = Math.Max(0L, rewardGems);
            RewardBlueprintCores = Math.Max(0L, rewardBlueprintCores);
        }

        public ShopActionStatus Status { get; }
        public ShopProductDefinition Definition { get; }
        public long RewardCredits { get; }
        public long RewardGems { get; }
        public long RewardBlueprintCores { get; }
    }

    public sealed class ShopService
    {
        private readonly ShopProductDefinition[] definitions;
        private readonly Dictionary<string, ShopProductDefinition> definitionsById;

        public ShopService(MiningContentCatalog catalog)
        {
            definitions = catalog.GetShopProducts().ToArray();
            definitionsById = definitions.ToDictionary(
                definition => definition.ProductId,
                StringComparer.Ordinal);
        }

        public bool Refresh(GameSaveData player, long nowUtcUnixSeconds, bool featureUnlocked)
        {
            if (!featureUnlocked || nowUtcUnixSeconds <= 0L)
            {
                return false;
            }

            var effectiveNow = Math.Max(player.lastObservedShopUnixSeconds, nowUtcUnixSeconds);
            player.lastObservedShopUnixSeconds = effectiveNow;
            var periodKey = GetDailyPeriodKey(effectiveNow);
            player.dailyShop ??= new DailyShopData();
            if (player.dailyShop.periodKey == periodKey)
            {
                return false;
            }

            player.dailyShop = new DailyShopData
            {
                periodKey = periodKey,
                claimedProductIds = Array.Empty<string>(),
                claimCounts = Array.Empty<ShopClaimCountData>()
            };
            return true;
        }

        public ShopBoardState GetBoard(
            GameSaveData player,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            if (!featureUnlocked)
            {
                return new ShopBoardState(string.Empty, 0L, Array.Empty<ShopProductState>());
            }

            Refresh(player, nowUtcUnixSeconds, true);
            var states = definitions.Select(definition => new ShopProductState(
                definition,
                GetClaimCount(player.dailyShop, definition),
                IsOwned(player, definition))).ToArray();
            var effectiveNow = Math.Max(player.lastObservedShopUnixSeconds, nowUtcUnixSeconds);
            return new ShopBoardState(
                player.dailyShop.periodKey,
                GetNextDailyRefresh(effectiveNow),
                states);
        }

        public ShopActionResult ClaimDaily(
            GameSaveData player,
            string productId,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            var validation = Validate(player, productId, ShopProductKind.DailyFree,
                nowUtcUnixSeconds, featureUnlocked, out var definition);
            if (validation != ShopActionStatus.Success)
            {
                return new ShopActionResult(validation, definition);
            }

            AddClaim(player.dailyShop, definition, true);
            return GrantRewards(player, definition);
        }

        public ShopActionResult GrantRewarded(
            GameSaveData player,
            string productId,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            var validation = Validate(player, productId, ShopProductKind.RewardedAd,
                nowUtcUnixSeconds, featureUnlocked, out var definition);
            if (validation != ShopActionStatus.Success)
            {
                return new ShopActionResult(validation, definition);
            }

            AddClaim(player.dailyShop, definition, false);
            return GrantRewards(player, definition);
        }

        public ShopActionResult PurchaseWithGems(
            GameSaveData player,
            string productId,
            long nowUtcUnixSeconds,
            bool featureUnlocked)
        {
            var validation = Validate(player, productId, ShopProductKind.GemExchange,
                nowUtcUnixSeconds, featureUnlocked, out var definition);
            if (validation != ShopActionStatus.Success)
            {
                return new ShopActionResult(validation, definition);
            }

            if (player.gems < definition.CostGems)
            {
                return new ShopActionResult(ShopActionStatus.InsufficientGems, definition);
            }

            player.gems -= definition.CostGems;
            return GrantRewards(player, definition);
        }

        public ShopActionResult GrantStarterPack(GameSaveData player)
        {
            if (!definitionsById.TryGetValue("starter_pack", out var definition))
            {
                return new ShopActionResult(ShopActionStatus.UnknownProduct);
            }

            if (player.starterPackPurchased)
            {
                return new ShopActionResult(ShopActionStatus.AlreadyOwned, definition);
            }

            player.starterPackPurchased = true;
            return GrantRewards(player, definition);
        }

        public ShopProductDefinition GetProduct(string productId) =>
            definitionsById.TryGetValue(productId ?? string.Empty, out var definition)
                ? definition
                : null;

        private ShopActionStatus Validate(
            GameSaveData player,
            string productId,
            ShopProductKind expectedKind,
            long nowUtcUnixSeconds,
            bool featureUnlocked,
            out ShopProductDefinition definition)
        {
            definition = null;
            if (!featureUnlocked)
            {
                return ShopActionStatus.FeatureLocked;
            }

            if (!definitionsById.TryGetValue(productId ?? string.Empty, out definition))
            {
                return ShopActionStatus.UnknownProduct;
            }

            if (definition.Kind != expectedKind)
            {
                return ShopActionStatus.WrongPurchaseMethod;
            }

            Refresh(player, nowUtcUnixSeconds, true);
            if (definition.DailyLimit > 0 &&
                GetClaimCount(player.dailyShop, definition) >= definition.DailyLimit)
            {
                return ShopActionStatus.DailyLimitReached;
            }

            return ShopActionStatus.Success;
        }

        private static ShopActionResult GrantRewards(
            GameSaveData player,
            ShopProductDefinition definition)
        {
            var credits = AddSafely(ref player.credits, definition.RewardCredits);
            var gems = AddSafely(ref player.gems, definition.RewardGems);
            var cores = AddSafely(ref player.blueprintCores, definition.RewardBlueprintCores);
            return new ShopActionResult(
                ShopActionStatus.Success,
                definition,
                credits,
                gems,
                cores);
        }

        private static int GetClaimCount(DailyShopData shop, ShopProductDefinition definition)
        {
            shop ??= new DailyShopData();
            if (definition.Kind == ShopProductKind.DailyFree)
            {
                return (shop.claimedProductIds ?? Array.Empty<string>())
                    .Contains(definition.ProductId, StringComparer.Ordinal) ? 1 : 0;
            }

            return (shop.claimCounts ?? Array.Empty<ShopClaimCountData>())
                .FirstOrDefault(entry => entry != null && entry.productId == definition.ProductId)
                ?.count ?? 0;
        }

        private static void AddClaim(
            DailyShopData shop,
            ShopProductDefinition definition,
            bool unique)
        {
            if (unique)
            {
                shop.claimedProductIds = (shop.claimedProductIds ?? Array.Empty<string>())
                    .Append(definition.ProductId)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                return;
            }

            var counts = (shop.claimCounts ?? Array.Empty<ShopClaimCountData>()).ToList();
            var entry = counts.FirstOrDefault(candidate =>
                candidate != null && candidate.productId == definition.ProductId);
            if (entry == null)
            {
                entry = new ShopClaimCountData { productId = definition.ProductId };
                counts.Add(entry);
            }

            entry.count = Math.Min(int.MaxValue, Math.Max(0, entry.count) + 1);
            shop.claimCounts = counts.ToArray();
        }

        private static bool IsOwned(GameSaveData player, ShopProductDefinition definition)
        {
            return definition.ProductId switch
            {
                "remove_ads" => player.adsRemoved,
                "starter_pack" => player.starterPackPurchased,
                _ => false
            };
        }

        private static string GetDailyPeriodKey(long utcUnixSeconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(Math.Max(0L, utcUnixSeconds))
                .UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        }

        private static long GetNextDailyRefresh(long utcUnixSeconds)
        {
            var next = DateTimeOffset.FromUnixTimeSeconds(Math.Max(0L, utcUnixSeconds))
                .UtcDateTime.Date.AddDays(1);
            return new DateTimeOffset(DateTime.SpecifyKind(next, DateTimeKind.Utc))
                .ToUnixTimeSeconds();
        }

        private static long AddSafely(ref long currency, long amount)
        {
            currency = Math.Max(0L, currency);
            amount = Math.Max(0L, amount);
            var granted = Math.Min(long.MaxValue - currency, amount);
            currency += granted;
            return granted;
        }
    }
}
