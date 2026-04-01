using UnityEngine;
using UnityEngine.AI;

namespace QuakeAliens.Enemies
{
    /// <summary>
    /// Base class for all alien enemies with common AI behavior
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AlienBase : MonoBehaviour
    {
        public enum AIState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Dead
        }

        [Header("Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected float detectionRange = 20f;
        [SerializeField] protected float fieldOfView = 120f;

        [Header("Movement")]
        [SerializeField] protected float walkSpeed = 3f;
        [SerializeField] protected float runSpeed = 6f;
        [SerializeField] protected float patrolWaitTime = 2f;

        [Header("Patrol")]
        [SerializeField] protected Transform[] patrolPoints;
        [SerializeField] protected bool randomPatrol = true;

        [Header("Effects")]
        [SerializeField] protected GameObject deathEffect;
        [SerializeField] protected GameObject hitEffect;
        [SerializeField] protected AudioClip[] idleSounds;
        [SerializeField] protected AudioClip[] attackSounds;
        [SerializeField] protected AudioClip[] hurtSounds;
        [SerializeField] protected AudioClip deathSound;

        protected float currentHealth;
        protected AIState currentState = AIState.Idle;
        protected NavMeshAgent agent;
        protected Transform player;
        protected float lastAttackTime;
        protected int currentPatrolIndex;
        protected float patrolWaitTimer;
        protected AudioSource audioSource;
        protected Animator animator;

        public float Health => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => currentState == AIState.Dead;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            animator = GetComponent<Animator>();
            
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            player = Player.PlayerController.Instance?.transform;
            agent.speed = walkSpeed;
            
            // Start in patrol state if we have patrol points
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                currentState = AIState.Patrol;
                GoToNextPatrolPoint();
            }
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            switch (currentState)
            {
                case AIState.Idle:
                    UpdateIdle();
                    break;
                case AIState.Patrol:
                    UpdatePatrol();
                    break;
                case AIState.Chase:
                    UpdateChase();
                    break;
                case AIState.Attack:
                    UpdateAttack();
                    break;
            }

            // Always check for player
            CheckForPlayer();
        }

        protected virtual void UpdateIdle()
        {
            // Play idle animation and occasionally make sounds
            if (Random.value < 0.001f && idleSounds.Length > 0)
            {
                PlayRandomSound(idleSounds);
            }
        }

        protected virtual void UpdatePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                currentState = AIState.Idle;
                return;
            }

            agent.speed = walkSpeed;

            // Check if reached patrol point
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                patrolWaitTimer += Time.deltaTime;
                
                if (patrolWaitTimer >= patrolWaitTime)
                {
                    patrolWaitTimer = 0f;
                    GoToNextPatrolPoint();
                }
            }
        }

        protected virtual void UpdateChase()
        {
            if (player == null)
            {
                currentState = AIState.Patrol;
                return;
            }

            agent.speed = runSpeed;
            agent.SetDestination(player.position);

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Check if in attack range
            if (distanceToPlayer <= attackRange)
            {
                currentState = AIState.Attack;
            }
            // Lost sight of player
            else if (distanceToPlayer > detectionRange * 1.5f)
            {
                currentState = AIState.Patrol;
                GoToNextPatrolPoint();
            }
        }

        protected virtual void UpdateAttack()
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
            if (distanceToPlayer > attackRange * 1.2f)
            {
                currentState = AIState.Chase;
                return;
            }

            // Stop moving while attacking
            agent.SetDestination(transform.position);

            // Attack if cooldown is ready
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }

        protected virtual void CheckForPlayer()
        {
            if (player == null || currentState == AIState.Attack) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                // Check if player is in field of view
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToPlayer);

                if (angle <= fieldOfView / 2f)
                {
                    // Raycast to check line of sight
                    if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, 
                        out RaycastHit hit, detectionRange))
                    {
                        if (hit.transform == player)
                        {
                            currentState = AIState.Chase;
                        }
                    }
                }
            }
        }

        protected virtual void GoToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            if (randomPatrol)
            {
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            }
            else
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }

            if (patrolPoints[currentPatrolIndex] != null)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }

        protected virtual void PerformAttack()
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

            // Deal damage to player
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange)
                {
                    Player.PlayerController.Instance?.TakeDamage(damage);
                }
            }
        }

        public virtual void TakeDamage(float amount, Vector3 hitPoint)
        {
            if (IsDead) return;

            currentHealth -= amount;

            // Spawn hit effect
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.identity);
                Destroy(effect, 1f);
            }

            // Play hurt sound
            if (hurtSounds.Length > 0)
            {
                PlayRandomSound(hurtSounds);
            }

            // Flash damage (could add material flash here)
            
            // Aggro on the player
            if (currentState != AIState.Attack)
            {
                currentState = AIState.Chase;
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            currentState = AIState.Dead;
            agent.enabled = false;

            // Play death sound
            if (deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            // Spawn death effect
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }

            // Notify game manager
            GameManager.Instance?.OnEnemyKilled(this);

            // Disable collider
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // Play death animation or ragdoll
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            // Destroy after delay
            Destroy(gameObject, 3f);
        }

        protected void PlayRandomSound(AudioClip[] clips)
        {
            if (clips.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw field of view
            Gizmos.color = Color.blue;
            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
            Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
        }
    }
}

