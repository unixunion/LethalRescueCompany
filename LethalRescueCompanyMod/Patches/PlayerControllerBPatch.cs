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

        [HarmonyPatch("GrabObject")]
        [HarmonyPrefix]
        static void grabHangingBody(ref GrabbableObject ___currentlyGrabbingObject, ref GrabbableObject ___currentlyHeldObject, ref PlayerControllerB __instance, ref GrabbableObject ___currentlyHeldObjectServer)
        {
            if (___currentlyGrabbingObject == null) return;

            log.LogInfo($"BeginGrabbbing: {___currentlyGrabbingObject.name}");

            var trait = ___currentlyGrabbingObject.GetComponentInParent<RevivableTrait>();

            if (trait != null)
            {
                log.LogInfo($"BeginGrabbbing: has RevivableTrait. obj.name: {___currentlyGrabbingObject.name}");

                var ragdollGrabbableObject = ___currentlyGrabbingObject.GetComponentInParent<RagdollGrabbableObject>();
                if (ragdollGrabbableObject != null)
                {
                    log.LogInfo("BeginGrabbbing: It is indeed a ragdollGrabbableObject body, dropping");
                    var originalDeadBodyInfo = ragdollGrabbableObject.ragdoll;

                    originalDeadBodyInfo.attachedLimb = null;
                    originalDeadBodyInfo.attachedTo = null;
                    originalDeadBodyInfo.wasMatchingPosition = false;
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
            if (Settings.debugAddRevive)
            {
                log.LogInfo("making revivable");
                __instance.deadBody.gameObject.AddComponent<RevivableTrait>();
                log.LogInfo("trait added");
            }
        }
    }
}
