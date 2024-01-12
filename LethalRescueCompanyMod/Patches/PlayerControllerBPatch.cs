using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using BepInEx;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.NetworkBehaviors;
using UnityEngine;
using LethalRescueCompanyMod.Hacks;
using System.Linq;
using LethalRescueCompanyMod.Models;

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
            foreach (var item in ___playersManager.allPlayerScripts)
            {
                if (item.gameObject.GetComponent<WelcomeMessage>() == null) item.gameObject.AddComponent<WelcomeMessage>();
                if (item.gameObject.GetComponent<SpiderSpawnBehavior>() == null) item.gameObject.AddComponent<SpiderSpawnBehavior>();
                if (Settings.isDebug)
                {
                    if (item.gameObject.GetComponent<PowerCheat>() == null) item.gameObject.AddComponent<PowerCheat>();
                    if (item.gameObject.GetComponent<SpeedCheat>() == null) item.gameObject.AddComponent<SpeedCheat>();
                }

            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updatePatch(
        ref PlayerControllerB __instance,
        ref StartOfRound ___playersManager,
            ref DeadBodyInfo ___deadBody)
        {
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
                //log.LogDebug("revivable trait is null");
                return;
            }
        }





        [HarmonyPatch("BeginGrabObject")]
        [HarmonyPrefix]
        static void ReplaceObjectWithSurrogate(ref Camera ___gameplayCamera, ref PlayerControllerB __instance)
        {
            Ray interactRay = new Ray(___gameplayCamera.transform.position, ___gameplayCamera.transform.forward);
            Physics.Raycast(interactRay, out var hit, __instance.grabDistance, 832);

            log.LogInfo($"ray hit: {hit}");
            var currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();

            if (currentlyGrabbingObject != null)
            {
                log.LogInfo($"grabbing hack: {currentlyGrabbingObject.GetComponent<GrabbableObject>()}");

                var trait = currentlyGrabbingObject.GetComponentInParent<RevivableTrait>();

                if (trait != null)
                {
   
                    log.LogInfo($"BeginGrabbbing: has trait: {currentlyGrabbingObject.name}");
                    trait.Interact();
                    var ragdollGrabbableObject = currentlyGrabbingObject.GetComponentInParent<RagdollGrabbableObject>();
                    if (ragdollGrabbableObject != null)
                    {
                        log.LogInfo("BeginGrabbbing: It is indeed a ragdollGrabbableObject body, dropping");
                        
                        var deadBodyInfo = currentlyGrabbingObject.GetComponentInParent<DeadBodyInfo>();
                        if (deadBodyInfo == null) return;
                        log.LogInfo("cloning this shit");
                        //BodyCloneBehavior.ReplacementBody(deadBodyInfo).GetComponent<GrabbableObject>();

                        BodyCloneBehavior.ReplacementBody(ragdollGrabbableObject.ragdoll).GetComponent<GrabbableObject>();

                        //Destroy(originalDeadBodyInfo);
                    }
                }


            }
            else
            {
                log.LogInfo($"object does not contain GrabbableObject ");
            }

            // currentlyGrabbingObject.InteractItem();

        }






        //[HarmonyPatch("GrabObject")]
        //[HarmonyPrefix]
        static void grabHangingBody(ref GrabbableObject ___currentlyGrabbingObject, ref GrabbableObject ___currentlyHeldObject, ref PlayerControllerB __instance, ref GrabbableObject ___currentlyHeldObjectServer)
        {
            if (___currentlyGrabbingObject == null)
            {
                log.LogError("grabbing null?");
                return;
            };

            log.LogInfo($"BeginGrabbbing: {___currentlyGrabbingObject.name}");

            var trait = ___currentlyGrabbingObject.GetComponentInParent<RevivableTrait>();

            if (trait != null)
            {
                log.LogInfo($"BeginGrabbbing: has RevivableTrait. obj.name: {___currentlyGrabbingObject.name}");

                var ragdollGrabbableObject = ___currentlyGrabbingObject.GetComponentInParent<RagdollGrabbableObject>();
                var deadBodyInfo = ___currentlyGrabbingObject.GetComponentInParent<DeadBodyInfo>();
                if (ragdollGrabbableObject != null)
                {
                    log.LogInfo("BeginGrabbbing: It is indeed a ragdollGrabbableObject body, dropping");
                    var originalDeadBodyInfo = ragdollGrabbableObject.ragdoll;

                    __instance.SpawnDeadBody(
                            (int)originalDeadBodyInfo.playerScript.playerClientId,
                            Vector3.zero,
                            (int)CauseOfDeath.Mauling,
                            deadBodyInfo.playerScript
                        );

                    //log.LogInfo($"debugging: attachedLimb: {originalDeadBodyInfo.attachedLimb} ||  attachedTo: {originalDeadBodyInfo.attachedTo} ||  attachedTo.parent:{originalDeadBodyInfo.attachedTo.parent} {originalDeadBodyInfo} || attachedTo.parent==base.transform {originalDeadBodyInfo.attachedTo == __instance.deadBody.transform} ");
                    //log.LogInfo($"debugging: lerpBeforeMatchingPosition: {originalDeadBodyInfo.lerpBeforeMatchingPosition} || wasMatchingPosition: {originalDeadBodyInfo.wasMatchingPosition} || matchPositionExactly: {originalDeadBodyInfo.matchPositionExactly}");
                    //log.LogInfo($"debugging: deactivated: {originalDeadBodyInfo.deactivated} || parentedToShip: {originalDeadBodyInfo.parentedToShip}");
                    //log.LogInfo($"debugging: attachedLimb.centerOfMass: {originalDeadBodyInfo.attachedLimb.centerOfMass}, attachedLimb.inertiaTensorRotation: {originalDeadBodyInfo.attachedLimb.inertiaTensorRotation}");

                    // attachedLimb = PlayerRagdoll(Clone)
                    // attachedTo = target ( check spider code )
                    // 

                    // notes of what happens when wasMatchingPosition = false.
                    //attachedLimb.position = attachedTo.position;
                    //attachedLimb.rotation = attachedTo.rotation;
                    //attachedLimb.centerOfMass = Vector3.zero;
                    //attachedLimb.inertiaTensorRotation = Quaternion.identity;


                    // fuck with the body parts as per wasMatchingPosition = false
                    for (int i = 0; i < deadBodyInfo.bodyParts.Length; i++)
                    {
                        deadBodyInfo.bodyParts[i].isKinematic = false;
                        deadBodyInfo.bodyParts[i].WakeUp();
                    }

                    if (deadBodyInfo.attachedLimb != null)
                    {
                        deadBodyInfo.attachedLimb.freezeRotation = false;
                        deadBodyInfo.attachedLimb.isKinematic = false;
                    }


                    // detaching stuff
                    //if (originalDeadBodyInfo.attachedLimb != null)
                    //{
                    //    originalDeadBodyInfo.attachedLimb.isKinematic = false;
                    //    originalDeadBodyInfo.attachedLimb.freezeRotation = false;
                    //}


                    deadBodyInfo.attachedLimb = null;
                    deadBodyInfo.attachedTo = null;

                    // more
                    //originalDeadBodyInfo.secondaryAttachedLimb = null;
                    //originalDeadBodyInfo.secondaryAttachedTo = null;

                    // makes things at least grabbable
                    // originalDeadBodyInfo.wasMatchingPosition = false;


                    // experiments 
                    // originalDeadBodyInfo.lerpBeforeMatchingPosition = false;
                    // ragdollGrabbableObject.EquipItem();
                    //originalDeadBodyInfo.matchPositionExactly = false;
                    //originalDeadBodyInfo.bodyParts[6].transform.
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
            if (Settings.isDebug)
            {
                log.LogInfo("making revivable");
                RevivableTrait revivableTrait = __instance.deadBody.gameObject.AddComponent<RevivableTrait>();
                revivableTrait.setPlayerControllerB(__instance);
                log.LogInfo("trait added");
            }
        }
    }
}
