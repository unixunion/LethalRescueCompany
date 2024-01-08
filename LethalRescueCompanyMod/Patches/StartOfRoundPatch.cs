using BepInEx.Logging;
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
    }
}
