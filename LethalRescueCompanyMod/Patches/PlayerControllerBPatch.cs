using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using BepInEx;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.NetworkBehaviors;
using UnityEngine;

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

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void startPatch(
           ref PlayerControllerB __instance,
           ref StartOfRound ___playersManager,
           ref DeadBodyInfo ___deadBody)
        {

        }


        [HarmonyPatch("BeginGrabObject")]
        [HarmonyPrefix]
        static void ReplaceObjectWithSurrogate(ref Camera ___gameplayCamera, ref PlayerControllerB __instance)
        {
            Ray interactRay = new Ray(___gameplayCamera.transform.position, ___gameplayCamera.transform.forward);
            Physics.Raycast(interactRay, out var hit, __instance.grabDistance, 832);

            if (hit.collider == null) return;
            log.LogInfo($"ray hit: {hit.collider.transform.gameObject}");

            var testing = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();

            //var testing = hit.collider.transform.gameObject.GetComponentInParent<DeadBodyInfo>();
            log.LogInfo($"testing: {testing}");
            //var t2 = hit.collider.transform.gameObject.GetComponentInParent<RagdollGrabbableObject>();
            //log.LogInfo($"t2: {t2}");
            //var t3 = hit.collider.transform.gameObject.GetComponent<RagdollGrabbableObject>();
            //log.LogInfo($"t3: {t3}");
            //var t4 = hit.collider.transform.gameObject.GetComponentInChildren<RagdollGrabbableObject>();
            //log.LogInfo($"t4: {t4}");


            var currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
            if (testing != null) // && testing.attachedTo != null
            {

                log.LogMessage("ray hit a grabbable");

                var dbinfo = hit.collider.transform.gameObject.GetComponentInParent<DeadBodyInfo>();
                if (dbinfo != null)
                {
                    log.LogInfo("freeing the body");
                    dbinfo.attachedLimb = null;
                    dbinfo.attachedTo = null;
                    dbinfo.wasMatchingPosition = false;
                } else
                {
                    log.LogWarning("not sure what the hell this is?");
                }
                
            }
        }


        //    }
        //    else
        //    {
        //        log.LogInfo($"object does not contain GrabbableObject ");
        //    }

        //    // currentlyGrabbingObject.InteractItem();

        //}

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
        }

        //[HarmonyPatch("GrabObject")]
        //[HarmonyPrefix]
        static void grabHangingBody(ref GrabbableObject ___currentlyGrabbingObject, ref GrabbableObject ___currentlyHeldObject, ref PlayerControllerB __instance, ref GrabbableObject ___currentlyHeldObjectServer)
        {

            // look at RagdollGrabbableObject
            // RagdollGrabbableObject

            if (___currentlyGrabbingObject == null) return;
            //if (___currentlyHeldObject != null) return;


            log.LogInfo($"BeginGrabbbing: {___currentlyGrabbingObject.name}");

            var trait = ___currentlyGrabbingObject.GetComponentInParent<RevivableTrait>();

            if (trait != null)
            {
                log.LogInfo($"BeginGrabbbing: has trait: {___currentlyGrabbingObject.name}");

                var ragdollGrabbableObject = ___currentlyGrabbingObject.GetComponentInParent<RagdollGrabbableObject>();
                if (ragdollGrabbableObject != null)
                {
                    log.LogInfo("BeginGrabbbing: It is indeed a ragdollGrabbableObject body, dropping");
                    var originalDeadBodyInfo = ragdollGrabbableObject.ragdoll;

                    // glutchfest 2024
                    //log.LogInfo("performing the switcheroo");
                    //var cloneDeadbodyInfo = BodyCloneBehavior.CloneDeadBody(originalDeadBodyInfo);

                    //ragdollGrabbableObject.ragdoll = cloneDeadbodyInfo;
                    //log.LogInfo($"BeginGrabbbing: now set to: {___currentlyGrabbingObject.name}");

                    // this was in order to grab the network spawned cube, dont delete it!
                    ___currentlyGrabbingObject = BodyCloneBehavior.ReplacementBody(originalDeadBodyInfo).GetComponent<GrabbableObject>();
                    //___currentlyHeldObjectServer = ___currentlyGrabbingObject;

                    //__instance.SpawnDeadBody(originalDeadBodyInfo.playerObjectId, originalDeadBodyInfo.transform.position, 0, originalDeadBodyInfo.playerScript);
                    //Destroy(___currentlyGrabbingObject);

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
                log.LogInfo("making revivable");
                __instance.deadBody.gameObject.AddComponent<RevivableTrait>();
                log.LogInfo("trait added");
            }
        }
    }
}
