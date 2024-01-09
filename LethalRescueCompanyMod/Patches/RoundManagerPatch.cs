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
    internal class RoundManagerPatch
    {

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RoundManager");


        [HarmonyPatch("GenerateNewFloor")]
        [HarmonyPostfix]
        static void SubscribeToHandler()
        {
            RescueCompanyPingPong.LevelEvent += ReceivedEventFromServer;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        static void UnsubscribeFromHandler()
        {
            RescueCompanyPingPong.LevelEvent -= ReceivedEventFromServer;
        }

        static void ReceivedEventFromServer(string eventName)
        {
            log.LogInfo($"event: {eventName}");
        }

        static void SendEventToClients(string eventName)
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            RescueCompanyPingPong.Instance.EventClientRpc(eventName);
        }

    }
}
