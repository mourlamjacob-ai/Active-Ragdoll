using UnityEngine;

namespace VRGun
{
    [DisallowMultipleComponent]
    public class VRGunMagazine : MonoBehaviour
    {
        [Header("Ammo")]
        public int capacity = 15;
        public int currentRounds = 15;

        [Header("Physics")]
        public Rigidbody magazineBody;
        public Collider[] colliders;

        public bool IsEmpty => currentRounds <= 0;

        private void Awake()
        {
            if (magazineBody == null)
            {
                magazineBody = GetComponent<Rigidbody>();
            }

            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponentsInChildren<Collider>();
            }

            currentRounds = Mathf.Clamp(currentRounds, 0, Mathf.Max(1, capacity));
        }

        public bool TryConsumeRound()
        {
            if (currentRounds <= 0)
            {
                return false;
            }

            currentRounds--;
            return true;
        }

        public void RefillToCapacity()
        {
            currentRounds = Mathf.Max(1, capacity);
        }

        public void SetKinematic(bool value)
        {
            if (magazineBody != null)
            {
                magazineBody.isKinematic = value;
                magazineBody.useGravity = !value;
                magazineBody.velocity = Vector3.zero;
                magazineBody.angularVelocity = Vector3.zero;
            }
        }

        public void SetCollidersEnabled(bool value)
        {
            if (colliders == null)
            {
                return;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = value;
                }
            }
        }
    }
}
