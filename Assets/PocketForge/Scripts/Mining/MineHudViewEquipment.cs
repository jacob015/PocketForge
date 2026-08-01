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
        }

        private GameObject equipmentPanel;
        private Image equipmentCard;
        private Image equipmentTitleSurface;
        private Text equipmentTitle;
        private Text equipmentSummary;
        private readonly List<Button> equipmentSlotButtons = new();
        private readonly List<RawImage> equipmentSlotIcons = new();
        private readonly List<Text> equipmentSlotLabels = new();
        private readonly List<EquipmentRowView> equipmentRows = new();
        private Text equipmentPageText;
        private Text equipmentCompareText;
        private Button equipmentPreviousButton;
        private Button equipmentNextButton;
        private Button equipmentPrimaryButton;
        private Button equipmentFuseButton;
        private Button equipmentAutoEquipButton;
        private Button closeEquipmentButton;
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
                new Vector2(920f, 1380f),
                new Color(0.035f, 0.075f, 0.15f, 0.995f));
            equipmentTitleSurface = CreatePanel(
                "EquipmentTitleSurface",
                equipmentCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -44f),
                new Vector2(650f, 132f),
                new Color(0.12f, 0.17f, 0.42f, 1f));
            equipmentTitle = CreateText(
                "EquipmentTitle",
                equipmentTitleSurface.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-42f, -14f),
                44,
                TextAnchor.MiddleCenter);
            equipmentTitle.font = UiFontProvider.GetCasual();
            equipmentSummary = CreateText(
                "EquipmentSummary",
                equipmentCard.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -154f),
                new Vector2(760f, 52f),
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
                -492f,
                120f,
                true);
            equipmentPreviousButton.GetComponentInChildren<Text>().text = "<";
            equipmentPreviousButton.onClick.AddListener(() =>
            {
                equipmentPage = Mathf.Max(0, equipmentPage - 1);
                RenderEquipment();
            });
            equipmentNextButton = CreateEquipmentActionButton(
                "EquipmentNext",
                300f,
                -492f,
                120f,
                true);
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
                new Vector2(0f, -492f),
                new Vector2(300f, 64f),
                24,
                TextAnchor.MiddleCenter);

            equipmentCompareText = CreateText(
                "EquipmentCompare",
                equipmentCard.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 244f),
                new Vector2(760f, 66f),
                25,
                TextAnchor.MiddleCenter);
            equipmentCompareText.font = UiFontProvider.GetCasual();
            equipmentCompareText.resizeTextForBestFit = true;
            equipmentCompareText.resizeTextMinSize = 18;
            equipmentCompareText.resizeTextMaxSize = 25;

            equipmentPrimaryButton = CreateEquipmentActionButton("EquipmentPrimary", -252f, 164f, 230f);
            equipmentPrimaryButton.onClick.AddListener(UseSelectedEquipment);
            equipmentFuseButton = CreateEquipmentActionButton("EquipmentFuse", 0f, 164f, 230f);
            equipmentFuseButton.onClick.AddListener(FuseSelectedEquipment);
            equipmentAutoEquipButton = CreateEquipmentActionButton("EquipmentAutoEquip", 252f, 164f, 230f);
            equipmentAutoEquipButton.onClick.AddListener(() => equipmentAutoEquipAction?.Invoke());
            closeEquipmentButton = CreateEquipmentActionButton("CloseEquipment", 0f, 62f, 360f);
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
                new Vector2(-300f + index * 200f, -270f),
                new Vector2(184f, 178f),
                new Color(0.08f, 0.15f, 0.32f, 1f));
            button.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage))
                .GetComponent<RawImage>();
            icon.transform.SetParent(button.transform, false);
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, 22f);
            iconRect.sizeDelta = new Vector2(88f, 74f);
            icon.raycastTarget = false;
            var label = CreateText(
                "SlotLabel",
                button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -48f),
                new Vector2(166f, 62f),
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
            row.Button = CreateButton(
                $"EquipmentInventoryRow{index + 1}",
                equipmentCard.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 274f - index * 112f),
                new Vector2(760f, 96f),
                new Color(0.055f, 0.12f, 0.23f, 0.98f));
            row.Button.GetComponentInChildren<Text>().gameObject.SetActive(false);
            row.Icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage))
                .GetComponent<RawImage>();
            row.Icon.transform.SetParent(row.Button.transform, false);
            row.Icon.rectTransform.anchorMin = row.Icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            row.Icon.rectTransform.anchoredPosition = new Vector2(-316f, 0f);
            row.Icon.rectTransform.sizeDelta = new Vector2(82f, 72f);
            row.Icon.raycastTarget = false;
            row.Name = CreateText(
                "ItemName",
                row.Button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-126f, 17f),
                new Vector2(300f, 38f),
                24,
                TextAnchor.MiddleLeft);
            row.Name.font = UiFontProvider.GetCasual();
            row.Details = CreateText(
                "ItemDetails",
                row.Button.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(116f, -21f),
                new Vector2(580f, 34f),
                19,
                TextAnchor.MiddleRight);
            row.Details.color = new Color(0.62f, 0.86f, 1f);
            return row;
        }

        private Button CreateEquipmentActionButton(
            string name,
            float x,
            float y,
            float width,
            bool centered = false)
        {
            var anchor = centered ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0f);
            var button = CreateButton(
                name,
                equipmentCard.transform,
                anchor,
                anchor,
                new Vector2(x, y),
                new Vector2(width, 82f),
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
                equipmentCompareText.text = LanguageService.Get("equipment_no_items");
                return;
            }

            var current = lastService.GetEquippedItem(lastState, selected.Definition.Slot);
            var delta = selected.PowerBonus - (current?.PowerBonus ?? 0f);
            equipmentCompareText.text = string.Format(
                LanguageService.Get("equipment_compare"),
                LanguageService.Get(selected.Definition.LocalizationKey),
                delta * 100f);
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

            ApplyStretchedSimpleSprite(equipmentCard, finalSkin.Simple("SettingsModal"), Color.white);
            ApplyStretchedSimpleSprite(
                equipmentTitleSurface,
                finalSkin.Simple("SettingsTitlePlaque"),
                Color.white);
            foreach (var button in equipmentSlotButtons)
            {
                ApplyStretchedSimpleSprite(button.image, finalSkin.Simple("SettingsRow"), Color.white);
            }

            foreach (var row in equipmentRows)
            {
                ApplyStretchedSimpleSprite(row.Button.image, finalSkin.Simple("SettingsRow"), Color.white);
            }

            foreach (var button in new[]
                     {
                         equipmentPreviousButton,
                         equipmentNextButton,
                         equipmentPrimaryButton,
                         equipmentFuseButton,
                         equipmentAutoEquipButton,
                         closeEquipmentButton
                     })
            {
                ApplyStretchedSimpleSprite(
                    button.image,
                    finalSkin.Simple(button == closeEquipmentButton
                        ? "SettingsCloseButton"
                        : "SettingsActionButton"),
                    Color.white);
            }

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
                EquipmentSlot.Pickaxe => "25_PickaxeEquipmentIcon",
                EquipmentSlot.Drill => "26_DrillEquipmentIcon",
                EquipmentSlot.Robot => "27_RobotEquipmentIcon",
                _ => "16_ResearchCompleteIcon"
            };
            ApplyRawTexture(image, finalSkin.V5Texture(asset), new Vector2(92f, 78f));
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
