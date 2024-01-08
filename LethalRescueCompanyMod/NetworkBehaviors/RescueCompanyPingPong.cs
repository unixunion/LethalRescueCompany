﻿using BepInEx.Logging;
using GameNetcodeStuff;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public class RescueCompanyPingPong : NetworkBehaviour
    {
        string jsonBody = "{}";
        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RescueCompanyPingPong");
        NetworkObject _networkObject = null;
        public void Start()
        {
            var networkManager = NetworkManager.Singleton;
            //networkManager.NetworkConfig.ForceSamePrefabs = false;
            //networkManager.AddNetworkPrefab(this.gameObject);
            var networkObject = gameObject.GetComponentInParent<NetworkObject>();
            if (networkObject == null)
            {
                log.LogInfo($"NETWORK OBJECT IS NULL, KEEP LOOKING  BRUH");
            }
            _networkObject = networkObject;

            log.LogInfo($"TESTING DEBUG: isServer:{networkManager.IsServer} || " +
                $"isOwner:{networkObject.IsOwner} || " +
                $"isClient:{networkManager.IsClient} ||" +
                $"networkManagerClientId: {networkManager.LocalClientId}");


            if (!networkManager.IsServer && networkObject.IsOwner) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
            {
                log.LogInfo($"STARTING RPC CHIT SHAT");
                StartCoroutine(delayAndSendRPC());
            }
        }

        private IEnumerator delayAndSendRPC()
        {
            yield return new WaitForSeconds(15);
            for (int i = 0; i < 5; i++)
            {
                TestServerRpc(i.ToString(), _networkObject.NetworkObjectId);
            }
        }

        [ClientRpc]
        void TestClientRpc(string value, ulong sourceNetworkObjectId)
        {
            log.LogInfo($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            if (_networkObject.IsOwner && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
            {
                TestServerRpc(value + 1, sourceNetworkObjectId);
            }
        }

        [ServerRpc]
        void TestServerRpc(string value, ulong sourceNetworkObjectId)
        {
            log.LogInfo($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            if (_networkObject.IsOwner && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                TestClientRpc(value, sourceNetworkObjectId);
            }
        }
    }
}
