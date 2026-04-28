using UnityEngine;

namespace ActiveRagdoll
{
    [DisallowMultipleComponent]
    public class ActiveRagdollIK : MonoBehaviour
    {
        public Animator animator;
        public Transform leftFootTarget;
        public Transform rightFootTarget;
        [Range(0f, 1f)] public float positionWeight = 1f;
        [Range(0f, 1f)] public float rotationWeight = 1f;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (animator == null)
            {
                return;
            }

            ApplyFoot(AvatarIKGoal.LeftFoot, leftFootTarget);
            ApplyFoot(AvatarIKGoal.RightFoot, rightFootTarget);
        }

        private void ApplyFoot(AvatarIKGoal goal, Transform target)
        {
            if (target == null)
            {
                animator.SetIKPositionWeight(goal, 0f);
                animator.SetIKRotationWeight(goal, 0f);
                return;
            }

            animator.SetIKPositionWeight(goal, positionWeight);
            animator.SetIKRotationWeight(goal, rotationWeight);
            animator.SetIKPosition(goal, target.position);
            animator.SetIKRotation(goal, target.rotation);
        }
    }
}
