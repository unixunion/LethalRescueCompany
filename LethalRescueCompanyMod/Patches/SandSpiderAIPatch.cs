﻿using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace LethalRescueCompanyMod.Patches;

[HarmonyPatch(typeof(SandSpiderAI))]
internal class SandSpiderAIPatch : BaseUnityPlugin
{
    internal static ManualLogSource log =
        BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.SandSpiderAIPatch");

    private static readonly Helper helper = new();
    private static DeadBodyInfo currentlyHeldBody;


    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void ExtractFields(ref SandSpiderAI __instance)
    {
        Settings.HangingBodyPrefab = __instance.hangBodyPhysicsPrefab;
    }


    [HarmonyPatch("GrabBody")]
    [HarmonyPrefix]
    private static void testDebug(ref PlayerControllerB ___targetPlayer)
    {
        //log.LogInfo($"DEBUGGY DEBUGGY :: {___targetPlayer.deadBody.bodyParts[6]} || {___targetPlayer.deadBody.attachedLimb}");
    }

    //[HarmonyPatch("GrabBody")]
    //[HarmonyPostfix]
    //static void thisisMyHorse(ref PlayerControllerB ___targetPlayer)
    //{
    //    log.LogInfo($"My horse is amazing :: {___targetPlayer.deadBody.bodyParts[6]} || {___targetPlayer.deadBody.attachedLimb}");
    //}

    [HarmonyPatch("HangBodyFromCeiling")]
    [HarmonyPrefix]
    private static void hangBodyFromCeilingPrePatch(ref DeadBodyInfo ___currentlyHeldBody)
    {
        log.LogInfo($"Getting currentlyHeldBody: {___currentlyHeldBody}");
        currentlyHeldBody = ___currentlyHeldBody;
        if (currentlyHeldBody == null) return;
        for (var i = 0; i < currentlyHeldBody.bodyBloodDecals.Length; i++)
            currentlyHeldBody.bodyBloodDecals[i].SetActive(false);
    }

    [HarmonyPatch("HangBodyFromCeiling")]
    [HarmonyPostfix]
    private static void hangBodyFromCeilingPostPatch(ref SandSpiderAI __instance)
    {
        if (currentlyHeldBody == null) return;
        makeWebbedBodyGrabbable(currentlyHeldBody);

        // if debug / solo
        if (Settings.IsDebug && Settings.IsSolo)
        {
            log.LogInfo("debug and solo, spider hacks to revive player");
            // spawn the player
            //helper.ReviveRescuedPlayer(currentlyHeldBody, StartOfRound.Instance);
            helper.ReviveRescuedPlayer(currentlyHeldBody.playerScript, currentlyHeldBody.transform.position, false);

            RoundManager.Instance.currentLevel.Enemies.Clear();
            RoundManager.Instance.currentLevel.OutsideEnemies.Clear();
            RoundManager.Instance.currentLevel.DaytimeEnemies.Clear();

            log.LogInfo("Destroying Self");
            Destroy(__instance.gameObject);
            Destroy(__instance);
        }
    }

    private static void makeWebbedBodyGrabbable(DeadBodyInfo deadBodyInfo)
    {
        if (Settings.IsDebug)
            log.LogInfo($"Making wrapped body grabbable, currently attached to: {deadBodyInfo.attachedTo.name}");

        //deadBodyInfo.secondaryAttachedTo = deadBodyInfo.attachedTo;
        //deadBodyInfo.attachedTo = null;
        //if (Settings.isDebug) log.LogInfo($"reattached via secondary to the primary attachment point");

        deadBodyInfo.canBeGrabbedBackByPlayers = true;

        // according to code, this is not needed, as the above line overrides it in the late update.
        //deadBodyInfo.grabBodyObject.grabbable = true;


        //var t = deadBodyInfo.GetComponent<RagdollGrabbableObject>();
        //if (t != null) t.grabbable = true;


        if (deadBodyInfo.gameObject != null)
        {
            if (deadBodyInfo.gameObject.GetComponent<RevivableTrait>() == null)
            {
                if (Settings.IsDebug) log.LogInfo("adding revivable trait");
                deadBodyInfo.gameObject.AddComponent<RevivableTrait>();
            }
            else
            {
                if (Settings.IsDebug) log.LogInfo("trait already present");
            }

            // configure revivable TEMPORARY DISABLE DEBUG
            var revivableTrait = deadBodyInfo.gameObject.GetComponent<RevivableTrait>();
            //var testrd = deadBodyInfo.GetComponentInParent<RagdollGrabbableObject>();
            //log.LogInfo($"parent ragdoll: {testrd}");
            //var ragdollGrabbableObject = deadBodyInfo.gameObject.AddComponent<LRCGrabbableObject>();
            //if (ragdollGrabbableObject == null) log.LogWarning("unable to find grabbable, fix this!");
            revivableTrait.setGrabbable(deadBodyInfo.grabBodyObject);
            revivableTrait.setPlayerControllerB(deadBodyInfo.playerScript);

            log.LogInfo("Dropping the body");
            deadBodyInfo.attachedTo = null;
        }
        else
        {
            log.LogError("deadbody gameobject was null, error making it revivable");
        }

        if (Settings.IsDebug)
        {
            log.LogInfo("lets inspect...");
            log.LogInfo($"db....grabbable: {deadBodyInfo.grabBodyObject.grabbable}, " +
                        $"db.canBeGrabbedBackByPlayers: {deadBodyInfo.canBeGrabbedBackByPlayers}, " +
                        $"db....hasHitGround: {deadBodyInfo.grabBodyObject.hasHitGround}, " +
                        $"db<ReviveTrait>: {deadBodyInfo.gameObject.GetComponent<RevivableTrait>()}");
        }
    }
}