﻿using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.Models;
using LethalRescueCompanyMod.NetworkBehaviors;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch : BaseUnityPlugin
    {
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.GameNetworkManager");
        public static GameObject networkPrefab { get; private set; }
        static List<string> itemsThatShouldBeGrabbable = new List<string>() { "Example", "CubePrefab" };
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void startPatch(ref GameNetworkManager __instance, ref PlayerControllerB ___localPlayerController)
        {
            log.LogInfo("Dialing the donut");
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = false;

            int itemId = 1512;
            AssetManager.assetMappings.ToList().ForEach(mapping =>
            {
                if (mapping.Key != "LethalRescueNetworkPrefab")
                {
                    var asset = AssetManager.GetAssetByKey(mapping.Key);
                    if (asset.GetComponent<NetworkObject>() != null)
                    {
                        log.LogInfo($"Adding prefab: {mapping.Key} to NetworkManager");

                        if(itemsThatShouldBeGrabbable.Contains(mapping.Key))
                        {
                            log.LogInfo($"Making {mapping.Key} grabbable");
                            asset = MakeGrabbable(asset, mapping.Key, mapping.Value);
                            itemId += 1;
                        }

                        NetworkManager.Singleton.AddNetworkPrefab(asset);
                    }
                    else
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

            });
        }

        private static GameObject MakeGrabbable(GameObject asset, string key, GameObject assetPath)
        {
            asset.AddComponent<LRCGrabbableObject>();
            var a = asset.GetComponent<LRCGrabbableObject>();
            log.LogInfo("Patching object to be grabbable");
            a.grabbable = true;
            a.itemProperties = ScriptableObject.CreateInstance<Item>();
            if (Settings.isDebug) a.itemProperties.canBeGrabbedBeforeGameStart = true;
            a.itemProperties.itemName = key;
            a.itemProperties.itemId = 1512;
            a.tag = "PhysicsProp";
            asset.layer = 6;

            // make the item savable, it needs bool and a prefab hint.
            a.itemProperties.saveItemVariable = true;
            a.itemProperties.spawnPrefab = assetPath;

            return asset;
        }

    }
}
