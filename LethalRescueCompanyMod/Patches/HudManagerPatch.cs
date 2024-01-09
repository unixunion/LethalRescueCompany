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

            //RescueCompanyPingPong.Instance.ToggleServerRpc(0.1f);

            RescueCompanyPingPong.Instance.SendEventToClients(___lastChatMessage);

            //___localPlayer.gameObject.GetComponent<RescueCompanyPingPong>().ToggleServerRpc(0.1f);
            // spider debugging stuff
            if (isDebug && ___localPlayer.IsServer)
            {
                //log.LogMessage($"playerName: {PlayerStateStore.playerControllerB?.name}");
                //log.LogMessage($"hasDeadBody: {PlayerStateStore.deadBodyInfo != null}");
                //log.LogMessage($"hasPlayerManager: {PlayerStateStore.playersManager != null}");
                //var spiderSpawnBehaviorComponent = ___localPlayer.gameObject.GetComponent<SpiderSpawnBehavior>();
                //if(spiderSpawnBehaviorComponent!=null) spiderSpawnBehaviorComponent.DebugHacksServerRpc(___localPlayer.thisPlayerBody, ___lastChatMessage, ___playersManager);
            }
        }


    }
}
