using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using Ships.Controls;
using Ships.Serialization;

using Ships;

using System.Linq;

using Utility;

using Networking;
using Mirror;

namespace StarWarsShields
{
    [System.Serializable]
    public class ShieldHull : HullComponent
    {

        [Header("Shield Stats")]
        [SerializeField]
        [Tooltip("This is the Integrity of your shield. Value = Amount of Component Damage it can take before going down")]
        protected float shieldIntegrityMax; // Max Shield Integrity

        [NonSerialized, ShipStat("shield-maxInteg", "Shield Integrity", "HP", InitializeFrom = "shieldIntegrityMax", MinValue = 0f)]
        public StatValue statShieldIntegrityMax; // Stat form for Maximum Shield Integrity

        [Tooltip("Whether or not Hull Damage resets shield Recharge")]
        public bool _hullDamageResetsShieldRecharge = false;

        [NonSerialized]
        public float shieldIntegrityCurrent; // Current Shield Integrity

        [SerializeField]
        [Tooltip("OPTIONAL: How much percent of incoming damage is removed, decimal value (0 - 1)")]
        protected float shieldResistance; // Shield Resistance

        [SerializeField]
        [Tooltip("OPTIONAL: Minimum Component Damage needed to affect shield")]
        protected float shieldThreshold; // Shield Damage Threshold

        [SerializeField]
        [Tooltip("OPTIONAL: This is how much shield Integrity is recharged per second")]
        protected float rechargeRate; // Recharge Rate per second

        [SerializeField]
        [Tooltip("OPTIONAL: This is how long you have to wait until shield begins to recharge")]
        protected float rechargeDelay; // Recharge delay

        [Tooltip("Whether or not the Damage Threshold is subtracted from the damage, if damage is higher than the damage Threshold")]
        public bool subtractDT = false;

        [Header("Shield Fragility Stats")]
        [SerializeField, Tooltip("(OPTIONAL) Enabling this will allow the shield to randomly fail by a set chance (default: 0) when a shot hits it")]
        public bool fragile = false;

        [SerializeField, Tooltip("Base chance for the shield to fail, Value between 0 and 1, only does something if 'fragile' is enabled."), Range(0f, 1f)]
        protected float _baseFragileChance = 0;

        [SerializeField, Tooltip("(OPTIONAL) Damage Threshold for the shield to roll the fail chance, in HP")]
        protected float _fragileDamageThreshold = 0;

        [NonSerialized]
        [ShipStat("shield-rechargeRate", "Shield Recharge Rate", "HP/s", InitializeFrom = "rechargeRate", MinValue = 0f)]
        public StatValue statRechargeRate; // Recharge rate per Second

        [NonSerialized]
        [ShipStat("shield-rechargeDelay", "Shield Recharge Delay", "s", InitializeFrom = "rechargeDelay", PositiveBad = true, MinValue = 0f)]
        public StatValue statRechargeDelay; // Recharge Delay, in Seconds

        [NonSerialized]
        [ShipStat("shield-resistance", "Shield Resistance", "%", InitializeFrom = "shieldResistance", MinValue = 0f, MaxValue = 1f, TextValueMultiplier = 100f)]
        public StatValue statShieldResistance; // Shield Resistance, in Percent

        [NonSerialized]
        [ShipStat("shield-threshold", "Shield Damage Threshold", "HP", InitializeFrom = "shieldThreshold", MinValue = 0f)]
        public StatValue statShieldThreshold; // Shield Damage Threshold, in HP

        [NonSerialized, ShipStat("shield-fragilityBase", "Base Shield Fragility Chance", "%", InitializeFrom = "_baseFragileChance", MinValue = 0f, MaxValue = 1f, TextValueMultiplier = 100f)]
        public StatValue statBaseFragilityChance; // Shield Fragility Base Chance, in Percent

        [NonSerialized, ShipStat("shield-FragilityThreshold", "Shield Fragility Damage Threshold", "HP", InitializeFrom = "_fragileDamageThreshold", MinValue = 0f)]
        public StatValue statFragilityDamageThreshold;

        //[SerializeField, Header("Shield Initialization")]
        // public ShieldNetworking _shieldNetworkingPrefab;


        /* public bool GetBattleshort // Call _battleShortEnabled function to figure out whether ship is using Battleshort
        {
            get
            {
                bool _a = _battleShortEnabled;
                return _a;
            }
        } */

        protected override void Start()
        {
            base.Start();
            if (this._baseRpcProvider.IsHost && NetworkServer.active && ShieldNetworking.Instance == null)
            {
                var shield = Instantiate(ModUtil._shieldNetworkingPrefab);

                if (!_baseRpcProvider.IsHost)
                {
                    return;
                }

                NetworkServer.Spawn(shield.gameObject);
                
                

                Debug.Log("SHIELD SUCCESSFULLY SPAWNED - KOMET");
            }


        }

        public bool GetActivity
        {
            get
            {
                ComponentActivity _a = GetActivityStatus();
                return (_a == ComponentActivity.Active || _a == ComponentActivity.Idle) ? true : false;
            }
        }

        public float GetBoundingRadius
        {
            get
            {
                HullSocket _a = base.Socket;
                BaseHull _b = _a.MyHull;

                float _f = _b.BoundingVolumeRadius;
                return _f;
            }
        }

        public BoxCollider BoundingCollder
        {
            get
            {
                HullSocket _b = base.Socket;
                BaseHull _c = _b.MyHull;

                return _c.BoundingVolume;
            }
        }

        public Vector3 GetBoundingVolumePosition
        {
            get
            {
                

                HullSocket _b = base.Socket;
                BaseHull _c = _b.MyHull;

                BoxCollider _d = _c.BoundingVolume;

                return _d.transform.position;
            }
        }

        public Vector3 GetBoundingVolumeSize
        {
            get
            {
                HullSocket _b = base.Socket;
                BaseHull _c = _b.MyHull;

                BoxCollider _d = _c.BoundingVolume;

                return _d.size;
            }
        }

        public Vector3 GetMaxBoundingVolumeSize
        {
            get
            {
                HullSocket _b = base.Socket;
                BaseHull _c = _b.MyHull;

                BoxCollider _d = _c.BoundingVolume;

                float _e = Mathf.Max(_d.size.x, _d.size.y, _d.size.z);

                return new Vector3(_e, _e, _e);
            }
        }

        public override void GetFormattedStats(List<(string, string)> rows, bool full, int groupSize = 1)
        {
            base.GetFormattedStats(rows, full, groupSize);

            rows.Add(statShieldIntegrityMax.FullTextWithLinkRow);
            rows.Add(statShieldThreshold.FullTextWithLinkRow);
            rows.Add(statShieldResistance.FullTextWithLinkRow);
            rows.Add(statRechargeRate.FullTextWithLinkRow);
            rows.Add(statRechargeDelay.FullTextWithLinkRow);

            if (fragile)
            {
                rows.Add(statBaseFragilityChance.FullTextWithLinkRow);
                rows.Add(statFragilityDamageThreshold.FullTextWithLinkRow);
                
            }
        }

        public override void GetFormattedStats(List<(string, string)> rows, bool full, IEnumerable<IHullComponent> group)
        {
            
            GetFormattedStats(rows, full, Enumerable.Count<IHullComponent>(group));
        }

    }
}
    
