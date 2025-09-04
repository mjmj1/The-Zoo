using System;
using Unity.Collections;
using Unity.Netcode;

namespace Players
{
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong clientId;
        public FixedString32Bytes name;
        public int animalIndex;

        public PlayerData(ulong id, FixedString32Bytes name, int index)
        {
            clientId = id;
            this.name = name;
            animalIndex = index;
        }

        public bool Equals(PlayerData other)
        {
            return clientId == other.clientId && name.Equals(other.name) &&
                   animalIndex.Equals(other.animalIndex);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref animalIndex);
        }
    }
}