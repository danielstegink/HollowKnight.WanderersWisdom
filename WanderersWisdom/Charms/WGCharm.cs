using ItemChanger;
using Modding;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using WanderersWisdom.Helpers;

namespace WanderersWisdom
{
    /// <summary>
    /// Wanderer's Guile gives the ability to ignore environmental damage for a short time
    /// </summary>
    public class WGCharm : Charm
    {
        public override string Name => "Wanderer's Guile";

        public override string Description => "This token contains the guile of one who has explored the farthest reaches of the world.\n\n" +
                                                "Enables the bearer to survive in harsh conditions.";

        public override int Cost => 2000;

        public override AbstractLocation Location()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores whether we are currently immune to damage
        /// </summary>
        private bool isImmune = false;

        /// <summary>
        /// Stores whether we can ignore the next time we take damage
        /// </summary>
        private bool canIgnore = true;

        /// <summary>
        /// Timer for tracking how long the invincibility lasts
        /// </summary>
        //private Stopwatch timer;

        public WGCharm() { }

        public override void ApplyEffects()
        {
            On.HeroController.TakeDamage += HazardShield;
        }

        /// <summary>
        /// Wanderer's Wisdom makes it possible to ignore environmental damage, such as spikes and acid
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="go"></param>
        /// <param name="damageSide"></param>
        /// <param name="damageAmount"></param>
        /// <param name="hazardType"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void HazardShield(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, 
            int damageAmount, int hazardType)
        {
            // Confirm charm is equipped, damage was taken,
            //  and the damage type wasn't Enemies (1)
            if (IsEquipped() &&
                damageAmount > 0 && 
                hazardType != 1)
            {
                // Check if currently immune or waiting for shield to reset
                if (!canIgnore &&
                    !isImmune)
                {
                    //SharedData.Log("Unable to ignore damage.");
                }
                else
                {
                    //SharedData.Log($"{damageAmount} damage of type {hazardType} dealt.");
                    damageAmount = 0;
                    if (!isImmune)
                    {
                        GameManager.instance.StartCoroutine(StayImmune());
                    }
                }
            }

            orig(self, go, damageSide, damageAmount, hazardType);
        }

        /// <summary>
        /// After ignoring damage, we stay immune for 3 seconds
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerator StayImmune()
        {
            isImmune = true;
            canIgnore = false;

            // Flash green for 2s then yellow for 1s
            //timer = Stopwatch.StartNew();
            HeroController.instance.GetComponent<SpriteFlash>().flash(Color.green, 0.7f, 0.25f, 0.5f, 0.25f);
            yield return new WaitForSeconds(1f);
            //SharedData.Log($"Green stop 1: {timer.ElapsedMilliseconds}");

            HeroController.instance.GetComponent<SpriteFlash>().flash(Color.green, 0.7f, 0.25f, 0.5f, 0.25f);
            yield return new WaitForSeconds(1f);
            //SharedData.Log($"Green stop 2: {timer.ElapsedMilliseconds}");

            HeroController.instance.GetComponent<SpriteFlash>().flash(Color.yellow, 0.7f, 0.25f, 0.5f, 0.25f);
            yield return new WaitForSeconds(1f);
            //SharedData.Log($"Yellow stop: {timer.ElapsedMilliseconds}");

            isImmune = false;
            GameManager.instance.StartCoroutine(BecomeVulnerable());
            yield break;
        }

        /// <summary>
        /// After we lose immunity, we cannot ignore hazard damage for 10 seconds
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerator BecomeVulnerable()
        {
            // Flash red for 10s
            for (int i = 0; i < 10; i++)
            {
                HeroController.instance.GetComponent<SpriteFlash>().flash(Color.red, 0.7f, 0.25f, 0.5f, 0.25f);
                yield return new WaitForSeconds(1f);
                //SharedData.Log($"Red stop {i+1}: {timer.ElapsedMilliseconds}ms");
            }

            //timer.Stop();
            canIgnore = true;
            yield break;
        }
    }
}