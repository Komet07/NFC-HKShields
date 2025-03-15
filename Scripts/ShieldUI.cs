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
        public ShieldSW[] _s;

        ModUtil _m = new ModUtil();

        RateLimitedLogger dA = new RateLimitedLogger(1);

        public Color[] _shieldColorLibrary = null;


        // INDEX SHIELD NETWORKING WITH REGISTER
        public int[] _register = {};

        float ReturnShieldHealthMax(int i) {
            return ShieldNetworking.Instance.healthValue(_register[i]);
        }

        float ReturnShieldHealthCurrent(ShieldSW s) {
            return s.shieldHullClass.statShieldIntegrityMax.Value;
        }

        bool AllRegistered() {
            for (int i = 0; i < _s.Length; i++) {
                if (_register.Length <= i || _register[i] == -1) {
                    return false;
                }
            }
            return true;
        }

        bool AllDestroyed() {
            for (int i = 0; i < _s.Length; i++) {
                if (_s[i].shieldHullClass.GetActivityStatus() != ComponentActivity.Destroyed) {
                    return false;
                }
            }
            return true;
        }

        bool AllDisabled() {
            for (int i = 0; i < _s.Length; i++) {
                if (_s[i].shieldHullClass.GetActivityStatus() != ComponentActivity.Disabled) {
                    return false;
                }
            }
            return true;
        }

        string ReturnShieldText(int i, float _mH, float _cH) {
            string _t = "";

            if (_s.Length > 1) {
                _t = "(Shield " + i + ")";
            }

            ComponentActivity _a = ComponentActivity.Active;
            string _tActivity = "";

            if (_register[i] != -1 && ShieldNetworking.Instance != null)
            {
                _a = _s[i].shieldHullClass.GetActivityStatus();
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

            if (_cH == 0 && _tActivity == "")
            {
                _tActivity = "<color=red>NO HEALTH</color>";

            }

            _t += ("Integrity: " + Mathf.Round(_cH / _mH) * 100).ToString() + "% (" + _cH + " HP / " + _s[i].shieldHullClass.statShieldIntegrityMax.Value + " HP)" + ((_tActivity != "") ? "(" + _tActivity + ")" : "");

            return _t;
        }

        public void FixedUpdate()
        {
            

            if (_s == null || _s.Length == 0 || _s[0] == null) // Safeguards so this doesn't run while not all shields are registered
            {
                return;
            }

            if (!AllRegistered()) {
                for (int i = 0; i < _s.Length; i++) {
                    if (_register[i] == -1 && _s[i]._register != -1)
                    {
                        _register[i] = _s[i]._register;
                    }
                }
                return;
            }


            if (_shieldColorLibrary == null)
            {
                _shieldColorLibrary = new Color[] { _s[0].shieldHullClass.ColorNominal, _s[0].shieldHullClass.ColorLightDamage, _s[0].shieldHullClass.ColorModerateDamage, _s[0].shieldHullClass.ColorHeavyDamage, _s[0].shieldHullClass.ColorVeryHeavyDamage, _s[0].shieldHullClass.ColorNoHealth, _s[0].shieldHullClass.ColorDisabled, _s[0].shieldHullClass.ColorDestroyed }; // Colored Library works off of the first shield's color palette
                
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

            float[] mHealth = new float[_s.Length];
            float[] cHealth = new float[_s.Length];

            float _totalMHealth = 0;
            float _totalCHealth = 0;

            for (int i = 0; i < _s.Length; i++) {
                mHealth[i] = ReturnShieldHealthMax(i);
                cHealth[i] = ReturnShieldHealthCurrent(_s[i]);

                _totalMHealth += mHealth[i];
                _totalCHealth += cHealth[i];
            }



            float _val = _totalMHealth / _totalCHealth;

            // Write Tooltip Text
            string _tooltip = "";
            if (_s.Length > 1) {
                _tooltip += "Total Integrity: "+ Mathf.Round(_val*100).ToString() + "% (" + _totalCHealth + " HP / " + _totalMHealth + " HP)";
            }

            for (int i = 0; i < _s.Length; i++) {
                _tooltip += (_s.Length != 1) ? "\n" : "";
                _tooltip += ReturnShieldText(i, mHealth[i], cHealth[i]);
            }


            

            

            //dA.LogLimited("(HK SHIELDS - " + ((ShieldNetworking.Instance.isServer) ? "HOST" : "CLIENT") + ") CURRENT UI REGISTER : " + _register + " - CURRENT SHIELD HEALTH VALUE: " + ShieldNetworking.Instance.healthValue(_register) + " HP");

            UpdateTooltipText(_tooltip);

            
                
                

            if (_iconImage != null)
            {

                if (_totalCHealth == 0)
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


                if (AllDestroyed())
                {
                    _iconImage.color = _shieldColorLibrary[7];
                }
                else if (AllDisabled())
                {
                    _iconImage.color = _shieldColorLibrary[6];
                }
            }
            else if (_s[0]._shieldIcon != null)
            {
                
                _iconImage = (Image)_graphic;

                _iconImage.sprite = _s[0]._shieldIcon;
                _graphic = _iconImage;
            }

        }

        
    }
}

