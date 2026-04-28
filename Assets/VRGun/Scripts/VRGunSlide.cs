using UnityEngine;

namespace VRGun
{
    [DisallowMultipleComponent]
    public class VRGunSlide : MonoBehaviour
    {
        [Header("References")]
        public VRGunController gun;
        public Transform slideVisual;

        [Header("Slide Travel")]
        public Vector3 localRearOffset = new Vector3(0f, 0f, -0.055f);
        [Range(0f, 1f)] public float pullAmount;
        public float returnSpeed = 12f;

        [Header("Behavior")]
        public bool releaseToChamber = true;
        public bool autoReleaseWhenNotGrabbed = true;

        private Vector3 _startLocalPosition;
        private bool _isGrabbed;

        private void Awake()
        {
            if (gun == null)
            {
                gun = GetComponentInParent<VRGunController>();
            }

            if (slideVisual == null)
            {
                slideVisual = transform;
            }

            _startLocalPosition = slideVisual.localPosition;
        }

        private void Update()
        {
            if (autoReleaseWhenNotGrabbed && !_isGrabbed)
            {
                pullAmount = Mathf.MoveTowards(pullAmount, 0f, returnSpeed * Time.deltaTime);
            }

            UpdateSlideVisual();
        }

        public void SetGrabbed(bool isGrabbed)
        {
            _isGrabbed = isGrabbed;
        }

        public void SetPullAmount(float value)
        {
            pullAmount = Mathf.Clamp01(value);
            UpdateSlideVisual();
        }

        public void ReleaseSlide()
        {
            _isGrabbed = false;
            pullAmount = 0f;
            UpdateSlideVisual();

            if (releaseToChamber && gun != null)
            {
                gun.RackSlide();
            }
        }

        private void UpdateSlideVisual()
        {
            if (slideVisual == null)
            {
                return;
            }

            slideVisual.localPosition = _startLocalPosition + (localRearOffset * pullAmount);
        }
    }
}
