using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.NetworkBehaviors
{
    public abstract class RescueCompanyPingPong : NetworkBehaviour
    {


        string jsonBody = "{}";

        public void Start()
        {
            if (base.IsOwner)
            {
                SyncToClients();
            }

        }

        private void Update()
        {
            SyncToClients();
        }

        private void SyncToClients()
        {

            if (base.IsServer)
            {
                // I am god, tell the clients what to update
                UpdateClientRpc();
            }
            else
            {
                // I need to via the server, tell all clients what to update
                UpdateServerRpc();
            }

        }



        [ServerRpc]
        private void UpdateServerRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {
                // not server host
                if (base.OwnerClientId != networkManager.LocalClientId)
                {
                    if (networkManager.LogLevel <= LogLevel.Normal)
                    {
                        Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
                    }

                    return;
                }

                // I am server host, complete the rpc. 
                ServerRpcParams serverRpcParams = default(ServerRpcParams);
                FastBufferWriter bufferWriter = __beginSendServerRpc(1u, serverRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValueBitPacked(bufferWriter, 0);
                __endSendServerRpc(ref bufferWriter, 3079913705u, serverRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                UpdateClientRpc();
            }
        }


        [ClientRpc]
        private void UpdateClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(1258118513u, clientRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, rotationY);
                    __endSendClientRpc(ref bufferWriter, 1258118513u, clientRpcParams, RpcDelivery.Reliable);
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    previousYRotation = base.transform.eulerAngles.y;
                    targetYRotation = rotationY;
                }
            }
        }



        private static void ServerSyncHandler(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
            {
                if (networkManager.LogLevel <= LogLevel.Normal)
                {
                    Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
                }
            }
            else
            {
                ByteUnpacker.ReadValueBitPacked(reader, out short value);
                target.__rpc_exec_stage = __RpcExecStage.Server;
                ((EnemyAI)target).UpdateEnemyRotationServerRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void ClientSyncHandler(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out short value);
                target.__rpc_exec_stage = __RpcExecStage.Client;
                ((EnemyAI)target).UpdateEnemyRotationClientRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }



        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeRPCS_EnemyAI()
        {
            NetworkManager.__rpc_func_table.Add(1u, ServerSyncHandler);
            NetworkManager.__rpc_func_table.Add(2u, ClientSyncHandler);
        }
    }
}
