using UnityEngine;

namespace QuakeAliens.Enemies
{
    /// <summary>
    /// Heavy alien brute - tank class with high health and damage
    /// Slow but devastating
    /// </summary>
    public class AlienBrute : AlienBase
    {
        [Header("Brute Settings")]
        [SerializeField] private float slamRadius = 4f;
        [SerializeField] private float slamDamage = 40f;
        [SerializeField] private float slamKnockback = 10f;
        [SerializeField] private float chargeDistance = 10f;
        [SerializeField] private float chargeSpeed = 12f;
        [SerializeField] private GameObject slamEffect;

        private bool isCharging;
        private Vector3 chargeTarget;

        protected override void Awake()
        {
            base.Awake();
            
            // Set brute stats - tanky and slow
            maxHealth = 250f;
            damage = 30f;
            attackRange = 3f;
            attackCooldown = 2.5f;
            detectionRange = 20f;
            walkSpeed = 2f;
            runSpeed = 4f;

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

            // Try to charge at player if in range
            if (!isCharging && distanceToPlayer <= chargeDistance && distanceToPlayer > attackRange * 2)
            {
                StartCharge();
            }
            else if (!isCharging)
            {
                base.UpdateChase();
            }
        }

        private void StartCharge()
        {
            if (player == null) return;

            isCharging = true;
            chargeTarget = player.position;
            agent.speed = chargeSpeed;
            agent.SetDestination(chargeTarget);

            // Play charge sound/animation
            if (animator != null)
            {
                animator.SetTrigger("Charge");
            }
        }

        private void Update()
        {
            base.Update();

            if (isCharging)
            {
                // Check if reached charge target or hit something
                if (agent.remainingDistance < 1f || !agent.pathPending && agent.velocity.magnitude < 0.1f)
                {
                    EndCharge();
                }
            }
        }

        private void EndCharge()
        {
            isCharging = false;
            agent.speed = runSpeed;

            // Ground slam at end of charge
            PerformGroundSlam();
        }

        protected override void PerformAttack()
        {
            // Play attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Play attack sound
            if (attackSounds.Length > 0)
            {
                PlayRandomSound(attackSounds);
            }

            // Heavy melee attack
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange)
                {
                    Player.PlayerController.Instance?.TakeDamage(damage);
                    
                    // Screen shake
                    var playerCam = FindObjectOfType<Player.PlayerCamera>();
                    if (playerCam != null)
                    {
                        playerCam.Shake(0.5f, 0.3f);
                    }
                }
            }
        }

        private void PerformGroundSlam()
        {
            // Spawn slam effect
            if (slamEffect != null)
            {
                Instantiate(slamEffect, transform.position, Quaternion.identity);
            }

            // Screen shake for all nearby
            var playerCam = FindObjectOfType<Player.PlayerCamera>();
            if (playerCam != null)
            {
                float distToPlayer = Vector3.Distance(transform.position, player.position);
                if (distToPlayer < slamRadius * 2)
                {
                    float intensity = Mathf.Lerp(0.8f, 0.2f, distToPlayer / (slamRadius * 2));
                    playerCam.Shake(intensity, 0.4f);
                }
            }

            // Damage in radius
            Collider[] hits = Physics.OverlapSphere(transform.position, slamRadius);
            foreach (var hit in hits)
            {
                var playerController = hit.GetComponent<Player.PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(slamDamage);
                    
                    // Apply knockback (would need rigidbody or special handling)
                    // For now, just do damage
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Deal damage if charging and hit player
            if (isCharging && collision.gameObject.CompareTag("Player"))
            {
                Player.PlayerController.Instance?.TakeDamage(damage * 2f);
                EndCharge();
            }
        }

        private void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // Draw slam radius
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, slamRadius);
        }
    }
}
