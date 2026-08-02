using System.Collections.Generic;
using NUnit.Framework;
using PocketForge.Presentation;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class Task13UiSliceCatalogTests
    {
        private static readonly IReadOnlyDictionary<string, Vector2> MinimumRuntimeSizes =
            new Dictionary<string, Vector2>
            {
                ["UiCollectionModalBody"] = new Vector2(920f, 1200f),
                ["UiEquipmentModalBody"] = new Vector2(920f, 1200f),
                ["UiEquipmentSlotCardBase"] = new Vector2(186f, 216f),
                ["UiEquipmentInventoryCardClean"] = new Vector2(190f, 328f),
                ["OverlayEquipmentRarityCommon"] = new Vector2(190f, 328f),
                ["OverlayEquipmentRarityRare"] = new Vector2(190f, 328f),
                ["OverlayEquipmentRarityEpic"] = new Vector2(190f, 328f),
                ["OverlayEquipmentRarityLegendary"] = new Vector2(190f, 328f),
                ["OverlayEquipmentSelected"] = new Vector2(190f, 328f),
                ["ButtonEquipmentUnequipRuntime"] = new Vector2(112f, 72f),
                ["ButtonEquipmentEquipRuntime"] = new Vector2(230f, 100f),
                ["ButtonEquipmentMergeRuntime"] = new Vector2(230f, 104f),
                ["ButtonEquipmentAutoEquipRuntime"] = new Vector2(230f, 104f),
                ["ButtonAchievementClaimRuntime"] = new Vector2(156f, 72f),
                ["TabCollectionActive"] = new Vector2(400f, 92f),
                ["TabCollectionInactive"] = new Vector2(400f, 92f),
                ["UiMuseumExhibitCardClean"] = new Vector2(388f, 330f),
                ["UiAchievementInProgressState"] = new Vector2(156f, 84f),
                ["UiTask13HorizontalPanelClean"] = new Vector2(824f, 120f)
            };

        [Test]
        public void EveryRuntimeSlicedAsset_HasMeasuredBordersAndFitsItsSmallestTarget()
        {
            var skin = MineUiSkin.Load();
            Assert.That(skin, Is.Not.Null);

            foreach (var entry in MinimumRuntimeSizes)
            {
                Assert.That(
                    MineUiSkin.TryGetTask13SliceBorder(entry.Key, out var border),
                    Is.True,
                    $"{entry.Key} is missing from the measured border catalog.");

                var texture = skin.Task13Texture(entry.Key);
                Assert.That(texture, Is.Not.Null, $"{entry.Key} is missing from Resources.");
                Assert.That(border.x + border.z + 16f, Is.LessThanOrEqualTo(entry.Value.x), entry.Key);
                Assert.That(border.y + border.w + 16f, Is.LessThanOrEqualTo(entry.Value.y), entry.Key);

                var sprite = skin.Task13Sliced(entry.Key);
                Assert.That(sprite, Is.Not.Null, entry.Key);
                Assert.That(sprite.border, Is.EqualTo(border), entry.Key);
            }
        }
    }
}
