using BepInEx.Logging;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class BodyCloneBehavior : NetworkBehaviour
    {

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.NetworkBehaviors.BodyCloneBehavior");


        public static GameObject ReplacementBody(DeadBodyInfo deadBodyInfo)
        {
            var tf = deadBodyInfo.transform;
            //AssetManager.GetAssetByKey("CubePrefab")
            log.LogInfo($"ReplacementBody: hanging body prefab: {Settings.hangingBodyPrefab}");
            GameObject gameObject = Instantiate(Settings.hangingBodyPrefab, tf.position, tf.rotation);
            RevivableTrait revivableTrait = gameObject.AddComponent<RevivableTrait>();
            revivableTrait.playerControllerB = deadBodyInfo.playerScript;
            revivableTrait.grabbableObject = gameObject.GetComponent<GrabbableObject>();

            if (NetworkManager.Singleton.IsServer) {
                log.LogInfo("spawning network object ");
                var t = gameObject.GetComponent<NetworkObject>();
                if (t == null) gameObject.AddComponent<NetworkObject>();
                gameObject.GetComponent<NetworkObject>().Spawn(true);
            }

            return gameObject;
        }

        public static GameObject ReplacementBody(GameObject original, PlayerControllerB playerControllerB)
        {
            var tf = original.transform;
            GameObject gameObject = Instantiate(original, tf.position, tf.rotation);
            RevivableTrait revivableTrait = gameObject.AddComponent<RevivableTrait>();
            revivableTrait.playerControllerB = playerControllerB;
            revivableTrait.grabbableObject = gameObject.GetComponent<GrabbableObject>();

            if (NetworkManager.Singleton.IsServer)
            {
                log.LogInfo("spawning network object ");
                gameObject.GetComponent<NetworkObject>().Spawn(true);
            }

            return gameObject;
        }




    }
}
