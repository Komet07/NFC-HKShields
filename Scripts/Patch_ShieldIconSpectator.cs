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

namespace StarWarsShields
{
    [HarmonyPatch(typeof(SpectatorShipItem))]
    public class Patch_ShieldIconSpectator
    {
        public const string newObjName = "ShieldStatusIcon";

        [HarmonyPostfix]
        [HarmonyPatch("SetShip")]
        static void PostfixSetShip(ref SpectatorShipItem __instance, ShipController ship)
        {
            bool _a = false;
            ModUtil _mod = new ModUtil();
            ShieldSW shield = null;
            foreach (ShieldHull _s1 in Enumerable.OfType<ShieldHull>(ship.Ship.Hull.AllComponents))
            {
                _a = true;
                ShieldSW _s = _s1.gameObject.GetComponent<ShieldSW>();
                shield = _s;
                continue;
            }

            if (_a)
            {
                MovementStatusIcon _ammoQuantityIcon = __instance.GetComponentInChildren<MovementStatusIcon>();

            
                GameObject originalObject = _ammoQuantityIcon.gameObject;
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
                

                _mod.SetPrivateField(_sUI, "_graphic", _g);
                _mod.SetPrivateField(_sUI, "_tooltip", _t);

                _sUI.Show();
            }
        
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        static void PostfixOnDestroy(ref SpectatorShipItem __instance)
        {

            ModUtil _mod = new ModUtil();
            ShipController _ship = (ShipController)_mod.GetPrivateField(__instance, "_ship");
            if (_ship == null)
                return;

        

            QuantityStatusIcon _ammoQuantityIcon = (QuantityStatusIcon)_mod.GetPrivateField(__instance, "_ammoQuantityIcon");
            Transform newObjectTransform = _ammoQuantityIcon.transform.parent.Find(newObjName);
            if (newObjectTransform == null)
                return;

            GameObject newObject = newObjectTransform.gameObject;
            UnityEngine.Object.Destroy(newObject);
        }
    }
}
