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
        public static RescueCompanyPingPong Instance { get; private set; }
        public static event Action<String> LevelEvent;

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RescueCompanyPingPong");

        public override void OnNetworkSpawn()
        {
            LevelEvent = null;
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;
            base.OnNetworkSpawn();
        }

        [ClientRpc]
        public void EventClientRpc(string eventName)
        {
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        [ServerRpc(RequireOwnership = false)]
        public void EventServerRpc(/*parameters here*/)
        {
            // code here
        }
    }
}
