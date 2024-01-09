using BepInEx.Logging;
using GameNetcodeStuff;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.CullingGroup;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class RescueCompanyPingPong : NetworkBehaviour
    {

        internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RescueCompanyPingPong");

        public static event Action<String> LevelEvent;

        public static RescueCompanyPingPong Instance;

     
        public override void OnNetworkSpawn()
        {
            log.LogInfo("network spawn");
            LevelEvent = null;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Spawn();
            Instance = this;

            LevelEvent += ReceivedEventFromServer;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            LevelEvent -= ReceivedEventFromServer;
            base.OnNetworkDespawn();
        }

        [ClientRpc]
        public void EventClientRpc(string eventName)
        {
            log.LogInfo("event client rpc");
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        [ServerRpc(RequireOwnership = false)]
        public void EventServerRpc(string eventName)
        {
            log.LogInfo("event server rpc");
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        public  void ReceivedEventFromServer(string eventName)
        {
            log.LogInfo($"event: {eventName}");
            EventParser(eventName);
        }

        public void SendEventToClients(string eventName)
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            {
                log.LogInfo("sending event to server");
                // send to server only
                RescueCompanyPingPong.Instance.EventServerRpc(eventName);
                return;
            }
            log.LogInfo("sending event to clients");
            RescueCompanyPingPong.Instance.EventClientRpc(eventName);
        }


        private void EventParser(string eventName)
        {
            if (!NetworkManager.Singleton.IsHost) return;

            switch(eventName.ToLower()){
                case "spider":
                    log.LogInfo("EventParser spider action");
                    break;
                case "deadbody":
                    log.LogInfo("EventParser deadbody action");
                    break;
                case "delete_deadbody":
                    log.LogInfo("EventParser delete_deadbody action");
                    break;
                default:
                    log.LogInfo($"EventParser unknown action: {eventName}");
                    break;
            }
        }
    }
}
