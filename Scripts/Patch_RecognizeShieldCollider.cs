using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using Munitions;
using HarmonyLib;

namespace StarWarsShields
{
    [HarmonyPatch(typeof(LookaheadMunitionBase))]
    public class Patch_RecognizeShieldCollider
    {

        [HarmonyPrefix]
        [HarmonyPatch("ProcessCollision")]
        static void Postfix(MunitionHitInfo hitInfo)
        {
            if (hitInfo.HitObject != null && hitInfo.HitObject.GetComponent<ShieldHull>() != null)
            {
                hitInfo.HitObject = (hitInfo.HitObject.GetComponent<ShieldHull>().Socket.MyHull.gameObject != null) ? hitInfo.HitObject.GetComponent<ShieldHull>().Socket.MyHull.gameObject : hitInfo.HitObject;
            }
        }
    }
}
