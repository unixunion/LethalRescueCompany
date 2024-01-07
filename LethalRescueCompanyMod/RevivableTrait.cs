using BepInEx.Logging;
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
        DeadBodyInfo _deadBodyInfo;
        StartOfRound _playersManager;
        public bool isRespawning = false;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RevivableTrait");
        void Awake()
        {
            var theParentOfThisComponent = this.gameObject.GetComponentInParent<PlayerControllerB>();
            log.LogMessage($"playerName: {theParentOfThisComponent.name}");
            log.LogMessage($"hasDeadBody: {theParentOfThisComponent.deadBody != null}");
            log.LogMessage($"hasPlayerManager: {theParentOfThisComponent.playersManager != null}");

            var player = theParentOfThisComponent;
            _deadBodyInfo = player.deadBody;
            _playersManager = player.playersManager;
        }

        public void playerIsDeadInShipAndRevivable()
        {
            try
            {
                if (_deadBodyInfo == null) return;
                // ignore dead bodies not in the ship
                if (!_deadBodyInfo.isInShip) return;

                if (_playersManager == null)
                {
                    log.LogError($"playersManager is null");
                    return;
                }

                if (isDebug)
                {
                    log.LogInfo($"db.isInShip: {_deadBodyInfo.isInShip}, " +
                                $"db....isHeld: {_deadBodyInfo.grabBodyObject.isHeld}, " +
                                $"db....velocity.mag: {_deadBodyInfo.bodyParts[0].velocity.magnitude}, " +
                                $"db<ReviveTrait>: {_deadBodyInfo.GetComponent<RevivableTrait>()}");
                }

                // detect if its dropped deadBodyInfo.grabBodyObject.hasHitGround  // && deadBodyInfo.bodyParts[0].velocity.magnitude<0.02
                if (!_deadBodyInfo.grabBodyObject.isHeld) // might be 6
                {
                    if (Settings.isDebug) log.LogInfo("trait found, reviving");
                    //RescueCompany.instance.RevivePlayer(deadBodyInfo.playerScript);
                    //deadBodyInfo.playerScript.HealClientRpc();
                    //helper.ReviveRescuedPlayer(deadBodyInfo, playersManager);
                    if(!isRespawning) StartCoroutine(WaitFiveSecondsAndRevive());
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


        public IEnumerator WaitFiveSecondsAndRevive()
        {
            isRespawning = true;
            print("Start waiting");
            yield return new WaitForSeconds(5);
            helper.ReviveRescuedPlayer(_deadBodyInfo, _playersManager);
            isRespawning = false;
        }
    }
}
