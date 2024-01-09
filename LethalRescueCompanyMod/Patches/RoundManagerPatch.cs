using BepInEx.Logging;
using HarmonyLib;
using LethalRescueCompanyMod.NetworkBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RoundManager");

        //[HarmonyPatch("GenerateNewFloor")]
        //[HarmonyPostfix]
        //static void SubscribeToHandler()
        //{
        //    log.LogInfo("subscribing to eventh handler");
        //    RescueCompanyPingPong.LevelEvent += RescueCompanyPingPong.Instance.ReceivedEventFromServer;
        //}

        //[HarmonyPatch("DespawnPropsAtEndOfRound")]
        //[HarmonyPostfix]
        //static void UnsubscribeFromHandler()
        //{
        //    log.LogInfo("unsubscribing to eventh handler");
        //    RescueCompanyPingPong.LevelEvent -= RescueCompanyPingPong.Instance.ReceivedEventFromServer;
        //}
    }
}
