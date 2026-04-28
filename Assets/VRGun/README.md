# VR Gun Controller (Customizable, Physics-Friendly)

This package gives you a procedural VR gun setup with:

- Recoil (visual + rigidbody impulse)
- Semi/Auto/Burst fire modes
- Spread, fire rate, range, hit impulse, and damage variables
- Removable/insertable magazines
- Slide rack/cock workflow for chambering
- Optional auto insert via trigger socket

## Included Scripts

- `VRGunController` - core firing/recoil/chamber/magazine logic
- `VRGunMagazine` - magazine ammo + rigidbody helper methods
- `VRGunSlide` - procedural slide pull + release/cock behavior
- `VRGunMagazineSocket` - optional trigger-based magazine insertion

## Basic Setup

1. Create a gun prefab root and add:
   - `Rigidbody` (optional if your gun is fully kinematic in your grab system)
   - `VRGunController`
2. Assign on `VRGunController`:
   - `Muzzle` transform
   - `Magazine Socket` transform
   - `Primary Hand Body` and optional `Support Hand Body` rigidbodies
3. Create a magazine prefab and add:
   - `VRGunMagazine`
   - `Rigidbody`
   - one or more colliders
4. Add a trigger collider near magazine well with `VRGunMagazineSocket` if you want auto insertion.
5. Add `VRGunSlide` to slide object and assign the gun reference.

## Runtime Calls You Can Hook to VR Input

- Trigger down/up: `SetTriggerHeld(true/false)`
- Safety toggle button: `ToggleSafety()`
- Mag release button: `ReleaseMagazine(ejectVelocity)`
- Slide release action: `ReleaseSlide()` (from `VRGunSlide`)
- Manual slide pull amount: `SetPullAmount(0..1)` (from `VRGunSlide`)

## Reload Flow

1. Press mag release -> `ReleaseMagazine()`
2. Insert fresh mag by pushing into socket (auto) or calling `TryInsertMagazine(mag)`
3. Rack slide -> `RackSlide()` or `VRGunSlide.ReleaseSlide()`
4. Gun now has chambered round and can fire

## Important Notes

- `chamberLoaded` must be true to shoot.
- By default, shooting consumes chamber then auto attempts to chamber from inserted mag.
- `ApplyDamage(float)` is sent via `SendMessage` to hit colliders (optional receiver).
- Tune recoil and spread per weapon profile in inspector.
