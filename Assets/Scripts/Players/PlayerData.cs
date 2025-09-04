using System;
using Scriptable;
using Unity.Collections;
using Unity.Netcode;

namespace Players
{
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong clientId;
        public FixedString32Bytes name;
        public AnimalType type;

        public PlayerData(ulong id, FixedString32Bytes name, AnimalType type)
        {
            clientId = id;
            this.name = name;
            this.type = type;
        }

        public bool Equals(PlayerData other)
        {
            return clientId == other.clientId && name.Equals(other.name) &&
                   type.Equals(other.type);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref type);
        }
    }
}