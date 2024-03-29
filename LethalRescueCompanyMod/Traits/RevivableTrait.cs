﻿using BepInEx.Logging;
using GameNetcodeStuff;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod
{
    public class RevivableTrait : NetworkBehaviour
    {
        bool isDebug = Settings.isDebug;
        Helper helper = new Helper();
        public bool isRespawning = false;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RevivableTrait");
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
                        StartCoroutine(WaitFiveSecondsAndRevive(deadBodyInfo, playersManager));
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


        public IEnumerator WaitFiveSecondsAndRevive(DeadBodyInfo deadBodyInfo, StartOfRound playersManager)
        {
            isRespawning = true;
            log.LogMessage("Start waiting");
            yield return new WaitForSeconds(5);
            log.LogMessage("Done waiting");
            helper.ReviveRescuedPlayer(deadBodyInfo, playersManager);
            isRespawning = false;
        }
    }
}
