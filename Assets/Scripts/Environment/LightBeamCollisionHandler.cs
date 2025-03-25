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
                if(!_isColliding)
                {
                    _isColliding = true;
                    if(transform.rotation.z != 0)
                    {
                        playerComponent.RotateCharacter(transform.localEulerAngles);   
                    }

                    playerComponent._rigidbody.MovePosition(collision.contacts[0].point);

                    _parentController.BeamModifierData.ApplyBeamEffect(_parentController,
                        _parentController.BeamPriority, 
                        playerComponent, 
                        transform.right);
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
                if(playerComponent.JumpRequested)
                {
                    if(_isColliding)
                    {
                        _isColliding = false;
                        _parentController.BeamModifierData.ClearBeamEffect(_parentController, 
                            _parentController.BeamPriority, playerComponent);
                    }

                    if(playerComponent.Grounded)
                    {
                        playerComponent.Grounded = false;
                    }

                    playerComponent.RotateCharacter(-transform.localEulerAngles);
                }
                else
                {
                    if(playerComponent.transform.position.x > _boxCollider.bounds.max.x ||
                    playerComponent.transform.position.x < _boxCollider.bounds.min.x)
                    {
                        if(_isColliding)
                        {
                            _isColliding = false;
                            _parentController.BeamModifierData.ClearBeamEffect(_parentController,
                                _parentController.BeamPriority, playerComponent);
                        }

                        if(playerComponent.Grounded)
                        {
                            playerComponent.Grounded = false;
                        }

                        playerComponent.RotateCharacter(-transform.localEulerAngles);
                        if(playerComponent.BeamCollisionCount > 1)
                        {
                            playerComponent.AddLinearVelocity(gameObject.GetHashCode(), 
                                _parentController.BeamModifierData.BeamForce * transform.right );
                        }
                        else
                        {
                            playerComponent.AddLinearVelocityRaw(_parentController.BeamModifierData.BeamForce * transform.right );
                        }
                    } 
                }
            }
        }
        
    }

}
