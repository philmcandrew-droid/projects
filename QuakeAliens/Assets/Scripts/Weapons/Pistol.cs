using UnityEngine;

namespace QuakeAliens.Weapons
{
    /// <summary>
    /// Standard pistol - reliable, accurate, infinite ammo reserve
    /// </summary>
    public class Pistol : WeaponBase
    {
        [Header("Pistol Settings")]
        [SerializeField] private float spreadAngle = 1f;
        [SerializeField] private GameObject bulletTrailPrefab;

        protected override void Awake()
        {
            base.Awake();
            weaponName = "Pistol";
            weaponIndex = 1;
            damage = 15f;
            fireRate = 0.3f;
            range = 100f;
            maxAmmo = 999; // Essentially infinite
            clipSize = 12;
            reloadTime = 1.5f;
            recoilKick = new Vector3(-1.5f, 0.3f, 0);
        }

        protected override void Fire()
        {
            nextFireTime = Time.time + fireRate;
            currentClip--;

            PlaySound(fireSound);
            CreateMuzzleFlash();
            ApplyRecoil();

            // Calculate spread
            Vector3 direction = playerCamera.transform.forward;
            direction += playerCamera.transform.right * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
            direction += playerCamera.transform.up * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
            direction.Normalize();

            if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, range))
            {
                CreateBulletTrail(firePoint.position, hit.point);
                CreateImpactEffect(hit.point, hit.normal);

                // Check if we hit an enemy
                var enemy = hit.collider.GetComponent<Enemies.AlienBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, hit.point);
                }
            }
            else
            {
                CreateBulletTrail(firePoint.position, firePoint.position + direction * range);
            }
        }

        private void CreateBulletTrail(Vector3 start, Vector3 end)
        {
            if (bulletTrailPrefab != null)
            {
                GameObject trail = Instantiate(bulletTrailPrefab);
                LineRenderer lr = trail.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, start);
                    lr.SetPosition(1, end);
                }
                Destroy(trail, 0.1f);
            }
        }
    }
}

