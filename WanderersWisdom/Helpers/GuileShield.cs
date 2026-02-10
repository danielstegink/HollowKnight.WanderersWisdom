using DanielSteginkUtils.Helpers.Shields;
using System;
using System.Collections;
using UnityEngine;

namespace WanderersWisdom.Helpers
{
    public class GuileShield : ShieldHelper
    {
        /// <summary>
        /// Tracks if the shield has been overloaded and cannot be triggered
        /// </summary>
        public bool overloaded = false;

        /// <summary>
        /// WG only triggers on environmental damage
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <param name="hazardType"></param>
        /// <returns></returns>
        public override bool CanTakeDamage(int damageAmount, int hazardType)
        {
            return base.CanTakeDamage(damageAmount, hazardType) &&
                    hazardType != 1;
        }

        public override bool CustomShieldCheck()
        {
            return !overloaded;
        }

        public override IEnumerator CustomEffects()
        {
            // First we flash green to show the shield is active
            for (int i = 0; i < 2; i++)
            {
                HeroController.instance.GetComponent<SpriteFlash>().flash(Color.green, 0.7f, 0.25f, 0.5f, 0.25f);
                yield return new WaitForSeconds(1f);
            }

            // Then we flash yellow to show the effect is about to end
            HeroController.instance.GetComponent<SpriteFlash>().flash(Color.yellow, 0.7f, 0.25f, 0.5f, 0.25f);
            yield return new WaitForSeconds(1f);

            // Then we have to trigger the cooldown period
            overloaded = true;
            GameManager.instance.StartCoroutine(Cooldown());
        }

        /// <summary>
        /// After we lose immunity, the shield is overloaded and cannot be used for a short time
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerator Cooldown()
        {
            // Flash red while the shield cannot be used
            for (int i = 0; i < 10; i++)
            {
                HeroController.instance.GetComponent<SpriteFlash>().flash(Color.red, 0.7f, 0.25f, 0.5f, 0.25f);
                yield return new WaitForSeconds(1f);
            }

            overloaded = false;
            yield break;
        }
    }
}
