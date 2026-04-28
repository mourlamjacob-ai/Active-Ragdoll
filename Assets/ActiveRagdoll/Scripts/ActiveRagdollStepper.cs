using UnityEngine;

namespace ActiveRagdoll
{
    [DisallowMultipleComponent]
    public class ActiveRagdollStepper : MonoBehaviour
    {
        public ActiveRagdollController controller;
        public LayerMask groundMask = ~0;
        public float footLiftDuration = 0.16f;
        public float raycastHeight = 1.2f;
        public float raycastDistance = 2.5f;

        private float _cooldownUntil;
        private bool _leftIsSwinging;
        private float _swingTime;
        private Vector3 _swingStart;
        private Vector3 _swingEnd;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<ActiveRagdollController>();
            }
        }

        private void Update()
        {
            if (controller == null || controller.leftFootTarget == null || controller.rightFootTarget == null)
            {
                return;
            }

            if (_swingTime > 0f)
            {
                UpdateSwing();
                return;
            }

            if (Time.time < _cooldownUntil)
            {
                return;
            }

            if (NeedsStep(out bool useLeft))
            {
                BeginStep(useLeft);
            }
        }

        private bool NeedsStep(out bool useLeft)
        {
            Vector3 com = controller.CenterOfMass;
            Vector3 pred = controller.PredictedCom;
            Vector3 left = controller.leftFootTarget.position;
            Vector3 right = controller.rightFootTarget.position;
            Vector3 supportCenter = (left + right) * 0.5f;

            float supportRadius = Vector3.Distance(left, right) * 0.5f;
            float distPred = Vector2.Distance(new Vector2(pred.x, pred.z), new Vector2(supportCenter.x, supportCenter.z));
            float margin = supportRadius - distPred;

            useLeft = pred.x >= supportCenter.x;
            return margin < controller.minStabilityMargin;
        }

        private void BeginStep(bool useLeft)
        {
            _leftIsSwinging = useLeft;
            _swingTime = footLiftDuration;

            Transform footTarget = useLeft ? controller.leftFootTarget : controller.rightFootTarget;
            _swingStart = footTarget.position;

            Vector3 moveDir = controller.transform.forward;
            Vector3 predicted = controller.PredictedCom;
            Vector3 supportCenter = (controller.leftFootTarget.position + controller.rightFootTarget.position) * 0.5f;
            Vector3 correction = (predicted - supportCenter);
            correction.y = 0f;

            Vector3 target = footTarget.position + (moveDir * controller.baseStepDistance) + correction;
            Vector3 from = target + Vector3.up * raycastHeight;
            if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, raycastDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                target.y = hit.point.y;
            }

            Vector3 hipPoint = controller.hipsBody != null ? controller.hipsBody.worldCenterOfMass : controller.transform.position;
            Vector3 hipToTarget = target - hipPoint;
            hipToTarget.y = 0f;
            if (hipToTarget.magnitude > controller.maxStepDistance)
            {
                hipToTarget = hipToTarget.normalized * controller.maxStepDistance;
                target = new Vector3(hipPoint.x + hipToTarget.x, target.y, hipPoint.z + hipToTarget.z);
            }

            _swingEnd = target;
            _cooldownUntil = Time.time + controller.stepCooldown;
        }

        private void UpdateSwing()
        {
            Transform footTarget = _leftIsSwinging ? controller.leftFootTarget : controller.rightFootTarget;
            _swingTime -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(_swingTime / Mathf.Max(footLiftDuration, 0.01f));

            Vector3 basePos = Vector3.Lerp(_swingStart, _swingEnd, t);
            float lift = Mathf.Sin(t * Mathf.PI) * controller.baseStepHeight;
            footTarget.position = basePos + Vector3.up * lift;

            if (_swingTime <= 0f)
            {
                footTarget.position = _swingEnd;
                Vector3 flatForward = Vector3.ProjectOnPlane(controller.transform.forward, Vector3.up).normalized;
                if (flatForward.sqrMagnitude > 0.0001f)
                {
                    footTarget.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
                }
            }
        }
    }
}
