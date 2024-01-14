using System;
using System.Collections.Generic;
using BepInEx.Logging;
using GameNetcodeStuff;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace LethalRescueCompanyMod;

public class Helper : MonoBehaviour
{
    private readonly ManualLogSource log = Logger.CreateLogSource("LethalRescueCompanyPlugin.Helpers");

    public void ReviveRescuedPlayerById(int id, Vector3 spawnPosistion, bool removeBody)
    {
        var playerControllerB = RoundManager.Instance.playersManager.allPlayerScripts[id];
        log.LogInfo($"reviving player: {playerControllerB.playerUsername}");
        ReviveRescuedPlayer(playerControllerB, spawnPosistion, removeBody);
    }

    public void ReviveRescuedPlayer(PlayerControllerB playerControllerB, Vector3 spawnPosistion, bool removeBody)
    {
        if (playerControllerB == null) return;
        log.LogInfo($"reviving player: {playerControllerB.playerUsername}");
        try
        {
            if (Settings.IsDebug)
                log.LogInfo(
                    $"ReviveRescuedPlayer called with db: {playerControllerB.playerClientId}, pm: {playerControllerB.playersManager}");
            // get the PlayerControllerB from the deadbody

            // this is stolen from the spawn logic
            playerControllerB.isClimbingLadder = false;
            playerControllerB.ResetZAndXRotation();
            playerControllerB.thisController.enabled = true;
            playerControllerB.health = 40;
            playerControllerB.disableLookInput = false;
            playerControllerB.isPlayerDead = false;
            playerControllerB.isPlayerControlled = true;
            playerControllerB.isInElevator = true;
            playerControllerB.isInHangarShipRoom = true;
            playerControllerB.isInsideFactory = false;
            playerControllerB.wasInElevatorLastFrame = false;
            playerControllerB.carryWeight = 1f;
            playerControllerB.isFreeCamera = false;
            playerControllerB.playerHudUIContainer.gameObject.SetActive(true);
            playerControllerB.TeleportPlayer(spawnPosistion);
            playerControllerB.setPositionOfDeadPlayer = false;

            // this is the disable player model 
            var componentsInChildren = playerControllerB.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (var i = 0; i < componentsInChildren.Length; i++) componentsInChildren[i].enabled = true;

            playerControllerB.thisPlayerModelArms.enabled = false; // not sure if this is fix for goro arms

            // more init stuff
            playerControllerB.helmetLight.enabled = false;
            playerControllerB.Crouch(false);
            playerControllerB.playerBodyAnimator.SetBool("Limp", false);
            playerControllerB.bleedingHeavily = false;
            playerControllerB.activatingItem = false;
            playerControllerB.twoHanded = false;
            playerControllerB.inSpecialInteractAnimation = false;
            playerControllerB.holdingWalkieTalkie = false;
            playerControllerB.speakingToWalkieTalkie = false;
            playerControllerB.isSinking = false;
            playerControllerB.isUnderwater = false;
            playerControllerB.sinkingValue = 0f;
            playerControllerB.statusEffectAudio.Stop();
            playerControllerB.DisableJetpackControlsLocally();
            playerControllerB.movementSpeed = 4.6f;
            playerControllerB.mapRadarDotAnimator.SetBool("dead", false);

            HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
            playerControllerB.hasBegunSpectating = false;
            HUDManager.Instance.RemoveSpectateUI();


            HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
            playerControllerB.hinderedMultiplier = 1f;
            playerControllerB.isMovementHindered = 0;
            playerControllerB.sourcesCausingSinking = 0;

            SoundManager.Instance.earsRingingTimer = 0f;
            playerControllerB.voiceMuffledByEnemy = false;
            playerControllerB.spectatedPlayerScript = null;
            playerControllerB.MakeCriticallyInjured(false);

            HUDManager.Instance.UpdateHealthUI(40, false);
            HUDManager.Instance.audioListenerLowPass.enabled = false;
            playerControllerB.playersManager.livingPlayers = playerControllerB.playersManager.livingPlayers + 1;
            HUDManager.Instance.HideHUD(false);

            // destroy deadbody
            if (removeBody)
            {
                Destroy(playerControllerB.deadBody.gameObject);
                Destroy(playerControllerB.deadBody);
            }

            if (Settings.IsDebug) log.LogInfo("end ReviveRescuedPlayer");
            if (Settings.IsDebug) playerControllerB.nightVision.enabled = true;
        }
        catch (Exception ex)
        {
            log.LogError($"Error in ReviveRescuedPlayer: {ex.Message}");
        }
    }

    public PlayerControllerB GetPlayerByName(string playerName, StartOfRound playersManager)
    {
        if (Settings.IsDebug) log.LogInfo($"GetPlayerByName: {playerName}");
        PlayerControllerB[] players = playersManager.allPlayerScripts; //  StartOfRound.Instance.allPlayerScripts;
        if (Settings.IsDebug) log.LogInfo($"got all player scripts: {players}");
        var list = new List<PlayerControllerB>();
        var array = players;
        foreach (var val in array)
            if (val.isPlayerDead)
                list.Add(val);
        foreach (var item in list)
        {
            if (Settings.IsDebug) log.LogInfo("player comparitor");
            if (item.playerUsername.Equals(playerName))
            {
                if (Settings.IsDebug) log.LogInfo($"GetPlayerByName found player: {item}");
                return item;
            }
        }

        return null;
    }
}