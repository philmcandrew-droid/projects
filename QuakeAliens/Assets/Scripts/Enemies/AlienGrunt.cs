using UnityEngine;

namespace QuakeAliens.Enemies
{
    /// <summary>
    /// Basic alien grunt - melee fighter that rushes the player
    /// Inspired by Quake 2's Strogg soldiers
    /// </summary>
    public class AlienGrunt : AlienBase
    {
        [Header("Grunt Settings")]
        [SerializeField] private float chargeSpeed = 8f;
        [SerializeField] private float leapDistance = 5f;
        [SerializeField] private float leapForce = 10f;
        [SerializeField] private bool canLeap = true;

        private bool isLeaping;
        private Rigidbody rb;

        protected override void Awake()
        {
            base.Awake();
            
            // Set grunt stats
            maxHealth = 80f;
            damage = 15f;
            attackRange = 2.5f;
            attackCooldown = 1.2f;
            detectionRange = 25f;
            walkSpeed = 3.5f;
            runSpeed = 7f;

            rb = GetComponent<Rigidbody>();
            currentHealth = maxHealth;
        }

        protected override void UpdateChase()
        {
            if (player == null)
            {
                currentState = AIState.Patrol;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Try to leap at player if in range
            if (canLeap && !isLeaping && distanceToPlayer <= leapDistance && distanceToPlayer > attackRange)
            {
                TryLeap();
            }
            else
            {
                base.UpdateChase();
            }
        }

        private void TryLeap()
        {
            if (rb == null || isLeaping) return;

            isLeaping = true;
            agent.enabled = false;

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0.3f; // Add some upward angle
            direction.Normalize();

            rb.isKinematic = false;
            rb.AddForce(direction * leapForce, ForceMode.Impulse);

            // Play leap sound/animation
            if (animator != null)
            {
                animator.SetTrigger("Leap");
            }

            Invoke(nameof(EndLeap), 0.8f);
        }

        private void EndLeap()
        {
            isLeaping = false;
            
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
            }
            
            agent.enabled = true;
        }

        protected override void PerformAttack()
        {
            base.PerformAttack();

            // Grunt does a powerful swipe attack
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange)
                {
                    // Screen shake on hit
                    var playerCam = FindObjectOfType<Player.PlayerCamera>();
                    if (playerCam != null)
                    {
                        playerCam.Shake(0.3f, 0.2f);
                    }
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Deal damage on leap collision
            if (isLeaping && collision.gameObject.CompareTag("Player"))
            {
                Player.PlayerController.Instance?.TakeDamage(damage * 1.5f);
                EndLeap();
            }
        }
    }
}

