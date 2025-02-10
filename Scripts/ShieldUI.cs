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

        RateLimitedLogger dA = new RateLimitedLogger(1);


        // INDEX SHIELD NETWORKING WITH REGISTER
        private int _register = -1;

        public void FixedUpdate()
        {
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

            dA.LogLimited("(HK SHIELDS - " + ((ShieldNetworking.Instance.isServer) ? "HOST" : "CLIENT") + ") CURRENT UI REGISTER : " + _register + " - CURRENT SHIELD HEALTH VALUE: " + ShieldNetworking.Instance.healthValue(_register) + " HP");

            UpdateTooltipText("Integrity: " + Mathf.Round(_val*100).ToString() + "% (" + ShieldNetworking.Instance.healthValue(_register) + " HP / " + _s.shieldHullClass.statShieldIntegrityMax.Value + " HP)" + ((_tActivity != "") ? "\n" + _tActivity : ""));

            if (_register == -1 && _s._register != -1)
            {
                _register = _s._register;
            }

            if (_iconImage != null)
            {

                if (_s.shieldHullClass.shieldIntegrityCurrent == 0)
                {
                    _iconImage.color = new Color(150, 0, 0);
                }
                else if (_val <= .1f)
                {
                    _iconImage.color = new Color(180, 0, 0);
                }
                else if (_val <= .25f)
                {
                    _iconImage.color = GameColors.Red;
                }
                else if (_val <= .5f)
                {
                    _iconImage.color = GameColors.Orange;
                }
                else if (_val <= .75f)
                {
                    _iconImage.color = GameColors.Yellow;
                }
                else
                {
                    _iconImage.color = GameColors.Green;
                }

                switch (_a)
                {
                    /* case ComponentActivity.MissingResource:
                        _iconImage.color = GameColors.Purple;
                        break; */
                    case ComponentActivity.Destroyed:
                        _iconImage.color = new Color(50,50,50);
                        break;
                    case ComponentActivity.Disabled:
                        _iconImage.color = new Color(75, 0, 0);
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

