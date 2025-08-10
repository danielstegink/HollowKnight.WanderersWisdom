using System.Collections.Generic;
using WanderersWisdom.Charms;

namespace WanderersWisdom
{
    /// <summary>
    /// Charm-related info that gets stored in the save file
    /// </summary>
    public class LocalSaveData
    {
        public Dictionary<string, bool> charmFound = new Dictionary<string, bool>()
        {
            { "WanderersWisdom", false },
            { "WanderersGuile", false },
        };

        public Dictionary<string, bool> charmEquipped = new Dictionary<string, bool>()
        {
            { "WanderersWisdom", false },
            { "WanderersGuile", false },
        };

        public Dictionary<string, int> charmCost = new Dictionary<string, int>()
        {
            { "WanderersWisdom", 1 },
            { "WanderersGuile", 4 },
        };

        public bool charmsPlaced = false;
    }
}