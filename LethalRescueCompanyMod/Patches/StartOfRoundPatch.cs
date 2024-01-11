using BepInEx.Logging;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using LethalRescueCompanyMod.Models;
using LethalRescueCompanyMod.Hacks;

namespace LethalRescueCompanyMod.Patches
{
    //76561197971961474 --kegan
    //76561198057628250 --arron
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        static bool isDebug = Settings.isDebug;
        static bool hasTriedToConnect = false;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.StartOfRound");
        static Helper helper = new Helper();
        
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(ref StartOfRound __instance)
        {
            if (Settings.isSolo)
            {
                if (__instance == null)
                {
                    log.LogWarning($"instance is null");
                    return;
                }
                if (GameNetworkManager.Instance == null)
                {
                    return;
                }
                if (GameNetworkManager.Instance.localPlayerController != null)
                {
                    if (!hasTriedToConnect)
                    {
                        __instance.connectedPlayersAmount += 2;
                        __instance.livingPlayers += 2;
                        hasTriedToConnect = true;
                        __instance.StartGame();
                        
                    }
                }

                if (__instance.localPlayerController.isPlayerDead)
                {
                    
                    RevivableTrait revivable = __instance.localPlayerController.deadBody.gameObject.GetComponent<RevivableTrait>();
                    if (revivable != null) revivable.revivePlayer(); // .DebugRevive(__instance.localPlayerController.deadBody, __instance.localPlayerController.playersManager);
                } else
                {
                    
                }

            }
        }
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void SpawnNetworkHandler(ref StartOfRound __instance)
        {
            
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                log.LogInfo("SpawnNetworkHandler: spawning the network object");
                var networkHandlerHost = UnityEngine.Object.Instantiate(AssetManager.GetAssetByKey("LethalRescueNetworkPrefab"), Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            } else
            {
                log.LogInfo("SpawnNetworkHandler: im not the host nor the server");
            }

            log.LogInfo("Hacks!!!: making cube prefab savable");
            var cubeAsset = AssetManager.GetAssetByKey("CubePrefab");
            if (cubeAsset != null)
            {
                __instance.allItemsList.itemsList.Add(cubeAsset.GetComponent<LRCGrabbableObject>()?.itemProperties);
            }

        }


        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        static void startOfRoundPatch(ref StartOfRound __instance)
        {
            AddWelcomeMessage(__instance);
        }


        private static void AddWelcomeMessage(StartOfRound playersManager)
        {
            if (playersManager != null)
            {
                foreach (var item in playersManager.allPlayerScripts)
                {
                    if (item.gameObject.GetComponent<WelcomeMessage>() == null) item.gameObject.AddComponent<WelcomeMessage>();
                    if (item.gameObject.GetComponent<PowerCheat>() == null)
                    {
                        if (Settings.isDebug) item.gameObject.AddComponent<PowerCheat>();
                    }
                    if (item.gameObject.GetComponent<SpeedCheat>() == null)
                    {
                        if (Settings.isDebug) item.gameObject.AddComponent<SpeedCheat>();
                    }
                }
            }
        }

    }
}
