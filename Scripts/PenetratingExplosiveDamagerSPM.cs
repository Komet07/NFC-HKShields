using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Munitions.InstancedDamagers;
using Munitions;

namespace StarWarsShields
{
    public class PenetratingExplosiveDamagerSPM : PenetratingExplosiveDamager, IShieldPenMunition
    {
        public PenetratingExplosiveDamagerSPM(IDamageCharacteristic character, float radius, float depthEnvelope, bool explodeOnFirstHit) : base(character, 0, 0, false)
        {
            this._radius = radius;
            this._depthEnvelope = depthEnvelope;
            this._explodeOnFirst = explodeOnFirstHit;
        } // REPLICATE CLASS DECLARATION FUNCTION FROM INHERITED CLASS.
    }
}
    
