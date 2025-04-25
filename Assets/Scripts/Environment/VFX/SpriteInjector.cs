using System;
using UnityEditor.Sprites;
using UnityEngine;

namespace Environment.VFX
{
    [Serializable]
    public class SpriteMaterialInputPair
    {
        public Sprite sprite;
        public string materialInputName;
    }

    public class SpriteInjector : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _targetRenderer;

        [SerializeField]
        private SpriteMaterialInputPair[] _retargetingData;

        void Start()
        {   
            var materialInstance = _targetRenderer.material;

            foreach (var pair in _retargetingData)
            {
                var newUVs = UnityEngine.Sprites.DataUtility.GetOuterUV(pair.sprite);
                var tiling = new Vector2(newUVs.z - newUVs.x, newUVs.w - newUVs.y);
                var offset = new Vector2(newUVs.x, newUVs.y);

                materialInstance.SetVector(pair.materialInputName + "_UVRect", new Vector4(tiling.x, tiling.y, offset.x, offset.y));
                materialInstance.SetTexture(pair.materialInputName, SpriteUtility.GetSpriteTexture(pair.sprite, false));
            }
        }

    }
}