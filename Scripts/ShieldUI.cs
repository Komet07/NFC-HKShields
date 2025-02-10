using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Game.UI;
using Game.Units;
using Ships;
using UI;
using Utility;

namespace StarWarsShields
{
    public class ShieldUI : StatusIcon
    {

        [SerializeField]
        private Image _iconImage = null;

        [SerializeField]
        public ShieldSW _s;

        ModUtil _m = new ModUtil();

        // RateLimitedLogger dA = new RateLimitedLogger(1);

        public Color[] _shieldColorLibrary = null;


        // INDEX SHIELD NETWORKING WITH REGISTER
        private int _register = -1;

        public void FixedUpdate()
        {

            if (_s == null)
            {
                return;
            }

            if (_shieldColorLibrary == null)
            {
                _shieldColorLibrary = new Color[] { _s.shieldHullClass.ColorNominal, _s.shieldHullClass.ColorLightDamage, _s.shieldHullClass.ColorModerateDamage, _s.shieldHullClass.ColorHeavyDamage, _s.shieldHullClass.ColorVeryHeavyDamage, _s.shieldHullClass.ColorNoHealth, _s.shieldHullClass.ColorDisabled, _s.shieldHullClass.ColorDestroyed };

                // <-- CHECK FOR COLORBLIND ACCESSIBILITY: --> (+ Override if different palette used than regularly)
                Color[] _standards = new Color[] { new Color(0.23f, 0.781f, 0.117f), new Color(0.824f, 0.816f, 0.121f), new Color(0.968f, 0.396f, 0.105f), new Color(0.929f, 0.128f, 0.113f)};


                // <- CHECK: GREEN ->
                if (GameColors.Green != _standards[0])
                {
                    _shieldColorLibrary[0] = GameColors.Green;
                }

                // <- CHECK: YELLOW ->
                if (GameColors.Yellow != _standards[1])
                {
                    _shieldColorLibrary[1] = GameColors.Yellow;
                }

                // <- CHECK: ORANGE ->
                if (GameColors.Orange != _standards[2])
                {
                    _shieldColorLibrary[2] = GameColors.Orange;
                }

                // <- CHECK: RED ->
                if (GameColors.Red != _standards[3])
                {
                    _shieldColorLibrary[3] = GameColors.Red;
                }
            }

            float _val = _s.shieldHullClass.shieldIntegrityCurrent / _s.shieldHullClass.statShieldIntegrityMax.Value;
            ComponentActivity _a = ComponentActivity.Active;
            string _tActivity = "";

            if (_register != -1 && ShieldNetworking.Instance != null)
            {
                _a = _s.shieldHullClass.GetActivityStatus();
            }

            switch (_a)
            {
                /* case ComponentActivity.MissingResource:
                    _tActivity = "<color=red>NO POWER</color>";
                    break; */
                case ComponentActivity.Destroyed:
                    _tActivity = "<color=red>DESTROYED</color>";
                    break;
                case ComponentActivity.Disabled:
                    _tActivity = "<color=red>DISABLED</color>";
                    break;
                default:
                    _tActivity = "";
                    break;
            }

            if (_s.shieldHullClass.shieldIntegrityCurrent == 0 && _tActivity == "")
            {
                _tActivity = "<color=red>NO HEALTH</color>";

            }
            else if (_s.DownBecauseOfCarrierOps && _tActivity == "")
            {
                _tActivity = "<color=red>CARRIER OPERATIONS</color>";
            }

            // dA.LogLimited("(HK SHIELDS - " + ((ShieldNetworking.Instance.isServer) ? "HOST" : "CLIENT") + ") CURRENT UI REGISTER : " + _register + " - CURRENT SHIELD HEALTH VALUE: " + ShieldNetworking.Instance.healthValue(_register) + " HP");

            UpdateTooltipText("Integrity: " + Mathf.Round(_val*100).ToString() + "% (" + ShieldNetworking.Instance.healthValue(_register) + " HP / " + _s.shieldHullClass.statShieldIntegrityMax.Value + " HP)" + ((_tActivity != "") ? "\n" + _tActivity : ""));

            if (_register == -1 && _s._register != -1)
            {
                _register = _s._register;
            }

            if (_iconImage != null)
            {

                if (_s.shieldHullClass.shieldIntegrityCurrent == 0)
                {
                    _iconImage.color = _shieldColorLibrary[5];

                }
                else if (_val <= .1f)
                {
                    _iconImage.color = _shieldColorLibrary[4];
                }
                else if (_val <= .25f)
                {
                    _iconImage.color = _shieldColorLibrary[3];
                }
                else if (_val <= .5f)
                {
                    _iconImage.color = _shieldColorLibrary[2];
                }
                else if (_val <= .75f)
                {
                    _iconImage.color = _shieldColorLibrary[1];
                }
                else
                {
                    _iconImage.color = _shieldColorLibrary[0];
                }


                // DOWN BECAUSE OF CARRIER OPERATIONS
                if (_s.shieldHullClass.shieldIntegrityCurrent != 0 && _s.DownBecauseOfCarrierOps)
                {
                    _iconImage.color = _shieldColorLibrary[6];
                }

                switch (_a)
                {
                    /* case ComponentActivity.MissingResource:
                        _iconImage.color = GameColors.Purple;
                        break; */
                    case ComponentActivity.Destroyed:
                        _iconImage.color = _shieldColorLibrary[7];
                        break;
                    case ComponentActivity.Disabled:
                        _iconImage.color = _shieldColorLibrary[6];
                        break;
                }
            }
            else if (_s._shieldIcon != null)
            {
                
                _iconImage = (Image)_graphic;

                _iconImage.sprite = _s._shieldIcon;
                _graphic = _iconImage;
            }

        }

        
    }
}

