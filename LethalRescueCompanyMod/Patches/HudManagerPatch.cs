﻿using BepInEx;
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
            // spider debugging stuff
            if (isDebug && ___localPlayer.IsServer)
            {
                var sourceid = ___localPlayer.NetworkObjectId;
                //log.LogMessage($"playerName: {PlayerStateStore.playerControllerB?.name}");
                //log.LogMessage($"hasDeadBody: {PlayerStateStore.deadBodyInfo != null}");
                //log.LogMessage($"hasPlayerManager: {PlayerStateStore.playersManager != null}");
                RescueCompanyPingPong.instance.TestServerRpc("suckmaballs", sourceid);


                //var spiderSpawnBehaviorComponent = ___localPlayer.gameObject.GetComponent<SpiderSpawnBehavior>();
                //if(spiderSpawnBehaviorComponent!=null) spiderSpawnBehaviorComponent.DebugHacks(___localPlayer.thisPlayerBody, ___lastChatMessage, ___playersManager);
            }
        }


    }
}
