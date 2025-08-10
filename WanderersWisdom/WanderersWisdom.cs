using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;
using Modding;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using WanderersWisdom.Charms;

namespace WanderersWisdom
{
    public class WanderersWisdom : Mod, IMod, ILocalSettings<LocalSaveData>
    {
        public static WanderersWisdom Instance;

        public override string GetVersion() => "1.1.0.0";

        #region Save Settings
        public void OnLoadLocal(LocalSaveData s)
        {
            SharedData.localSaveData = s;

            if (SharedData.wgCharm != null)
            {
                SharedData.wgCharm.OnLoadLocal();
            }

            if (SharedData.wwCharm != null)
            {
                SharedData.wwCharm.OnLoadLocal();
            }
        }

        public LocalSaveData OnSaveLocal()
        {
            if (SharedData.wgCharm != null)
            {
                SharedData.wgCharm.OnSaveLocal();
            }

            if (SharedData.wwCharm != null)
            {
                SharedData.wwCharm.OnSaveLocal();
            }

            return SharedData.localSaveData;
        }
        #endregion

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = new WanderersWisdom();

            if (ModHooks.GetMod("DebugMod") != null)
            {
                AddToGiveAllCharms(GiveCharms);
            }

            SharedData.wgCharm = new WGCharm();
            SharedData.wwCharm = new WWCharm();

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AddToShop;

            Log("Initialized");
        }

        /// <summary>
        /// If all maps have been collected, add the charms to Iselda's shop
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void AddToShop(Scene arg0, Scene arg1)
        {
            if (PlayerData.instance.GetBool("corniferAtHome"))
            {
                // Finder stores all locations in its local files, including Iselda's Shop
                ShopLocation iseldaShop = (ShopLocation)Finder.GetLocation("Iselda");
                ShopPlacement shopPlacement = (ShopPlacement)iseldaShop.Wrap();

                // Make sure to specify default items so we don't break Iselda's store
                shopPlacement.defaultShopItems = DefaultShopItems.IseldaMaps |
                                                    DefaultShopItems.IseldaCharms | 
                                                    DefaultShopItems.IseldaMapMarkers | 
                                                    DefaultShopItems.IseldaMapPins |
                                                    DefaultShopItems.IseldaMaps |
                                                    DefaultShopItems.IseldaQuill;

                if (!SharedData.wwCharm.GotCharm)
                {
                    // Get charm as an item from Finder
                    AbstractItem charmItem = Finder.GetItem(SharedData.wwCharm.GetItemChangerId());

                    // Add item to shop
                    shopPlacement.AddItemWithCost(charmItem, SharedData.wwCharm.geoCost);
                }

                if (!SharedData.wgCharm.GotCharm)
                {
                    // Get charm as an item from Finder
                    AbstractItem charmItem = Finder.GetItem(SharedData.wgCharm.GetItemChangerId());

                    // Add item to shop
                    shopPlacement.AddItemWithCost(charmItem, SharedData.wgCharm.geoCost);
                }

                // Add placement back to ItemChanger
                ItemChangerMod.AddPlacements(new List<AbstractPlacement> { shopPlacement }, PlacementConflictResolution.Replace);
            }
        }

        #region Debug Mod
        /// <summary>
        /// Links the given method into the Debug mod's "GiveAllCharms" function
        /// </summary>
        /// <param name="a"></param>
        public static void AddToGiveAllCharms(Action function)
        {
            var commands = Type.GetType("DebugMod.BindableFunctions, DebugMod");
            if (commands == null)
            {
                return;
            }

            var method = commands.GetMethod("GiveAllCharms", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return;
            }

            new Hook(method, (Action orig) =>
            {
                orig();
                function();
            }
            );
        }

        /// <summary>
        /// Adds the mod's charm to the player (used by Debug Mode mod)
        /// </summary>
        private void GiveCharms()
        {
            SharedData.wgCharm.GiveCharm();
            SharedData.wwCharm.GiveCharm();
        }
        #endregion
    }
}