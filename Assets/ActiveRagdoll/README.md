# Active Ragdoll Wizard (Unity)

1. Create a settings asset: `Assets -> Create -> Active Ragdoll -> Wizard Settings`.
2. Double-click the settings asset to open the wizard window.
3. Fill references/variables (character root, animator, hips rigidbody, step values, foot target offsets).
4. Click **Create Active Ragdoll**.

The wizard adds:
- `ActiveRagdollController`
- `ActiveRagdollIK`
- `ActiveRagdollStepper`

If foot targets are missing and auto-create is enabled, it creates:
- `IK_Targets/LeftFootTarget`
- `IK_Targets/RightFootTarget`
