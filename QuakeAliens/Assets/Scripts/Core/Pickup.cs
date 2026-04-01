using UnityEngine;

namespace QuakeAliens
{
    /// <summary>
    /// Base class for all pickups - health, ammo, weapons
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        public enum PickupType
        {
            Health,
            Armor,
            Ammo,
            Weapon
        }

        [Header("Pickup Settings")]
        [SerializeField] private PickupType type = PickupType.Health;
        [SerializeField] private float value = 25f;
        [SerializeField] private int weaponIndex = 0; // For weapon/ammo pickups

        [Header("Respawn")]
        [SerializeField] private bool canRespawn = true;
        [SerializeField] private float respawnTime = 30f;

        [Header("Effects")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private GameObject pickupEffect;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float rotateSpeed = 90f;

        private Vector3 startPosition;
        private MeshRenderer meshRenderer;
        private Collider pickupCollider;
        private bool isActive = true;

        private void Start()
        {
            startPosition = transform.position;
            meshRenderer = GetComponent<MeshRenderer>();
            pickupCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (!isActive) return;

            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Rotate
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;

            var player = other.GetComponent<Player.PlayerController>();
            if (player == null) return;

            bool pickedUp = false;

            switch (type)
            {
                case PickupType.Health:
                    if (player.Health < player.MaxHealth)
                    {
                        player.Heal(value);
                        pickedUp = true;
                    }
                    break;

                case PickupType.Ammo:
                    var weaponManager = Weapons.WeaponManager.Instance;
                    if (weaponManager != null)
                    {
                        weaponManager.AddAmmoToWeapon(weaponIndex, (int)value);
                        pickedUp = true;
                    }
                    break;

                case PickupType.Weapon:
                    // Could add weapon pickup logic here
                    pickedUp = true;
                    break;
            }

            if (pickedUp)
            {
                OnPickup();
            }
        }

        private void OnPickup()
        {
            // Play sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Spawn effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            if (canRespawn)
            {
                // Hide and respawn later
                isActive = false;
                if (meshRenderer != null) meshRenderer.enabled = false;
                if (pickupCollider != null) pickupCollider.enabled = false;
                
                Invoke(nameof(Respawn), respawnTime);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Respawn()
        {
            isActive = true;
            if (meshRenderer != null) meshRenderer.enabled = true;
            if (pickupCollider != null) pickupCollider.enabled = true;
        }
    }
}

