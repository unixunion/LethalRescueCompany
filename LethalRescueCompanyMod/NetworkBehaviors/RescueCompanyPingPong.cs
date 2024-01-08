using BepInEx.Logging;
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

        public void Awake()
        {
            NetworkManager networkManager = NetworkManager;
            networkManager.NetworkConfig.ForceSamePrefabs = false;
            networkManager.AddNetworkPrefab(this.gameObject);
            var networkObject = gameObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                log.LogInfo($"NETWORK OBJECT IS NULL, KEEP LOOKING  BRUH");
            }

            log.LogInfo($"TESTING DEBUG: isServer:{networkManager.IsServer} || " +
                $"isOwner:{networkObject.IsOwner} || " +
                $"isClient:{networkManager.IsClient} ||" +
                $"ownerClientId:{ base.OwnerClientId} || " +
                $"networkManagerClientId: {networkManager.LocalClientId}");

            log.LogInfo($"TESTING DEBUG: isServer:{networkManager.IsServer} || " +
                $"isOwner:{networkObject.IsOwner} || " +
                $"isClient:{networkManager.IsClient} ||" +
                $"ownerClientId:{base.IsOwner} || " +
                $"networkManagerClientId: {networkManager.LocalClientId}");


            if (!networkManager.IsServer && networkManager.IsClient) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
            {
                log.LogInfo($"STARTING RPC CHIT SHAT");
                StartCoroutine(delayAndSendRPC());
            }
        }

        public bool IsOwner
        {
            get
            {
                if (NetworkManager != null)
                {
                    return OwnerClientId == NetworkManager.LocalClientId;
                }

                log.LogInfo($"Unfortunately the Network Manager are fucking null");
                return false;
            }
        }


        private IEnumerator delayAndSendRPC()
        {
            yield return new WaitForSeconds(15);
            for (int i = 0; i < 5; i++)
            {
                TestServerRpc(i.ToString(), NetworkObjectId);
            }
        }

        [ClientRpc]
        void TestClientRpc(string value, ulong sourceNetworkObjectId)
        {
            log.LogInfo($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            if (base.IsOwner) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
            {
                TestServerRpc(value + 1, sourceNetworkObjectId);
            }
        }

        [ServerRpc]
        void TestServerRpc(string value, ulong sourceNetworkObjectId)
        {
            log.LogInfo($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            TestClientRpc(value, sourceNetworkObjectId);
        }
    }
}
