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

        static internal ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("LethalRescueCompanyPlugin.Patches.RescueCompanyPingPong");

        private NetworkList<Command> data;
        public bool addCommand = false;
        public Command newCommand;

        public static RescueCompanyPingPong Instance { get; private set; }


        void Awake()
        {
            log.LogInfo("initializeing in awake");
            Instance = this;
            data = new NetworkList<Command>();
            log.LogInfo($"data {data.Count}");
            log.LogInfo("awake done");
        }

        void Start()
        {
            log.LogInfo("start pingponger");
            //DontDestroyOnLoad(gameObject.GetComponentInParent<GameObject>());
            /*At this point, the object hasn't been network spawned yet, so you're not allowed to edit network variables! */
        }

        void Update()
        {
            if (!IsServer) return;
            if (addCommand)
            {
                log.LogInfo("adding command");
                addCommand = false;
                data.Add(newCommand);

            }
        }

        public override void OnNetworkSpawn()
        {

            if (IsServer)
            {
                log.LogInfo($"I am server: ownerClientId: {OwnerClientId}");


                //NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

                if (!IsHost)
                {
                    log.LogInfo("I am not the host");
                }
                else
                {
                    log.LogInfo("I am the host");
                }

                data.OnListChanged += OnServerListChanged;

            }
            else
            {
                log.LogInfo("I am not the server");
                data.OnListChanged += OnClientListChanged;
            }


        }

        void OnServerListChanged(NetworkListEvent<Command> changeEvent)
        {
            Debug.Log($"[S] The list changed and now has {data.Count} elements");
        }

        void OnClientListChanged(NetworkListEvent<Command> changeEvent)
        {
            Debug.Log($"[C] The list changed and now has {data.Count} elements");
        }



        [ServerRpc(RequireOwnership = false)]
        public void ToggleServerRpc(float value)
        {
            if (data == null)
            {
                log.LogError("Data list is null");
                return;
            }

            log.LogInfo("called");
            newCommand = new Command(0, Vector3.zero);
            log.LogInfo("update value");
            addCommand = true;
        }



    }

    public struct Command : INetworkSerializable, System.IEquatable<Command>
    {
        public int commandId;
        public Vector3 location;

        public Command(int commandId, Vector3 location)
        {
            this.commandId = commandId;
            this.location = location;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out commandId);
                reader.ReadValueSafe(out location);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(commandId);
                writer.WriteValueSafe(location);
            }
        }

        public bool Equals(Command other)
        {
            return commandId == other.commandId && location.Equals(other.location);
        }
    }
}
