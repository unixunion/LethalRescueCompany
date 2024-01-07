using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using LethalRescueCompanyMod.NetworkBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalRescueCompanyMod.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HudManagerPatch : BaseUnityPlugin
    {
        static bool isDebug = Settings.isDebug;
        static Helper helper = new Helper();
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.HudManagerPatch");
        [HarmonyPatch("AddTextToChatOnServer")]
        [HarmonyPostfix]
        static void AddTextToChatOnServerPatch()
        {
            // spider debugging stuff
            if (isDebug)
            {
                //DebugHacks(___localPlayer.thisPlayerBody);
                KillPlayer(GameNetworkManager.Instance.localPlayerController, Vector3.zero, false, CauseOfDeath.Unknown, 0);
                if (ReviveStore.instance == null) log.LogMessage("ReviveStore is null");
                if (ReviveStore.instance!=null) ReviveStore.instance.RequestReviveServerRpc(GameNetworkManager.Instance.localPlayerController.actualClientId);

                var foundstore = GameObject.Find("ReviveStore");
                if(foundstore == null) log.LogMessage("foundstore is null");
                if (foundstore != null) log.LogMessage("foundstore is not null");
                //helper.startCoroutine(___localPlayer.deadBody, ___playersManager);
            }
        }
        private static List<EnemyAI> spawnedSpiders = null;
        private static EnemyType spiderEnemyType = null;
        private static bool hasKilledSpiders = false;
        private static bool spidersExist = false;

        [HarmonyPatch("RemoveSpectateUI")]
        [HarmonyPostfix]
        static void unsetSpiderEntity()
        {
            spiderEnemyType = null;
            hasKilledSpiders = false;
            spawnedSpiders = null;
            spidersExist = false;
        }

        public static void KillPlayer(PlayerControllerB player, Vector3 bodyVelocity, bool spawnBody = true, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, int deathAnimation = 0)
        {
            player.KillPlayer(bodyVelocity, spawnBody, causeOfDeath, deathAnimation);
        }


        private static void DebugHacks(Transform thisPlayerBody)
        {
            List<EnemyAI> fuckingSpiders = null;
            try
            {
                fuckingSpiders = RoundManager.Instance.SpawnedEnemies.Where(x => x.enemyType.enemyPrefab.name.ToLower().Contains("spider")).ToList();
                if (fuckingSpiders != null && fuckingSpiders.Count > 0)
                {
                    spawnedSpiders = fuckingSpiders;
                    spidersExist = true;
                }
                else
                {
                    spidersExist = false;
                }
                log.LogInfo($"spidersExist:{spidersExist}, spiderCount:{fuckingSpiders.Count}");
            }
            catch { };
            if (!spidersExist)
            {
                if (spiderEnemyType == null)
                {
                    RoundManager.Instance.currentLevel.Enemies.ForEach(enemy =>
                    {
                        //log.LogInfo(enemy.enemyType.enemyPrefab.name);
                        if (enemy.enemyType.enemyPrefab.name.ToLower().Contains("spider"))
                        {
                            spiderEnemyType = enemy.enemyType;
                        }
                    });
                }

                if (spiderEnemyType != null && spawnedSpiders == null)
                {
                    log.LogInfo($"Spawning spider at: {thisPlayerBody.position}");
                    
                    Vector3 playerPos = thisPlayerBody.transform.position;
                    Vector3 playerDirection = thisPlayerBody.transform.forward;
                    Quaternion playerRotation = thisPlayerBody.transform.rotation;
                    float spawnDistance = 10;

                    Vector3 spawnPos = playerPos + playerDirection * spawnDistance;

                    var n = RoundManager.Instance.SpawnEnemyGameObject(spawnPos, 0, 99, spiderEnemyType);
                    spawnedSpiders = fuckingSpiders;
                }
            }
            else if (spidersExist)
            {
                if (fuckingSpiders != null)
                {
                    if (fuckingSpiders.Count > 0 && !hasKilledSpiders)
                    {
                        fuckingSpiders.ForEach(spider => Destroy(spider.gameObject));
                        fuckingSpiders.ForEach(spider => Destroy(spider));
                        RoundManager.Instance.SpawnedEnemies.RemoveAll(_ => true);
                        hasKilledSpiders = true;
                    }
                }

                if (hasKilledSpiders)
                {
                    spiderEnemyType = null;
                    //spidersExist = false;
                    spawnedSpiders = null;
                    hasKilledSpiders = false;
                }
            }
        }
    }
}
