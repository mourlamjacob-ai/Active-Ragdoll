using UnityEngine;

namespace ActiveRagdoll
{
    [DisallowMultipleComponent]
    public class ActiveRagdollController : MonoBehaviour
    {
        [Header("Core References")]
        public Animator animator;
        public Rigidbody hipsBody;
        public Transform bodyUpReference;

        [Header("Foot Targets")]
        public Transform leftFootTarget;
        public Transform rightFootTarget;

        [Header("Posture")]
        public float standingHeight = 1.0f;
        public float pelvisSpring = 250f;
        public float pelvisDamping = 35f;
        public float uprightTorque = 120f;

        [Header("Balance + Step")]
        public float minStabilityMargin = 0.06f;
        public float comPredictionTime = 0.20f;
        public float baseStepDistance = 0.26f;
        public float baseStepHeight = 0.09f;
        public float maxStepDistance = 0.55f;
        public float stepCooldown = 0.15f;

        private Vector3 _lastHipsPosition;
        private Vector3 _hipsVelocity;

        public Vector3 CenterOfMass => hipsBody != null ? hipsBody.worldCenterOfMass : transform.position;
        public Vector3 PredictedCom => CenterOfMass + new Vector3(_hipsVelocity.x, 0f, _hipsVelocity.z) * comPredictionTime;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (hipsBody != null)
            {
                _lastHipsPosition = hipsBody.worldCenterOfMass;
            }
        }

        private void FixedUpdate()
        {
            if (hipsBody == null)
            {
                return;
            }

            Vector3 current = hipsBody.worldCenterOfMass;
            _hipsVelocity = (current - _lastHipsPosition) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            _lastHipsPosition = current;

            ApplyUprightTorque();
            ApplyStandingHeight();
        }

        private void ApplyStandingHeight()
        {
            if (hipsBody == null)
            {
                return;
            }

            float groundY = transform.position.y;
            float error = standingHeight - (hipsBody.worldCenterOfMass.y - groundY);
            float velY = hipsBody.velocity.y;
            float forceY = (error * pelvisSpring) - (velY * pelvisDamping);
            hipsBody.AddForce(Vector3.up * forceY, ForceMode.Acceleration);
        }

        private void ApplyUprightTorque()
        {
            if (hipsBody == null)
            {
                return;
            }

            Vector3 desiredUp = bodyUpReference != null ? bodyUpReference.up : Vector3.up;
            Vector3 axis = Vector3.Cross(transform.up, desiredUp);
            if (axis.sqrMagnitude < 0.000001f)
            {
                return;
            }

            float angle = Vector3.SignedAngle(transform.up, desiredUp, axis);
            Vector3 correctiveTorque = axis.normalized * (angle * uprightTorque * Mathf.Deg2Rad);
            hipsBody.AddTorque(correctiveTorque, ForceMode.Acceleration);
        }
    }
}
