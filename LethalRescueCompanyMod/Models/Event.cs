using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.Models
{
    public struct Event : INetworkSerializable, System.IEquatable<Event>
    {
        public CommandContract.Command command;
        public Vector3 location;

        public Event(CommandContract.Command command, Vector3 location)
        {
            this.command = command;
            this.location = location;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out command);
                reader.ReadValueSafe(out location);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(command);
                writer.WriteValueSafe(location);
            }
        }


        public override string ToString()
        {
            return $"command: {command}, location.x: {location.x}";
        }

        public bool Equals(Event other)
        {
            return command == other.command && location.Equals(other.location);
        }
    }
}
