using BepInEx.Logging;
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
        StartOfRound _startOfRound;
        public bool isRespawning = false;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RevivableTrait");
        void Awake()
        {
            var deadBodyInfo = gameObject.GetComponentInParent<DeadBodyInfo>();
            var startOfRound = gameObject.GetComponentInParent<StartOfRound>();


            _deadBodyInfo = deadBodyInfo;
            _startOfRound = startOfRound;
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
                    //RescueCompany.instance.RevivePlayer(deadBodyInfo.playerScript);
                    //deadBodyInfo.playerScript.HealClientRpc();
                    //helper.ReviveRescuedPlayer(deadBodyInfo, playersManager);
                    if (!isRespawning)
                    {
                        if (Settings.isDebug) log.LogInfo("no respawning yet, and trait found, reviving");
                        StartCoroutine(WaitFiveSecondsAndRevive());
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


        public IEnumerator WaitFiveSecondsAndRevive()
        {
            isRespawning = true;
            print("Start waiting");
            yield return new WaitForSeconds(5);
            helper.ReviveRescuedPlayer(_deadBodyInfo, _startOfRound);
            isRespawning = false;
        }
    }
}
