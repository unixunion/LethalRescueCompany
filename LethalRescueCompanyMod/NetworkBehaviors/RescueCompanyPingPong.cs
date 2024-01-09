﻿using BepInEx.Logging;
using GameNetcodeStuff;
using LethalRescueCompanyMod.Models;
using LethalRescueCompanyPlugin;
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
using static CommandContract;
using static UnityEngine.CullingGroup;

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
                    GameObject go = Instantiate(LethalCompanyMemorableMomentsPlugin.instance.getTestPrefab(), transform.position, Quaternion.identity);
                    go.GetComponent<NetworkObject>().Spawn(true);
                    break;
                
                default:
                    log.LogInfo($"ReceivedEvent unknown action: {eventName.command}");
                    break;
            }

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
       