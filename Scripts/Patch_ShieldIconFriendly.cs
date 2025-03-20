using HarmonyLib;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Game.UI;
using Game.Units;
using Ships;
using UI;
using QuickGraph.Serialization;

namespace StarWarsShields
{
    [HarmonyPatch(typeof(FriendlyShipItem))]
    public class Patch_ShieldIconFriendly
    {
        public const string newObjName = "ShieldStatusIcon";

        [HarmonyPostfix]
        [HarmonyPatch("SetShip")]
        static void PostfixSetShip(ref FriendlyShipItem __instance, ShipController ship)
        {
            Debug.Log("(HK SHIELDS) HARMONY PATCH IS RUNNING!");
            bool _a = false;

            List<ShieldSW> shield = new List<ShieldSW>();
            List<int> r = new List<int>();
            ModUtil _mod = new ModUtil();
            foreach (ShieldHull _s1 in Enumerable.OfType<ShieldHull>(ship.Ship.Hull.AllComponents))
            {
                _a = true;
                ShieldSW _s = _s1.gameObject.GetComponent<ShieldSW>();
                shield.Add(_s);
                r.Add(-1);
                Debug.Log("(HK SHIELDS - SHIELD UI INITIALIZATION) FOUND SHIELD : " + ship.Ship.ShipDisplayName + " - AT SOCKET: " + _s1.Socket.Key + " - Shield : " + shield.Count);
                continue;
            }

            Debug.Log("(HK SHIELDS) PATCH FLAG 1 : " + _a);

            if (_a)
            {

                MovementStatusIcon _powerQuantityIcon = __instance.GetComponentInChildren<MovementStatusIcon>();
                
                GameObject originalObject = _powerQuantityIcon.gameObject;

                GameObject newObject = Object.Instantiate(originalObject);
                newObject.transform.SetParent(originalObject.transform.parent, false);
                newObject.transform.SetSiblingIndex(originalObject.transform.GetSiblingIndex() + 1);
                newObject.name = newObjName;

                MovementStatusIcon newStatusIcon = newObject.GetComponent<MovementStatusIcon>();


                TooltipTrigger _t = (TooltipTrigger)_mod.GetPrivateField(newStatusIcon, "_tooltip");
                Graphic _g = (Graphic)_mod.GetPrivateField(newStatusIcon, "_graphic");

                Object.Destroy(newStatusIcon);

                ShieldUI _sUI = newObject.AddComponent<ShieldUI>();
                _sUI._s = shield;
                _sUI._register = r;

                _mod.SetPrivateField(_sUI, "_showWhenNormal", true);

                _mod.SetPrivateField(_sUI, "_graphic", _g);
                _mod.SetPrivateField(_sUI, "_tooltip", _t);

                _sUI.Show();
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        static void PostfixOnDestroy(ref FriendlyShipItem __instance)
        {

            if (__instance.Ship == null)
                return;

            ModUtil _mod = new ModUtil();

            QuantityStatusIcon _powerQuantityIcon = (QuantityStatusIcon)_mod.GetPrivateField(__instance, "_powerQuantityIcon");
            Transform newObjectTransform = _powerQuantityIcon.transform.parent.Find(newObjName);
            if (newObjectTransform == null)
                return;

            GameObject newObject = newObjectTransform.gameObject;
            UnityEngine.Object.Destroy(newObject);
        }
    }
}
    
