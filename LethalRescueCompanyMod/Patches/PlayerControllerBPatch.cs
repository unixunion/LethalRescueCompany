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
            ref StartOfRound ___playersManager,
            ref DeadBodyInfo ___deadBody)
        {

            // nope out if not a body
            if (___deadBody == null) return;

            playerIsDeadAndWebbedInShipCheck(___deadBody, ___playersManager);
        }


        private static void playerIsDeadAndWebbedInShipCheck(DeadBodyInfo deadBodyInfo, StartOfRound playersManager)
        {
            try
            {
                // ignore dead bodies not in the ship
                if (!deadBodyInfo.isInShip) return;


                if (playersManager == null)
                {
                    log.LogError($"playersManager is null");
                    return;
                }

                if (isDebug)
                {
                    log.LogInfo($"db.isInShip: {deadBodyInfo.isInShip}, " +    
                                $"db....grabbable: {deadBodyInfo.grabBodyObject.grabbable}, " + 
                                $"db....hasHitGround: {deadBodyInfo.grabBodyObject.hasHitGround}, " +
                                $"db....velocity.mag: {deadBodyInfo.bodyParts[0].velocity.magnitude}, " +
                                $"db<ReviveTrait>: {deadBodyInfo.GetComponent<RevivableTrait>()}");
                }

                // detect if its dropped deadBodyInfo.grabBodyObject.hasHitGround 
                if (deadBodyInfo.grabBodyObject.grabbable && deadBodyInfo.bodyParts[0].velocity.magnitude<0.02) // might be 6
                {


                    //log.LogInfo("body dropped in ship, checking if its warpped in a web");
                    if (Settings.isDebug) log.LogInfo("checking dropped body has revivable trait");
                    if (deadBodyInfo.gameObject.GetComponentInChildren<RevivableTrait>() != null)
                    {
                        if (Settings.isDebug) log.LogInfo("trait found, reviving");
                        //RescueCompany.instance.RevivePlayer(deadBodyInfo.playerScript);
                        //deadBodyInfo.playerScript.HealClientRpc();
                        helper.ReviveRescuedPlayer(deadBodyInfo, playersManager);

                    } else if (Settings.isDebug)
                    {
                        log.LogInfo("isDebug=true, unconditional respawn drop of dead body in ship");
                        // RescueCompany.instance.RevivePlayer(deadBodyInfo.playerScript);
                        //deadBodyInfo.playerScript.HealClientRpc();
                        helper.ReviveRescuedPlayer(deadBodyInfo, playersManager);
                    }
                } else
                {
                    if (Settings.isDebug) log.LogInfo("waiting for body to drop, be grabbable and not move");
                }


            }
            catch (Exception ex)
            {
                log.LogError(JsonConvert.SerializeObject(ex));
            }
        }

        //public static void KillPlayer(PlayerControllerB player, Vector3 bodyVelocity, bool spawnBody = true, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, int deathAnimation = 0)
        //{

        //    if (player.IsOwner && !player.isPlayerDead && player.AllowPlayerDeath())
        //    {
        //        player.isPlayerDead = true;
        //        player.isPlayerControlled = false;
        //        player.thisPlayerModelArms.enabled = false;
        //        player.localVisor.position = player.playersManager.notSpawnedPosition.position;
        //        player.DisablePlayerModel(player.gameObject);
        //        player.isInsideFactory = false;
        //        player.IsInspectingItem = false;
        //        player.inTerminalMenu = false;
        //        player.twoHanded = false;
        //        player.carryWeight = 1f;
        //        player.fallValue = 0f;
        //        player.fallValueUncapped = 0f;
        //        player.takingFallDamage = false;
        //        player.isSinking = false;
        //        player.isUnderwater = false;
        //        StartOfRound.Instance.drowningTimer = 1f;
        //        HUDManager.Instance.setUnderwaterFilter = false;
        //        player.sourcesCausingSinking = 0;
        //        player.sinkingValue = 0f;
        //        player.hinderedMultiplier = 1f;
        //        player.isMovementHindered = 0;
        //        player.inAnimationWithEnemy = null;
        //        UnityEngine.Object.FindObjectOfType<Terminal>().terminalInUse = false;
        //        SoundManager.Instance.SetDiageticMixerSnapshot();
        //        HUDManager.Instance.SetNearDepthOfFieldEnabled(enabled: true);
        //        HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", value: false);
        //        HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
        //        HUDManager.Instance.HideHUD(hide: true);
        //        if (spawnBody)
        //        {
        //            player.SpawnDeadBody((int)player.playerClientId, bodyVelocity, (int)causeOfDeath, player, deathAnimation);
        //        }
        //        StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);
        //        player.isInGameOverAnimation = 1.5f;
        //        player.cursorTip.text = "";
        //        player.cursorIcon.enabled = false;
        //        player.DropAllHeldItems(spawnBody);
        //        player.DisableJetpackControlsLocally();
        //    }
        //}
    }

    

}