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

namespace LethalRescueCompanyPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch : BaseUnityPlugin
    {
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch");


        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updatePatch(ref bool ___isPlayerDead,
                                ref float ___movementSpeed,
                                ref int ___isMovementHindered,
                                ref float ___hinderedMultiplier,
                                ref string ___playerUsername,
                                ref DeadBodyInfo ___deadBody)
        {
            log.LogInfo(
                $"name: {___playerUsername}, " +
                $"isPlayerDead: {___isPlayerDead}, " +
                $"movementSpeed: {___movementSpeed}, " +
                $"isMovementHindered: {___isMovementHindered}," +
                $"hinderedMultiplier: {___hinderedMultiplier}");

            // nope out if not a body
            if (___deadBody == null) return;



            //&& ___playerUsername == "marzubus"
            if (___isPlayerDead)
            {

                log.LogInfo($"deadBody.isInShip: {___deadBody.isInShip} sharedMaterial.name: {___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name}, sharedMesh.name: {___deadBody.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.name}");


                if (___deadBody.isInShip && ___deadBody.grabBodyObject.grabbable)
                {
                    // playerbcontroller.playersManager.


                    // look what reset blood objects does

                    var ps = ___deadBody.playerScript;



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
                    
                    // look what set player object extrapolate does 

                    // transform.GetComponent<Rigidbody>().position
                    ps.TeleportPlayer(___deadBody.transform.GetComponent<Rigidbody>().position);
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

                    // playerB controller mapping stuff?
                    //PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
                    //playerControllerB.bleedingHeavily = false;
                    //playerControllerB.criticallyInjured = false;
                    //playerControllerB.playerBodyAnimator.SetBool("Limp", value: false);
                    //playerControllerB.health = 100;
                    //playerControllerB.movementSpeed = 0.5f;
                    //playerControllerB.hinderedMultiplier = 1f;


                    HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
                    //playerControllerB.spectatedPlayerScript = null;
                    HUDManager.Instance.audioListenerLowPass.enabled = false;


                    //RagdollGrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>();
                    //for (int j = 0; j < array.Length; j++)
                    //{
                    //    if (!array[j].isHeld)
                    //    {
                    //        if (ps.NetworkManager.IsServer)
                    //        {
                    //            if (array[j].NetworkObject.IsSpawned)
                    //            {
                    //                array[j].NetworkObject.Despawn();
                    //            }
                    //            else
                    //            {
                    //                UnityEngine.Object.Destroy(array[j].gameObject);
                    //            }
                    //        }
                    //    }
                    //    else if (array[j].isHeld && array[j].playerHeldBy != null)
                    //    {
                    //        array[j].playerHeldBy.DropAllHeldItems();
                    //    }
                    //}


                    // destroy deadbody
                    //UnityEngine.Object.Destroy(array2[k].gameObject);
                    Destroy(___deadBody.gameObject);


                }

                // cacoon 
                //___deadBody.ChangeMesh()
            }


        }



        public void UpdatePlayerVoiceEffects(PlayerControllerB player)
        {



            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }
            // updatePlayerVoiceInterval = 2f;

            PlayerControllerB playerControllerB = ((!GameNetworkManager.Instance.localPlayerController.isPlayerDead || !(GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null)) ? GameNetworkManager.Instance.localPlayerController : GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript);

            PlayerControllerB playerControllerB2 = player;
            if ((!playerControllerB2.isPlayerControlled && !playerControllerB2.isPlayerDead) || playerControllerB2 == GameNetworkManager.Instance.localPlayerController)
            {
                return;
            }
            if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatIngameSettings._playerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
            {
                RefreshPlayerVoicePlaybackObjects(player);
                if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                {
                    log.LogDebug($"Was not able to access voice chat object for player; {playerControllerB2.voicePlayerState == null}; {playerControllerB2.currentVoiceChatAudioSource == null}");
                    return;
                }
            }
            AudioSource currentVoiceChatAudioSource = player.currentVoiceChatAudioSource;
            bool flag = playerControllerB2.speakingToWalkieTalkie && playerControllerB.holdingWalkieTalkie && playerControllerB2 != playerControllerB;
            if (playerControllerB2.isPlayerDead)
            {
                currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().enabled = false;
                currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = false;
                currentVoiceChatAudioSource.panStereo = 0f;
                SoundManager.Instance.playerVoicePitchTargets[playerControllerB2.playerClientId] = 1f;
                SoundManager.Instance.SetPlayerPitch(1f, (int)playerControllerB2.playerClientId);
                if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    currentVoiceChatAudioSource.spatialBlend = 0f;
                    playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                    playerControllerB2.voicePlayerState.Volume = 1f;
                }
                else
                {
                    currentVoiceChatAudioSource.spatialBlend = 1f;
                    playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                    playerControllerB2.voicePlayerState.Volume = 0f;
                }
                return;
            }
            AudioLowPassFilter component = currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>();
            OccludeAudio component2 = currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
            component.enabled = true;
            component2.overridingLowPass = flag || player.voiceMuffledByEnemy;
            currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = flag;
            if (!flag)
            {
                currentVoiceChatAudioSource.spatialBlend = 1f;
                playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                currentVoiceChatAudioSource.bypassListenerEffects = false;
                currentVoiceChatAudioSource.bypassEffects = false;
                currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                component.lowpassResonanceQ = 1f;
            }
            else
            {
                currentVoiceChatAudioSource.spatialBlend = 0f;
                playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    currentVoiceChatAudioSource.panStereo = 0f;
                    currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                    currentVoiceChatAudioSource.bypassListenerEffects = false;
                    currentVoiceChatAudioSource.bypassEffects = false;
                }
                else
                {
                    currentVoiceChatAudioSource.panStereo = 0.4f;
                    currentVoiceChatAudioSource.bypassListenerEffects = false;
                    currentVoiceChatAudioSource.bypassEffects = false;
                    currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                }
                component2.lowPassOverride = 4000f;
                component.lowpassResonanceQ = 3f;
            }
            if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
            {
                playerControllerB2.voicePlayerState.Volume = 0.8f;
            }
            else
            {
                playerControllerB2.voicePlayerState.Volume = 1f;
            }

        }


        public void RefreshPlayerVoicePlaybackObjects(PlayerControllerB player)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }
            PlayerVoiceIngameSettings[] array = UnityEngine.Object.FindObjectsOfType<PlayerVoiceIngameSettings>(includeInactive: true);
            log.LogDebug($"Refreshing voice playback objects. Number of voice objects found: {array.Length}");

            PlayerControllerB playerControllerB = player;
            if (!playerControllerB.isPlayerControlled && !playerControllerB.isPlayerDead)
            {
                log.LogDebug($"Skipping player as they are not controlled or dead");
                return;
            }
            for (int j = 0; j < array.Length; j++)
            {
                if (array[j]._playerState == null)
                {
                    array[j].FindPlayerIfNull();
                    if (array[j]._playerState == null)
                    {
                        log.LogError($"Unable to connect player to voice B; {array[j].isActiveAndEnabled}; {array[j]._playerState == null}");
                    }
                }
                else if (!array[j].isActiveAndEnabled)
                {
                    log.LogDebug($"Unable to connect player to voice A; {array[j].isActiveAndEnabled}; {array[j]._playerState == null}");
                }
                else if (array[j]._playerState.Name == playerControllerB.gameObject.GetComponentInChildren<NfgoPlayer>().PlayerId)
                {
                    log.LogDebug($"Found a match for voice object #{j} and player object");
                    playerControllerB.voicePlayerState = array[j]._playerState;
                    playerControllerB.currentVoiceChatAudioSource = array[j].voiceAudio;
                    playerControllerB.currentVoiceChatIngameSettings = array[j];
                    playerControllerB.currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB.playerClientId];
                    log.LogDebug($"player voice chat audiosource: {playerControllerB.currentVoiceChatAudioSource}; set audiomixer to {SoundManager.Instance.playerVoiceMixers[playerControllerB.playerClientId]} ; {playerControllerB.currentVoiceChatAudioSource.outputAudioMixerGroup} ; {playerControllerB.playerClientId}");
                }
            }

        }

    }
}