using NUnit.Framework;
using PocketForge.Content;
using PocketForge.Mining;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Tests.Editor
{
    public sealed class EquipmentServiceTests
    {
        [Test]
        public void BossRewards_CycleThroughFourSlotsAndAdvanceSequence()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new EquipmentService(catalog);
            var player = new GameSaveData();

            var first = service.GrantBossReward(player, 1, "first");
            var second = service.GrantBossReward(player, 1, "second");
            var third = service.GrantBossReward(player, 1, "third");
            var fourth = service.GrantBossReward(player, 1, "fourth");

            Assert.That(catalog.GetEquipment(first.definitionId).Slot, Is.EqualTo(EquipmentSlot.Pickaxe));
            Assert.That(catalog.GetEquipment(second.definitionId).Slot, Is.EqualTo(EquipmentSlot.Drill));
            Assert.That(catalog.GetEquipment(third.definitionId).Slot, Is.EqualTo(EquipmentSlot.Robot));
            Assert.That(catalog.GetEquipment(fourth.definitionId).Slot, Is.EqualTo(EquipmentSlot.Charm));
            Assert.That(player.equipmentRewardSequence, Is.EqualTo(4));
            Assert.That(player.equipmentInventory, Has.Length.EqualTo(4));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void EquipAndAutoEquip_RespectUnlockAndSelectHighestPowerPerSlot()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new EquipmentService(catalog);
            var player = new GameSaveData
            {
                equipmentInventory = new[]
                {
                    Item("common", "rugged_pickaxe", EquipmentRarity.Common),
                    Item("epic", "rugged_pickaxe", EquipmentRarity.Epic),
                    Item("drill", "core_drill", EquipmentRarity.Rare)
                }
            };

            Assert.That(
                service.TryEquip(player, "common", false),
                Is.EqualTo(EquipmentActionStatus.FeatureLocked));
            Assert.That(
                service.AutoEquip(player, true),
                Is.EqualTo(EquipmentActionStatus.Success));
            Assert.That(service.GetEquipped(player, EquipmentSlot.Pickaxe)?.Item.instanceId, Is.EqualTo("epic"));
            Assert.That(service.GetEquipped(player, EquipmentSlot.Drill)?.Item.instanceId, Is.EqualTo("drill"));
            Assert.That(service.GetPowerMultiplier(player), Is.EqualTo(1.32f).Within(0.0001f));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Fuse_ConsumesThreeUnequippedMatchingItemsAndCreatesNextRarity()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var service = new EquipmentService(catalog);
            var player = new GameSaveData
            {
                equipmentInventory = new[]
                {
                    Item("a", "forge_bot", EquipmentRarity.Common),
                    Item("b", "forge_bot", EquipmentRarity.Common),
                    Item("c", "forge_bot", EquipmentRarity.Common),
                    Item("other", "core_drill", EquipmentRarity.Common)
                }
            };

            var result = service.TryFuse(
                player,
                "forge_bot",
                EquipmentRarity.Common,
                true,
                "fused");

            Assert.That(result, Is.EqualTo(EquipmentActionStatus.Success));
            Assert.That(player.equipmentInventory, Has.Length.EqualTo(2));
            Assert.That(player.equipmentInventory, Has.Some.Matches<EquipmentItemData>(
                item => item.instanceId == "fused" && item.rarity == (int)EquipmentRarity.Rare));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void SaveMigration_NormalizesInventoryAndDanglingEquipmentReferences()
        {
            var player = GameSaveMigrator.Normalize(new GameSaveData
            {
                version = 8,
                equipmentRewardSequence = -4,
                equipmentInventory = new[]
                {
                    Item("same", "rugged_pickaxe", EquipmentRarity.Common),
                    Item("same", "rugged_pickaxe", EquipmentRarity.Legendary),
                    new EquipmentItemData { instanceId = string.Empty, definitionId = "core_drill", rarity = 99 }
                },
                equippedEquipment = new[]
                {
                    new EquippedItemData { slot = 0, instanceId = "same" },
                    new EquippedItemData { slot = 1, instanceId = "missing" },
                    new EquippedItemData { slot = 99, instanceId = "same" }
                }
            });

            Assert.That(player.version, Is.EqualTo(GameSaveMigrator.CurrentVersion));
            Assert.That(player.equipmentRewardSequence, Is.Zero);
            Assert.That(player.equipmentInventory, Has.Length.EqualTo(1));
            Assert.That(player.equipmentInventory[0].rarity, Is.EqualTo((int)EquipmentRarity.Legendary));
            Assert.That(player.equippedEquipment, Has.Length.EqualTo(1));
            Assert.That(player.equippedEquipment[0].instanceId, Is.EqualTo("same"));
        }

        [Test]
        public void MiningGameService_EquipmentMultiplierScalesTapAutoAndOfflinePower()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var game = new MiningGameService(catalog);
            var baseState = game.CreateInitialState(new GameSaveData { minerLevel = 2, lastSavedUnixSeconds = 100 }, 1f);
            var equippedState = game.CreateInitialState(new GameSaveData
            {
                minerLevel = 2,
                lastSavedUnixSeconds = 100,
                equipmentInventory = new[] { Item("pick", "rugged_pickaxe", EquipmentRarity.Rare) },
                equippedEquipment = new[] { new EquippedItemData { slot = 0, instanceId = "pick" } }
            }, 1f);

            var basePower = game.GetMiningPower(baseState);
            var equippedPower = game.GetMiningPower(equippedState);
            var baseOffline = game.ClaimOfflineProgress(baseState, 4100);
            var equippedOffline = game.ClaimOfflineProgress(equippedState, 4100);

            Assert.That(equippedPower.AutoPowerPerSecond, Is.EqualTo(basePower.AutoPowerPerSecond * 1.1f).Within(0.0001f));
            Assert.That(equippedPower.TapDamage, Is.EqualTo(basePower.TapDamage * 1.1f).Within(0.0001f));
            Assert.That(equippedOffline.ProcessedOres, Is.GreaterThan(baseOffline.ProcessedOres));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void MiningGameService_BossClearGrantsEquipmentReward()
        {
            var catalog = MiningContentCatalog.CreateRuntimeDefault();
            var game = new MiningGameService(catalog);
            var state = game.CreateInitialState(new GameSaveData { stage = 10 }, 1f);
            state.Ore.Health = 0.01f;

            var result = game.Mine(state, 1f);

            Assert.That(result.ChapterCompleted, Is.True);
            Assert.That(result.RewardEquipment, Is.Not.Null);
            Assert.That(state.Player.equipmentInventory, Has.Length.EqualTo(1));
            Assert.That(state.Player.equipmentRewardSequence, Is.EqualTo(1));
            Object.DestroyImmediate(catalog);
        }

        private static EquipmentItemData Item(
            string instanceId,
            string definitionId,
            EquipmentRarity rarity)
        {
            return new EquipmentItemData
            {
                instanceId = instanceId,
                definitionId = definitionId,
                rarity = (int)rarity
            };
        }
    }
}
