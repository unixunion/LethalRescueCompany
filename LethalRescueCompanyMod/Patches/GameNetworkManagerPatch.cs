using BepInEx;
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
                        //if (asset.tag.ToLower().Equals("physicsprop")) {//this wont change regardless of how i build the asset, i suggest a managed asset key list to do this
                        if (itemsThatShouldBeGrabbable.Contains(mapping.Key))
                        {
                            log.LogInfo($"making: {mapping.Key} grabbable");
                            asset = MakeGrabbable(asset, mapping.Key);
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

                // hackes

                //GameObject foo = new GameObject();

                //prefab.AddComponent<GrabbableObject>();
                //GrabbableObject grabbable = prefab.GetComponent<GrabbableObject>();
                //grabbable.grabbable = true;
                //grabbable.itemProperties = new Item();
                //grabbable.itemProperties.canBeGrabbedBeforeGameStart = true;



            });


        }

        private static GameObject MakeGrabbable(GameObject asset, string key)
        {
            asset.AddComponent<LRCGrabbableObject>();
            var a = asset.GetComponent<LRCGrabbableObject>();
            log.LogInfo("Patching object to be grabbable");
            a.grabbable = true;
            a.itemProperties = ScriptableObject.CreateInstance<Item>();
            if (Settings.isDebug) a.itemProperties.canBeGrabbedBeforeGameStart = true;
            a.itemProperties.itemName = key;
            a.itemProperties.itemId = 1512;
            a.tag = asset.tag;
            asset.layer = 6;

            return asset;
        }

    }
}
