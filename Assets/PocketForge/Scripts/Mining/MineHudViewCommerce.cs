using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Ads;
using PocketForge.Content;
using PocketForge.Iap;
using PocketForge.Localization;
using PocketForge.Presentation;
using PocketForge.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace PocketForge.Mining
{
    public sealed partial class MineHudView
    {
        private enum CommerceTab
        {
            Shop,
            Event
        }

        private sealed class CommerceRowView
        {
            public Image Surface;
            public Image Icon;
            public Text Title;
            public Text Description;
            public Text Value;
            public Button Action;
            public string Id = string.Empty;
        }

        private GameObject commercePanel;
        private Image commerceCard;
        private Image commerceTitleSurface;
        private Text commerceTitle;
        private Button closeCommerceCornerButton;
        private Button shopTabButton;
        private Button eventTabButton;
        private Image commerceSummarySurface;
        private Text commerceSummary;
        private Text commerceRefresh;
        private readonly List<CommerceRowView> commerceRows = new();
        private Button closeCommerceButton;
        private CommerceTab selectedCommerceTab;
        private Action<string> dailyShopAction;
        private Action<string> rewardedShopAction;
        private Action<string> gemShopAction;
        private Action<string> eventRewardAction;
        private Action eventExchangeAction;
        private Action purchaseRemoveAdsAction;
        private Action purchaseStarterPackAction;
        private IapState starterPackState = IapState.Initializing;
        private string starterPackPrice = string.Empty;
        private bool starterPackOwned;
        private bool starterPackAvailable;

        public void BindCommerce(
            Action<string> claimDaily,
            Action<string> requestRewarded,
            Action<string> purchaseWithGems,
            Action<string> claimEventReward,
            Action purchaseEventExchange)
        {
            dailyShopAction = claimDaily;
            rewardedShopAction = requestRewarded;
            gemShopAction = purchaseWithGems;
            eventRewardAction = claimEventReward;
            eventExchangeAction = purchaseEventExchange;
        }

        public void BindCommerceIap(Action purchaseRemoveAds, Action purchaseStarterPack)
        {
            purchaseRemoveAdsAction = purchaseRemoveAds;
            purchaseStarterPackAction = purchaseStarterPack;
        }

        public void SetStarterPackState(
            IapState state,
            string localizedPrice,
            bool owned,
            bool available)
        {
            starterPackState = state;
            starterPackPrice = localizedPrice ?? string.Empty;
            starterPackOwned = owned;
            starterPackAvailable = available;
            RenderCommerce();
        }

        private void CreateCommercePanel()
        {
            var backdrop = CreatePanel(
                "CommerceBackdrop", transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.005f, 0.012f, 0.04f, 0.86f));
            commercePanel = backdrop.gameObject;
            commerceCard = CreatePanel(
                "CommerceCard", commercePanel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 24f), new Vector2(920f, 1700f),
                new Color(0.035f, 0.075f, 0.15f, 0.995f));
            commerceTitleSurface = CreatePanel(
                "CommerceTitleSurface", commerceCard.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-30f, -62f), new Vector2(610f, 124f), Color.white);
            commerceTitle = CreateText(
                "CommerceTitle", commerceTitleSurface.transform,
                Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -8f),
                44, TextAnchor.MiddleCenter);
            ConfigureCommerceText(commerceTitle, 24, 44);

            closeCommerceCornerButton = CreateButton(
                "CloseCommerceCorner", commerceCard.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-70f, -70f), new Vector2(84f, 84f), Color.white);
            closeCommerceCornerButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var cornerIcon = CreateSimpleImage(
                "Icon", closeCommerceCornerButton.transform,
                Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-22f, -22f), Color.white);
            cornerIcon.raycastTarget = false;
            closeCommerceCornerButton.onClick.AddListener(() => commercePanel.SetActive(false));

            shopTabButton = CreateCommerceTabButton("ShopTab", new Vector2(-206f, 650f));
            eventTabButton = CreateCommerceTabButton("EventTab", new Vector2(206f, 650f));
            shopTabButton.onClick.AddListener(() => SelectCommerceTab(CommerceTab.Shop));
            eventTabButton.onClick.AddListener(() => SelectCommerceTab(CommerceTab.Event));

            commerceSummarySurface = CreatePanel(
                "CommerceSummarySurface", commerceCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 540f), new Vector2(824f, 130f), Color.white);
            commerceSummary = CreateText(
                "CommerceSummary", commerceSummarySurface.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(270f, 0f), new Vector2(480f, 68f), 28, TextAnchor.MiddleLeft);
            ConfigureCommerceText(commerceSummary, 18, 28);
            commerceRefresh = CreateText(
                "CommerceRefresh", commerceSummarySurface.transform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-160f, 0f), new Vector2(280f, 58f), 21, TextAnchor.MiddleRight);
            ConfigureCommerceText(commerceRefresh, 15, 21);

            var rowY = new[] { 365f, 185f, 5f, -175f, -355f, -535f };
            for (var index = 0; index < rowY.Length; index++)
            {
                commerceRows.Add(CreateCommerceRow(index, rowY[index]));
            }

            closeCommerceButton = CreateButton(
                "CloseCommerceButton", commerceCard.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 62f), new Vector2(360f, 88f), Color.white);
            ConfigureCommerceButton(closeCommerceButton);
            closeCommerceButton.onClick.AddListener(() => commercePanel.SetActive(false));
            commercePanel.SetActive(false);
        }

        private Button CreateCommerceTabButton(string name, Vector2 position)
        {
            var button = CreateButton(
                name, commerceCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(400f, 92f), Color.white);
            ConfigureCommerceButton(button);
            return button;
        }

        private CommerceRowView CreateCommerceRow(int index, float y)
        {
            var row = new CommerceRowView();
            row.Surface = CreatePanel(
                $"CommerceRow{index}", commerceCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, y), new Vector2(824f, 160f), Color.white);
            row.Icon = CreateSimpleImage(
                "CommerceIcon", row.Surface.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(76f, 0f), new Vector2(104f, 104f), Color.white);
            row.Icon.raycastTarget = false;
            row.Title = CreateText(
                "CommerceName", row.Surface.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(318f, 30f), new Vector2(320f, 54f), 26, TextAnchor.MiddleLeft);
            ConfigureCommerceText(row.Title, 17, 26);
            row.Description = CreateText(
                "CommerceDescription", row.Surface.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(318f, -32f), new Vector2(320f, 54f), 19, TextAnchor.MiddleLeft);
            ConfigureCommerceText(row.Description, 14, 19);
            row.Value = CreateText(
                "CommerceValue", row.Surface.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                // Tall enough for the three-line starter pack reward at the readable size.
                new Vector2(130f, 0f), new Vector2(120f, 100f), 22, TextAnchor.MiddleCenter);
            ConfigureCommerceText(row.Value, 15, 22);
            row.Action = CreateButton(
                "CommerceAction", row.Surface.transform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-98f, 0f), new Vector2(168f, 104f), Color.white);
            ConfigureCommerceButton(row.Action);
            row.Action.onClick.AddListener(() => ActivateCommerceRow(row));
            return row;
        }

        private void ShowCommerce()
        {
            if (lastState == null || lastService == null)
            {
                return;
            }

            if (!lastService.IsFeatureUnlocked(lastState.Player.minerLevel, ProgressionFeature.Shop))
            {
                ShowFeedback(LanguageService.Get("shop_locked"), new Color(0.66f, 0.82f, 1f));
                return;
            }

            selectedCommerceTab = CommerceTab.Shop;
            commercePanel.SetActive(true);
            commercePanel.transform.SetAsLastSibling();
            RenderCommerce();
        }

        private void SelectCommerceTab(CommerceTab tab)
        {
            if (tab == CommerceTab.Event &&
                !lastService.IsFeatureUnlocked(lastState.Player.minerLevel, ProgressionFeature.Events))
            {
                ShowFeedback(LanguageService.Get("events_locked"), new Color(0.66f, 0.82f, 1f));
                return;
            }

            selectedCommerceTab = tab;
            RenderCommerce();
        }

        private void RenderCommerce()
        {
            if (commercePanel == null || !commercePanel.activeSelf ||
                lastState == null || lastService == null)
            {
                return;
            }

            commerceTitle.text = LanguageService.Get("commerce_title").ToUpper();
            shopTabButton.GetComponentInChildren<Text>().text = LanguageService.Get("feature_shop").ToUpper();
            eventTabButton.GetComponentInChildren<Text>().text = LanguageService.Get("feature_events").ToUpper();
            closeCommerceButton.GetComponentInChildren<Text>().text = LanguageService.Get("close").ToUpper();
            shopTabButton.interactable = selectedCommerceTab != CommerceTab.Shop;
            eventTabButton.interactable = selectedCommerceTab != CommerceTab.Event;
            ApplyCommerceTabSkin(shopTabButton, selectedCommerceTab == CommerceTab.Shop);
            ApplyCommerceTabSkin(eventTabButton, selectedCommerceTab == CommerceTab.Event);

            if (selectedCommerceTab == CommerceTab.Shop)
            {
                RenderShopRows();
            }
            else
            {
                RenderEventRows();
            }
        }

        private void RenderShopRows()
        {
            var board = lastService.GetShopBoard(lastState);
            commerceSummary.text = LanguageService.Get("shop_summary");
            commerceRefresh.text = string.Format(
                LanguageService.Get("shop_refresh_in"), FormatCommerceRefresh(board.RefreshAtUnixSeconds));
            for (var index = 0; index < commerceRows.Count; index++)
            {
                var row = commerceRows[index];
                var active = index < board.Products.Count;
                row.Surface.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                var state = board.Products[index];
                var definition = state.Definition;
                row.Id = definition.ProductId;
                row.Title.text = LanguageService.Get(definition.TitleLocalizationKey).ToUpper();
                row.Description.text = LanguageService.Get(definition.DescriptionLocalizationKey);
                row.Icon.sprite = GetShopProductSprite(definition);
                row.Icon.type = Image.Type.Simple;
                row.Icon.preserveAspect = true;
                RenderShopAction(row, state);
            }
        }

        private void RenderShopAction(CommerceRowView row, ShopProductState state)
        {
            var definition = state.Definition;
            var interactable = true;
            string label;
            switch (definition.Kind)
            {
                case ShopProductKind.DailyFree:
                    label = state.Remaining <= 0 ? LanguageService.Get("claimed") : LanguageService.Get("free");
                    row.Value.text = FormatShopRewards(definition);
                    interactable = state.Remaining > 0;
                    break;
                case ShopProductKind.RewardedAd:
                    label = rewardedAdState == RewardedAdState.Failed
                        ? LanguageService.Get("ad_retry")
                        : LanguageService.Get("watch_ad");
                    row.Value.text = $"{FormatShopRewards(definition)}\n{state.ClaimedCount}/{definition.DailyLimit}";
                    interactable = state.Remaining > 0 &&
                                   rewardedAdState is RewardedAdState.Ready or RewardedAdState.Failed;
                    break;
                case ShopProductKind.GemExchange:
                    label = LanguageService.Get("buy");
                    row.Value.text = $"{CompactNumberFormatter.Format(definition.CostGems)} \u25C6";
                    interactable = lastState.Player.gems >= definition.CostGems;
                    break;
                default:
                    var starter = definition.ProductId == "starter_pack";
                    var owned = starter ? starterPackOwned : adsRemoved;
                    var available = starter ? starterPackAvailable : !string.IsNullOrWhiteSpace(removeAdsPrice);
                    var price = starter ? starterPackPrice : removeAdsPrice;
                    label = owned
                        ? LanguageService.Get("owned")
                        : available ? price : LanguageService.Get("unavailable");
                    row.Value.text = starter ? FormatShopRewards(definition) : LanguageService.Get("shop_permanent");
                    interactable = !owned && available &&
                                   (starter ? starterPackState : iapState) is not (IapState.Purchasing or IapState.Restoring);
                    break;
            }

            row.Action.interactable = interactable;
            row.Action.GetComponentInChildren<Text>().text = label.ToUpper();
            ApplyCommerceActionSkin(row.Action, interactable);
        }

        private void RenderEventRows()
        {
            var board = lastService.GetMiningEventBoard(lastState);
            if (board.Definition == null)
            {
                commerceSummary.text = LanguageService.Get("event_unavailable");
                commerceRefresh.text = string.Empty;
                foreach (var row in commerceRows)
                {
                    row.Surface.gameObject.SetActive(false);
                }
                return;
            }

            commerceSummary.text = $"{LanguageService.Get(board.Definition.TitleLocalizationKey)}  " +
                                   $"{LanguageService.Get("event_tokens")}: {CompactNumberFormatter.Format(board.TokenBalance)}";
            commerceRefresh.text = string.Format(
                LanguageService.Get("event_ends_in"), FormatCommerceRefresh(board.RefreshAtUnixSeconds));
            var rowIndex = 0;
            foreach (var reward in board.Rewards)
            {
                var row = commerceRows[rowIndex++];
                row.Surface.gameObject.SetActive(true);
                row.Id = reward.Definition.TierId;
                row.Title.text = string.Format(
                    LanguageService.Get("event_tier_title"),
                    CompactNumberFormatter.Format(reward.Definition.RequiredTokens));
                row.Description.text = board.Definition.DescriptionLocalizationKey.Length > 0
                    ? LanguageService.Get(board.Definition.DescriptionLocalizationKey)
                    : string.Empty;
                row.Value.text = FormatEventReward(reward.Definition.RewardType, reward.Definition.RewardAmount);
                row.Icon.sprite = GetEventRewardSprite(reward.Definition.RewardType);
                var canClaim = !reward.Claimed && board.EarnedTokens >= reward.Definition.RequiredTokens;
                row.Action.interactable = canClaim;
                row.Action.GetComponentInChildren<Text>().text = LanguageService.Get(
                    reward.Claimed ? "claimed" : canClaim ? "claim" : "in_progress").ToUpper();
                ApplyCommerceActionSkin(row.Action, canClaim);
            }

            var exchange = commerceRows[rowIndex++];
            exchange.Surface.gameObject.SetActive(true);
            exchange.Id = "event_exchange";
            exchange.Title.text = LanguageService.Get("event_exchange").ToUpper();
            exchange.Description.text = string.Format(
                LanguageService.Get("event_exchange_limit"),
                board.ExchangePurchases,
                board.Definition.ExchangeLimit);
            exchange.Value.text = $"{CompactNumberFormatter.Format(board.Definition.ExchangeCostTokens)} " +
                                  LanguageService.Get("event_tokens");
            exchange.Icon.sprite = GetEventRewardSprite(board.Definition.ExchangeRewardType);
            var canExchange = board.TokenBalance >= board.Definition.ExchangeCostTokens &&
                              board.ExchangePurchases < board.Definition.ExchangeLimit;
            exchange.Action.interactable = canExchange;
            exchange.Action.GetComponentInChildren<Text>().text = LanguageService.Get("exchange").ToUpper();
            ApplyCommerceActionSkin(exchange.Action, canExchange);

            while (rowIndex < commerceRows.Count)
            {
                commerceRows[rowIndex++].Surface.gameObject.SetActive(false);
            }
        }

        private void ActivateCommerceRow(CommerceRowView row)
        {
            if (selectedCommerceTab == CommerceTab.Event)
            {
                if (row.Id == "event_exchange")
                {
                    eventExchangeAction?.Invoke();
                }
                else
                {
                    eventRewardAction?.Invoke(row.Id);
                }
                return;
            }

            var definition = lastService.GetShopBoard(lastState).Products
                .FirstOrDefault(state => state.Definition.ProductId == row.Id).Definition;
            if (definition == null)
            {
                return;
            }

            switch (definition.Kind)
            {
                case ShopProductKind.DailyFree:
                    dailyShopAction?.Invoke(definition.ProductId);
                    break;
                case ShopProductKind.RewardedAd:
                    rewardedShopAction?.Invoke(definition.ProductId);
                    break;
                case ShopProductKind.GemExchange:
                    gemShopAction?.Invoke(definition.ProductId);
                    break;
                default:
                    if (definition.ProductId == "starter_pack")
                    {
                        purchaseStarterPackAction?.Invoke();
                    }
                    else
                    {
                        purchaseRemoveAdsAction?.Invoke();
                    }
                    break;
            }
        }

        private string FormatShopRewards(ShopProductDefinition definition)
        {
            var parts = new List<string>();
            if (definition.RewardCredits > 0) parts.Add($"{CompactNumberFormatter.Format(definition.RewardCredits)} C");
            if (definition.RewardGems > 0) parts.Add($"{CompactNumberFormatter.Format(definition.RewardGems)} \u25C6");
            if (definition.RewardBlueprintCores > 0) parts.Add($"{CompactNumberFormatter.Format(definition.RewardBlueprintCores)} {LanguageService.Get("blueprint_core_short")}");
            // One reward per line. Joining with " + " makes the three-currency starter
            // pack far too wide for the narrow value column at the readable font size.
            return string.Join("\n", parts);
        }

        private static string FormatCommerceRefresh(long refreshAtUnixSeconds)
        {
            var remaining = Math.Max(0L, refreshAtUnixSeconds - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return $"{remaining / 3600L:00}:{remaining % 3600L / 60L:00}";
        }

        private string FormatEventReward(EventRewardType type, long amount)
        {
            return type switch
            {
                EventRewardType.Gems => $"{CompactNumberFormatter.Format(amount)} \u25C6",
                EventRewardType.BlueprintCores => $"{CompactNumberFormatter.Format(amount)} {LanguageService.Get("blueprint_core_short")}",
                _ => $"{CompactNumberFormatter.Format(amount)} C"
            };
        }

        private Sprite GetShopProductSprite(ShopProductDefinition definition)
        {
            return definition.ProductId switch
            {
                "gem_credit_crate" => finalSkin?.V5Simple("05_GemIcon"),
                "gem_core_crate" => finalSkin?.V5Simple("06_BlueprintCoreIcon"),
                "remove_ads" => finalSkin?.Simple("IconAdsOff"),
                "starter_pack" => finalSkin?.Task13Simple("IconAchievementEquipment"),
                "rewarded_supply" => finalSkin?.Simple("IconVideo"),
                _ => finalSkin?.V5Simple("04_CreditsIcon")
            };
        }

        private Sprite GetEventRewardSprite(EventRewardType type)
        {
            return type switch
            {
                EventRewardType.Gems => finalSkin?.V5Simple("05_GemIcon"),
                EventRewardType.BlueprintCores => finalSkin?.V5Simple("06_BlueprintCoreIcon"),
                _ => finalSkin?.V5Simple("04_CreditsIcon")
            };
        }

        private void ApplyCommerceSkin()
        {
            if (finalSkin == null || commerceCard == null)
            {
                return;
            }

            ApplyBorderedSprite(commerceCard, finalSkin.Task13Sliced("UiCollectionModalBody"), Color.white);
            ApplySimpleSprite(commerceTitleSurface, finalSkin.Task13Simple("UiCollectionTitlePlaque"));
            ApplyBorderedSprite(commerceSummarySurface, finalSkin.Task13Sliced("UiTask13HorizontalPanelClean"), Color.white);
            ApplySimpleSprite(closeCommerceCornerButton.image, finalSkin.Task13Simple("UiModalCloseButtonSurface"));
            ApplySimpleSprite(
                closeCommerceCornerButton.transform.Find("Icon")?.GetComponent<Image>(),
                finalSkin.Task13Simple("IconCloseX"));
            ApplyBorderedSprite(closeCommerceButton.image, finalSkin.Task13Sliced("ButtonAchievementClaimRuntime"), Color.white);
            foreach (var row in commerceRows)
            {
                ApplySimpleSprite(row.Surface, finalSkin.Task13Simple("UiAchievementRowBase"));
                ApplyCommerceActionSkin(row.Action, false);
            }
            ApplyCommerceTabSkin(shopTabButton, true);
            ApplyCommerceTabSkin(eventTabButton, false);
        }

        private void ApplyCommerceTabSkin(Button button, bool active)
        {
            ApplyBorderedSprite(
                button.image,
                finalSkin?.Task13Sliced(active ? "TabCollectionActive" : "TabCollectionInactive"),
                Color.white);
        }

        private void ApplyCommerceActionSkin(Button button, bool active)
        {
            ApplyBorderedSprite(
                button.image,
                finalSkin?.Task13Sliced(active ? "ButtonAchievementClaimRuntime" : "UiAchievementInProgressState"),
                Color.white);
        }

        private static void ConfigureCommerceText(Text text, int minimumSize, int maximumSize)
        {
            text.font = UiFontProvider.GetCasual();
            text.resizeTextForBestFit = true;
            text.resizeTextMaxSize = Mathf.Max(maximumSize, MinimumReadableFontSize);
            text.resizeTextMinSize = Mathf.Clamp(
                minimumSize,
                MinimumReadableFontSize,
                text.resizeTextMaxSize);
        }

        private static void ConfigureCommerceButton(Button button)
        {
            var label = button.GetComponentInChildren<Text>();
            label.font = UiFontProvider.GetCasual();
            label.fontSize = 22;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = MinimumReadableFontSize;
            label.resizeTextMaxSize = 22;
        }
    }
}
