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
            { new WWCharm().InternalName(), false },
            { new WGCharm().InternalName(), false },
        };

        public Dictionary<string, bool> charmEquipped = new Dictionary<string, bool>()
        {
            { new WWCharm().InternalName(), false },
            { new WGCharm().InternalName(), false },
        };

        public Dictionary<string, int> charmCost = new Dictionary<string, int>()
        {
            { new WWCharm().InternalName(), 1 },
            { new WGCharm().InternalName(), 4 },
        };

        public bool charmsPlaced = false;
    }
}