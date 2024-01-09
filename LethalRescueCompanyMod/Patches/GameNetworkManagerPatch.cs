﻿using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.NetworkBehaviors;
using LethalRescueCompanyPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

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

            log.LogInfo("Loading prefab");
            networkPrefab = LethalCompanyMemorableMomentsPlugin.instance.getPrefab();

            log.LogInfo($"Adding components to prefab: {networkPrefab}");
            networkPrefab.AddComponent<RescueCompanyPingPong>();
            
            log.LogInfo("Adding the prefab via networking manager");
            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);

        }



    }
}
