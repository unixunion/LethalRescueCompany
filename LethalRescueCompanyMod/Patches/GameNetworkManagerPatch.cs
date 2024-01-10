﻿using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.Models;
using LethalRescueCompanyMod.NetworkBehaviors;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch : BaseUnityPlugin
    {
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.GameNetworkManager");
        public static GameObject networkPrefab {  get; private set; }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void startPatch(ref GameNetworkManager __instance, ref PlayerControllerB ___localPlayerController)
        {
            log.LogInfo("Dialing the donut");
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = false;

            AssetManager.assetMappings.ToList().ForEach(mapping =>
            {
                if (mapping.Key != "LethalRescueNetworkPrefab")
                {
                    var asset = AssetManager.GetAssetByKey(mapping.Key);
                    if ( asset.GetComponent<NetworkObject>() != null)
                    {
                        log.LogInfo($"Adding prefab: {mapping.Key} to NetworkManager");

                        //Destroy(asset.GetComponent<NetworkRigidbody>());
                        //Destroy(asset.GetComponent<Rigidbody>());

                        asset.AddComponent<LRCGrabbableObject>();

                        if (asset.tag == "PhysicsProp")
                        {
                            var a = asset.GetComponent<LRCGrabbableObject>();
                            log.LogInfo("Patching object to be grabbable");
                            a.grabbable = true;
                            a.itemProperties = ScriptableObject.CreateInstance<Item>();
                            a.itemProperties.canBeGrabbedBeforeGameStart = true;
                            a.itemProperties.itemName = mapping.Key;
                            a.itemProperties.itemId = 1512;
                            a.tag = "PhysicsProp";
                            asset.layer = 6;
                        }

                        NetworkManager.Singleton.AddNetworkPrefab(asset);
                    } else
                    {
                        log.LogWarning($"Asset {mapping.Key} has no NetworkObject");
                    }

                }
                else
                {
                    log.LogInfo("Loading LethalRescueNetworkPrefab");
                    networkPrefab = AssetManager.GetAssetByKey("LethalRescueNetworkPrefab");

                    log.LogInfo($"Adding RescueCompanyController to LethalRescueNetworkPrefab: {networkPrefab}");
                    networkPrefab.AddComponent<RescueCompanyController>();

                    log.LogInfo("Adding the RescueCompanyController prefab via networking manager");
                    NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
                }

                // hackes
                
                //GameObject foo = new GameObject();

                //prefab.AddComponent<GrabbableObject>();
                //GrabbableObject grabbable = prefab.GetComponent<GrabbableObject>();
                //grabbable.grabbable = true;
                //grabbable.itemProperties = new Item();
                //grabbable.itemProperties.canBeGrabbedBeforeGameStart = true;
               


            });
            

        }



    }
}
