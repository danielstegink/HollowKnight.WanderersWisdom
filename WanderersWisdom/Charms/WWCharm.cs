using DanielSteginkUtils.Helpers.Charms.Shroom;
using DanielSteginkUtils.Helpers.Charms.Templates;
using DanielSteginkUtils.Utilities;
using ItemChanger;
using Modding;
using SFCore;
using System;
using System.Collections;
using UnityEngine;
using WanderersWisdom.Helpers;

namespace WanderersWisdom.Charms
{
    /// <summary>
    /// Wanderer's Wisdom creates new synergies between charms and abilities (ie. Mothwing Cloak, Crystal Heart, etc)
    /// </summary>
    public class WWCharm : TemplateCharm
    {
        public WWCharm() : base(WanderersWisdom.Instance.Name, false) 
        {
            ModHooks.CharmUpdateHook += OnCharmUpdate;
        }

        protected override string GetName()
        {
            return "Wanderer's Wisdom";
        }

        protected override string GetDescription()
        {
            return "This token contains the wisdom of one who has explored the farthest reaches of the world.\n\n" +
                    "Gives the bearer new insight into their abilities.";
        }

        protected override int GetCharmCost()
        {
            return 1;
        }

        protected override Sprite GetSpriteInternal()
        {
            return SpriteHelper.Get("WanderersWisdom");
        }

        public int geoCost => 500;

        public override AbstractLocation ItemChangerLocation()
        {
            throw new NotImplementedException();
        }

        #region Settings
        public override void OnLoadLocal()
        {
            EasyCharmState charmSettings = new EasyCharmState()
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
            // Mothwing Cloak already synergizes with Dashmaster, Sprintmaster and Sharp Shadow

            On.HealthManager.TakeDamage += MantisClaw;

            On.HealthManager.TakeDamage += CrystalHeart;

            On.HeroController.Move += MonarchWings;

            ResetSporeHelper();
            sporeHelper.Start();

            On.HeroController.Update += IsmasCloud;

            // Dream Nail already synergizes with Dreamwielder and (indirectly) Dream Shield
        }

        public override void Unequip()
        {
            On.HealthManager.TakeDamage -= MantisClaw;

            On.HealthManager.TakeDamage -= CrystalHeart;

            On.HeroController.Move -= MonarchWings;

            ResetSporeHelper();
            
            On.HeroController.Update -= IsmasCloud;
        }

        /// <summary>
        /// When charms are equipped/unequipped, the various helpers also need to be reset
        /// </summary>
        /// <param name="data"></param>
        /// <param name="controller"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnCharmUpdate(PlayerData data, HeroController controller)
        {
            ResetSporeHelper();
            if (IsEquipped)
            {
                sporeHelper.Start();
            }
        }

        /// <summary>
        /// If Mantis Claw is unlocked, the player deals increased nail damage when Mark of Pride or Longnail is equipped
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="hitInstance"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MantisClaw(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (PlayerData.instance.GetBool("hasWalljump") &&
                Logic.IsNailAttack(hitInstance))
            {
                // The player can have up to 11 charm notches w/o mods. Since we use 1 for this charm, that leaves 10 more
                
                // We always want to promote different charm loadouts, so for balance we will say that this charm can give
                // 1 notch of utility if the player has 5 notches worth of relevant charms equipped

                // In the case of Mantis Claw, 1 notch of nail damage is worth 10% per my Utils folder, or
                // 2 damage if the nail is fully upgraded

                // LN and MOP together equal 2 damage, and their costs are almost equal, so having both give 1 point of damage is balanced

                int bonusDamage = 0;
                if (PlayerData.instance.GetBool("equippedCharm_18"))
                {
                    bonusDamage++;
                }

                if (PlayerData.instance.GetBool("equippedCharm_13"))
                {
                    bonusDamage++;
                }

                hitInstance.DamageDealt += bonusDamage;
            }

            orig(self, hitInstance);
        }

        #region Crystal Heart
        /// <summary>
        /// Crystal Heart deals increased damage when Swift Focus and Deep Focus are equipped
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="hitInstance"></param>
        private void CrystalHeart(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (hitInstance.Source.name.Equals("SuperDash Damage"))
            {
                int bonusDamage = (int)GetCDBonus();
                hitInstance.DamageDealt += bonusDamage;
                //WanderersWisdom.Instance.Log($"CDash damage increased by {bonusDamage}");
            }

            orig(self, hitInstance);
        }

        /// <summary>
        /// Gets the damage bonus for CDash
        /// </summary>
        /// <returns></returns>
        private float GetCDBonus()
        {
            float bonusDamage = 0f;

            // Per my logic in Mantis Claw, 5 notches of equipped charms will be worth 1 notch of increased damage
            // In the case of CDash, we can treat the bonus as an increases in dash damage
            float bonusDamagePerNotch = NotchCosts.DashDamagePerNotch() / 5;
            //WanderersWisdom.Instance.Log($"CDash bonus damage per notch: {bonusDamagePerNotch}");

            // Quick Focus costs 3 notches
            if (PlayerData.instance.GetBool("equippedCharm_7"))
            {
                bonusDamage += 3 * bonusDamagePerNotch;
            }

            // Deep Focus is worth 4 notches
            if (PlayerData.instance.GetBool("equippedCharm_34"))
            {
                bonusDamage += 4 * bonusDamagePerNotch;
            }

            return bonusDamage;
        }
        #endregion

        #region Monarch Wings
        /// <summary>
        /// If the player has Monarch Wings unlocked, equipping Gathering Swarm and/or Sprintmaster
        /// will allow them to move side-to-side quickly while mid-air
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="move_direction"></param>
        private void MonarchWings(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (!self.cState.onGround &&
                !self.cState.swimming &&
                PlayerData.instance.GetBool("hasDoubleJump"))
            {
                float newModifier = GetMWModifier();
                float newSpeed = move_direction * newModifier;
                move_direction = newSpeed;
            }

            orig(self, move_direction);
        }

        /// <summary>
        /// Gets the speed modifier for Monarch Wings
        /// </summary>
        /// <returns></returns>
        private float GetMWModifier()
        {
            float modifier = 1f;

            // Per above, 5 notches is worth 1 notch of utility
            // Per Sprintmaster, 1 notch is worth a 20% boost in speed
            // However, this only affects airborne velocity
            // Going off vibes, I'd say a boost in airborne speed would be worth 40%
            float speedPerNotch = 0.4f / 5;

            // Dashmaster and Gatherwing Swarm are both worth 1 notch
            if (PlayerData.instance.GetBool("equippedCharm_1"))
            {
                modifier += speedPerNotch;
            }

            if (PlayerData.instance.GetBool("equippedCharm_37"))
            {
                modifier += speedPerNotch;
            }

            return modifier;
        }
        #endregion

        #region Isma's Tear
        /// <summary>
        /// Isma's Tear increases damage dealt by Spore Shroom
        /// </summary>
        SporeDamageHelper sporeHelper;

        /// <summary>
        /// Resets the SporeDamageHelper
        /// </summary>
        private void ResetSporeHelper()
        {
            if (sporeHelper != null)
            {
                sporeHelper.Stop();
            }

            sporeHelper = new SporeDamageHelper(WanderersWisdom.Instance.Name, GetItemChangerId(), GetSporeModifier());
        }

        /// <summary>
        /// Gets the damage modifier for Spore Shroom
        /// </summary>
        /// <returns></returns>
        private float GetSporeModifier()
        {
            float modifier = 1f;

            // Per above, 5 notches is worth 1 notch of value
            // Spore Shroom is worth 1 notch, so we can increase its damage by 1 / 5 = 20%
            if (PlayerData.instance.GetBool("equippedCharm_17"))
            {
                modifier += 0.2f;
            }

            return modifier;
        }

        /// <summary>
        /// Tracks whether Isma's Clodu has been triggered
        /// </summary>
        private bool ismasCloudActive = false;

        /// <summary>
        /// Isma's Tear causes the player to emit a green damaging cloud while healing with Shape of Unn
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void IsmasCloud(On.HeroController.orig_Update orig, HeroController self)
        {
            if (PlayerData.instance.GetBool("equippedCharm_28") &&
                self.cState.focusing && 
                !ismasCloudActive)
            {
                ismasCloudActive = true;
                GameManager.instance.StartCoroutine(TriggerIsmasCloud());
            }

            orig(self);
        }

        /// <summary>
        /// Parallel thread for handling Isma's Cloud
        /// </summary>
        /// <returns></returns>
        private IEnumerator TriggerIsmasCloud()
        {
            // Find the dung cloud prefab
            foreach (var pool in ObjectPool.instance.startupPools)
            {
                if (pool.prefab.name == "Knight Dung Trail")
                {
                    // Create a duplicate of the dung cloud
                    GameObject ismaCloud = UnityEngine.GameObject.Instantiate(pool.prefab, HeroController.instance.transform.position, Quaternion.identity);
                    ismaCloud.name = "WanderersWisdom.IsmaCloud";

                    // Make it green so it looks like acid fumes instead of...you know...
                    foreach (ParticleSystem cloudPt in ismaCloud.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        ParticleSystem.MainModule cloudMain = cloudPt.main;
                        cloudMain.startColor = new Color(0.47f, 1f, 0.66f, 0.5f);
                    }

                    // Per above, we want 1 notch of utility per 5 notches used
                    // SOU costs 2 notches, so its worth 2/5 of a notch
                    float notchValue = 2f / 5f;

                    // However, we only produce the clouds while SOU is active
                    // Per my Utils, an ability is 5x as valuable when it only happens during SOU
                    notchValue /= NotchCosts.UnnModifier();

                    // In total, this means we should produce 2 notches worth of clouds
                    // However, the clouds are inconvenient due to their size, so we will spend 1 notch increasing their size
                    // Based on other mods I've done, 1 notch is worth a 50% size increase in Dung Clouds
                    notchValue -= 1;
                    ismaCloud.transform.localScale *= 1.5f;

                    // Defender's Crest produces these clouds ever 0.75 seconds for 1 notch, so we can spend the remaining notch
                    // keeping the cooldown the same
                    float cooldown = 0.75f / notchValue;
                    yield return new WaitForSeconds(cooldown);
                    break;
                }
            }

            ismasCloudActive = false;
            yield return new WaitForSeconds(0f);
        }
        #endregion
        #endregion
    }
}