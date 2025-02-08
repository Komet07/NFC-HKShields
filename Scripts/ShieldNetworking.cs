using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using Networking;
using Ships;

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
        private List<object[]> _shieldVFXQueue = new List<object[]>();

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

        // <-- SHIELD REGISTRATION FUNCTIONS -->
        [Server]
        public int DoRegisterShieldTable(string r1, string r2, float _h, ComponentActivity _activity)
        {
            int index = -1;

            Debug.Log("HT - THIS SHOULD BE THE HOST (HK SHIELDS)");

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

            object[] _a = {r1, r2, _h, _activity};

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

        // <-- SHIELD HEALTH UPDATE FUNCTIONS -->
        [Server]
        public void DoWriteUpdateShieldTable(int i, float _h, ComponentActivity _a)
        {

            if (!isServer) // FAILSAFE FOR IF TRIGGERED AS CLIENT!
            {
                return;
            }

            // UPDATE VALUE
            _shieldTable[i][2] = _h;
            _shieldTable[i][3] = _a;

            // START RPC
            RpcUpdateShieldTable(i, _h, _a);
        }


        [ClientRpc]
        void RpcUpdateShieldTable(int i, float _h, ComponentActivity _a)
        {
            // UPDATE VALUE
            _shieldTable[i][2] = _h;
            _shieldTable[i][3] = _a;
        }

        // RETRIEVE HEALTH VALUE
        public float healthValue(int index)
        {
            float _h = 0;

            // CHECK IF INDEX IS WITHIN BOUNDS
            if (index >= 0 && index < _shieldTable.Count)
            {
                return (float)_shieldTable[index][2]; // RETRIEVE VALUE
            }

            return _h;

        }

        // RETRIEVE ACTIVITY VALUE
        public ComponentActivity activityValue(int index)
        {
            ComponentActivity _a = ComponentActivity.Active;

            // CHECK IF INDEX IS WITHIN BOUNDS
            if (index >= 0 && index < _shieldTable.Count)
            {
                return (ComponentActivity)_shieldTable[index][3]; // RETRIEVE VALUE
            }

            return _a;
        }

        // <-- SHIELD VFX POOL FUNCTIONS -->
        [Server]
        public void DoAddVFX(int index, int type, Vector3 _pos)
        {
            // COMPILE ENTRY
            object[] entry = { index, type, _pos};

            RpcAddVFXQueueEntry(entry); // PASS ENTRY ALONG TO RPC
        }

        [ClientRpc]
        void RpcAddVFXQueueEntry(object[] entry)
        {
            if (isServer) // DO NOT ADD TO QUEUE IF ALREADY HOST - VFX HAS ALREADY PLAYED THERE
            {
                return;
            }
            _shieldVFXQueue.Add(entry);
        }


        // RETRIEVE ALL VFX COMMANDS IN QUEUE FOR THIS SHIELD
        public List<object[]> RetrievePool(int register)
        {
            List<object[]> _entries = new List<object[]>();

            // RETRIEVE COMMANDS
            for (int i = 0; i < _shieldVFXQueue.Count; i++)
            {
                if ((int)_shieldVFXQueue[i][0] != register)
                {
                    continue; // MOVE ON TO NEXT ITEM IF INDEX DOESN'T MATCH
                }


                object[] _entry = { _shieldVFXQueue[i][1], _shieldVFXQueue[i][2] }; // COMPILE ENTRY OUT OF ENTRIES -> REMOVE UNWANTED INDEX

                // ADD TO ENTRIES
                _entries.Add(_entry);

                // DELETE UNNEEDED ENTRY FROM _shieldVFXQueue, LOWER INDEX BY 1 TO NOT SKIP OVER OTHER ELEMENTS
                _shieldVFXQueue.Remove(_shieldVFXQueue[i]);
                i--;

            }

            // RETURN COMPILED LIST
            return _entries;
        }
    }
}
