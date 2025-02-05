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
        private int _register = -1; // -1 = NO REGISTER



        // FUNCTION THAT APPLIES DAMAGE TO THE SHIELD
        public void DoDamage()
        {


            if (_damager == null || _hitInfo == null /* || !isServer */)
            {

                return;
            }

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

                    if (!_fragileVFX)
                    {
                        GameObject _vfxParent = new GameObject();

                        _vfxParent.SetActive(true);

                        VisualEffect _vfx2 = _vfxParent.AddComponent<VisualEffect>();
                        _vfx2.visualEffectAsset = _fragileVFX.visualEffectAsset;

                        _vfxParent.transform.position = _hitInfo.Point;
                        transform.localScale = shieldHullClass.GetBoundingVolumeSize * (scaleFactor - 0.02f) * 2;
                        Vector3 _closestPoint = GetComponent<MeshCollider>().ClosestPoint(_hitInfo.Point);
                        transform.localScale = shieldHullClass.GetBoundingVolumeSize * scaleFactor * 2;
                        _vfxParent.transform.LookAt(_closestPoint * .99f);
                        _vfx2.transform.localScale = new Vector3(1, 1, 1);


                        _vfx2.Play();
                        Destroy(_vfxParent, _vfxRepoolDelay);
                    }
                }
            }

            shieldHullClass.shieldIntegrityCurrent = (shieldHullClass.shieldIntegrityCurrent > 0) ? (shieldHullClass.shieldIntegrityCurrent - _damage) : shieldHullClass.shieldIntegrityCurrent;

            shieldHullClass.shieldIntegrityCurrent = Mathf.Clamp(shieldHullClass.shieldIntegrityCurrent, 0, shieldHullClass.statShieldIntegrityMax.Value);
            tPassedSinceLastDamage = 0; // Reset Time Counter, stop Recharge
            if (GetComponent<MeshCollider>().enabled && shieldHullClass.shieldIntegrityCurrent <= 0)
            {
                GetComponent<MeshCollider>().enabled = false;

            }

            // RUNS SHIELD VFX
            if (_vfx != null /* && _pooledObjects.Count != 0 */)
            {
                GameObject _vfxParent = new GameObject();

                /* if (_vfxParent == null)
                {
                    return;
                } */

                _vfxParent.SetActive(true);

                VisualEffect _vfx2 = _vfxParent.AddComponent<VisualEffect>();
                _vfx2.visualEffectAsset = _vfx.visualEffectAsset;

                _vfxParent.transform.position = _hitInfo.Point;
                transform.localScale = shieldHullClass.GetBoundingVolumeSize * (scaleFactor - 0.02f) * 2;
                Vector3 _closestPoint = GetComponent<MeshCollider>().ClosestPoint(_hitInfo.Point);
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


        // THE MAIN GOAL WITH THE SYNCING IS JUST TO SYNC _cSH
        // SO THAT THE SERVER/HOST INSTANCE CAN WRITE ON IT AND EVERYTHING ELSE CAN READ OFF OF IT FOR THEIR SHIELD VALUES,
        // BECAUSE ATM ONLY THE HOST CAN SEE THE CORRECT SHIELD HEALTH VALUES.
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
                    _register = ShieldNetworking.Instance.DoRegisterShieldTable(shieldHullClass.Socket.MyHull.MyShip.netId.ToString(), shieldHullClass.Socket.Key, shieldHullClass.shieldIntegrityCurrent);
                    if (_register == -1)
                    {
                        _register = ShieldNetworking.Instance.ReturnRegister(shieldHullClass.Socket.MyHull.MyShip.netId.ToString(), shieldHullClass.Socket.Key);
                    }
                }

                // UPDATE / READ IF REGISTERED
                if (_register >= 0 && ShieldNetworking.Instance != null)
                {
                    // UPDATE VALUE
                    ShieldNetworking.Instance.DoWriteUpdateShieldTable(_register, shieldHullClass.shieldIntegrityCurrent);

                    // READ VALUE
                    shieldHullClass.shieldIntegrityCurrent = ShieldNetworking.Instance.healthValue(_register);
                }

                /* if (ShieldNetworking.Instance != null)
                {
                    ShieldNetworking.Instance.DoUpdateShieldHealth(this);
                }
                else
                {
                    Debug.Log("SHIELD NETWORKING NOT FOUND (HK SHIELDS) !");
                } */
            }


            /* if (isServer)
            {
                _cSH = shieldHullClass.shieldIntegrityCurrent;

            }
            else
            {
                shieldHullClass.shieldIntegrityCurrent = _cSH;

                Debug.Log(_cSH);
            }*/

            
            

        }

        



    }



}
