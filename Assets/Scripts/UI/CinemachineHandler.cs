using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CinemachineHandler : MonoBehaviour
{
    public float Value;
    public CinemachineVolumeSettings settings;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
        var profile = settings.Profile;
        profile.TryGet(out ColorAdjustments colorAdjustments);
        colorAdjustments.saturation.value = Value;
        settings.Profile = profile;
        }
    }
}