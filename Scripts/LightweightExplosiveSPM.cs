using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Munitions;
using Munitions.InstancedDamagers;

namespace StarWarsShields
{
	


	[CreateAssetMenu(fileName = "New LW Explosive Shell with Shield Pen", menuName = "Nebulous/LW Shells/Explosive Shell")]
    public class LightweightExplosiveSPM : LightweightKineticShell // COPY CODE FROM SHELL CLASS YOU WISH TO EMULATE. MAKE SURE TO COPY OVER ALL OF THE CODE AND INHERIT FROM THE SAME CLASS
    {

		[Header("Explosive Shell")]
		[SerializeField]
		[Tooltip("With nothing in the way, the explosion will be placed between [(pendistance * envelope), pendistance]")]
		private float _penetrationEnvelope = 0.8f;

		[SerializeField]
		private float _explosionRadius = 1f;

		[SerializeField]
		private bool _explodeOnFirstUnDestroyed = false;


		protected override string GetDamageStatsText()
		{
			string str = string.Concat(base.GetDamageStatsText(), string.Format("Explosion Radius: {0}\n", this._explosionRadius * 10f));
			return str;
		}

		protected override IDamageDealer MakeDamageDealer(MunitionHitInfo hitInfo)
		{
			float single = _penetrationEnvelope;
			if ((object)hitInfo.ExitPoint != (object)null)
			{
				single = Mathf.Min(single, Vector3.Distance(hitInfo.Point, hitInfo.ExitPoint.Point) * 0.8f);
			}
			return new PenetratingExplosiveDamagerSPM(this, _explosionRadius, single, this._explodeOnFirstUnDestroyed); // EXCHANGE THE DAMAGER FOR THE NEW SPM ("SHIELD PEN MUNITION") DAMAGER. DO THIS FOR EVERY TYPE.
		}



	}
}
    
