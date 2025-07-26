using System;
using Unity.Collections;
using Unity.Netcode;

namespace Players
{
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong ClientId;
        public FixedString32Bytes Name;

        public PlayerData(ulong id, FixedString32Bytes name)
        {
            ClientId = id;
            Name = name;
        }

        public bool Equals(PlayerData other)
        {
            return ClientId == other.ClientId && Name.Equals(other.Name);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Name);
        }
    }
}