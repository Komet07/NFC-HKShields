using Game;
using Munitions.InstancedDamagers;
using Sound;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Utility;
using Munitions;

using System.Linq;
using System.Reflection;
using Game.Units;

namespace StarWarsShields
{
    public class IonCannonShell : LightweightExplosiveShell
    {
        [Header("Ion Cannon stats")]
        [Min(0f), Tooltip("Multiplier for component damage dealt to shield"), SerializeField]
        protected float _shieldDamageMultiplier = 1f;

        public override bool DamageableImpact(LightweightKineticMunitionContainer attachedTo, IDamageable hitObject, MunitionHitInfo hitInfo, bool trigger, out HitResult hitResult, out float damageDone, out bool targetDestroyed, out Vector3 repoolPosition)
		{ 
			foreach (ShieldHull _s1 in Enumerable.OfType<ShieldHull>(hitObject.GameObj.GetComponent<ShipController>().Ship.Hull.AllComponents))
			{
				ShieldSW _s = _s1.gameObject.GetComponent<ShieldSW>();
				if (_s.active)
				{


					_s._isIon = true;
					_s._ionMulti = _shieldDamageMultiplier;

					continue;
				}


			}

			this._attachedToAtImpact = attachedTo;
			repoolPosition = hitInfo.Point;
			hitResult = hitObject.DoDamage(hitInfo, this.MakeDamageDealer(hitInfo), out damageDone, out targetDestroyed);

			attachedTo.DoImpactEffect(hitInfo, hitResult);
			this._attachedToAtImpact = null;
			return hitResult == HitResult.Penetrated || hitResult == HitResult.Stopped || hitResult == HitResult.Ricochet;
		}
	}
}
    
