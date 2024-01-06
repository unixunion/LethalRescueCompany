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

        static bool isDebug = true;
        static bool spawnedSpider = false;

        static Stopwatch reviveTimer;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch");
        static PlayerControllerB _host = null;
        [HarmonyPatch("Emote1_performed")]
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
            // spider debugging stuff
            if (isDebug)
            {
                DebugHacks(___performingEmote, ___thisPlayerBody);
            }
        }


        [HarmonyPatch("DropItemAheadOfPlayer")]
        [HarmonyPostfix]
        static void dropItemAheadOfPlayerPatch(ref bool ___isPlayerDead,
                                ref float ___movementSpeed,
                                ref int ___isMovementHindered,
                                ref float ___hinderedMultiplier,
                                ref string ___playerUsername,
                                ref bool ___performingEmote,
                                ref Transform ___thisPlayerBody,
                                ref StartOfRound ___playersManager,
                                ref DeadBodyInfo ___deadBody)
        {
            try
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

                // ignore dead bodies not in the ship
                if (!___deadBody.isInShip) return;

                if (isDebug)
                {
                    log.LogInfo(
                        $"name: {___playerUsername}, " +
                        $"isPlayerDead: {___isPlayerDead}, " +
                        $"movementSpeed: {___movementSpeed}, " +
                        $"isMovementHindered: {___isMovementHindered}, " +
                        $"hinderedMultiplier: {___hinderedMultiplier}");
                }

                GetHost(___playersManager);

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
                            //log.LogInfo("webbed body dropped in ship");
                            if (reviveTimer == null)
                            {
                                //log.LogInfo("starting revive timer");
                                reviveTimer = new Stopwatch();
                                reviveTimer.Start();
                            }

                            if (reviveTimer.Elapsed.TotalSeconds > 5)
                            {
                                //log.LogInfo("revive timer elapsed, reviving player");
                                ReviveRescuedPlayer(___deadBody, ___playersManager);
                                reviveTimer = null;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.LogError(JsonConvert.SerializeObject(ex));
            }
        }


        private static List<EnemyAI> spawnedSpiders = null;
        private static EnemyType spiderEnemyType = null;
        private static Stopwatch cooldown = null;
        private static bool hasKilledSpiders = false;
        private static void DebugHacks(bool performingEmote, Transform thisPlayerBody)
        {
            List<EnemyAI> fuckingSpiders = null;
            if (RoundManager.Instance.SpawnedEnemies.Count > 0)
            {
                try
                {
                    fuckingSpiders = RoundManager.Instance.SpawnedEnemies.Where(x => x.enemyType.enemyPrefab.name.ToLower().Contains("spider")).ToList();
                    if(fuckingSpiders!=null && fuckingSpiders.Count > 0)
                    {
                        spawnedSpiders = fuckingSpiders;
                    }
                }
                catch { };
            }
            if (performingEmote && !spawnedSpider)
            {
                if (spiderEnemyType == null)
                {
                    RoundManager.Instance.currentLevel.Enemies.ForEach(enemy =>
                    {
                        //log.LogInfo(enemy.enemyType.enemyPrefab.name);
                        if (enemy.enemyType.enemyPrefab.name.ToLower().Contains("spider"))
                        {
                            spiderEnemyType = enemy.enemyType;
                        }
                    });
                }

                if (spiderEnemyType != null && spawnedSpiders == null)
                {
                    //log.LogInfo($"Spawning spider at: {thisPlayerBody.position}");
                    var n = RoundManager.Instance.SpawnEnemyGameObject(thisPlayerBody.position, 0, 99, spiderEnemyType);
                    //RoundManager.Instance.SpawnedEnemies.Add(spiderEnemyType.enemyPrefab.GetComponent<EnemyAI>());
                    spawnedSpiders = fuckingSpiders;
                    spawnedSpider = true;
                    if (cooldown == null)
                    {
                        cooldown = new Stopwatch();
                        cooldown.Start();
                    }
                }
            }
            else if (performingEmote)
            {
                if (spawnedSpider && fuckingSpiders != null)
                {
                    if (cooldown != null)
                    {
                        if (cooldown.Elapsed.TotalSeconds > 5)
                        {
                            if (fuckingSpiders.Count > 0 && !hasKilledSpiders)
                            {
                                fuckingSpiders.ForEach(spider => Destroy(spider.gameObject));
                                fuckingSpiders.ForEach(spider => Destroy(spider));
                                RoundManager.Instance.SpawnedEnemies.RemoveAll(_ => true);
                                hasKilledSpiders = true;
                            }
                        }
                    }
                }


                if (cooldown != null && cooldown.Elapsed.TotalSeconds > 10 && hasKilledSpiders)
                {
                    cooldown.Stop();
                    cooldown = null;
                    spiderEnemyType = null;
                    spawnedSpider = false;
                    spawnedSpiders = null;
                    hasKilledSpiders = false;
                }
            }
        }

        private static void ReviveRescuedPlayer(DeadBodyInfo deadbody, StartOfRound playersManager)
        {
            try
            {

                // get the PlayerControllerB from the deadbody
                var ps = deadbody.playerScript;

                // this is stolen from the spawn logic
                ps.isClimbingLadder = false;
                ps.ResetZAndXRotation();
                ps.thisController.enabled = true;
                ps.health = 40;
                ps.disableLookInput = false;
                ps.isPlayerDead = false;
                ps.isPlayerControlled = true;
                ps.isInElevator = true;
                ps.isInHangarShipRoom = true;
                ps.isInsideFactory = false;
                ps.wasInElevatorLastFrame = false;
                ps.carryWeight = 1f;
                ps.isFreeCamera = false;
                ps.playerHudUIContainer.gameObject.SetActive(value: true);
                ps.TeleportPlayer(deadbody.transform.GetComponent<Rigidbody>().position);
                ps.setPositionOfDeadPlayer = false;

                // this is the disable player model 
                SkinnedMeshRenderer[] componentsInChildren = ps.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].enabled = true;
                }
                ps.thisPlayerModelArms.enabled = false; // not sure if this is fix for goro arms

                // more init stuff
                ps.helmetLight.enabled = false;
                ps.Crouch(crouch: false);
                ps.playerBodyAnimator.SetBool("Limp", value: false);
                ps.bleedingHeavily = false;
                ps.activatingItem = false;
                ps.twoHanded = false;
                ps.inSpecialInteractAnimation = false;
                ps.holdingWalkieTalkie = false;
                ps.speakingToWalkieTalkie = false;
                ps.isSinking = false;
                ps.isUnderwater = false;
                ps.sinkingValue = 0f;
                ps.statusEffectAudio.Stop();
                ps.DisableJetpackControlsLocally();
                ps.movementSpeed = 4.6f;
                ps.mapRadarDotAnimator.SetBool("dead", value: false);


                HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
                ps.hasBegunSpectating = false;
                HUDManager.Instance.RemoveSpectateUI();


                HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                ps.hinderedMultiplier = 1f;
                ps.isMovementHindered = 0;
                ps.sourcesCausingSinking = 0;

                SoundManager.Instance.earsRingingTimer = 0f;
                ps.voiceMuffledByEnemy = false;
                ps.spectatedPlayerScript = null;
                ps.MakeCriticallyInjured(false);

                HUDManager.Instance.UpdateHealthUI(40, hurtPlayer: false);
                HUDManager.Instance.audioListenerLowPass.enabled = false;
                playersManager.livingPlayers = playersManager.livingPlayers + 1;
                HUDManager.Instance.HideHUD(hide: false);

                // destroy deadbody
                Destroy(deadbody.gameObject);
            }
            catch (Exception ex)
            {
                log.LogError($"Error in ReviveRescuedPlayer: {ex.Message}");
            }
        }


        public static void GetHost(StartOfRound startOfRound)
        {
            if (_host == null)
            {
                startOfRound.allPlayerScripts.ToList().ForEach(p =>
                {
                    if (p.IsOwner)
                    {
                        _host = p;
                    }
                });
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