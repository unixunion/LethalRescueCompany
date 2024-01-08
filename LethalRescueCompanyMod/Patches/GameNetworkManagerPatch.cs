using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.NetworkBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch : BaseUnityPlugin
    {
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.GameNetworkManager");

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void startPatch(ref GameNetworkManager __instance)
        {
            log.LogInfo("Dialing the donut");
            var netman = __instance.GetComponent<NetworkManager>();
            log.LogInfo("Initializing the ping pooog");
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = false;
            log.LogInfo("Singling out the wingus");
            GameObject rescuePingPong = new GameObject("RescueCompanyPingPong");
            rescuePingPong.AddComponent<RescueCompanyPingPong>();
            rescuePingPong.AddComponent<NetworkObject>();
            log.LogInfo("Shaking the dog");
            DontDestroyOnLoad(rescuePingPong);
            log.LogInfo("Whisking the mayo");
            NetworkManager.Singleton.AddNetworkPrefab(rescuePingPong);
        }
    }
}
