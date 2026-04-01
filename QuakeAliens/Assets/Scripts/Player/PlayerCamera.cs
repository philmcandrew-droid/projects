using UnityEngine;

namespace QuakeAliens.Player
{
    /// <summary>
    /// Handles camera effects like head bob, view kick, and screen shake
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Head Bob")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobAmplitude = 0.05f;

        [Header("View Kick")]
        [SerializeField] private float kickRecoverySpeed = 10f;

        [Header("Screen Shake")]
        [SerializeField] private float shakeDecay = 1.5f;

        private Vector3 originalPosition;
        private float bobTimer;
        private Vector3 currentKick;
        private float shakeIntensity;
        private float shakeDuration;

        private CharacterController playerController;

        private void Start()
        {
            originalPosition = transform.localPosition;
            playerController = GetComponentInParent<CharacterController>();
        }

        private void Update()
        {
            // Apply head bob
            if (enableHeadBob && playerController != null)
            {
                ApplyHeadBob();
            }

            // Recover from view kick
            RecoverKick();

            // Apply screen shake
            ApplyScreenShake();
        }

        private void ApplyHeadBob()
        {
            Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0, playerController.velocity.z);
            float speed = horizontalVelocity.magnitude;

            if (speed > 0.1f && playerController.isGrounded)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude * (speed / 10f);
                transform.localPosition = originalPosition + Vector3.up * bobOffset;
            }
            else
            {
                bobTimer = 0;
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5f);
            }
        }

        private void RecoverKick()
        {
            currentKick = Vector3.Lerp(currentKick, Vector3.zero, Time.deltaTime * kickRecoverySpeed);
            transform.localEulerAngles = currentKick;
        }

        private void ApplyScreenShake()
        {
            if (shakeDuration > 0)
            {
                transform.localPosition += Random.insideUnitSphere * shakeIntensity;
                shakeDuration -= Time.deltaTime;
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0, Time.deltaTime * shakeDecay);
            }
        }

        /// <summary>
        /// Apply a view kick (recoil) effect
        /// </summary>
        public void ApplyKick(Vector3 kick)
        {
            currentKick += kick;
        }

        /// <summary>
        /// Trigger a screen shake effect
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeDuration = duration;
        }
    }
}

