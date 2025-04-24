using System;
using UnityEngine;

namespace Environment.VFX
{
    [Serializable]
    public class ParameterMaterialData
    {
        public SpriteRenderer target;
        public string parameterName;
        public Texture[] textures;
    }


    public class MaterialValueDriverTexture : MonoBehaviour
    {
        [SerializeField]
        private ParameterMaterialData _data;

        public void SetTexture(int index)
        {
            _data.target.material.SetTexture(_data.parameterName, _data.textures[index]);
        }
    }
}