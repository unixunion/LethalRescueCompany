using BepInEx.Logging;
using GameNetcodeStuff;
using LethalRescueCompanyMod.Models;
using LethalRescueCompanyPlugin;
using System;
using Unity.Netcode;
using UnityEngine;


namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class RescueCompanyPingPong : NetworkBehaviour
    {

        internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RescueCompanyPingPong");

        public static event Action<Event> LevelEvent;

        public static RescueCompanyPingPong Instance;

        public override void OnNetworkSpawn()
        {
            log.LogInfo("network spawn");
            LevelEvent = null;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Spawn();
            Instance = this;

            LevelEvent += ReceivedEvent;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            LevelEvent -= ReceivedEvent;
            base.OnNetworkDespawn();
        }

        [ClientRpc]
        public void EventClientRpc(Event eventName)
        {
            log.LogInfo("EventClientRpc: ");
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        [ServerRpc(RequireOwnership = false)]
        public void EventServerRpc(Event eventName)
        {
            log.LogInfo("where clients send event server rpc");
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        public void ReceivedEvent(Event eventName)
        {
            log.LogInfo($"ReceivedEvent: event: {eventName}");
            switch (eventName.command)
            {
                case CommandContract.Command.SpawnSpider:
                    
                    log.LogInfo("ReceivedEvent spawn action");
                    PlayerControllerB[] players = GameObject.FindObjectsOfType<PlayerControllerB>();
                    var r = RoundManager.Instance;
                    if (r != null)
                    {
                        foreach(var player in r.playersManager.allPlayerScripts)
                        {
                            if (!player.isPlayerDead)
                            {
                                Spawn("CubePrefab", player.transform.position);
                                break;
                            }
                        };
                    }
                    break;
                
                default:
                    log.LogInfo($"ReceivedEvent unknown action: {eventName.command}");
                    break;
            }

        }

        /**
         * Spawn the object correctly regardless of server / client, and return a ref.
         * identifier, is the prefab name, as defined in unity editor!
         **/
        public NetworkObjectReference Spawn(String identifier, Vector3 position)
        {
            log.LogInfo($"spawning {identifier}");
            GameObject go = Instantiate(AssetManager.GetAssetByKey(identifier), position, UnityEngine.Quaternion.identity);
            if (IsServer)
            {
                log.LogInfo($"Server Spawn of a {identifier}");
                go.GetComponent<NetworkObject>().Spawn(true);
            }

            return go.GetComponentInChildren<NetworkObject>();
        }

        public void SendEventToClients(Event eventName)
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            {
                log.LogInfo("i am client, sending event to server");
                RescueCompanyPingPong.Instance.EventServerRpc(eventName);
                return;
            }
            log.LogInfo("i am server, sending event to clients");
            RescueCompanyPingPong.Instance.EventClientRpc(eventName);
        }




        }

    
}
       