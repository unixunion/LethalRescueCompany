using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.Models;
using LethalRescueCompanyMod.NetworkBehaviors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
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
                case "spider":
                    if (isDebug && ___localPlayer.IsServer)
                    {
                        log.LogInfo("spawning spider");
                        var spiderSpawnBehaviorComponent = ___localPlayer.gameObject.GetComponent<SpiderSpawnBehavior>();
                        if (spiderSpawnBehaviorComponent != null) spiderSpawnBehaviorComponent.DebugHacks(___localPlayer.thisPlayerBody);
                    }
                    break;
                case "cube":
                    log.LogInfo("ClientEventParser spawn action");
                    var commandObj = new Event(CommandContract.Command.SpawnSpider, Vector3.zero);
                    RescueCompanyPingPong.Instance.SendEventToClients(commandObj);
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


            

            //___localPlayer.gameObject.GetComponent<RescueCompanyPingPong>().ToggleServerRpc(0.1f);
            // spider debugging stuff
            
        }


    }
}
