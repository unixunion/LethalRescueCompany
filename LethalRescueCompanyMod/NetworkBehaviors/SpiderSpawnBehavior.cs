using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class SpiderSpawnBehavior : NetworkBehaviour
    {
        internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.NetworkBehaviors.SpiderSpawnBehavior");
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

        public void DebugHacks(Transform thisPlayerBody)
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
                            // looking for wayt so dump prefab into
                            // PrefabUtility.
                            // UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.prefabAssetPath;
                            // log.LogInfo(enemy.enemyType.enemyPrefab);
                            spiderEnemyType = enemy.enemyType;
                            return;//breaks out
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
                    spawnedSpiders = null;
                    hasKilledSpiders = false;
                }
            }
        }

    }
}
