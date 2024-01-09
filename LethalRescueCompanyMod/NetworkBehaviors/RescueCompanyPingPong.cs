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
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            LevelEvent += ReceivedEventFromServer;
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            LevelEvent -= ReceivedEventFromServer;
            base.OnNetworkDespawn();
        }

        [ClientRpc]
        public void EventClientRpc(string eventName)
        {
            log.LogInfo("event client rpc");
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        [ServerRpc]
        public void EventServerRpc(string eventName)
        {
            log.LogInfo("event server rpc");
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        public  void ReceivedEventFromServer(string eventName)
        {
            log.LogInfo($"event: {eventName}");
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
    }

    //public struct Command : INetworkSerializable, System.IEquatable<Command>
    //{
    //    public int commandId;
    //    public Vector3 location;

    //    public Command(int commandId, Vector3 location)
    //    {
    //        this.commandId = commandId;
    //        this.location = location;
    //    }

    //    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    //    {
    //        if (serializer.IsReader)
    //        {
    //            var reader = serializer.GetFastBufferReader();
    //            reader.ReadValueSafe(out commandId);
    //            reader.ReadValueSafe(out location);
    //        }
    //        else
    //        {
    //            var writer = serializer.GetFastBufferWriter();
    //            writer.WriteValueSafe(commandId);
    //            writer.WriteValueSafe(location);
    //        }
    //    }

    //    public bool Equals(Command other)
    //    {
    //        return commandId == other.commandId && location.Equals(other.location);
    //    }
    //}
}
