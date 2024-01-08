using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.Linq;
using System;
using UnityEngine.Video;
using System.Threading.Tasks;
using System.Diagnostics;
using BepInEx;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using Dissonance.Integrations.Unity_NFGO;
using Newtonsoft.Json;
using DunGen;
using System.Collections.Generic;
using System.Collections;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.NetworkBehaviors;

//round manager has spawn enemies

// Interesting Files
//
// DeadBodyInfo
// PlayerControllerBPatch
// SandSpiderAI
// https://thunderstore.io/c/lethal-company/p/Noop/UnityExplorer/


namespace LethalRescueCompanyPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch : BaseUnityPlugin
    {

        static bool isDebug = Settings.isDebug;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch");
        static Helper helper = new Helper();
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updatePatch(
            ref PlayerControllerB __instance,
            ref StartOfRound ___playersManager,
            ref DeadBodyInfo ___deadBody)
        {
            if (isDebug)
            {
                if (___playersManager != null)
                {
                    foreach (var item in ___playersManager.allPlayerScripts)
                    {
                        if (item.gameObject.GetComponent<SpiderSpawnBehavior>() == null) item.gameObject.AddComponent<SpiderSpawnBehavior>();
                    }
                }
            }
            // nope out if not a body
            if (___deadBody == null) return;
            if (!__instance.isPlayerDead) return;
            if (__instance == null)
            {
                log.LogError("instance is null");
                return;
            }

            var revivabletrait = ___deadBody.gameObject.GetComponent<RevivableTrait>();
            if (revivabletrait == null)
            {
                log.LogWarning("revivable trait is null");
                return;
            }
            revivabletrait.playerIsDeadInShipAndRevivable(___deadBody, ___playersManager);

        }


        [HarmonyPatch("BeginGrabObject")]
        [HarmonyPostfix]
        static void grabHangingBody(ref GrabbableObject ___currentlyGrabbingObject)
        {
            if (___currentlyGrabbingObject == null) return;
            if (___currentlyGrabbingObject.GetComponentInChildren<RevivableTrait>() != null)
            {
                var db = ___currentlyGrabbingObject.GetComponentInParent<DeadBodyInfo>();
                if (db != null)
                {
                    var strungup = db.GetComponent<SetLineRendererPoints>();
                    if (strungup != null)
                    {
                        Destroy(strungup);
                    }
                    else
                    {
                        log.LogWarning("no strungup attached");
                    }

                    Destroy(db.attachedTo);
                    db.attachedTo = null;
                }
                else
                {
                    log.LogWarning("no deadbody attached");
                }

            }
            else
            {
                log.LogDebug("no revivable trait found, cant grab this");
            }
        }

    }
}