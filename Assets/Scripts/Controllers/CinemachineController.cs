using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    public const string SWAP_FUNCTION = "CinemachineSwapCameras";

    [SerializeField]
    private Animator _animator;

    public void CinemachineSwapCameras(int cameraNumber)
    {
        switch(cameraNumber)
        {
            case 1:
            {
                _animator.Play("PlayerCamera");
                break;
            }
            case 2:
            {
                _animator.Play("FamiliarCamera");
                break;
            }
            case 3:
            {
                _animator.Play("OverworldCamera");
                break;
            }
        }
    }
}
