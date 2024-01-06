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
        static Helper reviveHelper = new Helper();
        static bool isDebug = Settings.isDebug;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch");

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void udpatePatch(ref bool ___isPlayerDead,
                               ref float ___movementSpeed,
                               ref int ___isMovementHindered,
                               ref float ___hinderedMultiplier,
                               ref string ___playerUsername,
                               ref bool ___performingEmote,
                               ref Transform ___thisPlayerBody,
                               ref StartOfRound ___playersManager,
                               ref DeadBodyInfo ___deadBody)
        {
            // nope out if not a body
            if (___deadBody == null) return;

            // fix to make spider hung bodies grabbable.
            // todo fixme, need to grab it, drop it and grab it again to release it.
            // check if body is wrapped in a spider web material,
            if (___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name == "SpooledPlayerMat" && !___deadBody.grabBodyObject.grabbable && ___deadBody.attachedTo != null)
            {
                if (___deadBody.attachedTo.name != "MouthTarget")
                {
                    log.LogInfo($"Making wrapped body grabbable, currently attached to: {___deadBody.attachedTo.name}");
                    ___deadBody.grabBodyObject.grabbable = true;
                    ___deadBody.canBeGrabbedBackByPlayers = true;

                }
            }
        }


        //static PlayerControllerB _host = null;
        [HarmonyPatch("Emote2_performed")]
        [HarmonyPostfix]
        static void emote1performedPatch(ref bool ___isPlayerDead,
                               ref float ___movementSpeed,
                               ref int ___isMovementHindered,
                               ref float ___hinderedMultiplier,
                               ref string ___playerUsername,
                               ref bool ___performingEmote,
                               ref Transform ___thisPlayerBody,
                               ref StartOfRound ___playersManager,
                               ref DeadBodyInfo ___deadBody)
        {

        }


        [HarmonyPatch("DropItemAheadOfPlayer")]
        [HarmonyPostfix]
        static void dropItemAheadOfPlayerPatch(ref bool ___isPlayerDead,
                                ref StartOfRound ___playersManager,
                                ref DeadBodyInfo ___deadBody)
        {
            try
            {
                // ignore dead bodies not in the ship
                if (!___deadBody.isInShip) return;

                // GetHost(___playersManager);

                if (___playersManager == null)
                {
                    log.LogError($"playersManager is null");
                    return;
                }

                //&& ___playerUsername == "marzubus"
                if (___isPlayerDead)
                {
                    if (isDebug)
                    {
                        log.LogInfo($"db.isInShip: {___deadBody.isInShip}, " +
                                    $"db.sharedMaterial.name: {___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name}, " +
                                    $"db.sharedMesh.name: {___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.name}, " +
                                    $"db.causeOfDeath: {___deadBody.causeOfDeath}, " +
                                    $"db.canBeGrabbedBackByPlayers: {___deadBody.grabBodyObject.grabbable}");
                    }


                    // detect if its dropped
                    if (___deadBody.isInShip && ___deadBody.grabBodyObject.grabbable)
                    {
                        //log.LogInfo("body dropped in ship, checking if its warpped in a web");
                        if (___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name == "SpooledPlayerMat")
                        {

                            reviveHelper.startCoroutine(___deadBody, ___playersManager);
                            //log.LogInfo("webbed body dropped in ship");
                            //if (reviveTimer == null)
                            //{
                            //    //log.LogInfo("starting revive timer");
                            //    reviveTimer = new Stopwatch();
                            //    reviveTimer.Start();
                            //}

                            //if (reviveTimer.Elapsed.TotalSeconds > 5)
                            //{
                            //    //log.LogInfo("revive timer elapsed, reviving player");
                            //    ReviveRescuedPlayer(___deadBody, ___playersManager);
                            //    reviveTimer = null;
                            //}
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.LogError(JsonConvert.SerializeObject(ex));
            }
        }


       

        public static void KillPlayer(PlayerControllerB player, Vector3 bodyVelocity, bool spawnBody = true, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, int deathAnimation = 0)
        {

            if (player.IsOwner && !player.isPlayerDead && player.AllowPlayerDeath())
            {
                player.isPlayerDead = true;
                player.isPlayerControlled = false;
                player.thisPlayerModelArms.enabled = false;
                player.localVisor.position = player.playersManager.notSpawnedPosition.position;
                player.DisablePlayerModel(player.gameObject);
                player.isInsideFactory = false;
                player.IsInspectingItem = false;
                player.inTerminalMenu = false;
                player.twoHanded = false;
                player.carryWeight = 1f;
                player.fallValue = 0f;
                player.fallValueUncapped = 0f;
                player.takingFallDamage = false;
                player.isSinking = false;
                player.isUnderwater = false;
                StartOfRound.Instance.drowningTimer = 1f;
                HUDManager.Instance.setUnderwaterFilter = false;
                player.sourcesCausingSinking = 0;
                player.sinkingValue = 0f;
                player.hinderedMultiplier = 1f;
                player.isMovementHindered = 0;
                player.inAnimationWithEnemy = null;
                UnityEngine.Object.FindObjectOfType<Terminal>().terminalInUse = false;
                SoundManager.Instance.SetDiageticMixerSnapshot();
                HUDManager.Instance.SetNearDepthOfFieldEnabled(enabled: true);
                HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", value: false);
                HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
                HUDManager.Instance.HideHUD(hide: true);
                if (spawnBody)
                {
                    player.SpawnDeadBody((int)player.playerClientId, bodyVelocity, (int)causeOfDeath, player, deathAnimation);
                }
                StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);
                player.isInGameOverAnimation = 1.5f;
                player.cursorTip.text = "";
                player.cursorIcon.enabled = false;
                player.DropAllHeldItems(spawnBody);
                player.DisableJetpackControlsLocally();
            }
        }
    }
}