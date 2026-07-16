using System;

namespace PocketForge.Save
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int version = 1;
        public int credits;
        public int stage = 1;
        public int pickaxeLevel;
        public int drillLevel;
        public int robotLevel;
    }
}
