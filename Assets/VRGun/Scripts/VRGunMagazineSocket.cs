using UnityEngine;

namespace VRGun
{
    [DisallowMultipleComponent]
    public class VRGunMagazineSocket : MonoBehaviour
    {
        [Header("References")]
        public VRGunController gun;
        public Collider socketTrigger;

        [Header("Insert Rules")]
        public bool autoInsertOnTriggerEnter = true;
        public float maxInsertAngle = 35f;

        private void Awake()
        {
            if (gun == null)
            {
                gun = GetComponentInParent<VRGunController>();
            }

            if (socketTrigger == null)
            {
                socketTrigger = GetComponent<Collider>();
            }

            if (socketTrigger != null)
            {
                socketTrigger.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!autoInsertOnTriggerEnter || gun == null)
            {
                return;
            }

            VRGunMagazine mag = other.GetComponentInParent<VRGunMagazine>();
            if (mag == null)
            {
                return;
            }

            if (!IsRotationValid(mag.transform))
            {
                return;
            }

            gun.TryInsertMagazine(mag);
        }

        private bool IsRotationValid(Transform magazineTransform)
        {
            float angle = Quaternion.Angle(transform.rotation, magazineTransform.rotation);
            return angle <= maxInsertAngle;
        }
    }
}
