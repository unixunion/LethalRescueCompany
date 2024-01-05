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

namespace LethalRescueCompanyPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch : BaseUnityPlugin
    {
        static bool logEnabled = false;
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
                                ref StartOfRound ___playersManager,
                                ref DeadBodyInfo ___deadBody)
        {
            try
            {
                // nope out if not a body
                if (___deadBody == null ) return;
                //if (!___deadBody.isInShip) return;

                if (logEnabled)
                {
                    log.LogInfo(
                        $"name: {___playerUsername}, " +
                        $"isPlayerDead: {___isPlayerDead}, " +
                        $"movementSpeed: {___movementSpeed}, " +
                        $"isMovementHindered: {___isMovementHindered}, " +
                        $"hinderedMultiplier: {___hinderedMultiplier}");
                }

                if(_host == null)
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
                // [Error  :LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch] {"ClassName":"System.NullReferenceException"
                // ,"Message":"Object reference not set to an instance of an object","Data":null,"InnerException":null,"HelpURL"
                // :null,"StackTraceString":"  at LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch.updatePatch (System.
                // Boolean& ___isPlayerDead, System.Single& ___movementSpeed, System.Int32& ___isMovementHindered, System.Single&
                // ___hinderedMultiplier, System.String& ___playerUsername, System.Boolean& ___performingEmote, StartOfRound&
                // ___playersManager, DeadBodyInfo& ___deadBody) [0x0000d] in <1792070e033446bea933494d743d11f4>:0 ",
                // "RemoteStackTraceString":null,"RemoteStackIndex":0,"ExceptionMethod":null,"HResult":-2147467261,
                // "Source":"LethalRescueCompanyMod"}

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
                        if(___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name == "SpooledPlayerMat") { 
                            if (reviveTimer == null)
                            {
                                reviveTimer = new Stopwatch();
                                reviveTimer.Start();
                            }

                            if(reviveTimer.Elapsed.TotalSeconds > 5) { 
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

        private static void ReviveRescuedPlayer(DeadBodyInfo deadbody, StartOfRound playersManager)
        {
            try
            {
                // playerbcontroller.playersManager.
                // look what reset blood objects does

                var ps = deadbody.playerScript;

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
                // look what set player object extrapolate does 

                // transform.GetComponent<Rigidbody>().position
                ps.TeleportPlayer(deadbody.transform.GetComponent<Rigidbody>().position);
                ps.setPositionOfDeadPlayer = false;

                // this is the disable player model 
                SkinnedMeshRenderer[] componentsInChildren = ps.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].enabled = true;
                }
                ps.thisPlayerModelArms.enabled = false;

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
                //playerControllerB.spectatedPlayerScript = null;
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