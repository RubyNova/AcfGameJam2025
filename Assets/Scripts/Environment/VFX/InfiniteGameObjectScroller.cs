using System;
using UnityEngine;

namespace Environment.VFX
{
    [Serializable]
    public class ObjectMoveVectorPair
    {
        public GameObject target;
        public Vector2 moveVelocity;

        public void Deconstruct(out GameObject target, out Vector2 moveVelocity)
        {
            target = this.target;
            moveVelocity = this.moveVelocity;
        }
    }

    public class InfiniteGameObjectScroller : MonoBehaviour
    {
        [SerializeField]
        private ObjectMoveVectorPair[] _moveData;

        // Update is called once per frame
        void Update()
        {
            foreach (var (target, MoveVelocity) in _moveData)
            {
                target.transform.Translate(MoveVelocity * Time.deltaTime);
            }
        }
    }
}