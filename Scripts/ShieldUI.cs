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

        public void Update()
        {
            float _val = _s.shieldHullClass.shieldIntegrityCurrent / _s.shieldHullClass.statShieldIntegrityMax.Value;
            UpdateTooltipText("Integrity: " + Mathf.Round(_val*100).ToString() + "% (" + _s.shieldHullClass.shieldIntegrityCurrent + " HP / " + _s.shieldHullClass.statShieldIntegrityMax.Value + " HP)");
            

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

