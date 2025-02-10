using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using Mirror;


using System.Collections.Generic;
using System.Collections;
using Munitions;
using Ships;
using UnityEngine.Networking;

using Game.Units;
using Utility;

using Game;


using Ships.Serialization;



namespace StarWarsShields
{
    // Can only have one shield per component
    [DisallowMultipleComponent]
    // Requires ShieldHull
    [RequireComponent(typeof(ShieldHull))]

    // Requires MeshCollider
    [RequireComponent(typeof(MeshCollider))]


    public class ShieldSW : MonoBehaviour
    {
        [Header("Collider stats")]
        [SerializeField, Min(0.025f)]
        protected float scaleFactor = 1.25f; // Scale factor from bounding box

        protected float xScale = 1; // Scale on the x-Axis

        protected float yScale = 1; // Scale on the y-Axis

        protected float zScale = 1; // Scale on the z-Axis

        [Header("Collider Options")]
        [SerializeField, Tooltip("Will scale the Collider by a uniform amount (Max value on the Boundary Collider) instead of stretching it to match the Boundary Collider's values")]
        protected bool scaleUniformly = false;

        [Header("Shield Sprites & VFX")]
        [Tooltip("Icon that'll be visible on the Status GUI.")]
        public Sprite _shieldIcon;

        public VisualEffect _vfx;

        [Tooltip("VFX for when Shield gets broken, Only required if 'fragile' is enabled in the ShieldHull Component.")]
        public VisualEffect _fragileVFX;

        [Min(0.1f)]
        public float _vfxRepoolDelay = 1f;

        [Header("Runtime stats")]
        [SerializeField]
        protected bool battleshort = false; // Check whether Battleshort is on or not

        public bool active = true; // Check whether component is active

        public ShieldHull shieldHullClass;

        public float tPassedSinceLastDamage = 0;

        RateLimitedLogger dA = new RateLimitedLogger(1);

        [NonSerialized]
        public MunitionHitInfo _hitInfo;

        [NonSerialized]
        public IDamageDealer _damager;

        /* [SyncVar(hook = nameof(InitHook))] */
        protected bool _hasInitialized = false;

        [Header("Ion Weapon Stats")]
        [NonSerialized]
        public bool _isIon = false;
        [NonSerialized]
        public float _ionMulti = 1f;

        // <- NETWORKING REGISTRATION ->
        [HideInInspector]
        public int _register = -1; // -1 = NO REGISTER



        // FUNCTION THAT APPLIES DAMAGE TO THE SHIELD
        public void DoDamage()
        {

            // STOP FUNCTION IF IMPORTANT DATA IS MISSING
            if (_damager == null || _hitInfo == null /* || !isServer */)
            {

                return;
            }

            

            // CALCULATE DAMAGE
            float _damage = _damager.ComponentDamage * (1f - shieldHullClass.statShieldResistance.Value);
            // Debug.Log(_isIon ? "IS ION MUNITION" : "ISN'T ION MUNITION");
            _damage *= (_isIon) ? _ionMulti : 1f;
            _damage = (_damager.ComponentDamage > shieldHullClass.statShieldThreshold.Value) ? _damage : 0;
            _damage = (shieldHullClass.subtractDT == true && _damage > 0) ? _damage - shieldHullClass.statShieldThreshold.Value : _damage;

            _isIon = false;

            if (shieldHullClass.fragile && _damage > shieldHullClass.statFragilityDamageThreshold.Value)
            {
                float _a = UnityEngine.Random.Range(0f, 1f);

                if (_a < shieldHullClass.statBaseFragilityChance.Value)
                {
                    _damage = 0;
                    shieldHullClass.shieldIntegrityCurrent = 0;

                    PlayVFX(1, _hitInfo.Point); // PLAY VFX NATIVELY

                    ShieldNetworking.Instance.DoAddVFX(_register, 1, _hitInfo.Point); // ADD VFX TO QUEUE FOR ALL CLIENTS TO PLAY.
                }
            }

            // APPLY DAMAGE
            shieldHullClass.shieldIntegrityCurrent = (shieldHullClass.shieldIntegrityCurrent > 0) ? (shieldHullClass.shieldIntegrityCurrent - _damage) : shieldHullClass.shieldIntegrityCurrent;

            shieldHullClass.shieldIntegrityCurrent = Mathf.Clamp(shieldHullClass.shieldIntegrityCurrent, 0, shieldHullClass.statShieldIntegrityMax.Value);
            tPassedSinceLastDamage = 0; // Reset Time Counter, stop Recharge


            // DISABLE COLLIDER IF HEALTH IS 0
            if (GetComponent<MeshCollider>().enabled && shieldHullClass.shieldIntegrityCurrent <= 0)
            {
                GetComponent<MeshCollider>().enabled = false;

            }

            // RUNS SHIELD VFX
            if (_vfx != null /* && _pooledObjects.Count != 0 */)
            {
                PlayVFX(0, _hitInfo.Point); // PLAY VFX NATIVELY

                ShieldNetworking.Instance.DoAddVFX(_register, 0, _hitInfo.Point); // ADD VFX TO QUEUE FOR ALL CLIENTS TO PLAY.
            }
        }

        // SHELL FOR RUNNING VFX
        public void PlayVFX(int type, Vector3 _pos)
        {
            if (type == 0) // RUN BASE HIT VFX
            {

                if (_vfx == null)
                {
                    return; // CAN'T PLAY VFX IF VFX ISN'T THERE
                }

                // SETUP VFX OBJECT
                GameObject _vfxParent = new GameObject();

                _vfxParent.SetActive(true);

                // GRAB VFX ASSET
                VisualEffect _vfx2 = _vfxParent.AddComponent<VisualEffect>();
                _vfx2.visualEffectAsset = _vfx.visualEffectAsset;

                _vfxParent.transform.position = _pos;


                // DETERMINE VIEW DIRECTION THROUGH SHENANIGANS
                transform.localScale = shieldHullClass.GetBoundingVolumeSize * (scaleFactor - 0.02f) * 2;
                Vector3 _closestPoint = GetComponent<MeshCollider>().ClosestPoint(_pos);
                transform.localScale = shieldHullClass.GetBoundingVolumeSize * scaleFactor * 2;
                _vfxParent.transform.LookAt(_closestPoint * .99f);
                _vfx2.transform.localScale = new Vector3(1, 1, 1);

                // PLAY VFX
                _vfx2.Play();

                // REMOVE VFX AFTER PLAYING
                Destroy(_vfxParent, _vfxRepoolDelay);
            }
            else if (type == 1) // FRAGILE EVENT VFX
            {
                if (_fragileVFX == null)
                {
                    return; // CAN'T PLAY VFX, SO RETURN
                }

                GameObject _vfxParent = new GameObject();

                _vfxParent.SetActive(true);

                VisualEffect _vfx2 = _vfxParent.AddComponent<VisualEffect>();
                _vfx2.visualEffectAsset = _fragileVFX.visualEffectAsset;

                _vfxParent.transform.position = _pos;
                transform.localScale = shieldHullClass.GetBoundingVolumeSize * (scaleFactor - 0.02f) * 2;
                Vector3 _closestPoint = GetComponent<MeshCollider>().ClosestPoint(_pos);
                transform.localScale = shieldHullClass.GetBoundingVolumeSize * scaleFactor * 2;
                _vfxParent.transform.LookAt(_closestPoint * .99f);
                _vfx2.transform.localScale = new Vector3(1, 1, 1);


                _vfx2.Play();
                Destroy(_vfxParent, _vfxRepoolDelay);
            }
        }



        // DOES NOTHING ATM
        private void Awake()
        {
            /* if (_vfx != null)
            {
                _pooledObjects = new List<GameObject>();
                GameObject obj;
                for (int i = 0; i < _poolAmount; i++)
                {
                    obj = new GameObject();
                    obj.SetActive(false);

                    VisualEffect objVFX = obj.AddComponent<VisualEffect>();
                    objVFX.visualEffectAsset = _vfx.visualEffectAsset;

                    _pooledObjects.Add(obj);
                }
            } */

        }

        /* private GameObject GetPooledObject()
        {
            for (int i = 0; i < _poolAmount && i < _pooledObjects.Count; i++)
            {
                if (!_pooledObjects[i].activeInHierarchy)
                {
                    return _pooledObjects[i];
                }
            }

            GameObject obj = new GameObject();
            obj.SetActive(false);

            VisualEffect objVFX = obj.AddComponent<VisualEffect>();
            objVFX.visualEffectAsset = _vfx.visualEffectAsset;

            _pooledObjects.Add(obj);
            _poolAmount++;

            return obj;
        } */

        static ShieldSW()
        {

        }

        /* [Command]
        void CmdInitializeShields()
        {
            if (_cSH != shieldHullClass.shieldIntegrityCurrent)
            {
                _cSH = shieldHullClass.shieldIntegrityCurrent;
            }
        } */

        /* [ClientRpc]
        void RpcSendMessage()
        {
            Debug.Log("B");
        } */

        void Update()
        {
            


            /* if (GetComponent<NetworkIdentity>() == null)
            {
                transform.gameObject.AddComponent<NetworkIdentity>();
            } */

            if (shieldHullClass == null)
            {
                shieldHullClass = gameObject.GetComponent<ShieldHull>();
            }


            if (!_hasInitialized && shieldHullClass.statShieldIntegrityMax != null)
            {
                shieldHullClass.shieldIntegrityCurrent = shieldHullClass.statShieldIntegrityMax.Value;
                /* _cSH = shieldHullClass.statShieldIntegrityMax.Value; */
                _hasInitialized = true;
            }
            else if (_hasInitialized)
            {
                _hasInitialized = true;

                MeshCollider _collider = gameObject.GetComponent<MeshCollider>();

                // battleshort = shieldHullClass.GetBattleshort;
                active = shieldHullClass.GetActivity;

                transform.position = shieldHullClass.GetBoundingVolumePosition;

                // Get Scales
                xScale = shieldHullClass.GetBoundingRadius * scaleFactor;
                yScale = xScale;
                zScale = xScale;

                // Shield Recharge Code
                if (active)
                {
                    tPassedSinceLastDamage += (active) ? Time.deltaTime : 0;
                    shieldHullClass.shieldIntegrityCurrent += ((tPassedSinceLastDamage > shieldHullClass.statRechargeDelay.Value && active) || shieldHullClass.statRechargeDelay.Value <= 0f) ? shieldHullClass.statRechargeRate.Value * Time.deltaTime : 0;
                    shieldHullClass.shieldIntegrityCurrent = Mathf.Clamp(shieldHullClass.shieldIntegrityCurrent, 0, shieldHullClass.statShieldIntegrityMax.Value);
                    shieldHullClass.shieldIntegrityCurrent = Mathf.Round(shieldHullClass.shieldIntegrityCurrent * 20) / 20;
                }


                bool _active = shieldHullClass.shieldIntegrityCurrent > 0 ? true : false;


                // Disable Shield Scaler & Collider if not active & vice versa
                if (_collider != null && _collider.enabled && !_active)
                {
                    _collider.enabled = false;

                }
                else if (_collider != null && !_collider.enabled && _active && active)
                {
                    _collider.enabled = true;

                }
                else if (_collider != null && !active)
                {
                    _collider.enabled = false;

                }

                // Scale Scaler
                transform.localScale = active ? (!scaleUniformly) ? shieldHullClass.GetBoundingVolumeSize * scaleFactor * 2 : shieldHullClass.GetMaxBoundingVolumeSize * scaleFactor * 2 : new Vector3(1, 1, 1);

                

                // CHECK IF REGISTERED YET, IF NOT AND INSTANCE EXISTS, REGISTER
                if (_register == -1 && ShieldNetworking.Instance != null)
                {
                    // Debug.Log("REGISTER PROCESS A - (HK SHIELDS) ");
                    _register = ShieldNetworking.Instance.DoRegisterShieldTable(shieldHullClass.Socket.MyHull.MyShip.netId.ToString(), shieldHullClass.Socket.Key, shieldHullClass.shieldIntegrityCurrent);
                    // ShieldNetworking.Instance.DumpTable();
                    if (_register == -1)
                    {
                        _register = ShieldNetworking.Instance.ReturnRegister(shieldHullClass.Socket.MyHull.MyShip.netId.ToString(), shieldHullClass.Socket.Key);
                        // Debug.Log("REGISTER PROCESS B - (HK SHIELDS) ");
                    }

                    // Debug.Log("REGISTER VALUE: (HK SHIELDS) " + _register);

                }

                // UPDATE / READ IF REGISTERED
                if (_register >= 0 && ShieldNetworking.Instance != null)
                {
                    // <- SHIELD HEALTH NETWORKING ->
                    // UPDATE VALUE
                    ShieldNetworking.Instance.DoWriteUpdateShieldTable(_register, shieldHullClass.shieldIntegrityCurrent);

                    // READ VALUE
                    if (ShieldNetworking.Instance.healthValue(_register) >= 0)
                    {
                        shieldHullClass.shieldIntegrityCurrent = ShieldNetworking.Instance.healthValue(_register);
                        // dA.LogLimited("CURRENT HEALTH VALUE: (HK SHIELDS) " + shieldHullClass.Socket.MyHull.MyShip.ShipDisplayName + " - " + ShieldNetworking.Instance.healthValue(_register));
                    }

                    // <- SHIELD VFX NETWORKING ->
                    // RETRIEVE ITEMS IN POOL AND PLAY
                    foreach (object[] _entry in ShieldNetworking.Instance.RetrievePool(_register))
                    {
                        PlayVFX((int)_entry[0], (Vector3)_entry[1]);
                    }

                }
                else
                {
                    // dA.LogLimited("SHIELD NOT YET REGISTERED (HK SHIELDS) !");
                }
            }



            
            

        }

        



    }



}
