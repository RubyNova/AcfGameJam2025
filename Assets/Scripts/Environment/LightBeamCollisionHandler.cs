using Controllers;
using Environment;
using UnityEngine;

public class LightBeamCollisionHandler : MonoBehaviour
{
    [Header("Dependencies")]

    [SerializeField]
    private LightBeamController _parentController;

    [SerializeField]
    private Collider2D _boxCollider;

    private bool _isColliding = false;
    

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            var playerComponent = collision.gameObject.GetComponent<PlayerController>();
            if(playerComponent != null)
            {
                var cachedBeamOnPlayer = playerComponent.GetCachedBeamData();

                if(cachedBeamOnPlayer != null && cachedBeamOnPlayer.Priority > _parentController.BeamPriority)
                {
                    //fuck right off
                    return;
                }

                if(!_isColliding)
                {
                    _isColliding = true;

                    playerComponent.FlipCharacterSprite(_parentController.BeamTransform.right.x >= 0);

                    if(playerComponent.transform.rotation.z == 0 || !playerComponent.Triggered)
                    {
                        playerComponent._rigidbody.linearVelocity = Vector2.zero;
                        Transform parentTransform = gameObject.GetComponentInParent<Transform>();
                        Vector3 angles = Vector3.zero;
                        if(transform.localEulerAngles.z != 0)
                        {
                            angles = transform.localEulerAngles.z > 180 ? transform.eulerAngles : transform.localEulerAngles;
                        }
                        else if(parentTransform != null && parentTransform.eulerAngles.z != 0)
                        {
                            angles = parentTransform.eulerAngles;
                        }
                        playerComponent.RotateCharacterToBeam(angles);
                    }
                    
                    _parentController.BeamModifierData.ApplyBeamEffectToPlayer(_parentController,
                        _parentController.BeamPriority, 
                        playerComponent, 
                        transform.right);
                    _parentController.CurrentPlayer = playerComponent;
                }

                playerComponent.Grounded = true;
            }
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            var playerComponent = collision.gameObject.GetComponent<PlayerController>();
            if(playerComponent != null)
            {
                var cachedBeamOnPlayer = playerComponent.GetCachedBeamData();

                _parentController.BeamModifierData.ClearBeamEffectOnPlayer(_parentController, 
                    _parentController.BeamPriority, playerComponent);
                _parentController.CurrentPlayer = null;

                if(cachedBeamOnPlayer != null && cachedBeamOnPlayer.Priority > _parentController.BeamPriority && playerComponent.BeamCollisionCount > 1)
                {
                    //fuck right off
                    return;
                }

                if(playerComponent.JumpRequested)
                {
                    if(_isColliding)
                    {
                        _isColliding = false;
                    }

                    if(playerComponent.Grounded)
                    {
                        playerComponent.Grounded = false;
                    }
                }
                else
                {
                    if(playerComponent.transform.position.x > _boxCollider.bounds.max.x ||
                    playerComponent.transform.position.x < _boxCollider.bounds.min.x)
                    {
                        if(_isColliding)
                        {
                            _isColliding = false;
                        }

                        if(playerComponent.Grounded)
                        {
                            playerComponent.Grounded = false;
                        }
                    } 
                }
            }
        }
        
    }

}
