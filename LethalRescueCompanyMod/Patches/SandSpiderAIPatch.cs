using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            for (int i = 0; i < currentlyHeldBody.bodyBloodDecals.Length; i++)
            {
                currentlyHeldBody.bodyBloodDecals[i].SetActive(value: false);
            }
        }

        [HarmonyPatch("HangBodyFromCeiling")]
        [HarmonyPostfix]
        static void hangBodyFromCeilingPostPatch(ref DeadBodyInfo ___currentlyHeldBody)
        {
            makeWebbedBodyGrabbable(currentlyHeldBody);
        }

        private static void makeWebbedBodyGrabbable(DeadBodyInfo deadBodyInfo)
        {
            if (Settings.isDebug) log.LogInfo($"Making wrapped body grabbable, currently attached to: {deadBodyInfo.attachedTo.name}");

            //deadBodyInfo.secondaryAttachedTo = deadBodyInfo.attachedTo;
            //deadBodyInfo.attachedTo = null;
            //if (Settings.isDebug) log.LogInfo($"reattached via secondary to the primary attachment point");

            deadBodyInfo.grabBodyObject.grabbable = true;
            deadBodyInfo.canBeGrabbedBackByPlayers = true;

            if (deadBodyInfo.gameObject != null && deadBodyInfo.gameObject.GetComponent<RevivableTrait>() == null)
            {
                if (Settings.isDebug) log.LogInfo("adding revivable trait");
                deadBodyInfo.gameObject.AddComponent<RevivableTrait>();
            }
            else
            {
                log.LogWarning("deadbody gameobject was null");
            }

            if (Settings.isDebug)
            {
                log.LogInfo("lets inspect...");
                log.LogInfo($"db....grabbable: {deadBodyInfo.grabBodyObject.grabbable}, " +
                            $"db.canBeGrabbedBackByPlayers: {deadBodyInfo.canBeGrabbedBackByPlayers}, " +
                            $"db....hasHitGround: {deadBodyInfo.grabBodyObject.hasHitGround}, " +
                            $"db<ReviveTrait>: {deadBodyInfo.gameObject.GetComponent<RevivableTrait>()}");
            }

        }

    }
}
