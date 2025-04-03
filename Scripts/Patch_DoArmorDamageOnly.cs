using Game.Units;
using HarmonyLib;
using Munitions;
using Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using StarWarsShields;
using UnityEngine;
using Game;


namespace StarWarsShields
{
    [HarmonyPatch]
    internal class Patch_DoArmorDamageOnly
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            return (from method in AccessTools.GetDeclaredMethods(typeof(ShipController))
            where method.Name.ToLower().Contains("doarmordamageonly")
            select method).Cast<MethodBase>().First<MethodBase>();
        }

        /* private static void Postfix(ShipController __instance, MunitionHitInfo hitInfo, IDamageDealer damager, ref HitResult __result)
        {
            foreach (ShieldHull _s1 in Enumerable.OfType<ShieldHull>(__instance.Ship.Hull.AllComponents))
            {
                ShieldSW _s = _s1.gameObject.GetComponent<ShieldSW>();
                if (_s.active && _s1.shieldIntegrityCurrent > 0)
                {
                    if (damager as IShieldPenMunition != null)
                    {
                        __result = HitResult.Overpenetrated;
                        return;
                    }

                    break;
                }


            }
        } */

        private static bool Prefix(ShipController __instance, MunitionHitInfo hitInfo, IDamageDealer damager)
        {
            
            // CHECK FOR DEFENSIVE-ONLY AOE EFFECT
            if (damager.GetType() == typeof(AOEExplosionEffectModule))
            {
                ModUtil _mod = new ModUtil();
                if ((bool)_mod.GetPrivateField(damager, "_munitionsOnly") == true)
                {
                    
                    return true;
                }
            }
            

            foreach (ShieldHull _s1 in Enumerable.OfType<ShieldHull>(__instance.Ship.Hull.AllComponents))
            {
                ShieldSW _s = _s1.gameObject.GetComponent<ShieldSW>();
                if (_s.active && _s1.shieldIntegrityCurrent > 0)
                {
                    /* if (damager as IShieldPenMunition != null)
                    {
                        __result = HitResult.Overpenetrated;
                        return true;
                    } */

                    _s._hitInfo = hitInfo;
                    _s._damager = damager;

                    _s.DoDamage();

                    break;
                }
                else if (_s1.shieldIntegrityCurrent <= 0 && _s1._hullDamageResetsShieldRecharge)
                {
                    _s.tPassedSinceLastDamage = 0;

                    continue;
                }


            }

            return true;
        }
        
    }

}