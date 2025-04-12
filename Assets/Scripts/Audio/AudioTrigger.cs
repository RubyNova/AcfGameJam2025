using System;
using Managers;
using UnityEngine;

namespace Audio 
{
    public class AudioTrigger : MonoBehaviour
    {
        public int LayerNumber;

        public void FadeDesignatedLayerIn()
        {
            AudioManager.Instance.AddLayer.Invoke(LayerNumber);
        }

        public void FadeDesignatedLayerOut()
        {
            AudioManager.Instance.RemoveLayer.Invoke(LayerNumber);
        }
    }
}