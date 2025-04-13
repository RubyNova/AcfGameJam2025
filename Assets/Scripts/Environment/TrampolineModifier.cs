using Controllers;
using UnityEngine;

namespace Environment
{
    public class TrampolineModifier : LightBeamModifier
    {
        PhysicsMaterial2D bounceMaterial;


        protected void Awake()
        {
            bounceMaterial = new PhysicsMaterial2D();
            bounceMaterial.bounciness = 3.0f;
            bounceMaterial.friction = 0.5f;   
        }

        protected void Start()
        {
            
        }

        public override void Initialise(LightBeamController sender)
        {
            if(bounceMaterial != null)
                sender.BoxCollider.sharedMaterial = bounceMaterial;
        }

        public override void Shutdown(LightBeamController sender)
        {
            sender.BoxCollider.sharedMaterial = null;
        }

        public override void ApplyBeamEffectToPlayer(LightBeamController sender, int beamPriority, PlayerController player, Vector2 senderBeamDirection)
        {
            // if (player.gameObject.layer != _ghostedLayer)
            // {
            //     player.gameObject.layer = _ghostedLayer;
            // }

             base.ApplyBeamEffectToPlayer(sender, beamPriority, player, senderBeamDirection);
        }

        public override void ClearBeamEffectOnPlayer(LightBeamController sender, int beamPriority, PlayerController player)
        {
            // player.gameObject.layer = _defaultLayer; 
             base.ClearBeamEffectOnPlayer(sender, beamPriority, player);
        }
    }
}