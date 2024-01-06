using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SandSpiderAIPatch : BaseUnityPlugin
    {

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.SandSpiderAIPatch");

        static DeadBodyInfo currentlyHeldBody = null;
        [HarmonyPatch("HangBodyFromCeiling")]
        [HarmonyPrefix]
        static void hangBodyFromCeilingPrePatch(ref DeadBodyInfo ___currentlyHeldBody)
        {
            currentlyHeldBody = ___currentlyHeldBody;
        }

        [HarmonyPatch("HangBodyFromCeiling")]
        [HarmonyPostfix]
        static void hangBodyFromCeilingPostPatch(ref DeadBodyInfo ___currentlyHeldBody)
        {
            makeWebbedBodyGrabbable(currentlyHeldBody);
        }

        private static void makeWebbedBodyGrabbable(DeadBodyInfo deadBodyInfo)
        {
            log.LogInfo($"Making wrapped body grabbable, currently attached to: {deadBodyInfo.attachedTo.name}");
            deadBodyInfo.grabBodyObject.grabbable = true;
            deadBodyInfo.canBeGrabbedBackByPlayers = true;
        }

    }
}
