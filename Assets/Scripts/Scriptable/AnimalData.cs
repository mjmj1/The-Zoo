using UnityEngine;

namespace Scriptable
{
    public enum AnimalType
    {
        Cat,
        Dog,
        Dove,
        Emu,
        Gecko,
        Kookaburra,
        Mouse,
        Muskrat,
        Parrot,
        Pigeon,
        Platypus,
        Possum,
        Pudu,
        Quokka,
        Rabbit,
        Sparrow,
        TasmanianDevil,
        Tortoise,
        Wombat,
    }

    [CreateAssetMenu(fileName = "AnimalData", menuName = "Game/Animals", order = 0)]
    public class AnimalData : ScriptableObject
    {
        public AnimalType type;
        public GameObject playerPrefab;
        public GameObject npcPrefab;
    }
}