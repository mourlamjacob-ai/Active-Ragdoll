using UnityEditor;
using UnityEngine;

namespace ActiveRagdoll.Editor
{
    public static class ActiveRagdollWizardBuilder
    {
        public static void BuildFromSettings(ActiveRagdollWizardSettings settings)
        {
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Active Ragdoll Wizard", "Missing settings asset.", "OK");
                return;
            }

            if (settings.characterRoot == null)
            {
                EditorUtility.DisplayDialog("Active Ragdoll Wizard", "Assign Character Root in settings.", "OK");
                return;
            }

            GameObject root = settings.characterRoot;
            Animator animator = settings.animator != null ? settings.animator : root.GetComponent<Animator>();
            Rigidbody hips = settings.hipsBody;

            if (hips == null)
            {
                hips = root.GetComponentInChildren<Rigidbody>();
            }

            if (animator == null)
            {
                EditorUtility.DisplayDialog("Active Ragdoll Wizard", "No Animator found. Assign one in settings.", "OK");
                return;
            }

            if (hips == null)
            {
                EditorUtility.DisplayDialog("Active Ragdoll Wizard", "No Rigidbody found for hips. Assign one in settings.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(root, "Create Active Ragdoll");

            EnsureTargets(settings, animator, root.transform, out Transform leftTarget, out Transform rightTarget);

            ActiveRagdollController controller = root.GetComponent<ActiveRagdollController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<ActiveRagdollController>(root);
            }

            controller.animator = animator;
            controller.hipsBody = hips;
            controller.leftFootTarget = leftTarget;
            controller.rightFootTarget = rightTarget;
            controller.standingHeight = settings.standingHeight;
            controller.pelvisSpring = settings.pelvisSpring;
            controller.pelvisDamping = settings.pelvisDamping;
            controller.uprightTorque = settings.uprightTorque;
            controller.minStabilityMargin = settings.minStabilityMargin;
            controller.comPredictionTime = settings.predictionTime;
            controller.baseStepDistance = settings.stepDistance;
            controller.baseStepHeight = settings.stepHeight;
            controller.maxStepDistance = settings.maxStepDistance;
            controller.stepCooldown = settings.stepCooldown;

            ActiveRagdollIK ik = root.GetComponent<ActiveRagdollIK>();
            if (ik == null)
            {
                ik = Undo.AddComponent<ActiveRagdollIK>(root);
            }

            ik.animator = animator;
            ik.leftFootTarget = leftTarget;
            ik.rightFootTarget = rightTarget;

            ActiveRagdollStepper stepper = root.GetComponent<ActiveRagdollStepper>();
            if (stepper == null)
            {
                stepper = Undo.AddComponent<ActiveRagdollStepper>(root);
            }

            stepper.controller = controller;
            stepper.groundMask = settings.groundMask;

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            EditorUtility.DisplayDialog("Active Ragdoll Wizard", "Active ragdoll components created successfully.", "OK");
        }

        private static void EnsureTargets(ActiveRagdollWizardSettings settings, Animator animator, Transform root, out Transform leftTarget, out Transform rightTarget)
        {
            leftTarget = settings.leftFootTarget;
            rightTarget = settings.rightFootTarget;

            if (!settings.autoCreateFootTargets && leftTarget != null && rightTarget != null)
            {
                return;
            }

            Transform targetRoot = root.Find("IK_Targets");
            if (targetRoot == null)
            {
                GameObject go = new GameObject("IK_Targets");
                Undo.RegisterCreatedObjectUndo(go, "Create IK target root");
                targetRoot = go.transform;
                targetRoot.SetParent(root, false);
            }

            if (leftTarget == null)
            {
                leftTarget = CreateOrFindTarget(targetRoot, "LeftFootTarget");
            }

            if (rightTarget == null)
            {
                rightTarget = CreateOrFindTarget(targetRoot, "RightFootTarget");
            }

            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            if (leftFoot != null)
            {
                leftTarget.position = leftFoot.position + settings.leftFootOffset;
                leftTarget.rotation = leftFoot.rotation;
            }

            if (rightFoot != null)
            {
                rightTarget.position = rightFoot.position + settings.rightFootOffset;
                rightTarget.rotation = rightFoot.rotation;
            }

            settings.leftFootTarget = leftTarget;
            settings.rightFootTarget = rightTarget;
        }

        private static Transform CreateOrFindTarget(Transform parent, string targetName)
        {
            Transform existing = parent.Find(targetName);
            if (existing != null)
            {
                return existing;
            }

            GameObject go = new GameObject(targetName);
            Undo.RegisterCreatedObjectUndo(go, "Create IK foot target");
            go.transform.SetParent(parent, false);
            return go.transform;
        }
    }
}
