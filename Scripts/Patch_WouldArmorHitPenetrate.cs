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


namespace StarWarsShields
{
    [HarmonyPatch]
    internal class Patch_WouldArmorHitPenetrate
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            return (from method in AccessTools.GetDeclaredMethods(typeof(BaseHull))
            where method.Name.ToLower().Contains("wouldarmorhitpenetrate")
            select method).Cast<MethodBase>().First<MethodBase>();
        }

        private static void Postfix(ref bool __result, ref bool __state, ref float damageScaling, ref bool ricochet)
        {
            if (__state)
            {
                __result = false;
                ricochet = false;
                damageScaling = 0f;
            }
        }


        private static bool Prefix(Hull __instance, ref bool __state, ref float damage)
        {
            foreach (ShieldHull _s1 in Enumerable.OfType<ShieldHull>(__instance.AllComponents))
            {
                ShieldSW _s = _s1.gameObject.GetComponent<ShieldSW>();
                if (_s.active && _s1.shieldIntegrityCurrent > 0) 
                {
                    float a = damage;
                    damage = 0f;
                    __state = true;

                    
                    // _s.DoDamage();

                    continue;
                }
                
            }

            return true;
        }

        
    }

}