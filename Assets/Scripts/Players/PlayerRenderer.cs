using UnityEngine;
using Utils;

namespace Players
{
    public class PlayerRenderer : MonoBehaviour
    {
        [SerializeField] private Shader ghostShader;

        private Shader originShader;

        private LOD[] lods;
        private MaterialPropertyBlock mpb;

        private void OnEnable()
        {
            lods = GetComponent<LODGroup>().GetLODs();
            originShader = lods[0].renderers[0].sharedMaterial.shader;
        }

        internal void UseOrigin()
        {
            lods[0].renderers[0].sharedMaterial.shader = originShader;
        }

        internal void UseGhost()
        {
            MyLogger.Print(this, "UseGhost");
            lods[0].renderers[0].sharedMaterial.shader = ghostShader;
        }
    }
}