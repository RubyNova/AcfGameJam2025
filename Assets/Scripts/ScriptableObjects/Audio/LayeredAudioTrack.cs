using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Audio
{
    [CreateAssetMenu(fileName = "NewLayeredAudioTrack", menuName = "Audio / Layered Audio Track")]
    public class LayeredAudioTrack : ScriptableObject
    {
        [SerializeField]
        private List<AudioClip> _layers;

        public IReadOnlyList<AudioClip> LayeredTracks => _layers;
    }
}