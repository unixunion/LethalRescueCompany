using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using BepInEx;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.NetworkBehaviors;
using UnityEngine;
using System.Linq;
using LethalRescueCompanyMod.Hacks;

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

        /// <summary>
        /// Lets do the swap early of the body if its what we want. 
        /// </summary>
        /// <param name="___gameplayCamera"></param>
        /// <param name="__instance"></param>
        [HarmonyPatch("BeginGrabObject")]
        [HarmonyPrefix]
        static void ReplaceObjectWithSurrogate(ref Camera ___gameplayCamera, ref PlayerControllerB __instance)
        {
            Ray interactRay = new Ray(___gameplayCamera.transform.position, ___gameplayCamera.transform.forward);
            Physics.Raycast(interactRay, out var hit, __instance.grabDistance, 832);

            if (hit.collider == null) return;
            log.LogInfo($"ray hit: {hit}");
            var testing = hit.collider.transform.gameObject.GetComponentInParent<DeadBodyInfo>();
            var currentlyGrabbingObject = hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
            if (testing != null && testing.attachedTo != null)
            {
                Rigidbody? rigidbody = testing.GetComponent<Rigidbody>();
                Rigidbody? anotherOne = testing.GetComponentInParent<Rigidbody>();
                Rigidbody? pinky = testing.GetComponentInChildren<Rigidbody>();
                log.LogInfo($"RIGADOODLE: {rigidbody} : {anotherOne} : {pinky}");
                testing.attachedLimb = null;
                testing.attachedTo = null;
                if (rigidbody != null)
                {
                    log.LogInfo($"RAZZLE DAZZLE");
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                }
            }
        }

        [HarmonyPatch("BeginGrabObject")]
        [HarmonyPostfix]
        static void ReplaceObjectWithSurrogatePost(ref Camera ___gameplayCamera, ref PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObject == null) return;
            var grabbableComponent = __instance.currentlyHeldObject.GetComponent<GrabbableObject>();
            if (grabbableComponent == null) return;
            var rigidBody = grabbableComponent.GetComponent<Rigidbody>();
            if (rigidBody != null && !rigidBody.isKinematic)
            {
                log.LogInfo("DETECTED Rigid Body that should drop like a mf");
                __instance.currentlyHeldObject.DiscardItemOnClient();
                __instance.currentlyHeldObjectServer.DiscardItemOnClient();
                rigidBody.isKinematic = true;
            }
        }

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
