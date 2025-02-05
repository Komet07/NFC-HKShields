using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HarmonyLib;
using Game.Units;
using System.Linq;
using System.Reflection;
using Ships;
using Munitions;

namespace StarWarsShields
{
    [HarmonyPatch]
    internal class Patch_WouldArmorHitPenetrateB 
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            return (from method in AccessTools.GetDeclaredMethods(typeof(ShipController))
                    where method.Name.ToLower().Contains("wouldarmorhitpenetrate")
                    select method).Cast<MethodBase>().First<MethodBase>();
        }

        private static void Postfix(ref bool __result, ref bool __state)
        {
            if (__state)
            {
                __result = true;
                
            }
        }


        private static bool Prefix(ShipController __instance, ref bool __state, ref IDamageDealer character)
        {
            foreach (ShieldHull _s1 in Enumerable.OfType<ShieldHull>(__instance.Ship.Hull.AllComponents))
            {
                ShieldSW _s = _s1.gameObject.GetComponent<ShieldSW>();
                if (_s.active && _s1.shieldIntegrityCurrent > 0 && character as IShieldPenMunition != null)
                {


                    __state = true;


                    // _s.DoDamage();

                    continue;
                }

            }

            return true;
        }
    }
}
