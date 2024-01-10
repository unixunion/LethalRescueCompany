using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.NetworkBehaviors;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using LethalRescueCompanyPlugin;
using LethalRescueCompanyMod.Models;

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

        
        

    }
}
