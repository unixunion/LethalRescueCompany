using BepInEx.Logging;
using GameNetcodeStuff;
using LethalRescueCompanyMod.Models;
using System;
using Unity.Netcode;
using UnityEngine;


namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class RescueCompanyPingPong : NetworkBehaviour
    {

        internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RescueCompanyPingPong");

        public static event Action<Event> ServerEvent;
        public static event Action<Event> ClientEvent;

        public static RescueCompanyPingPong Instance;

        void Awake()
        {
            gameObject.AddComponent<SpiderSpawnBehavior>();
        }

        public override void OnNetworkSpawn()
        {
            log.LogInfo("network spawn");
            ServerEvent = null;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Spawn();
            Instance = this;

            ServerEvent += ServerEventHandler;
            ClientEvent += ClientEventHandler;

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {

            ServerEvent -= ServerEventHandler;
            ClientEvent -= ClientEventHandler;

            // this made the exit shit the bed, maybe the base takes care of this?
            //if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            //    Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            //Instance = this;

            base.OnNetworkDespawn();
        }

        [ClientRpc]
        public void EventClientRpc(Event eventName)
        {
            log.LogInfo($"EventClientRpc: received event: {eventName}, notifying subscribers within this runtime");
            ClientEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }


        /**
         * This is the RPC clients talk to
         **/
        [ServerRpc(RequireOwnership = false)]
        public void EventServerRpc(Event eventName)
        {
            log.LogInfo($"EventServerRpc: event: {eventName}, notifying subscribers within this runtime");
            ServerEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        public void ServerEventHandler(Event eventName)
        {
            log.LogInfo($"ServerEventHandler notified: event: {eventName}");
            switch (eventName.command)
            {
                case CommandContract.Command.SpawnCube:

                    PlayerControllerB[] players = GameObject.FindObjectsOfType<PlayerControllerB>();
                    var r = RoundManager.Instance;
                    if (r != null)
                    {
                        foreach(var player in r.playersManager.allPlayerScripts)
                        {
                            if (!player.isPlayerDead || player.hasBegunSpectating)
                            {
                                log.LogInfo($"ServerEventHandler: spawn cube at player: {player.playerSteamId}");
                                ServerSpawnAsset("CubePrefab", player.transform.position);
                                break;
                            }
                        };
                    }
                    break;

                case CommandContract.Command.SpawnSpider:

                    log.LogInfo("ServerEventHandler spawn spider action");
                    var p = RoundManager.Instance.playersManager.localPlayerController;
                    if (p.IsHost)
                    {
                        log.LogInfo($"ServerEventHandler: spawn cube player: {p}");
                        SpawnViaComponent("spider", p);
                    } else
                    {
                        log.LogWarning("ServerEventHandler ignoring spawn spider because localplayercontroller is not host");
                    }
                    break;

                default:
                    log.LogInfo($"ServerEventHandler: unknown action: {eventName.command}");
                    break;
            }

        }


        /**
         * this is not probably needed, but could be useful. 
         **/
        public void ClientEventHandler(Event eventName)
        {
            log.LogInfo($"ClientEventHandler notified: event: {eventName}");
        }


        /**
         * Main event handler, so we call this from other stuff when we want things to happen.
         * Such as from the HudManager.
         **/
        public void HandleEvent(Event eventName)
        {
            log.LogInfo($"HandleEvent: {eventName}");

            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                // Perform server-side handling of the event
                log.LogInfo("HandleEvent: I am server, calling EventClientRpc");
                ServerEventHandler(eventName);
                // update clients, but they just log this.
                EventClientRpc(eventName);
            }
            else
            {
                // I am a client, so I'll request the server to handle this event
                log.LogInfo("HandleEvent: I am client, calling EventServerRpc");
                EventServerRpc(eventName);
            }
        }



        /**
         * Spawn the object correctly regardless of server / client
         * identifier, is the prefab name, as defined in unity editor!
         **/
        public void ServerSpawnAsset(String identifier, Vector3 position)
        {
            log.LogInfo($"ServerSpawnAsset: spawning {identifier} at {position}");
            GameObject go = Instantiate(AssetManager.GetAssetByKey(identifier), position, UnityEngine.Quaternion.identity);
            if (IsServer)
            {
                log.LogInfo($"ServerSpawnAsset: isServer = true, NetworkObject Spawn of a {identifier}");
                go.GetComponent<NetworkObject>().Spawn(true);
            }

        }


        /**
         * Experiment, using a subcomponent of this game object to invoke spawns.
         **/
        public void SpawnViaComponent(string identifier, PlayerControllerB p)
        {
            log.LogInfo($"SpawnViaComponent: identifier: {identifier}"); 
            switch (identifier)
            {
                case "spider":
                    SpiderSpawnBehavior spiderSpawnBehavior = p.GetComponent<SpiderSpawnBehavior>();
                    if (spiderSpawnBehavior != null)
                    {
                        log.LogInfo("SpawnViaComponent: calling spider spawn debug hack");
                        spiderSpawnBehavior.DebugHacks(p.transform);
                    } else
                    {
                        log.LogWarning("SpawnViaComponent: no spiderspawn on the component");
                    }
                    break;
                default:
                    log.LogInfo("SpawnViaComponent: unknown spawnable");
                    break;
            }
        }



    }


}
       