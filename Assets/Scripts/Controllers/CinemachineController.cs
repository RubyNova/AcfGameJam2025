using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineController : MonoBehaviour
{
    public const string SWAP_FUNCTION = "CinemachineSwapCameras";

    [SerializeField]
    private Animator _animator;
    private InputAction _playerZoomOut;
    private InputAction _familiarZoomOut;

    [SerializeField]
    private int _viewState = 1;
    [SerializeField]
    private int _previousState = 0;


    void Start()
    {
        var playerActions = InputSystem.actions.FindActionMap("Player");
        var familiarActions = InputSystem.actions.FindActionMap("Familiar");
        if(playerActions != null)
        {
            _playerZoomOut = playerActions.FindAction("ZoomOut");
        }
        if(familiarActions != null)
        {
            _familiarZoomOut = familiarActions.FindAction("ZoomOut");
        }
    }

    void Update()
    {
        if(_playerZoomOut != null && _viewState == 3 && _previousState == 1)
        {
            if(!_playerZoomOut.IsPressed())
            {
                Debug.Log("Swapping to player...");
                CinemachineSwapCameras(1);
            }
        }
        
        if(_familiarZoomOut != null && _viewState == 3  && _previousState == 2)
        {
            if(!_familiarZoomOut.IsPressed())
            {
                Debug.Log("Swapping to familiar...");
                CinemachineSwapCameras(2);
            }
        }
    }

    public void CinemachineSwapCameras(int cameraNumber)
    {
        if(cameraNumber != _viewState)
        {
            _previousState = _viewState;
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
            _viewState = cameraNumber;
        }
    }
}
