using UnityEngine;

namespace QuakeAliens.Enemies
{
    /// <summary>
    /// Projectile fired by alien shooters
    /// </summary>
    public class AlienProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private GameObject impactEffect;
        [SerializeField] private TrailRenderer trail;

        private Vector3 direction;
        private float speed;
        private float damage;
        private bool initialized;

        private void Start()
        {
            Destroy(gameObject, lifetime);

            // Setup trail if exists
            if (trail == null)
            {
                trail = GetComponent<TrailRenderer>();
            }
        }

        public void Initialize(Vector3 dir, float spd, float dmg)
        {
            direction = dir;
            speed = spd;
            damage = dmg;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;

            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Don't hit other enemies
            if (other.GetComponent<AlienBase>() != null) return;

            // Check if hit player
            var player = other.GetComponent<Player.PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            // Spawn impact effect
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Don't hit other enemies
            if (collision.gameObject.GetComponent<AlienBase>() != null) return;

            // Check if hit player
            var player = collision.gameObject.GetComponent<Player.PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            // Spawn impact effect
            if (impactEffect != null)
            {
                ContactPoint contact = collision.contacts[0];
                Instantiate(impactEffect, contact.point, Quaternion.LookRotation(contact.normal));
            }

            Destroy(gameObject);
        }
    }
}
