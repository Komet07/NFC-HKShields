

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Bundles;
using Modding;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Utility;

using HarmonyLib;
using Networking;
using Mirror;

namespace StarWarsShields
{
    public class StarWarsShields 
    {
       
    }

    public class ModUtil : IModEntryPoint
    {

        public static ShieldNetworking _shieldNetworkingPrefab;

        public void PostLoad()
        {

            (new Harmony("nebulous.StarWarsShields")).PatchAll();


            /* IEnumerable<AssetBundle> assetBundles = AssetBundle.GetAllLoadedAssetBundles();

            foreach (AssetBundle bundle in assetBundles)
            {
                Debug.Log("CHECKED ASSET BUNDLE : " + bundle.name);
               

                if (bundle.name == "d84cba018ffea57bc22d7fd1bb8f576a.bundle" || bundle.name == "hkshields")
                {
                    string[] n = bundle.GetAllAssetNames();

                    foreach (string n1 in n)
                    {
                        Debug.Log("ASSET : " + n1);
                    }
                    
                    GameObject SNP = (GameObject)BundleManager.Instance.LoadAsset("ShieldNetworkObj");
                    if (SNP != null)
                    {
                        _shieldNetworkingPrefab = SNP.GetComponent<ShieldNetworking>();
                        Debug.Log("FOUND ASSET (HK SHIELDS) !! ");
                    }
                    else
                    {
                        Debug.Log("COULDN'T FIND ASSET 'ShieldNetworkObj' !");
                    }

                    Debug.Log("FOUND ASSET BUNDLE (HK SHIELDS) ! : " + bundle.name);
                }
            } */

            IReadOnlyList<ModRecord> _r = ModDatabase.Instance.AllMods;
            foreach (ModRecord _mr in _r)
            {
                if (_mr.Info.ModName.Contains("HK Shields"))
                {
                    Debug.Log("FOUND MOD (HK SHIELDS) !");


                    List<AssetBundle> bundles = (List<AssetBundle>)(GetPrivateField(_mr, "_loadedBundles"));
                    if (bundles.Count > 0)
                    {
                        Debug.Log("BUNDLE EXISTS");

                        /* foreach (AssetBundle b in bundles)
                        {
                            if (b != null)
                            {
                                Debug.Log("<LOADED BUNDLE CONTENTS> " + b.name);
                            }
                            else
                            {
                                Debug.Log("<LOADED BUNDLE CONTENTS> THIS ENTRY IS NULL!");
                            }
                                
                        } */


                        AssetBundle bundle = null;

                        try
                        {
                            string[] assetBundles = _mr.Info.AssetBundles;
                            int length = 0;
                            int num = 0;
                            if (assetBundles != null)
                            {
                                length = (int)assetBundles.Length;
                            }
                            else
                            {
                                length = 0;
                            }
                            string[] assetCatalogs = _mr.Info.AssetCatalogs;
                            if (assetCatalogs != null)
                            {
                                num = (int)assetCatalogs.Length;
                            }
                            else
                            {
                                num = 0;
                            }
                            bool flag1 = (_mr.Info.AssetBundles == null ? false : _mr.Info.AssetBundles.Length != 0);
                            if (flag1)
                            {
                                string[] strArray = _mr.Info.AssetBundles;
                                if (strArray.Length > 0)
                                {
                                    string str5 = strArray[0];
                                    FilePath _p = new FilePath(str5, _mr.Info.FileLocation.Directory);

                                    bundle = AssetBundle.LoadFromFile(_p.RelativePath);
                                    Debug.Log("BUNDLE SUCCESSFULLY LOADED (HK SHIELDS) !");
                                    
                                }
                                else
                                {
                                    Debug.Log("BUNDLE NOT FOUND (HK SHIELDS) !");
                                }    
                                    
                                
                                strArray = null;
                                
                            }
                            
                        }
                        catch (Exception exception1)
                        {
                            Exception exception2 = exception1;
                            string str1 = (exception2 != null) ? exception2.ToString() : "";
                            
                            Debug.Log(string.Concat("ERROR LOADING HK SHIELDS ASSET BUNDLE (HK SHIELDS) ! : ", str1));
                        }

                        
                        
                        string[] n = bundle.GetAllAssetNames();

                        foreach (string n1 in n)
                        {
                            Debug.Log("ASSET : " + n1);
                        }

                        GameObject SNP = (GameObject)bundle.LoadAsset("ShieldNetworkObj");
                        if (SNP != null)
                        {
                            _shieldNetworkingPrefab = SNP.GetComponent<ShieldNetworking>();
                            Debug.Log("FOUND ASSET (HK SHIELDS) !! ");
                            bundle.Unload(false);
                            break;
                        }
                        else
                        {
                            Debug.Log("COULDN'T FIND ASSET 'ShieldNetworkObj' !");
                        }

                        bundle.Unload(false);
                    }
                    else
                    {
                        Debug.LogError("NO LOADED BUNDLES !!!");
                    }
                }
            }

            if (_shieldNetworkingPrefab != null)
            {
                NetworkClient.RegisterPrefab(_shieldNetworkingPrefab.gameObject, SpawnShield, UnspawnShield);
                Debug.Log("SCRIPT FOUND (HK SHIELDS) !!!");
            }
            else
            {
                Debug.LogError("SHIELD NETWORKING NOT FOUND IN ASSET BUNDLE (HK SHIELDS) !!! ");
            }
                
        }

        public void AddShields() {
        }

        public void PreLoad()
        {

        }

        public GameObject SpawnShield(SpawnMessage msg)
        {

            Debug.Log("Spawning Shield");
            ShieldNetworking shield = ShieldNetworking.Instantiate(_shieldNetworkingPrefab);
            
            
            return shield.gameObject;
        }

        public void UnspawnShield(GameObject spawned)
        {
            GameObject.Destroy(spawned);
        }

        public object GetPrivateField(object instance, string fieldName)
        {
            static object GetPrivateFieldInternal(object instance, string fieldName, Type type)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    return field.GetValue(instance);
                }
                else if (type.BaseType != null)
                {
                    return GetPrivateFieldInternal(instance, fieldName, type.BaseType);
                }
                else
                {
                    return null;
                }
            }

            return GetPrivateFieldInternal(instance, fieldName, instance.GetType());
        }

        public void SetPrivateField(object instance, string fieldName, object value)
        {
            static void SetPrivateFieldInternal(object instance, string fieldName, object value, Type type)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(instance, value);
                    return;
                }
                else if (type.BaseType != null)
                {
                    SetPrivateFieldInternal(instance, fieldName, value, type.BaseType);
                    return;
                }
            }

            SetPrivateFieldInternal(instance, fieldName, value, instance.GetType());
        }
    }
}