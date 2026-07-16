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
        public OreState(OreDefinition definition, float durability, bool isRare)
        {
            Definition = definition;
            Durability = durability;
            Health = durability;
            IsRare = isRare;
        }

        public OreDefinition Definition { get; }
        public float Durability { get; }
        public float Health { get; set; }
        public bool IsRare { get; }
    }
}
