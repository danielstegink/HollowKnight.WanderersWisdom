using ItemChanger;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WanderersWisdom.Helpers
{
    /// <summary>
    /// Template class for charms created by this mod
    /// </summary>
    public abstract class Charm : MonoBehaviour
    {
        /// <summary>
        /// Charm's name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Charm's description
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Numeric ID indicating the charm's position in the game's charm list. Assigned 
        /// by the mod at startup
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// Location where the charm should be placed on the map
        /// </summary>
        /// <returns></returns>
        public abstract AbstractLocation Location();

        /// <summary>
        /// Sprite for the charm icon
        /// </summary>
        public Sprite Sprite { get; set; }

        /// <summary>
        /// Specially formatted version of the name. Used as a key for
        /// linking the charm to certain settings
        /// </summary>
        /// <returns></returns>
        public string InternalName()
        {
            return Regex.Replace(Name, @"[^0-9a-zA-Z\._]", "");
        }

        /// <summary>
        /// Whether or not the charm is currently equipped by the player
        /// </summary>
        /// <returns></returns>
        public bool IsEquipped()
        {
            return PlayerData.instance.GetBool($"equippedCharm_{Num}");
        }

        /// <summary>
        /// Hooks the charm's effect methods into the game
        /// </summary>
        public virtual void ApplyEffects() { }
    }
}
