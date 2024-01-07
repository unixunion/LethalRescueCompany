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
using System.Collections;
using LethalRescueCompanyMod;
using LethalRescueCompanyMod.NetworkBehaviors;

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
        static Helper reviveHelper = new Helper();
        static bool isDebug = Settings.isDebug;
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.PlayerControllerBPatch");

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
            // nope out if not a body
            if (___deadBody == null) return;

            playerIsDeadAndWebbedInShipCheck(___deadBody, ___playersManager, ___isPlayerDead);
        }

        
        private static void playerIsDeadAndWebbedInShipCheck(DeadBodyInfo deadBodyInfo, StartOfRound playersManager, bool isPlayerDead)
        {
            try
            {
                // ignore dead bodies not in the ship
                if (!deadBodyInfo.isInShip) return;

                // GetHost(___playersManager);

                if (playersManager == null)
                {
                    log.LogError($"playersManager is null");
                    return;
                }

                //&& ___playerUsername == "marzubus"
                if (isPlayerDead)
                {
                    if (isDebug)
                    {
                        log.LogInfo($"db.isInShip: {deadBodyInfo.isInShip}, " +
                                    $"db.sharedMaterial.name: {deadBodyInfo.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name}, " +
                                    $"db.sharedMesh.name: {deadBodyInfo.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.name}, " +
                                    $"db.causeOfDeath: {deadBodyInfo.causeOfDeath}, " +
                                    $"db.canBeGrabbedBackByPlayers: {deadBodyInfo.grabBodyObject.grabbable}");
                    }


                    // detect if its dropped
                    if (deadBodyInfo.isInShip && deadBodyInfo.grabBodyObject.grabbable)
                    {
                        //log.LogInfo("body dropped in ship, checking if its warpped in a web");
                        if (deadBodyInfo.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.name == "SpooledPlayerMat")
                        {
                            if (Settings.isDebug) log.LogInfo("calling revivehelper");
                            reviveHelper.startCoroutine(deadBodyInfo, playersManager);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.LogError(JsonConvert.SerializeObject(ex));
            }
        }

        
    }
}