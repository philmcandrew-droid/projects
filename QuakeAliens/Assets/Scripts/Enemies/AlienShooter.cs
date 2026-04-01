using UnityEngine;

namespace QuakeAliens.Enemies
{
    /// <summary>
    /// Ranged alien that shoots projectiles at the player
    /// </summary>
    public class AlienShooter : AlienBase
    {
        [Header("Shooter Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileDamage = 20f;
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstDelay = 0.2f;
        [SerializeField] private float preferredRange = 15f;

        private int currentBurst;
        private bool isBursting;

        protected override void Awake()
        {
            base.Awake();
            
            // Set shooter stats
            maxHealth = 60f;
            damage = projectileDamage;
            attackRange = 25f; // Long range
            attackCooldown = 2f;
            detectionRange = 30f;
            walkSpeed = 2.5f;
            runSpeed = 4f;

            currentHealth = maxHealth;

            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        protected override void UpdateChase()
        {
            if (player == null)
            {
                currentState = AIState.Patrol;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Try to maintain preferred distance
            if (distanceToPlayer < preferredRange * 0.5f)
            {
                // Too close, back up
                Vector3 awayFromPlayer = transform.position - player.position;
                awayFromPlayer.y = 0;
                awayFromPlayer.Normalize();
                
                agent.speed = runSpeed;
                agent.SetDestination(transform.position + awayFromPlayer * 5f);
            }
            else if (distanceToPlayer <= attackRange)
            {
                currentState = AIState.Attack;
            }
            else
            {
                base.UpdateChase();
            }
        }

        protected override void UpdateAttack()
        {
            if (player == null)
            {
                currentState = AIState.Patrol;
                return;
            }

            // Face the player
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, 
                    Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Out of attack range, chase
            if (distanceToPlayer > attackRange)
            {
                currentState = AIState.Chase;
                return;
            }

            // Stop moving while attacking
            agent.SetDestination(transform.position);

            // Attack if cooldown is ready and not bursting
            if (!isBursting && Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(BurstFire());
                lastAttackTime = Time.time;
            }
        }

        private System.Collections.IEnumerator BurstFire()
        {
            isBursting = true;
            currentBurst = 0;

            while (currentBurst < burstCount)
            {
                FireProjectile();
                currentBurst++;
                yield return new WaitForSeconds(burstDelay);
            }

            isBursting = false;
        }

        protected override void PerformAttack()
        {
            // Shooter uses burst fire instead
        }

        private void FireProjectile()
        {
            if (player == null) return;

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

            // Calculate lead on target (predict where player will be)
            Vector3 targetPos = player.position;
            var playerController = player.GetComponent<CharacterController>();
            if (playerController != null)
            {
                float timeToTarget = Vector3.Distance(firePoint.position, player.position) / projectileSpeed;
                targetPos += playerController.velocity * timeToTarget * 0.5f;
            }

            Vector3 direction = (targetPos - firePoint.position).normalized;

            // Spawn projectile
            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
                AlienProjectile projectile = proj.GetComponent<AlienProjectile>();
                if (projectile != null)
                {
                    projectile.Initialize(direction, projectileSpeed, projectileDamage);
                }
                else
                {
                    // Fallback: just add velocity
                    Rigidbody rb = proj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = direction * projectileSpeed;
                    }
                }
            }
        }
    }
}
