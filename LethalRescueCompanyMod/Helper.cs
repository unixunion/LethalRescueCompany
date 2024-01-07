using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalRescueCompanyMod
{
    public class Helper : MonoBehaviour
    {
        internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Helpers");
        Stopwatch stopwatch;

        public Helper()
        {
            
        }

        public void ReviveRescuedPlayer(DeadBodyInfo deadbody, StartOfRound playersManager)
        {
            try
            {
                if (Settings.isDebug) log.LogInfo($"starting ReviveRescuedPlayer");
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
                //Destroy(deadbody.gameObject);

                if (Settings.isDebug) log.LogInfo($"end ReviveRescuedPlayer");
            }
            catch (Exception ex)
            {
                log.LogError($"Error in ReviveRescuedPlayer: {ex.Message}");
            }
        }

        public PlayerControllerB GetPlayerByName(string playerName, StartOfRound playersManager)
        {
            if (Settings.isDebug) { log.LogInfo($"GetPlayerByName: {playerName}"); }
            PlayerControllerB[] players = playersManager.allPlayerScripts; //  StartOfRound.Instance.allPlayerScripts;
            if (Settings.isDebug) { log.LogInfo($"got all player scripts: {players}"); }
            List<PlayerControllerB> list = new List<PlayerControllerB>();
            PlayerControllerB[] array = players;
            foreach (PlayerControllerB val in array)
            {
                if (val.isPlayerDead)
                {
                    list.Add(val);
                }
            }
            foreach (PlayerControllerB item in list)
            {
                if (Settings.isDebug) log.LogInfo("player comparitor");
                if (item.playerUsername.Equals(playerName))
                {
                    if (Settings.isDebug) log.LogInfo($"GetPlayerByName found player: {item}");
                    return item;
                }
            }
            return null;
        }

    }



}
