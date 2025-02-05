using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using Networking;

namespace StarWarsShields
{
    public class ShieldNetworking : NetworkBehaviour
    {

        //private ShieldSW _shield = null;

        #region Singleton
        public static ShieldNetworking Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("SHIELD NETWORKING INITIALIZED (HK SHIELDS) !");
            }
        }
        #endregion

        private List<object[]> _shieldTable = new List<object[]>();

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        /* void Update()
        {
            if (transform.parent != null)
            {
                if (_shield != null)
                {
                    DoUpdateShieldHealth();
                }
                else if (_shield == null && transform.parent.GetComponent<ShieldSW>() != null)
                {
                    _shield = transform.parent.GetComponent<ShieldSW>();
                }
                else if (_shield == null && transform.GetComponent<ShieldSW>() != null)
                {
                    _shield = transform.GetComponent<ShieldSW>();
                }
                else
                {
                    Debug.Log("SHIELD NOT PRESENT");
                }
            }
            else
            {
                Debug.Log("HAS NO PARENT! (HK SHIELDS)");
            }
                
                
        } */

        /* [Server]
        public void DoUpdateShieldHealth(ShieldSW _shield)
        {
            RpcUpdateShieldHealth(_shield.shieldHullClass.shieldIntegrityCurrent, _shield);
        }

        [ClientRpc]
        void RpcUpdateShieldHealth(float _health, ShieldSW _shield)
        {
            _shield.shieldHullClass.shieldIntegrityCurrent = _health;
        } */

        [Server]
        public int DoRegisterShieldTable(string r1, string r2, float _h)
        {
            int index = -1;

            // CHECK IF ENTRY EXISTS ALREADY
            for (int i = 0; i < _shieldTable.Count; i++)
            {
                if ((string)_shieldTable[i][0] == r1 && (string)_shieldTable[i][1] == r2)
                {
                    return i; // RETURN REGISTER FOR FUTURE READ/WRITE
                }
            }

            // -> DOESN'T EXIST YET, SO CREATE NEW ENTRY
            index = _shieldTable.Count; // SINCE NEW ENTRY IS AT END OF LIST, RETURN LIST COUNT SIZE.

            object[] _a = {r1, r2, _h};

            _shieldTable.Add(_a);

            // TELL RPC TO ALSO ASSIGN NEW ENTRY
            RpcAddShieldTableEntry(_a);

            return index;
        }

        // FINDING REGISTER FOR NON-SERVER REGISTRATION
        public int ReturnRegister(string r1, string r2)
        {
            int index = -1; // -1 == NOT FOUND

            // CHECK IF ENTRY EXISTS 
            for (int i = 0; i < _shieldTable.Count; i++)
            {
                if ((string)_shieldTable[i][0] == r1 && (string)_shieldTable[i][1] == r2)
                {
                    return i; // RETURN REGISTER FOR FUTURE READ/WRITE
                }
            }

            return index;
        }

        [ClientRpc] // ADD NEW ENTRY TO Shield Table
        void RpcAddShieldTableEntry(object[] entry)
        {
            _shieldTable.Add(entry);
        }

        [Server]
        public void DoWriteUpdateShieldTable(int i, float _h)
        {
            // UPDATE VALUE
            _shieldTable[i][2] = _h;

            // START RPC
            RpcUpdateShieldTable(i, _h);
        }


        [ClientRpc]
        void RpcUpdateShieldTable(int i, float _h)
        {
            // UPDATE VALUE
            _shieldTable[i][2] = _h;
        }

        // RETRIEVE HEALTH VALUE
        public float healthValue(int index)
        {
            float _h = 0;

            // CHECK IF INDEX IS WITHIN BOUNDS
            if (index >= 0 && index < _shieldTable.Count)
            {
                return (float)_shieldTable[index][2];
            }

            return _h;

        }
    }
}
