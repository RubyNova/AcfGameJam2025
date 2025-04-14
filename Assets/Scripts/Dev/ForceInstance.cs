using Managers;
using UnityEngine;

public class ForceInstance : MonoBehaviour
{
    
    private AudioManager _audioManager;

    void Awake()
    {
        if(Debug.isDebugBuild)
        {
            _audioManager = AudioManager.Instance;
        }
    }

    
}
