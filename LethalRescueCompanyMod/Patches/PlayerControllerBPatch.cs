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
        static bool logEnabled = false;
        static bool isDebug = true;
        static bool spawnedSpider = false;

        static Stopwatch reviveTimer;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch");
        static PlayerControllerB _host = null;
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updatePatch(ref bool ___isPlayerDead,
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
                DebugHacks(___performingEmote, ___thisPlayerBody);

                // nope out if not a body
                if (___deadBody == null) return;
                if (___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name == "SpooledPlayerMat" && !___deadBody.grabBodyObject.grabbable)
                {
                    log.LogInfo("Making wrapped body grabbable");
                    ___deadBody.grabBodyObject.grabbable = true;
                }
                if (!___deadBody.isInShip) return;

                if (logEnabled)
                {
                    log.LogInfo(
                        $"name: {___playerUsername}, " +
                        $"isPlayerDead: {___isPlayerDead}, " +
                        $"movementSpeed: {___movementSpeed}, " +
                        $"isMovementHindered: {___isMovementHindered}, " +
                        $"hinderedMultiplier: {___hinderedMultiplier}");
                }

                if (_host == null)
                {
                    ___playersManager.allPlayerScripts.ToList().ForEach(p =>
                    {
                        if (p.IsOwner)
                        {
                            _host = p;
                        }
                    });
                }

                if (___playersManager == null)
                {
                    log.LogError($"playersManager is null");
                    return;
                }

                //&& ___playerUsername == "marzubus"
                if (___isPlayerDead)
                {
                    log.LogInfo($"db.isInShip: {___deadBody.isInShip}, " +
                                $"db.sharedMaterial.name: {___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name}, " +
                                $"db.sharedMesh.name: {___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.name}, " +
                                $"db.causeOfDeath: {___deadBody.causeOfDeath}, " +
                                $"db.canBeGrabbedBackByPlayers: {___deadBody.grabBodyObject.grabbable}");


                    if (___deadBody.isInShip && ___deadBody.grabBodyObject.grabbable)
                    {

                        //:LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch] deadBody.isInShip: False sharedMaterial.name: SpooledPlayerMat, sharedMesh.name: Circle
                        if (___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name == "SpooledPlayerMat")
                        {
                            if (reviveTimer == null)
                            {
                                reviveTimer = new Stopwatch();
                                reviveTimer.Start();
                            }

                            if (reviveTimer.Elapsed.TotalSeconds > 5)
                            {
                                ReviveRescuedPlayer(___deadBody, ___playersManager);
                                reviveTimer = null;
                            }
                        }
                    }

                    // cacoon 
                    //___deadBody.ChangeMesh()
                }
            }
            catch (Exception ex)
            {
                log.LogError(JsonConvert.SerializeObject(ex));
            }
        }


        private static GameObject spawnedSpiderEnemy = null;
        private static Stopwatch cooldown = null;
        private static void DebugHacks(bool performingEmote, Transform thisPlayerBody)
        {

            if (performingEmote && !spawnedSpider)
            {

                EnemyType enemyType = null;
                if (enemyType == null)
                {
                    RoundManager.Instance.currentLevel.Enemies.ForEach(enemy =>
                    {
                        log.LogInfo(enemy.enemyType.enemyPrefab.name);
                        if (enemy.enemyType.enemyPrefab.name.ToLower().Contains("spider"))
                        {
                            enemyType = enemy.enemyType;
                        }
                    });

                    //RoundManager.Instance.SpawnedEnemies.ForEach(x =>
                    //{
                    //    log.LogInfo(x.enemyType.enemyPrefab.name);
                    //    if (x.enemyType.enemyPrefab.name.ToLower().Contains("spider"))
                    //    {
                    //        enemyType = x.enemyType;
                    //    }

                    //});
                }


                if (spawnedSpider)
                {
                    if (cooldown != null)
                    {
                        if (cooldown.Elapsed.TotalSeconds > 5)
                        {
                            Destroy(spawnedSpiderEnemy);
                            enemyType = null;
                            spawnedSpider = false;
                            cooldown.Stop();
                            cooldown = null;
                        }
                    }
                }

                if (enemyType != null && spawnedSpiderEnemy == null)
                {
                    log.LogInfo($"Spawning spider at: {thisPlayerBody.position}");
                    GameObject thespider = UnityEngine.Object.Instantiate(enemyType.enemyPrefab, thisPlayerBody.position, Quaternion.Euler(new Vector3(0f, 0, 0f)));
                    thespider.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                    RoundManager.Instance.SpawnedEnemies.Add(thespider.GetComponent<EnemyAI>());
                    spawnedSpiderEnemy = thespider;
                    spawnedSpider = true;
                    if (cooldown == null)
                    {
                        cooldown = new Stopwatch();
                        cooldown.Start();
                    }

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