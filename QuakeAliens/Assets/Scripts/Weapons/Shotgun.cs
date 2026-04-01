using UnityEngine;

namespace QuakeAliens.Weapons
{
    /// <summary>
    /// Shotgun - high damage at close range, fires multiple pellets
    /// </summary>
    public class Shotgun : WeaponBase
    {
        [Header("Shotgun Settings")]
        [SerializeField] private int pelletCount = 8;
        [SerializeField] private float spreadAngle = 5f;
        [SerializeField] private float damagePerPellet = 12f;
        [SerializeField] private GameObject bulletTrailPrefab;

        protected override void Awake()
        {
            base.Awake();
            weaponName = "Shotgun";
            weaponIndex = 2;
            damage = damagePerPellet;
            fireRate = 0.8f;
            range = 50f;
            maxAmmo = 50;
            clipSize = 8;
            reloadTime = 2.5f;
            recoilKick = new Vector3(-4f, 1f, 0);
        }

        protected override void Fire()
        {
            nextFireTime = Time.time + fireRate;
            currentClip--;

            PlaySound(fireSound);
            CreateMuzzleFlash();
            ApplyRecoil();

            // Fire multiple pellets
            for (int i = 0; i < pelletCount; i++)
            {
                FirePellet();
            }
        }

        private void FirePellet()
        {
            // Calculate spread
            Vector3 direction = playerCamera.transform.forward;
            direction += playerCamera.transform.right * Random.Range(-spreadAngle, spreadAngle) * 0.02f;
            direction += playerCamera.transform.up * Random.Range(-spreadAngle, spreadAngle) * 0.02f;
            direction.Normalize();

            if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, range))
            {
                CreateBulletTrail(firePoint.position, hit.point);
                CreateImpactEffect(hit.point, hit.normal);

                // Check if we hit an enemy
                var enemy = hit.collider.GetComponent<Enemies.AlienBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerPellet, hit.point);
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
                Destroy(trail, 0.05f);
            }
        }
    }
}

