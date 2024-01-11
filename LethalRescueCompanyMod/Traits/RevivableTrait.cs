using BepInEx.Logging;
using GameNetcodeStuff;
using Newtonsoft.Json;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod
{
    public class RevivableTrait : NetworkBehaviour
    {
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RevivableTrait");


        bool isDebug = Settings.isDebug;
        Helper helper = new Helper();
        
        public bool isRespawning = false;
        public bool isUsed = false;
        public PlayerControllerB playerControllerB; // the controller player instance
        public GrabbableObject grabbableObject; // pointer to the grabbable parent
        
        void Update()
        {
            revivePlayer();
        }

        public void Interact()
        {
            log.LogDebug("interacting...");

        }

        public void revivePlayer()
        {
            log.LogInfo("checking1");
            if (grabbableObject == null) return;
            log.LogInfo("checking2");
            if (playerControllerB == null) return;
            log.LogInfo("checking3");
            if (grabbableObject.isInShipRoom && playerControllerB.playersManager!=null)
            {
                log.LogInfo("checking4");
                if (!grabbableObject.isHeld) {
                    log.LogInfo("checking5");
                    if (!isRespawning && !isUsed)
                    {
                        log.LogInfo("revive conditions met, object is not held");
                        StartCoroutine(WaitFiveSecondsAndRevive(playerControllerB, playerControllerB.playersManager.playerSpawnPositions[0].position));
                        
                    }
                }
            } else
            {
                log.LogError($"revive conditions unmet: {grabbableObject.isInShipRoom}, {playerControllerB.playersManager} ");
            }
        }

        public void playerIsDeadInShipAndRevivable(DeadBodyInfo deadBodyInfo, StartOfRound playersManager)
        {
            try
            {
                if (deadBodyInfo == null) return;
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
                                $"db....isHeld: {deadBodyInfo.grabBodyObject.isHeld}, " +
                                $"db....velocity.mag: {deadBodyInfo.bodyParts[0].velocity.magnitude}, " +
                                $"db<ReviveTrait>: {deadBodyInfo.GetComponent<RevivableTrait>()}");
                }

                // detect if its dropped deadBodyInfo.grabBodyObject.hasHitGround  // && deadBodyInfo.bodyParts[0].velocity.magnitude<0.02
                if (!deadBodyInfo.grabBodyObject.isHeld) // might be 6
                {
                    
                    if (!isRespawning)
                    {
                        if (Settings.isDebug) log.LogInfo("trait found, reviving coroutine");
                        //StartCoroutine(WaitFiveSecondsAndRevive(deadBodyInfo, playersManager));
                    }
                }
                else
                {
                    if (Settings.isDebug) log.LogInfo("waiting for body to drop, be grabbable and not move");
                }
            }
            catch (Exception ex)
            {
                log.LogError(JsonConvert.SerializeObject(ex));
            }
        }


        public void DebugRevive(DeadBodyInfo deadBodyInfo)
        {
            if (isDebug)
            {
                if (!isRespawning)
                {
                    if (Settings.isDebug) log.LogInfo("trait found, reviving coroutine");
                    StartCoroutine(WaitFiveSecondsAndRevive(deadBodyInfo.playerScript, deadBodyInfo.transform.position));
                }
            }
        }

        public IEnumerator WaitFiveSecondsAndRevive(PlayerControllerB playerControllerB, Vector3 spawnPositions)
        {
            isRespawning = true;
            isUsed = true;
            log.LogMessage("Start waiting");
            yield return new WaitForSeconds(5);
            log.LogMessage("Done waiting");
            helper.ReviveRescuedPlayer(playerControllerB, spawnPositions);
            isUsed = true;
            isRespawning = false;
        }
    }
}
