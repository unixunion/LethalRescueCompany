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
        static void grabHangingBody(ref GrabbableObject ___currentlyGrabbingObject, ref GrabbableObject ___currentlyHeldObject)
        {

            // look at RagdollGrabbableObject
            // RagdollGrabbableObject

            if (___currentlyGrabbingObject == null) return;
            if (___currentlyHeldObject != null) return;

            log.LogInfo($"BeginGrabbbing: {___currentlyGrabbingObject.name}");
            var trait = ___currentlyGrabbingObject.GetComponentInParent<RevivableTrait>();

            if (trait != null)
            {
                log.LogInfo($"BeginGrabbbing: has trait: {___currentlyGrabbingObject.name}");
                var ragdollGrabbableObject = ___currentlyGrabbingObject.GetComponentInParent<RagdollGrabbableObject>();
                if (ragdollGrabbableObject != null)
                {
                    log.LogInfo("BeginGrabbbing: It is indeed a ragdollGrabbableObject body");

                    ragdollGrabbableObject.ragdoll.bodyParts.ToList().ForEach(bodypart => {
                        bodypart.interpolation = RigidbodyInterpolation.Extrapolate;
                    }); 

                    ragdollGrabbableObject.DiscardItemFromEnemy();
                    


                    var db = ragdollGrabbableObject.ragdoll;
                    db.attachedTo = null;

                    //if (db != null) 
                    //{
                        
                    //    var line = db.attachedTo.GetComponentInParent<SetLineRendererPoints>();
                    //    if (line != null)
                    //    {
                    //        log.LogInfo("BeginGrabbbing: destroy the line");
                    //        Destroy(line);
                    //    } else
                    //    {
                    //        log.LogInfo("BeginGrabbbing: SetLineRendererPoints is null");
                    //    }

                    //    log.LogInfo("BeginGrabbbing: destroy the attachedTo.gameObject");
                    //    try
                    //    {
                    //        Destroy(db.attachedTo.gameObject);
                    //    }
                    //    catch
                    //    {
                    //        log.LogError("BeginGrabbbing: error detroying attachedTo");
                    //    }
                    //}
                    

                    //log.LogInfo("BeginGrabbbing: destroy attachedTo");
                    //Destroy(db.attachedTo);

                    //db.attachedTo = null;

                    ragdollGrabbableObject.FallToGround();

                    // unsetting kinematics
                    //log.LogInfo("BeginGrabbbing: setting kinematics");
                    //db.SetBodyPartsKinematic(false);

                    //db.grabBodyObject.OnHitGround();

                }
                else
                {
                    log.LogWarning("BeginGrabbbing: not ragdollGrabbableObject");
                }

                log.LogInfo("ive done all I can");
            }



            else
            {
                log.LogDebug("no revivable trait found, cant grab patch this");
            }
        }




        [HarmonyPatch("SpawnDeadBody")]
        [HarmonyPostfix]
        static void debugDeath(ref PlayerControllerB __instance)
        {
            if (isDebug)
            {
                log.LogInfo("making fucknut revivable");
                __instance.deadBody.gameObject.AddComponent<RevivableTrait>();
            }
        }



    }
}
