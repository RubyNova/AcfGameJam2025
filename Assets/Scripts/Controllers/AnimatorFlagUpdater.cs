using UnityEngine;


namespace Environment
{
    public class AnimatorFlagUpdater : MonoBehaviour
    {
        [SerializeField]
        Animator _animatorToDrive;

        public void ToggleBoolean(string name) => _animatorToDrive.SetBool(name, !_animatorToDrive.GetBool(name));
    }
}