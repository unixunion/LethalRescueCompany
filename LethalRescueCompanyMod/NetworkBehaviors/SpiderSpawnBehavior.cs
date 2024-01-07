using BepInEx.Logging;
using Dissonance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class SpiderSpawnBehavior : NetworkBehaviour
    {
        internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.SpiderSpawnBehavior");
        private List<EnemyAI> spawnedSpiders = null;
        private EnemyType spiderEnemyType = null;
        private bool hasKilledSpiders = false;
        private bool spidersExist = false;

        public void unsetSpiderEntity()
        {
            spiderEnemyType = null;
            hasKilledSpiders = false;
            spawnedSpiders = null;
            spidersExist = false;
        }

        [ServerRpc(RequireOwnership = false)]
        public void DebugHacks(Transform thisPlayerBody, string lastChatMessage, StartOfRound playersManager)
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
                        if (enemy.enemyType.enemyPrefab.name.ToLower().Contains("spider"))
                        {
                            spiderEnemyType = enemy.enemyType;
                            // how to break out ?
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

                    try
                    {

                        // get the spawned spooder
                        SandSpiderAI x = (SandSpiderAI)RoundManager.Instance.SpawnedEnemies.Last();

                        // to make a spider target someone / body 
                        //SetterHandler targetPlayer, currentBehaviourStateIndex = 2
                        //if (Settings.isDebug) log.LogInfo($"choosing target: {lastChatMessage}");
                        //x.TriggerChaseWithPlayer(helper.GetPlayerByName(lastChatMessage, playersManager));
                    }
                    catch (Exception ex)
                    {
                        if (Settings.isDebug) log.LogWarning($"Unable to target player, ignore this, {ex}");
                    }
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
                    spawnedSpiders = null;
                    hasKilledSpiders = false;
                }
            }
        }

    }
}
