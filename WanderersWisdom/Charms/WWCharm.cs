using ItemChanger;
using Modding;
using System;
using System.Collections;
using UnityEngine;
using WanderersWisdom.Helpers;

namespace WanderersWisdom.Charms
{
    /// <summary>
    /// Wanderer's Wisdom creates new synergies between charms and abilities (ie. Mothwing Cloak, Crystal Heart, etc)
    /// </summary>
    public class WWCharm : Charm
    {
        public override string Name => "Wanderer's Wisdom";

        public override string Description => "This token contains the wisdom of one who has explored the farthest reaches of the world.\n\n" +
                                                "Gives the bearer new insight into their abilities.";

        public override AbstractLocation Location()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores the damage modifier applied to CDash
        /// </summary>
        private float cDashModifier = 1;

        /// <summary>
        /// Stores the base damage for spore clouds
        /// </summary>
        private float sporeDamage = -1f;

        /// <summary>
        /// Tracks whether Isma's Clodu has been triggered
        /// </summary>
        private bool ismasCloudActive = false;

        public WWCharm() { }

        public override void ApplyEffects()
        {
            // Mothwing Cloak already synergizes with Dashmaster, Sprintmaster and Sharp Shadow

            On.HealthManager.TakeDamage += MantisClaw;

            On.HeroController.CharmUpdate += CrystalHeart;

            On.HeroController.Move += MonarchWings;

            ModHooks.ObjectPoolSpawnHook += IsmasSpore;
            On.HeroController.Update += IsmasCloud;

            // Dream Nail already synergizes with Dreamwielder and (indirectly) Dream Shield
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
            bool isNailAttack = SharedData.nailAttackNames.Contains(hitInstance.Source.name) ||
                                SharedData.nailArtNames.Contains(hitInstance.Source.name) ||
                                hitInstance.Source.name.Contains("Grubberfly");
            //SharedData.Log($"Attack detected: {hitInstance.Source.name}");

            if (IsEquipped() &&
                PlayerData.instance.hasWalljump &&
                isNailAttack)
            {
                int bonusDamage = 0;
                if (PlayerData.instance.equippedCharm_18)
                {
                    bonusDamage++;
                }

                if (PlayerData.instance.equippedCharm_13)
                {
                    bonusDamage++;
                }

                hitInstance.DamageDealt += bonusDamage;
                //SharedData.Log($"Nail damage increased by {bonusDamage}");
            }

            orig(self, hitInstance);
        }

        #region Crystal Heart
        /// <summary>
        /// Crystal Heart deals increased damage when Swift Focus and Deep Focus are equipped
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void CrystalHeart(On.HeroController.orig_CharmUpdate orig, HeroController self)
        {
            orig(self);

            if (PlayerData.instance.hasSuperDash &&
                IsEquipped())
            {
                float modifier = GetCDModifier();
                Transform superDash = HeroController.instance.transform.Find("SuperDash Damage");
                ApplyCDashBuff(superDash, modifier);

                superDash = HeroController.instance.transform.Find("Effects/SD Burst");
                ApplyCDashBuff(superDash, modifier);

                cDashModifier = modifier;
            }
        }

        /// <summary>
        /// Gets the damage modifier for CDash
        /// </summary>
        /// <returns></returns>
        private float GetCDModifier()
        {
            float modifier = 1;

            // Quick Focus and Deep Focus both boost the damage by 50% (additively)
            if (PlayerData.instance.equippedCharm_7)
            {
                modifier += 0.5f;
            }

            if (PlayerData.instance.equippedCharm_34)
            {
                modifier += 0.5f;
            }

            return modifier;
        }

        /// <summary>
        /// Reviews the super dash and applies the damage buff as needed
        /// </summary>
        /// <param name="superDash"></param>
        /// <param name="modifier"></param>
        private void ApplyCDashBuff(Transform superDash, float modifier)
        {
            if (superDash != null)
            {
                // Get the bonus damage of the CDash
                GameObject dashObject = superDash.gameObject;
                PlayMakerFSM fsm = dashObject.LocateMyFSM("damages_enemy");

                // Adjust the damage
                if (modifier != cDashModifier)
                {
                    int baseDamage = fsm.FsmVariables.GetFsmInt("damageDealt").Value;
                    int oldBaseDamage = (int)(baseDamage / cDashModifier);
                    int newDamage = (int)(oldBaseDamage * modifier);
                    fsm.FsmVariables.GetFsmInt("damageDealt").Value = newDamage;
                    //SharedData.Log($"{superDash.name} damage changed from {baseDamage} to {newDamage}");
                }
            }
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
            if (IsEquipped() &&
                !self.cState.onGround &&
                !self.cState.swimming &&
                PlayerData.instance.hasDoubleJump)
            {
                float newModifier = GetMonarchWingsModifier();
                float newSpeed = move_direction * newModifier;
                //SharedData.Log($"Speed changed from {move_direction} to {newSpeed}");
                move_direction = newSpeed;
            }

            orig(self, move_direction);
        }

        /// <summary>
        /// Gathering Swarm and Sprintmaster both increase airborne movement by 20%
        /// </summary>
        /// <returns></returns>
        private float GetMonarchWingsModifier()
        {
            float modifier = 1f;
            if (PlayerData.instance.equippedCharm_1)
            {
                modifier += 0.2f;
            }

            if (PlayerData.instance.equippedCharm_37)
            {
                modifier += 0.2f;
            }

            return modifier;
        }
        #endregion

        #region Isma's Tear
        /// <summary>
        /// Isma's Tear increases damage dealt by Spore Shroom
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private GameObject IsmasSpore(GameObject gameObject)
        {
            if (gameObject.name.Equals("Knight Spore Cloud(Clone)") &&
                PlayerData.instance.equippedCharm_17 &&
                IsEquipped())
            {
                if (sporeDamage < 0)
                {
                    sporeDamage = gameObject.GetComponent<DamageEffectTicker>().damageInterval;
                }

                gameObject.GetComponent<DamageEffectTicker>().damageInterval = sporeDamage * 0.1f; // 0.9
            }

            return gameObject;
        }

        /// <summary>
        /// Isma's Tear causes the player to emit a green damaging cloud while healing with Shape of Unn
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void IsmasCloud(On.HeroController.orig_Update orig, HeroController self)
        {
            if (IsEquipped() &&
                PlayerData.instance.equippedCharm_28 &&
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
                    GameObject dungPrefab = pool.prefab;

                    // Create a duplicate of the dung cloud
                    GameObject ismaCloud = Instantiate(dungPrefab, HeroController.instance.transform.position, Quaternion.identity);

                    // Make it green so it looks like acid fumes instead of...you know...
                    foreach (ParticleSystem cloudPt in ismaCloud.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        ParticleSystem.MainModule cloudMain = cloudPt.main;
                        cloudMain.startColor = new Color(0.47f, 1f, 0.66f, 0.5f);
                    }

                    yield return new WaitForSeconds(0.25f);
                    break;
                }
            }

            ismasCloudActive = false;
            yield return new WaitForSeconds(0f);
        }
        #endregion
    }
}