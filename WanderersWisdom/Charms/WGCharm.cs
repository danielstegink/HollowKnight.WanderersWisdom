using DanielSteginkUtils.Helpers.Charms.Templates;
using ItemChanger;
using SFCore;
using System;
using UnityEngine;
using WanderersWisdom.Helpers;

namespace WanderersWisdom
{
    /// <summary>
    /// Wanderer's Guile gives the ability to ignore environmental damage for a short time
    /// </summary>
    public class WGCharm : TemplateCharm
    {
        public WGCharm() : base(WanderersWisdom.Instance.Name, false) { }

        protected override string GetName()
        {
            return "Wanderer's Guile";
        }

        protected override string GetDescription()
        {
            return "This token contains the guile of one who has explored the farthest reaches of the world.\n\n" +
                    "Enables the bearer to survive in harsh conditions.";
        }

        protected override int GetCharmCost()
        {
            return 4;
        }

        protected override Sprite GetSpriteInternal()
        {
            return SpriteHelper.Get("WanderersGuile");
        }

        public int geoCost => 2000;

        public override AbstractLocation ItemChangerLocation()
        {
            throw new NotImplementedException();
        }

        #region Settings
        public override void OnLoadLocal()
        {
            ExaltedCharmState charmSettings = new ExaltedCharmState()
            {
                IsEquipped = SharedData.localSaveData.charmEquipped[GetItemChangerId()],
                GotCharm = SharedData.localSaveData.charmFound[GetItemChangerId()],
                IsNew = false,
            };

            RestoreCharmState(charmSettings);
        }

        public override void OnSaveLocal()
        {
            EasyCharmState charmSettings = GetCharmState();
            SharedData.localSaveData.charmEquipped[GetItemChangerId()] = IsEquipped;
            SharedData.localSaveData.charmFound[GetItemChangerId()] = GotCharm;
        }
        #endregion

        #region Activation
        public override void Equip()
        {
            ResetHelper();
            helper.Start();
        }

        public override void Unequip()
        {
            ResetHelper();
        }

        /// <summary>
        /// Custom shield
        /// </summary>
        GuileShield helper;

        /// <summary>
        /// Resets the GuileShield
        /// </summary>
        private void ResetHelper()
        {
            if (helper != null)
            {
                helper.Stop();
            }

            helper = new GuileShield();
        }
        #endregion
    }
}