using System;
using Unity.Netcode;
using UnityEngine;

namespace LethalRescueCompanyMod.Models;

public struct Event : INetworkSerializable, IEquatable<Event>
{
    public CommandContract.Command command;
    public Vector3 location;
    public int playerId;

    public Event(CommandContract.Command command, Vector3 location, int playerId)
    {
        this.command = command;
        this.location = location;
        this.playerId = playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out command);
            reader.ReadValueSafe(out location);
            reader.ReadValueSafe(out playerId);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(command);
            writer.WriteValueSafe(location);
            writer.WriteValueSafe(playerId);
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