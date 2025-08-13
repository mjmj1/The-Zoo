using System;
using Unity.Collections;
using Unity.Netcode;

namespace Players
{
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong ClientId;
        public FixedString32Bytes Name;
        public int AnimalIndex;

        public PlayerData(ulong id, FixedString32Bytes name, int index)
        {
            ClientId = id;
            Name = name;
            AnimalIndex = index;
        }

        public bool Equals(PlayerData other)
        {
            return ClientId == other.ClientId && Name.Equals(other.Name) && AnimalIndex.Equals(other.AnimalIndex);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref AnimalIndex);
        }
    }
}