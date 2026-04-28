using UnityEngine;

namespace ActiveRagdoll
{
    [CreateAssetMenu(fileName = "ActiveRagdollWizardSettings", menuName = "Active Ragdoll/Wizard Settings")]
    public class ActiveRagdollWizardSettings : ScriptableObject
    {
        [Header("Target")]
        public GameObject characterRoot;
        public Animator animator;
        public Rigidbody hipsBody;

        [Header("Foot Targets")]
        public Transform leftFootTarget;
        public Transform rightFootTarget;

        [Header("Auto-create Foot Targets")]
        public bool autoCreateFootTargets = true;
        public Vector3 leftFootOffset = new Vector3(-0.1f, 0f, 0.08f);
        public Vector3 rightFootOffset = new Vector3(0.1f, 0f, 0.08f);

        [Header("Controller Values")]
        public float standingHeight = 1f;
        public float pelvisSpring = 250f;
        public float pelvisDamping = 35f;
        public float uprightTorque = 120f;
        public float minStabilityMargin = 0.06f;
        public float predictionTime = 0.2f;
        public float stepDistance = 0.26f;
        public float stepHeight = 0.09f;
        public float maxStepDistance = 0.55f;
        public float stepCooldown = 0.15f;

        [Header("Ground")]
        public LayerMask groundMask = ~0;
    }
}
