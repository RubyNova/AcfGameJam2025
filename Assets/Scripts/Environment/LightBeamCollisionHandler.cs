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
                    
                    
                    playerComponent.FlipCharacterSprite(transform.right.x >= 0);

                    var parentTransform = gameObject.GetComponentInParent<Transform>();

                    if(transform.rotation.z != 0)
                    {
                        playerComponent.RotateCharacterToBeam(transform.localEulerAngles);   
                    }
                    else if(parentTransform != null && parentTransform.rotation.z != 0)
                    {
                        playerComponent.RotateCharacterToBeam(parentTransform.localEulerAngles);   
                    }

                    playerComponent._rigidbody.MovePosition(collision.contacts[0].point);
                    _parentController.BeamModifierData.ApplyBeamEffect(_parentController,
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
        print($"PONG - {collision.gameObject.name} - {_parentController.BeamPriority}");
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

                var parentTransform = gameObject.GetComponentInParent<Transform>();
                var rotationReductionAmount = transform.localEulerAngles.z == 0 ? -parentTransform.localEulerAngles : -transform.localEulerAngles;

                if(playerComponent.JumpRequested)
                {
                    if(_isColliding)
                    {
                        _isColliding = false;
                        _parentController.BeamModifierData.ClearBeamEffect(_parentController, 
                            _parentController.BeamPriority, playerComponent);
                        _parentController.CurrentPlayer = null;
                    }

                    if(playerComponent.Grounded)
                    {
                        playerComponent.Grounded = false;
                    }

                    playerComponent.RotateCharacterToBeam(rotationReductionAmount);
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

                        playerComponent.RotateCharacterToBeam(rotationReductionAmount);
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
