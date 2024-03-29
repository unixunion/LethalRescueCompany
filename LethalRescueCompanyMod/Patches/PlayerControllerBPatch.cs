﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using BepInEx;
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

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void startPatch(
           ref PlayerControllerB __instance,
           ref StartOfRound ___playersManager,
           ref DeadBodyInfo ___deadBody)
        {
        }

        //[HarmonyPatch("BeginGrabObject")]
        //[HarmonyPrefix]
        //static void BGO(ref Camera ___gameplayCamera, ref PlayerControllerB __instance)
        //{
        //    Ray interactRay = new Ray(___gameplayCamera.transform.position, ___gameplayCamera.transform.forward);
        //    Physics.Raycast(interactRay, out var hit, __instance.grabDistance, 832);
        //    log.LogInfo($"ray hit: {hit}");
        //    //var currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<LRCGrabbableObject>();
        //    var currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();

        //    log.LogInfo($"layer1: {hit.collider.transform.gameObject.layer}");
        //    log.LogInfo($"layer2: {currentlyGrabbingObject.gameObject.layer}");

        //    if (currentlyGrabbingObject != null)
        //    {
        //        log.LogInfo($"grabbing hack: {currentlyGrabbingObject.GetComponent<GrabbableObject>()}");
        //    } else
        //    {
        //        log.LogInfo($"object does not contain LRCGrabbableObject ");
        //    }
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
            
            AddWelcomeMessage(___playersManager);
            //AddPingPong(___playersManager);

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

        //private static void AddPingPong(StartOfRound playersManager)
        //{
        //    if (playersManager != null)
        //    {
        //        foreach (var item in playersManager.allPlayerScripts)
        //        {
        //            if (item.gameObject.GetComponent<RescueCompanyController>() == null) item.gameObject.AddComponent<RescueCompanyController>();
        //        }
        //    }
        //}

        private static void AddWelcomeMessage(StartOfRound playersManager)
        {
            if (playersManager != null)
            {
                foreach (var item in playersManager.allPlayerScripts)
                {
                    if (item.gameObject.GetComponent<WelcomeMessage>() == null) item.gameObject.AddComponent<WelcomeMessage>();
                }
            }
        }

        [HarmonyPatch("GrabObject")]
        [HarmonyPostfix]
        static void grabHangingBody(ref GrabbableObject ___currentlyGrabbingObject, ref GrabbableObject ___currentlyHeldObject, ref PlayerControllerB __instance)
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
                    var db = ragdollGrabbableObject.ragdoll;
                    __instance.SpawnDeadBody(db.playerObjectId, db.transform.position, 0, db.playerScript);
                    Destroy(___currentlyGrabbingObject);

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
