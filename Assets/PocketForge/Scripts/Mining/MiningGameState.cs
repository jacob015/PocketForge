using PocketForge.Content;
using PocketForge.Save;

namespace PocketForge.Mining
{
    public sealed class MiningGameState
    {
        public MiningGameState(GameSaveData player, OreState ore)
        {
            Player = player;
            Ore = ore;
        }

        public GameSaveData Player { get; }
        public OreState Ore { get; private set; }

        public void ReplaceOre(OreState ore) => Ore = ore;
    }

    public sealed class OreState
    {
        public OreState(OreDefinition definition, ChapterDefinition chapter, float durability, bool isRare, bool isBoss)
        {
            Definition = definition;
            Chapter = chapter;
            Durability = durability;
            Health = durability;
            IsRare = isRare;
            IsBoss = isBoss;
            BossTimeRemaining = isBoss ? chapter.BossTimeLimitSeconds : 0f;
        }

        public OreDefinition Definition { get; }
        public ChapterDefinition Chapter { get; }
        public float Durability { get; }
        public float Health { get; set; }
        public bool IsRare { get; }
        public bool IsBoss { get; }
        public float BossTimeRemaining { get; set; }
        public float TapCooldownRemaining { get; private set; }

        public bool CanTap => TapCooldownRemaining <= 0f;

        public void BeginTapCooldown(float seconds)
        {
            TapCooldownRemaining = System.Math.Max(0f, seconds);
        }

        public void TickTapCooldown(float deltaTime)
        {
            TapCooldownRemaining = System.Math.Max(0f, TapCooldownRemaining - System.Math.Max(0f, deltaTime));
        }

        public void ResetBossAttempt()
        {
            Health = Durability;
            BossTimeRemaining = IsBoss ? Chapter.BossTimeLimitSeconds : 0f;
            TapCooldownRemaining = 0f;
        }
    }
}
