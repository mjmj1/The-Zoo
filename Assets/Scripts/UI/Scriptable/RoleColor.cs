using UnityEngine;

namespace UI.Scriptable
{
    [CreateAssetMenu(fileName = "RoleColorData", menuName = "UI/Player/RoleColors", order = 0)]
    public class RoleColor : ScriptableObject
    {
        [Header("역할 별 색상")]
        public Color hiderColors;
        public Color seekerColors;
    }
}