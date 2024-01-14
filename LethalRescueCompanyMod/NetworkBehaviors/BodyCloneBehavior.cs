using BepInEx.Logging;
using GameNetcodeStuff;
using LethalRescueCompanyMod.Models;
using Unity.Netcode;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace LethalRescueCompanyMod.NetworkBehaviors;

public class BodyCloneBehavior : NetworkBehaviour
{
    internal static ManualLogSource log =
        Logger.CreateLogSource("LethalRescueCompanyPlugin.NetworkBehaviors.BodyCloneBehavior");


    public static void ReplacementBody(DeadBodyInfo deadBodyInfo)
    {
        //Int32.TryParse(deadBodyInfo.playerScript.playerClientId.ToString(), out int playerId);
        log.LogInfo("generating command");
        var cmd = new Event(CommandContract.Command.SpawnBody, deadBodyInfo.transform.position,
            (int)deadBodyInfo.playerScript.playerClientId);
        log.LogInfo($"command: {cmd}");
        RescueCompanyController.Instance.HandleEvent(cmd);


        //var tf = deadBodyInfo.transform;
        ////AssetManager.GetAssetByKey("CubePrefab")
        //log.LogInfo($"ReplacementBody: hanging standin");
        //GameObject gameObject = Instantiate(AssetManager.GetAssetByKey("CubePrefab"), tf.position, tf.rotation);
        //RevivableTrait revivableTrait = gameObject.AddComponent<RevivableTrait>();
        //revivableTrait.playerControllerB = deadBodyInfo.playerScript;
        //revivableTrait.grabbableObject = gameObject.GetComponent<GrabbableObject>();

        //if (NetworkManager.Singleton.IsServer) {
        //    log.LogInfo("spawning network object ");
        //    var t = gameObject.GetComponent<NetworkObject>();
        //    if (t == null) gameObject.AddComponent<NetworkObject>();
        //    gameObject.GetComponent<NetworkObject>().Spawn(true);
        //} else
        //{
        //    // implement a server RPC call to spawn the cube serverside.
        //    throw new NotImplementedException("should make a server rpc call here.");
        //    //var commandObj = new Event(CommandContract.Command.SpawnCube, tf.position);
        //    //RescueCompanyController.Instance.HandleEvent(commandObj);
        //}

        //return gameObject;
    }

    public static GameObject ReplacementBody(GameObject original, PlayerControllerB playerControllerB)
    {
        var tf = original.transform;
        var gameObject = Instantiate(original, tf.position, tf.rotation);
        var revivableTrait = gameObject.AddComponent<RevivableTrait>();
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