using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.Models;
using LethalRescueCompanyMod.NetworkBehaviors;
using System;
using System.Linq;
using UnityEngine;

namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HudManagerPatch : BaseUnityPlugin
    {
        static Helper helper = new Helper();
        static bool isDebug = Settings.isDebug;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.HudManagerPatch");
        [HarmonyPatch("AddTextToChatOnServer")]
        [HarmonyPostfix]
        static void AddTextToChatOnServerPatch(ref PlayerControllerB ___localPlayer, ref String ___lastChatMessage, ref StartOfRound ___playersManager)
        {
            if (___localPlayer == null ) return;
            var sourceid = ___localPlayer.NetworkObjectId;

            if (!___lastChatMessage.StartsWith("lrc")) return;
            String command = ___lastChatMessage.Split().ElementAt(1).ToLower();


            switch (command)
            {
                case "spawn":
                    SpawnCommand(___localPlayer, ___lastChatMessage);
                    break;
                
                case "destroy":
                    log.LogInfo("ClientEventParser destory action");
                    break;
                case "clone":
                    log.LogInfo("ClientEventParser clone action");
                    break;
                default:
                    log.LogInfo($"ClientEventParser unknown action: {command}");
                    break;
            }


            

            //___localPlayer.gameObject.GetComponent<RescueCompanyController>().ToggleServerRpc(0.1f);
            // spider debugging stuff
            
        }

        private static void SpawnCommand(PlayerControllerB player, string inputString)
        {
            var command = inputString.Split().ElementAt(2).ToLower();

            switch (command)
            {
                case "spider":
                    if (isDebug && player.IsServer)
                    {
                        log.LogInfo("spawning spider");
                        var spiderSpawnBehaviorComponent = player.gameObject.GetComponent<SpiderSpawnBehavior>();
                        if (spiderSpawnBehaviorComponent != null) spiderSpawnBehaviorComponent.DebugHacks(player.thisPlayerBody);
                    }
                    break;
                default:
                    log.LogInfo("default spawn action interpreter");
                    var commandObj = new Event(CommandContract.Command.SpawnCube, Vector3.zero.normalized, (int)player.playerClientId);
                    RescueCompanyController.Instance.HandleEvent(commandObj);
                    break;
            }
        }

    }
}
