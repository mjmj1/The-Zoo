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
            foreach (var lod in lods)
            {
                lod.renderers[0].material.shader = originShader;
            }
        }

        internal void UseGhost()
        {
            foreach (var lod in lods)
            {
                lod.renderers[0].material.shader = ghostShader;
            }
        }
    }
}