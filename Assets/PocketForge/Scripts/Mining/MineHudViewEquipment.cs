using System;
using System.Collections.Generic;
using System.Linq;
using PocketForge.Content;
using PocketForge.Localization;
using PocketForge.Presentation;
using PocketForge.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace PocketForge.Mining
{
    public sealed partial class MineHudView
    {
        private const int EquipmentRowsPerPage = 6;

        private sealed class EquipmentRowView
        {
            public Button Button;
            public RawImage Icon;
            public Text Name;
            public Text Details;
            public Image RarityOverlay;
            public Image SelectionOverlay;
            public Image EquippedBadge;
            public Image MergeBadge;
            public Image CountSurface;
            public Text Count;
        }

        private GameObject equipmentPanel;
        private Image equipmentCard;
        private Image equipmentTitleSurface;
        private Text equipmentTitle;
        private Text equipmentSummary;
        private Text equipmentCapacityText;
        private Image equipmentCapacitySurface;
        private Image equipmentCompareSurface;
        private Image equipmentCompareDividerLeft;
        private Image equipmentCompareDividerRight;
        private readonly List<Button> equipmentSlotButtons = new();
        private readonly List<RawImage> equipmentSlotIcons = new();
        private readonly List<Text> equipmentSlotLabels = new();
        private readonly List<EquipmentRowView> equipmentRows = new();
        private Text equipmentPageText;
        private Text equipmentCompareText;
        private Text equipmentCompareLeftText;
        private Text equipmentCompareRightText;
        private Button equipmentPreviousButton;
        private Button equipmentNextButton;
        private Button equipmentPrimaryButton;
        private Button equipmentFuseButton;
        private Button equipmentAutoEquipButton;
        private Button closeEquipmentButton;
        private Button closeEquipmentCornerButton;
        private Action<string> equipmentEquipAction;
        private Action<EquipmentSlot> equipmentUnequipAction;
        private Action<string, EquipmentRarity> equipmentFuseAction;
        private Action equipmentAutoEquipAction;
        private IReadOnlyList<EquipmentItemState> equipmentItems = Array.Empty<EquipmentItemState>();
        private string selectedEquipmentInstanceId = string.Empty;
        private int equipmentPage;

        private void CreateEquipmentPanel()
        {
            var backdrop = CreatePanel(
                "EquipmentBackdrop",
                transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.005f, 0.012f, 0.04f, 0.86f));
            equipmentPanel = backdrop.gameObject;
            equipmentCard = CreatePanel(
                "EquipmentCard",
                equipmentPanel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 24f),
                new Vector2(920f, 1700f),
                new Color(0.035f, 0.075f, 0.15f, 0.995f));
            equipmentTitleSurface = CreatePanel(
                "EquipmentTitleSurface",
                equipmentCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-30f, -62f),
                new Vector2(610f, 124f),
                new Color(0.12f, 0.17f, 0.42f, 1f));
            equipmentTitle = CreateText(
                "EquipmentTitle",
                equipmentTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(0f, -8f),
                44,
                TextAnchor.MiddleCenter);
            equipmentTitle.font = UiFontProvider.GetCasual();

            equipmentCapacitySurface = CreatePanel(
                "EquipmentCapacitySurface",
                equipmentCard.transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(145f, -184f),
                new Vector2(190f, 58f),
                Color.white);
            equipmentCapacityText = CreateText(
                "EquipmentCapacity",
                equipmentCapacitySurface.transform,
                Vector2.zero,
                Vector2.one,
                new Vector2(20f, 0f),
                new Vector2(-32f, -8f),
                22,
                TextAnchor.MiddleCenter);
            equipmentCapacityText.font = UiFontProvider.GetCasual();

            closeEquipmentCornerButton = CreateButton(
                "CloseEquipmentCorner",
                equipmentCard.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-70f, -70f),
                new Vector2(84f, 84f),
                Color.white);
            closeEquipmentCornerButton.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var cornerCloseIcon = CreateSimpleImage(
                "Icon",
                closeEquipmentCornerButton.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-22f, -22f),
                Color.white);
            cornerCloseIcon.raycastTarget = false;
            closeEquipmentCornerButton.onClick.AddListener(() => equipmentPanel.SetActive(false));

            equipmentSummary = CreateText(
                "EquipmentSummary",
                equipmentCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(112f, -184f),
                new Vector2(540f, 58f),
                27,
                TextAnchor.MiddleCenter);
            equipmentSummary.font = UiFontProvider.GetCasual();
            equipmentSummary.color = new Color(0.5f, 0.92f, 1f);

            for (var index = 0; index < 4; index++)
            {
                CreateEquipmentSlot((EquipmentSlot)index, index);
            }

            for (var index = 0; index < EquipmentRowsPerPage; index++)
            {
                equipmentRows.Add(CreateEquipmentRow(index));
            }

            equipmentPreviousButton = CreateEquipmentActionButton(
                "EquipmentPrevious",
                -300f,
                -360f,
                112f,
                true,
                72f);
            equipmentPreviousButton.GetComponentInChildren<Text>().text = "<";
            equipmentPreviousButton.onClick.AddListener(() =>
            {
                equipmentPage = Mathf.Max(0, equipmentPage - 1);
                RenderEquipment();
            });
            equipmentNextButton = CreateEquipmentActionButton(
                "EquipmentNext",
                300f,
                -360f,
                112f,
                true,
                72f);
            equipmentNextButton.GetComponentInChildren<Text>().text = ">";
            equipmentNextButton.onClick.AddListener(() =>
            {
                equipmentPage++;
                RenderEquipment();
            });
            equipmentPageText = CreateText(
                "EquipmentPage",
                equipmentCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -360f),
                new Vector2(280f, 58f),
                24,
                TextAnchor.MiddleCenter);

            equipmentCompareSurface = CreatePanel(
                "EquipmentCompareSurface",
                equipmentCard.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 390f),
                new Vector2(824f, 120f),
                Color.white);
            equipmentCompareDividerLeft = CreateSimpleImage(
                "EquipmentCompareDividerLeft",
                equipmentCompareSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-140f, 0f),
                new Vector2(3f, 88f),
                new Color(0.42f, 0.55f, 0.94f, 0.72f));
            equipmentCompareDividerRight = CreateSimpleImage(
                "EquipmentCompareDividerRight",
                equipmentCompareSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(140f, 0f),
                new Vector2(3f, 88f),
                new Color(0.42f, 0.55f, 0.94f, 0.72f));
            equipmentCompareLeftText = CreateText(
                "EquipmentCompareSelected",
                equipmentCompareSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-276f, 0f),
                new Vector2(244f, 82f),
                22,
                TextAnchor.MiddleCenter);
            equipmentCompareText = CreateText(
                "EquipmentCompare",
                equipmentCompareSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(244f, 82f),
                25,
                TextAnchor.MiddleCenter);
            equipmentCompareText.font = UiFontProvider.GetCasual();
            equipmentCompareText.resizeTextForBestFit = true;
            equipmentCompareText.resizeTextMinSize = 18;
            equipmentCompareText.resizeTextMaxSize = 25;
            equipmentCompareRightText = CreateText(
                "EquipmentCompareCurrent",
                equipmentCompareSurface.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(276f, 0f),
                new Vector2(244f, 82f),
                22,
                TextAnchor.MiddleCenter);
            foreach (var text in new[] { equipmentCompareLeftText, equipmentCompareRightText })
            {
                text.font = UiFontProvider.GetCasual();
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 16;
                text.resizeTextMaxSize = 22;
            }

            equipmentPrimaryButton = CreateEquipmentActionButton("EquipmentPrimary", -252f, 250f, 230f);
            equipmentPrimaryButton.onClick.AddListener(UseSelectedEquipment);
            equipmentFuseButton = CreateEquipmentActionButton("EquipmentFuse", 0f, 250f, 230f);
            equipmentFuseButton.onClick.AddListener(FuseSelectedEquipment);
            equipmentAutoEquipButton = CreateEquipmentActionButton("EquipmentAutoEquip", 252f, 250f, 230f);
            equipmentAutoEquipButton.onClick.AddListener(() => equipmentAutoEquipAction?.Invoke());
            closeEquipmentButton = CreateEquipmentActionButton("CloseEquipment", 0f, 70f, 360f, false, 72f);
            closeEquipmentButton.onClick.AddListener(() => equipmentPanel.SetActive(false));
            equipmentPanel.SetActive(false);
        }

        private void CreateEquipmentSlot(EquipmentSlot slot, int index)
        {
            var button = CreateButton(
                $"EquipmentSlot{slot}",
                equipmentCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-303f + index * 202f, -340f),
                new Vector2(186f, 216f),
                new Color(0.08f, 0.15f, 0.32f, 1f));
            button.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage))
                .GetComponent<RawImage>();
            icon.transform.SetParent(button.transform, false);
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, 30f);
            iconRect.sizeDelta = new Vector2(104f, 94f);
            icon.raycastTarget = false;
            var label = CreateText(
                "SlotLabel",
                button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -64f),
                new Vector2(166f, 74f),
                19,
                TextAnchor.MiddleCenter);
            label.font = UiFontProvider.GetCasual();
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = 19;
            var captured = slot;
            button.onClick.AddListener(() => equipmentUnequipAction?.Invoke(captured));
            equipmentSlotButtons.Add(button);
            equipmentSlotIcons.Add(icon);
            equipmentSlotLabels.Add(label);
        }

        private EquipmentRowView CreateEquipmentRow(int index)
        {
            var row = new EquipmentRowView();
            var column = index % 3;
            var line = index / 3;
            row.Button = CreateButton(
                $"EquipmentInventoryRow{index + 1}",
                equipmentCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-222f + column * 222f, -650f - line * 352f),
                new Vector2(190f, 328f),
                new Color(0.055f, 0.12f, 0.23f, 0.98f));
            row.Button.GetComponentInChildren<Text>().gameObject.SetActive(false);

            row.RarityOverlay = CreateSimpleImage(
                "RarityOverlay",
                row.Button.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                Color.white);
            row.RarityOverlay.raycastTarget = false;
            row.Icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage))
                .GetComponent<RawImage>();
            row.Icon.transform.SetParent(row.Button.transform, false);
            row.Icon.rectTransform.anchorMin = row.Icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            row.Icon.rectTransform.anchoredPosition = new Vector2(0f, 70f);
            row.Icon.rectTransform.sizeDelta = new Vector2(120f, 112f);
            row.Icon.raycastTarget = false;
            row.Name = CreateText(
                "ItemName",
                row.Button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -40f),
                new Vector2(162f, 48f),
                22,
                TextAnchor.MiddleCenter);
            row.Name.font = UiFontProvider.GetCasual();
            row.Name.resizeTextForBestFit = true;
            row.Name.resizeTextMinSize = 15;
            row.Name.resizeTextMaxSize = 22;
            row.Details = CreateText(
                "ItemDetails",
                row.Button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -91f),
                new Vector2(164f, 44f),
                18,
                TextAnchor.MiddleCenter);
            row.Details.color = new Color(0.62f, 0.86f, 1f);

            row.CountSurface = CreateSimpleImage(
                "CountSurface",
                row.Button.transform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-36f, 28f),
                new Vector2(58f, 40f),
                Color.white);
            row.CountSurface.raycastTarget = false;
            row.Count = CreateText(
                "ItemCount",
                row.CountSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-8f, -8f),
                17,
                TextAnchor.MiddleCenter);
            row.Count.font = UiFontProvider.GetCasual();

            row.EquippedBadge = CreateSimpleImage(
                "EquippedBadge",
                row.Button.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-30f, -30f),
                new Vector2(44f, 44f),
                Color.white);
            row.EquippedBadge.raycastTarget = false;
            row.MergeBadge = CreateSimpleImage(
                "MergeBadge",
                row.Button.transform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-30f, -74f),
                new Vector2(42f, 42f),
                Color.white);
            row.MergeBadge.raycastTarget = false;
            row.SelectionOverlay = CreateSimpleImage(
                "SelectionOverlay",
                row.Button.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                Color.white);
            row.SelectionOverlay.raycastTarget = false;
            return row;
        }

        private Button CreateEquipmentActionButton(
            string name,
            float x,
            float y,
            float width,
            bool centered = false,
            float height = 104f)
        {
            var anchor = centered ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0f);
            var button = CreateButton(
                name,
                equipmentCard.transform,
                anchor,
                anchor,
                new Vector2(x, y),
                new Vector2(width, height),
                new Color(0.25f, 0.68f, 0.2f, 1f));
            var label = button.GetComponentInChildren<Text>();
            label.font = UiFontProvider.GetCasual();
            label.fontSize = 23;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 15;
            label.resizeTextMaxSize = 23;
            return button;
        }

        private void ShowEquipment()
        {
            if (lastState == null || lastService == null)
            {
                return;
            }

            if (!lastService.IsFeatureUnlocked(lastState.Player.minerLevel, ProgressionFeature.Equipment))
            {
                ShowFeedback(LanguageService.Get("equipment_locked"), new Color(1f, 0.55f, 0.3f));
                return;
            }

            RenderEquipment();
            equipmentPanel.SetActive(true);
            equipmentPanel.transform.SetAsLastSibling();
        }

        private void RenderEquipment()
        {
            if (equipmentPanel == null || lastState == null || lastService == null)
            {
                return;
            }

            equipmentItems = lastService.GetEquipmentStates(lastState);
            if (equipmentItems.Count > 0 &&
                equipmentItems.All(item => item.Item.instanceId != selectedEquipmentInstanceId))
            {
                selectedEquipmentInstanceId = equipmentItems[0].Item.instanceId;
            }

            equipmentTitle.text = LanguageService.Get("feature_equipment").ToUpper();
            equipmentCapacityText.text = $"{equipmentItems.Count} / 50";
            equipmentSummary.text = string.Format(
                LanguageService.Get("equipment_power_summary"),
                (lastService.GetEquipmentPowerMultiplier(lastState) - 1f) * 100f);
            for (var index = 0; index < equipmentSlotButtons.Count; index++)
            {
                var slot = (EquipmentSlot)index;
                var equipped = lastService.GetEquippedItem(lastState, slot);
                equipmentSlotLabels[index].text = equipped.HasValue
                    ? $"{GetSlotName(slot)}\n{GetRarityName(equipped.Value.Rarity)} +{equipped.Value.PowerBonus * 100f:0.#}%"
                    : $"{GetSlotName(slot)}\n{LanguageService.Get("equipment_empty")}";
                equipmentSlotButtons[index].interactable = equipped.HasValue;
                ApplyEquipmentIcon(equipmentSlotIcons[index], slot);
            }

            var pageCount = Mathf.Max(1, Mathf.CeilToInt(equipmentItems.Count / (float)EquipmentRowsPerPage));
            equipmentPage = Mathf.Clamp(equipmentPage, 0, pageCount - 1);
            equipmentPageText.text = $"{equipmentPage + 1} / {pageCount}";
            equipmentPreviousButton.interactable = equipmentPage > 0;
            equipmentNextButton.interactable = equipmentPage + 1 < pageCount;
            for (var index = 0; index < equipmentRows.Count; index++)
            {
                var itemIndex = equipmentPage * EquipmentRowsPerPage + index;
                var row = equipmentRows[index];
                row.Button.onClick.RemoveAllListeners();
                row.Button.gameObject.SetActive(itemIndex < equipmentItems.Count);
                if (itemIndex >= equipmentItems.Count)
                {
                    continue;
                }

                var item = equipmentItems[itemIndex];
                row.Name.text = LanguageService.Get(item.Definition.LocalizationKey);
                row.Details.text =
                    $"{GetRarityName(item.Rarity)}  +{item.PowerBonus * 100f:0.#}%" +
                    (item.IsEquipped ? $"  • {LanguageService.Get("equipment_equipped")}" : string.Empty);
                row.Button.image.color = item.Item.instanceId == selectedEquipmentInstanceId
                    ? new Color(0.2f, 0.34f, 0.62f, 1f)
                    : Color.white;
                row.Details.text = $"{GetRarityName(item.Rarity)}  +{item.PowerBonus * 100f:0.#}%";
                row.Button.image.color = Color.white;
                row.RarityOverlay.sprite = finalSkin?.Task13Simple(item.Rarity switch
                {
                    EquipmentRarity.Rare => "OverlayEquipmentRarityRare",
                    EquipmentRarity.Epic => "OverlayEquipmentRarityEpic",
                    EquipmentRarity.Legendary => "OverlayEquipmentRarityLegendary",
                    _ => "OverlayEquipmentRarityCommon"
                });
                row.RarityOverlay.type = Image.Type.Simple;
                row.SelectionOverlay.gameObject.SetActive(
                    item.Item.instanceId == selectedEquipmentInstanceId);
                row.EquippedBadge.gameObject.SetActive(item.IsEquipped);
                var matchingCount = equipmentItems.Count(candidate =>
                    candidate.Definition.DefinitionId == item.Definition.DefinitionId &&
                    candidate.Rarity == item.Rarity);
                row.MergeBadge.gameObject.SetActive(matchingCount >= 3);
                row.Count.text = $"x{matchingCount}";

                var capturedId = item.Item.instanceId;
                row.Button.onClick.AddListener(() =>
                {
                    selectedEquipmentInstanceId = capturedId;
                    RenderEquipment();
                });
                ApplyEquipmentIcon(row.Icon, item.Definition.Slot);
            }

            var selected = equipmentItems.FirstOrDefault(
                item => item.Item.instanceId == selectedEquipmentInstanceId);
            var hasSelected = selected.Item != null;
            equipmentPrimaryButton.interactable = hasSelected;
            equipmentFuseButton.interactable = hasSelected;
            equipmentAutoEquipButton.interactable = equipmentItems.Count > 0;
            equipmentPrimaryButton.GetComponentInChildren<Text>().text = hasSelected && selected.IsEquipped
                ? LanguageService.Get("equipment_unequip").ToUpper()
                : LanguageService.Get("equipment_equip").ToUpper();
            equipmentFuseButton.GetComponentInChildren<Text>().text = LanguageService.Get("equipment_fuse").ToUpper();
            equipmentAutoEquipButton.GetComponentInChildren<Text>().text = LanguageService.Get("equipment_auto_equip").ToUpper();
            closeEquipmentButton.GetComponentInChildren<Text>().text = LanguageService.Get("close").ToUpper();
            if (!hasSelected)
            {
                equipmentCompareLeftText.text = LanguageService.Get("equipment_no_items");
                equipmentCompareText.text = "—";
                equipmentCompareRightText.text = LanguageService.Get("equipment_empty");
                return;
            }

            var current = lastService.GetEquippedItem(lastState, selected.Definition.Slot);
            var delta = selected.PowerBonus - (current?.PowerBonus ?? 0f);
            equipmentCompareLeftText.text = LanguageService.Get(selected.Definition.LocalizationKey);
            equipmentCompareText.text = $"{delta * 100f:+0.#;-0.#;0}%";
            equipmentCompareRightText.text = current.HasValue
                ? LanguageService.Get(current.Value.Definition.LocalizationKey)
                : LanguageService.Get("equipment_empty");
        }

        private void UseSelectedEquipment()
        {
            var selected = equipmentItems.FirstOrDefault(
                item => item.Item.instanceId == selectedEquipmentInstanceId);
            if (selected.Item == null)
            {
                return;
            }

            if (selected.IsEquipped)
            {
                equipmentUnequipAction?.Invoke(selected.Definition.Slot);
            }
            else
            {
                equipmentEquipAction?.Invoke(selected.Item.instanceId);
            }
        }

        private void FuseSelectedEquipment()
        {
            var selected = equipmentItems.FirstOrDefault(
                item => item.Item.instanceId == selectedEquipmentInstanceId);
            if (selected.Item != null)
            {
                equipmentFuseAction?.Invoke(selected.Definition.DefinitionId, selected.Rarity);
            }
        }

        private void ApplyEquipmentSkin()
        {
            if (finalSkin == null || equipmentCard == null)
            {
                return;
            }

            ApplyBorderedSprite(
                equipmentCard,
                finalSkin.Task13Sliced("UiEquipmentModalBody", new Vector4(30f, 30f, 30f, 30f)),
                Color.white);
            ApplySimpleSprite(equipmentTitleSurface, finalSkin.Task13Simple("UiEquipmentTitlePlaque"));
            ApplySimpleSprite(equipmentCapacitySurface, finalSkin.Task13Simple("UiEquipmentCapacityCapsule"));
            ApplyBorderedSprite(
                equipmentCompareSurface,
                finalSkin.Task13Sliced("UiEquipmentModalBody", new Vector4(30f, 30f, 30f, 30f)),
                Color.white);
            ApplySimpleSprite(
                closeEquipmentCornerButton.image,
                finalSkin.Task13Simple("UiModalCloseButtonSurface"));
            var cornerIcon = closeEquipmentCornerButton.transform.Find("Icon")?.GetComponent<Image>();
            if (cornerIcon != null)
            {
                cornerIcon.sprite = finalSkin.Task13Simple("IconCloseX");
                cornerIcon.type = Image.Type.Simple;
                cornerIcon.preserveAspect = true;
            }

            foreach (var button in equipmentSlotButtons)
            {
                ApplyBorderedSprite(
                    button.image,
                    finalSkin.Task13Sliced("UiEquipmentSlotCardBase", new Vector4(32f, 32f, 32f, 32f)),
                    Color.white);
            }

            foreach (var row in equipmentRows)
            {
                ApplySimpleSprite(row.Button.image, finalSkin.Task13Simple("UiEquipmentInventoryCardBase"));
                row.SelectionOverlay.sprite = finalSkin.Task13Simple("OverlayEquipmentSelected");
                row.SelectionOverlay.type = Image.Type.Simple;
                row.SelectionOverlay.preserveAspect = true;
                row.EquippedBadge.sprite = finalSkin.Task13Simple("BadgeEquipmentEquipped");
                row.EquippedBadge.type = Image.Type.Simple;
                row.EquippedBadge.preserveAspect = true;
                row.MergeBadge.sprite = finalSkin.Task13Simple("BadgeEquipmentMergeReady");
                row.MergeBadge.type = Image.Type.Simple;
                row.MergeBadge.preserveAspect = true;
                row.CountSurface.sprite = finalSkin.Task13Simple("BadgeEquipmentCountBlank");
                row.CountSurface.type = Image.Type.Simple;
                row.CountSurface.preserveAspect = true;
            }

            ApplyBorderedSprite(
                equipmentPreviousButton.image,
                finalSkin.Task13Sliced("ButtonEquipmentUnequip", new Vector4(36f, 28f, 36f, 28f)),
                Color.white);
            ApplyBorderedSprite(
                equipmentNextButton.image,
                finalSkin.Task13Sliced("ButtonEquipmentUnequip", new Vector4(36f, 28f, 36f, 28f)),
                Color.white);
            ApplyBorderedSprite(
                equipmentPrimaryButton.image,
                finalSkin.Task13Sliced("ButtonEquipmentEquip", new Vector4(36f, 28f, 36f, 28f)),
                Color.white);
            ApplyBorderedSprite(
                equipmentFuseButton.image,
                finalSkin.Task13Sliced("ButtonEquipmentMerge", new Vector4(42f, 34f, 42f, 34f)),
                Color.white);
            ApplyBorderedSprite(
                equipmentAutoEquipButton.image,
                finalSkin.Task13Sliced("ButtonEquipmentAutoEquip", new Vector4(48f, 44f, 48f, 44f)),
                Color.white);
            ApplyBorderedSprite(
                closeEquipmentButton.image,
                finalSkin.Task13Sliced("ButtonEquipmentUnequip", new Vector4(36f, 28f, 36f, 28f)),
                Color.white);

            for (var index = 0; index < equipmentSlotIcons.Count; index++)
            {
                ApplyEquipmentIcon(equipmentSlotIcons[index], (EquipmentSlot)index);
            }
        }

        private void ApplyEquipmentIcon(RawImage image, EquipmentSlot slot)
        {
            if (finalSkin == null || image == null)
            {
                return;
            }

            var asset = slot switch
            {
                EquipmentSlot.Pickaxe => "IconEquipmentPickaxe",
                EquipmentSlot.Drill => "IconEquipmentDrill",
                EquipmentSlot.Robot => "IconEquipmentRobot",
                _ => "IconEquipmentCharm"
            };
            ApplyRawTexture(image, finalSkin.Task13Texture(asset), image.rectTransform.sizeDelta);
        }

        private static string GetSlotName(EquipmentSlot slot)
        {
            return LanguageService.Get(slot switch
            {
                EquipmentSlot.Pickaxe => "equipment_slot_pickaxe",
                EquipmentSlot.Drill => "equipment_slot_drill",
                EquipmentSlot.Robot => "equipment_slot_robot",
                _ => "equipment_slot_charm"
            });
        }

        private static string GetRarityName(EquipmentRarity rarity)
        {
            return LanguageService.Get(rarity switch
            {
                EquipmentRarity.Rare => "equipment_rarity_rare",
                EquipmentRarity.Epic => "equipment_rarity_epic",
                EquipmentRarity.Legendary => "equipment_rarity_legendary",
                _ => "equipment_rarity_common"
            });
        }
    }
}
