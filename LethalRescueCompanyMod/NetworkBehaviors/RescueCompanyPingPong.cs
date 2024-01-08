using BepInEx.Logging;
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
        public static RescueCompanyPingPong instance;
        public void Start()
        {
            instance = this;
            _networkObject = gameObject.GetComponent<NetworkObject>();
        }

        [ClientRpc]
        public void TestClientRpc(string value, ulong sourceNetworkObjectId)
        {
            log.LogInfo($"Client[{NetworkManager.Singleton.LocalClientId}] Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            if (!_networkObject.IsOwner) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
            {
                TestServerRpc(value + 1, sourceNetworkObjectId);
            }
        }

        [ServerRpc]
        public void TestServerRpc(string value, ulong sourceNetworkObjectId)
        {
            log.LogInfo($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            TestClientRpc(value, sourceNetworkObjectId);
        }
    }
}
