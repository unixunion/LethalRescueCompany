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
                var spiderSpawnBehaviorComponent = ___localPlayer.gameObject.GetComponent<SpiderSpawnBehavior>();
                if(spiderSpawnBehaviorComponent!=null) spiderSpawnBehaviorComponent.DebugHacks(___localPlayer.thisPlayerBody, ___lastChatMessage, ___playersManager);
            }
        }

        
    }
}
