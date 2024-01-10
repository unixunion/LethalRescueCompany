using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.NetworkBehaviors;
using System.Linq;
using Unity.Netcode;
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

            log.LogInfo("Loading prefabs");
            networkPrefab = AssetManager.GetAssetByKey("LethalRescueNetworkPrefab");

            log.LogInfo($"Adding components to prefab: {networkPrefab}");
            networkPrefab.AddComponent<RescueCompanyController>();
            
            log.LogInfo("Adding the prefab via networking manager");
            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);

            
            AssetManager.assetMappings.ToList().ForEach(mapping =>
            {
                if (mapping.Key != "LethalRescueNetworkPrefab")
                {
                    var asset = AssetManager.GetAssetByKey(mapping.Key);
                    if ( asset.GetComponent<NetworkObject>() != null)
                    {
                        log.LogInfo($"Adding prefab: {mapping.Key} to NetworkManager");
                        NetworkManager.Singleton.AddNetworkPrefab(asset);
                    } else
                    {
                        log.LogWarning("Asset {} has no NetworkObject");
                    }
                    
                }
            });
            

        }



    }
}
