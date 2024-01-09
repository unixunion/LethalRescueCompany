using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.NetworkBehaviors;
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
        static GameObject networkPrefab;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void startPatch(ref GameNetworkManager __instance, ref PlayerControllerB ___localPlayerController)
        {
            log.LogInfo("Dialing the donut");
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = false;
        }

        [HarmonyPatch("SetLobbyJoinable")]
        [HarmonyPostfix]
        static void setLobbyJoinablePatch(ref GameNetworkManager __instance, ref PlayerControllerB ___localPlayerController)
        {
            log.LogInfo("Initializing the ping pooog");
            
            log.LogInfo("Loading prefab");
            var MainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "prefabs"));
            if (MainAssetBundle == null)
            {
                log.LogError("MainAssetBundle is null");
                return;
            }

            networkPrefab = (GameObject)MainAssetBundle.LoadAsset("assets/lethalrescuenetworkprefab.prefab");


            //GameObject rescuePingPong = new GameObject("RescueCompanyPingPong");
            log.LogInfo($"Adding components to prefab: {networkPrefab}");
            

            networkPrefab.AddComponent<RescueCompanyPingPong>();
            //rescuePingPong.AddComponent<NetworkObject>();
            //log.LogInfo("Shaking the dog");
            //DontDestroyOnLoad(rescuePingPong);
            //log.LogInfo("Whisking the mayo");
            if (___localPlayerController != null && NetworkManager.Singleton.IsServer)
            {
                log.LogInfo("Adding the prefab via networking manager");
                NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
            }

        }
    }
}
