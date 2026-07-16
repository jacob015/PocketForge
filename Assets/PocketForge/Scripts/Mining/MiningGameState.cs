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
        public OreState(float durability, bool isRare)
        {
            Durability = durability;
            Health = durability;
            IsRare = isRare;
        }

        public float Durability { get; }
        public float Health { get; set; }
        public bool IsRare { get; }
    }
}
