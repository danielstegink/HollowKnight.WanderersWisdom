﻿using ItemChanger.Tags;
using ItemChanger.UIDefs;
using ItemChanger;
using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using WanderersWisdom.Helpers;
using MonoMod.RuntimeDetour;
using System.Reflection;
using SFCore;
using System.Linq;
using UnityEngine.SceneManagement;
using ItemChanger.Placements;
using ItemChanger.Locations;
using WanderersWisdom.Charms;

namespace WanderersWisdom
{
    public class WanderersWisdom : Mod, IMod, ILocalSettings<LocalSaveData>
    {
        public override string GetVersion() => "1.0.1.0";

        #region Save Settings
        /// <summary>
        /// Data for the save file
        /// </summary>
        private static LocalSaveData localSaveData { get; set; } = new LocalSaveData();

        public void OnLoadLocal(LocalSaveData s)
        {
            localSaveData = s;
        }

        public LocalSaveData OnSaveLocal()
        {
            return localSaveData;
        }
        #endregion

        #region Charms
        public List<Charm> charms = new List<Charm>()
        {
            new WWCharm(),
            new WGCharm(),
        };
        #endregion

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            ModHooks.LanguageGetHook += GetCharmText;
            ModHooks.GetPlayerBoolHook += GetCharmBools;
            ModHooks.SetPlayerBoolHook += SetCharmBools;
            ModHooks.GetPlayerIntHook += GetCharmCosts;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;

            if (ModHooks.GetMod("DebugMod") != null)
            {
                AddToGiveAllCharms(GiveCharms);
            }

            foreach (Charm charm in charms)
            {
                // Add the charm to the charm list and get its new ID number
                Sprite sprite = SpriteHelper.Get(charm.InternalName());
                int charmId = CharmHelper.AddSprites(new Sprite[] { sprite })[0];
                charm.Num = charmId;
                SharedData.Log($"Sprite found for {charm.Name}. ID assigned: {charmId}");

                // Apply charm effects
                charm.ApplyEffects();

                // Add the charm to ItemChanger for placement
                var item = new ItemChanger.Items.CharmItem()
                {
                    charmNum = charm.Num,
                    name = charm.InternalName(),
                    UIDef = new MsgUIDef()
                    {
                        name = new LanguageString("UI", $"CHARM_NAME_{charm.Num}"),
                        shopDesc = new LanguageString("UI", $"CHARM_DESC_{charm.Num}"),
                        sprite = new Helpers.ItemChangerSprite(charm.InternalName(), sprite)
                    }
                };

                var mapModTag = item.AddTag<InteropTag>();
                mapModTag.Message = "RandoSupplementalMetadata";
                mapModTag.Properties["ModSource"] = GetName();
                mapModTag.Properties["PoolGroup"] = "Charms";

                Finder.DefineCustomItem(item);
            }

            Log("Initialized");
        }

        #region Charm Data and Settings
        /// <summary>
        /// Gets text data related to the charms (name and description)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sheetName"></param>
        /// <param name="orig"></param>
        /// <returns></returns>
        private string GetCharmText(string key, string sheetName, string orig)
        {
            foreach (Charm charm in charms)
            {
                if (key.EndsWith(charm.Num.ToString()))
                {
                    if (key.StartsWith("CHARM_NAME"))
                    {
                        return charm.Name;
                    }
                    else if (key.StartsWith("CHARM_DESC"))
                    {
                        return charm.Description;
                    }
                }
            }

            return orig;
        }

        /// <summary>
        /// Gets boolean values related to the charms (equipped, new, found)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private bool GetCharmBools(string key, bool defaultValue)
        {
            foreach (Charm charm in charms)
            {
                if (key.EndsWith(charm.Num.ToString()))
                {
                    if (key.StartsWith("gotCharm_"))
                    {
                        return localSaveData.charmFound[charm.InternalName()];
                    }
                    else if (key.StartsWith("equippedCharm_"))
                    {
                        return localSaveData.charmEquipped[charm.InternalName()];
                    }
                    else if (key.StartsWith("newCharm_"))
                    {
                        return false;
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets boolean values related to the charms (equipped, new, found)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="orig"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool SetCharmBools(string key, bool orig)
        {
            foreach (Charm charm in charms)
            {
                if (key.EndsWith(charm.Num.ToString()))
                {
                    if (key.StartsWith("gotCharm_"))
                    {
                        localSaveData.charmFound[charm.InternalName()] = orig;
                    }
                    else if (key.StartsWith("equippedCharm_"))
                    {
                        localSaveData.charmEquipped[charm.InternalName()] = orig;
                    }
                }
            }

            return orig;
        }

        /// <summary>
        /// Gets the costs of charms
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int GetCharmCosts(string key, int defaultValue)
        {
            foreach (Charm charm in charms)
            {
                if (key.EndsWith(charm.Num.ToString()))
                {
                    if (key.StartsWith("charmCost_"))
                    {
                        return localSaveData.charmCost[charm.InternalName()];
                    }
                }
            }

            return defaultValue;
        }
        #endregion

        /// <summary>
        /// If all maps have been collected, add the charms to Iselda's shop
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SceneChanged(Scene arg0, Scene arg1)
        {
            if (PlayerData.instance.corniferAtHome)
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

                foreach (Charm charm in charms)
                {
                    // Skip if charm has already been bought
                    if (localSaveData.charmFound[charm.InternalName()])
                    {
                        continue;
                    }

                    // Get charm as an item from Finder
                    AbstractItem charmItem = Finder.GetItem(charm.InternalName());

                    // Add item to shop
                    shopPlacement.AddItemWithCost(charmItem, charm.Cost);
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
            string[] keys = localSaveData.charmFound.Keys.ToArray();
            foreach (string key in keys)
            {
                localSaveData.charmFound[key] = true;
            }
        }
        #endregion
    }
}