using BepInEx;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
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
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.HudManagerPatch");
        [HarmonyPatch("AddTextToChatOnServer")]
        [HarmonyPostfix]
        static void AddTextToChatOnServerPatch(ref PlayerControllerB ___localPlayer)
        {
            // spider debugging stuff
            if (isDebug)
            {
                DebugHacks(___localPlayer.thisPlayerBody);
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
                    var n = RoundManager.Instance.SpawnEnemyGameObject(thisPlayerBody.position, 0, 99, spiderEnemyType);
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
