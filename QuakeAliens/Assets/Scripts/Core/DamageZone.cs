using UnityEngine;

namespace QuakeAliens
{
    /// <summary>
    /// Zone that damages players/enemies over time (lava, acid, etc.)
    /// </summary>
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] private float damagePerSecond = 10f;
        [SerializeField] private bool damagesPlayer = true;
        [SerializeField] private bool damagesEnemies = true;
        [SerializeField] private float damageInterval = 0.5f;

        private float lastDamageTime;

        private void OnTriggerStay(Collider other)
        {
            if (Time.time - lastDamageTime < damageInterval)
                return;

            lastDamageTime = Time.time;
            float damage = damagePerSecond * damageInterval;

            if (damagesPlayer)
            {
                var player = other.GetComponent<Player.PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }

            if (damagesEnemies)
            {
                var enemy = other.GetComponent<Enemies.AlienBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, other.transform.position);
                }
            }
        }
    }

    /// <summary>
    /// Jump pad that launches the player
    /// </summary>
    public class JumpPad : MonoBehaviour
    {
        [SerializeField] private float launchForce = 20f;
        [SerializeField] private Vector3 launchDirection = Vector3.up;
        [SerializeField] private AudioClip launchSound;

        private void OnTriggerEnter(Collider other)
        {
            var controller = other.GetComponent<CharacterController>();
            if (controller != null)
            {
                // Note: CharacterController doesn't use physics forces
                // This would need to be handled by the PlayerController
                // For now, just play sound
                if (launchSound != null)
                {
                    AudioSource.PlayClipAtPoint(launchSound, transform.position);
                }
            }
        }
    }

    /// <summary>
    /// Teleporter that moves player to another location
    /// </summary>
    public class Teleporter : MonoBehaviour
    {
        [SerializeField] private Transform destination;
        [SerializeField] private AudioClip teleportSound;
        [SerializeField] private GameObject teleportEffect;
        [SerializeField] private float cooldown = 1f;

        private float lastTeleportTime;

        private void OnTriggerEnter(Collider other)
        {
            if (destination == null) return;
            if (Time.time - lastTeleportTime < cooldown) return;

            var player = other.GetComponent<Player.PlayerController>();
            if (player != null)
            {
                lastTeleportTime = Time.time;

                // Play effects
                if (teleportSound != null)
                {
                    AudioSource.PlayClipAtPoint(teleportSound, transform.position);
                }
                if (teleportEffect != null)
                {
                    Instantiate(teleportEffect, transform.position, Quaternion.identity);
                    Instantiate(teleportEffect, destination.position, Quaternion.identity);
                }

                // Teleport
                player.Respawn(destination.position);
            }
        }
    }
}

