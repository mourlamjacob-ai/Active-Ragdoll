Active Ragdoll (No Animation Clips) — Unity Setup Guide
This system creates a procedural active ragdoll that can:

-stand upright using physics

-keep balance using COM prediction

-decide when it needs to step

-place feet using IK targets

-recover from pushes without pre-made walk animations

-No animation clips are required for stepping behavior.

  Quick Start (30 Seconds)
-Create Wizard Settings asset (Assets → Create → Active Ragdoll → Wizard Settings).

-Double-click the settings asset to open the wizard.

-Assign:

  -Character Root

  -Animator

  -Hips Rigidbody

  -Left/Right Foot Bones

  -Left/Right Foot Targets

  -Ground Mask

  -Click Create Active Ragdoll.

Press Play and tune Standing Height, Step Distance, and Step Height first.

What This System Adds
The wizard adds these runtime components to your character root:

ActiveRagdollController (upright + height + balance core)

ActiveRagdollStepper (step trigger + swing movement)

ActiveRagdollIK (applies IK targets to feet)

If enabled, it can auto-create:

IK_Targets/LeftFootTarget

IK_Targets/RightFootTarget

Prerequisites
Before setup, make sure your character already has:

A valid character rig (with hips/pelvis, left foot, right foot bones)

Animator on the character root

Rigidbody/collider setup for physical interaction

Ground colliders in a ground layer (for raycast/foot placement)

Correct scale (roughly human-sized if using the default tuning ranges below)

Component Placement by Body Part
Use this exact checklist.

1) Character Root (top-level player object)
Assign or place:

Animator

ActiveRagdollController

ActiveRagdollStepper

ActiveRagdollIK

Also set this object as Character Root in wizard settings.

2) Hips / Pelvis
Assign:

Main hips/pelvis Rigidbody (used for balance + standing control)

Set this as Hips Rigidbody in settings.

3) Left Foot Bone
Assign:

Actual left foot rig bone transform

Set this as Left Foot Bone.

4) Right Foot Bone
Assign:

Actual right foot rig bone transform

Set this as Right Foot Bone.

5) IK Target Objects (separate helper transforms)
Use dedicated targets (not the foot bones themselves):

IK_Targets/LeftFootTarget

IK_Targets/RightFootTarget

Set these as Left Foot Target and Right Foot Target.

Tip: Start each target near the ground and slightly forward of the hips in a neutral standing pose.

Wizard Setup (Step by Step)
In Project window, create settings asset:
Assets → Create → Active Ragdoll → Wizard Settings

Name it (example: AR_Player_Settings).

Double-click the asset to open the wizard window.

Fill all required references.

Set tuning values (use starting values below).

Click Create Active Ragdoll.

Enter Play mode and tune in the recommended order.

Variable Guide (Plain Language + Starting Values)
Use these as safe starting points for a human-scale character.

Standing / Posture
Standing Height
Desired hips-to-ground idle height.
Start: 0.9 – 1.1

Upright Strength / Torque
How strongly the body tries to stay upright.
Too high = jitter/snappy corrections.
Too low = slouch/fall.

Stepping
Step Distance
How far balance can drift before taking a step.
Start: 0.25 – 0.45

Step Height
Arc height of the swing foot.
Start: 0.08 – 0.20

Step Duration / Step Speed
How quickly foot reaches target.
Start: 0.15 – 0.30 seconds

Balance
COM Prediction / Balance Sensitivity
How early the system reacts to imbalance.
Increase if it reacts too late.
Decrease if it constantly shuffles in place.

Foot Offsets
Forward Offset: move foot goals forward/back

Lateral Offset: widen/narrow stance

Vertical Offset: fix clipping/floating at contact

Grounding
Ground Mask
Must include only valid walkable ground layers.
If feet never find ground, this is the first thing to check.

Recommended Tuning Order (Important)
Tune in this order to avoid chaos:

Ground Mask and raycast hits

Standing Height

Upright Strength

Step Distance

Step Height + Step Duration

Foot offsets (final polish)

Play Mode Validation Checklist
In Play mode, verify all of these:

 Character stands without constant wobble

 Feet remain grounded (no long-term floating/clipping)

 Pushes cause a recovery step

 Feet do not both step at the same time repeatedly

 Feet don’t slide heavily after planting

If any item fails, return to the tuning order above.

Troubleshooting
Feet never step
Step Distance too large

Balance sensitivity too low

Foot bones/targets not assigned

Ground Mask incorrect

Character jittering in place
Upright Strength too high

Step trigger too sensitive

Ground raycasts hitting noisy colliders

Feet slide after planting
Step Duration too slow

Offsets too aggressive

Target updates too unstable

Feet float or clip through floor
Vertical offset not tuned

Ground detection mismatch

Character scale vs standing height mismatch

Character collapses or crouches too much
Standing Height too low

Upright Strength too weak

Hips Rigidbody missing/wrong reference

Minimum Required Assignments (Must Have)
At minimum, assign these before pressing Create:

Character Root

Animator

Hips Rigidbody

Left Foot Bone

Right Foot Bone

Left Foot Target

Right Foot Target

Ground Mask

Standing Height

Step Distance

Step Height

Notes
This is a procedural system; quality depends heavily on tuning.

Start conservative, then increase responsiveness.

Small parameter changes can have large behavior effects.

Save a few settings presets as you tune (stable / responsive / aggressive).

