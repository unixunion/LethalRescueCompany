using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyMod.Models;
using Unity.Netcode;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace LethalRescueCompanyMod.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    private static bool hasTriedToConnect;
    internal static ManualLogSource log = Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.StartOfRound");


    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void UpdatePatch(ref StartOfRound __instance)
    {
        if (Settings.IsSolo)
        {
            if (__instance == null)
            {
                log.LogWarning("instance is null");
                return;
            }

            if (GameNetworkManager.Instance == null) return;
            if (GameNetworkManager.Instance.localPlayerController != null)
                if (!hasTriedToConnect)
                {
                    __instance.connectedPlayersAmount += 2;
                    __instance.livingPlayers += 2;
                    hasTriedToConnect = true;
                    __instance.StartGame();
                }

            //if (__instance.localPlayerController.isPlayerDead)
            //{
            //    RevivableTrait revivable = __instance.localPlayerController.deadBody.gameObject.GetComponent<RevivableTrait>();
            //    if (revivable != null) revivable.revivePlayer(); // .DebugRevive(__instance.localPlayerController.deadBody, __instance.localPlayerController.playersManager);
            //} else
            //{
            //}
        }
    }

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void SpawnNetworkHandler(ref StartOfRound __instance)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            log.LogInfo("SpawnNetworkHandler: spawning the network object");
            var networkHandlerHost = Object.Instantiate(AssetManager.GetAssetByKey("LethalRescueNetworkPrefab"),
                Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            log.LogInfo("SpawnNetworkHandler: im not the host nor the server");
        }

        log.LogInfo("Hacks!!!: making cube prefab savable");
        var cubeAsset = AssetManager.GetAssetByKey("CubePrefab");
        if (cubeAsset != null)
            __instance.allItemsList.itemsList.Add(cubeAsset.GetComponent<LrcGrabbableObject>()?.itemProperties);
    }
}