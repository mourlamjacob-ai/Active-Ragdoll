using UnityEngine;
using UnityEngine.Events;

namespace VRGun
{
    public enum VRGunFireMode
    {
        Semi,
        Auto,
        Burst
    }

    [DisallowMultipleComponent]
    public class VRGunController : MonoBehaviour
    {
        [Header("Core References")]
        public Transform muzzle;
        public Rigidbody gunBody;
        public Rigidbody primaryHandBody;
        public Rigidbody supportHandBody;

        [Header("Magazine + Chamber")]
        public Transform magazineSocket;
        public VRGunMagazine insertedMagazine;
        public bool chamberLoaded;
        public bool allowChamberingOnInsert;

        [Header("Fire Mode")]
        public VRGunFireMode fireMode = VRGunFireMode.Semi;
        [Min(60f)] public float roundsPerMinute = 600f;
        [Range(1, 8)] public int burstCount = 3;

        [Header("Ballistics")]
        [Min(0.5f)] public float range = 120f;
        [Min(0f)] public float baseSpreadDegrees = 1.2f;
        [Min(0f)] public float movingSpreadMultiplier = 1.5f;
        [Min(0f)] public float maxSpreadDegrees = 8f;
        public LayerMask hitMask = ~0;

        [Header("Damage + Hit Impulse")]
        [Min(0f)] public float damage = 20f;
        [Min(0f)] public float hitImpulse = 12f;
        [Min(0f)] public float hitUpwardModifier = 0.2f;

        [Header("Recoil")]
        [Min(0f)] public float kickBackDistance = 0.05f;
        [Min(0f)] public float kickUpDegrees = 6f;
        [Min(0f)] public float recoilReturnSpeed = 15f;
        [Min(0f)] public float recoilSnappiness = 25f;
        [Min(0f)] public float handRecoilImpulse = 1.0f;

        [Header("Runtime State")]
        public bool safetyOn;
        public bool triggerHeld;
        [SerializeField] private int _shotsLeftInBurst;

        [Header("Events")]
        public UnityEvent onShotFired;
        public UnityEvent onDryFire;
        public UnityEvent onMagazineInserted;
        public UnityEvent onMagazineReleased;
        public UnityEvent onSlideRacked;

        private float _nextAllowedFireTime;
        private Vector3 _targetRecoilPosition;
        private Vector3 _currentRecoilPosition;
        private Quaternion _targetRecoilRotation = Quaternion.identity;
        private Quaternion _currentRecoilRotation = Quaternion.identity;
        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;

        public bool HasMagazine => insertedMagazine != null;
        public bool CanShoot => !safetyOn && chamberLoaded;
        public float SecondsPerShot => 60f / Mathf.Max(1f, roundsPerMinute);

        private void Awake()
        {
            if (gunBody == null)
            {
                gunBody = GetComponent<Rigidbody>();
            }

            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            HandleContinuousFire();
            UpdateRecoilVisual();
        }

        public void SetTriggerHeld(bool isHeld)
        {
            triggerHeld = isHeld;

            if (!triggerHeld)
            {
                _shotsLeftInBurst = 0;
                return;
            }

            if (fireMode == VRGunFireMode.Burst && _shotsLeftInBurst <= 0)
            {
                _shotsLeftInBurst = burstCount;
            }

            if (fireMode == VRGunFireMode.Semi)
            {
                TryShoot();
            }
        }

        public bool TryShoot()
        {
            if (Time.time < _nextAllowedFireTime)
            {
                return false;
            }

            _nextAllowedFireTime = Time.time + SecondsPerShot;

            if (!CanShoot)
            {
                onDryFire?.Invoke();
                return false;
            }

            FireBallisticTrace();
            ConsumeChamberAndCycle();
            ApplyRecoil();
            onShotFired?.Invoke();
            return true;
        }

        public void RackSlide()
        {
            onSlideRacked?.Invoke();

            if (chamberLoaded)
            {
                chamberLoaded = false;
            }

            TryChamberRoundFromMagazine();
        }

        public bool TryInsertMagazine(VRGunMagazine magazine)
        {
            if (magazine == null || magazineSocket == null || insertedMagazine != null)
            {
                return false;
            }

            insertedMagazine = magazine;
            insertedMagazine.transform.SetParent(magazineSocket, true);
            insertedMagazine.transform.position = magazineSocket.position;
            insertedMagazine.transform.rotation = magazineSocket.rotation;
            insertedMagazine.SetKinematic(true);
            insertedMagazine.SetCollidersEnabled(false);

            if (allowChamberingOnInsert && !chamberLoaded)
            {
                TryChamberRoundFromMagazine();
            }

            onMagazineInserted?.Invoke();
            return true;
        }

        public VRGunMagazine ReleaseMagazine(Vector3 ejectVelocity)
        {
            if (insertedMagazine == null)
            {
                return null;
            }

            VRGunMagazine released = insertedMagazine;
            insertedMagazine = null;

            released.transform.SetParent(null, true);
            released.SetCollidersEnabled(true);
            released.SetKinematic(false);

            if (released.magazineBody != null)
            {
                released.magazineBody.AddForce(ejectVelocity, ForceMode.VelocityChange);
            }

            onMagazineReleased?.Invoke();
            return released;
        }

        public void ToggleSafety()
        {
            safetyOn = !safetyOn;
        }

        private void HandleContinuousFire()
        {
            if (!triggerHeld)
            {
                return;
            }

            if (fireMode == VRGunFireMode.Auto)
            {
                TryShoot();
                return;
            }

            if (fireMode == VRGunFireMode.Burst && _shotsLeftInBurst > 0)
            {
                if (TryShoot())
                {
                    _shotsLeftInBurst--;
                }
            }
        }

        private void FireBallisticTrace()
        {
            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            Vector3 direction = muzzle != null ? muzzle.forward : transform.forward;
            direction = ApplySpread(direction);

            if (!Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Rigidbody hitBody = hit.rigidbody;
            if (hitBody != null)
            {
                Vector3 impulse = direction * hitImpulse;
                impulse += Vector3.up * (hitImpulse * hitUpwardModifier);
                hitBody.AddForceAtPosition(impulse, hit.point, ForceMode.Impulse);
            }

            if (hit.collider != null)
            {
                hit.collider.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        private Vector3 ApplySpread(Vector3 baseDirection)
        {
            float handSpeed = primaryHandBody != null ? primaryHandBody.velocity.magnitude : 0f;
            float movementFactor = 1f + (handSpeed * movingSpreadMultiplier * 0.1f);
            float spread = Mathf.Min(maxSpreadDegrees, baseSpreadDegrees * movementFactor);

            Quaternion spreadRot = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            );

            return spreadRot * baseDirection;
        }

        private void ConsumeChamberAndCycle()
        {
            chamberLoaded = false;
            TryChamberRoundFromMagazine();
        }

        private bool TryChamberRoundFromMagazine()
        {
            if (insertedMagazine == null)
            {
                return false;
            }

            if (!insertedMagazine.TryConsumeRound())
            {
                return false;
            }

            chamberLoaded = true;
            return true;
        }

        private void ApplyRecoil()
        {
            _targetRecoilPosition += Vector3.back * kickBackDistance;
            _targetRecoilRotation *= Quaternion.Euler(-kickUpDegrees, Random.Range(-0.65f, 0.65f), 0f);

            if (gunBody != null)
            {
                gunBody.AddForce(-transform.forward * handRecoilImpulse, ForceMode.Impulse);
            }

            if (primaryHandBody != null)
            {
                primaryHandBody.AddForce(-transform.forward * handRecoilImpulse, ForceMode.Impulse);
            }

            if (supportHandBody != null)
            {
                supportHandBody.AddForce(-transform.forward * (handRecoilImpulse * 0.5f), ForceMode.Impulse);
            }
        }

        private void UpdateRecoilVisual()
        {
            _targetRecoilPosition = Vector3.Lerp(_targetRecoilPosition, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
            _targetRecoilRotation = Quaternion.Slerp(_targetRecoilRotation, Quaternion.identity, recoilReturnSpeed * Time.deltaTime);

            _currentRecoilPosition = Vector3.Lerp(_currentRecoilPosition, _targetRecoilPosition, recoilSnappiness * Time.deltaTime);
            _currentRecoilRotation = Quaternion.Slerp(_currentRecoilRotation, _targetRecoilRotation, recoilSnappiness * Time.deltaTime);

            transform.localPosition = _baseLocalPosition + _currentRecoilPosition;
            transform.localRotation = _baseLocalRotation * _currentRecoilRotation;
        }
    }
}
