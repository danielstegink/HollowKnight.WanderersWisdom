using System.Collections.Generic;
using WanderersWisdom.Charms;
using WanderersWisdom.Helpers;

namespace WanderersWisdom
{
    /// <summary>
    /// Stores variables and functions used by multiple files in this project
    /// </summary>
    public static class SharedData
    {
        public static WGCharm wgCharm;

        public static WWCharm wwCharm;

        /// <summary>
        /// Data for the save file
        /// </summary>
        public static LocalSaveData localSaveData { get; set; } = new LocalSaveData();
    }
}
