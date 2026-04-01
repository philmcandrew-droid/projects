using UnityEngine;

namespace QuakeAliens.Weapons
{
    /// <summary>
    /// Railgun - high damage, penetrates enemies, slow fire rate
    /// Classic Quake 2 weapon!
    /// </summary>
    public class Railgun : WeaponBase
    {
        [Header("Railgun Settings")]
        [SerializeField] private int maxPenetrations = 3;
        [SerializeField] private float penetrationDamageMultiplier = 0.7f;
        [SerializeField] private Color railColor = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private float railDuration = 0.5f;
        [SerializeField] private float railWidth = 0.1f;

        private LineRenderer railBeam;

        protected override void Awake()
        {
            base.Awake();
            weaponName = "Railgun";
            weaponIndex = 3;
            damage = 100f;
            fireRate = 1.5f;
            range = 200f;
            maxAmmo = 30;
            clipSize = 10;
            reloadTime = 3f;
            recoilKick = new Vector3(-6f, 2f, 0);

            SetupRailBeam();
        }

        private void SetupRailBeam()
        {
            // Create a line renderer for the rail trail
            railBeam = gameObject.AddComponent<LineRenderer>();
            railBeam.startWidth = railWidth;
            railBeam.endWidth = railWidth * 0.5f;
            railBeam.material = new Material(Shader.Find("Sprites/Default"));
            railBeam.startColor = railColor;
            railBeam.endColor = new Color(railColor.r, railColor.g, railColor.b, 0.3f);
            railBeam.positionCount = 2;
            railBeam.enabled = false;
        }

        protected override void Fire()
        {
            nextFireTime = Time.time + fireRate;
            currentClip--;

            PlaySound(fireSound);
            CreateMuzzleFlash();
            ApplyRecoil();

            Vector3 origin = playerCamera.transform.position;
            Vector3 direction = playerCamera.transform.forward;
            Vector3 endPoint = origin + direction * range;

            // Penetrating raycast
            float currentDamage = damage;
            int penetrations = 0;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, range);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (penetrations >= maxPenetrations)
                    break;

                endPoint = hit.point;
                CreateImpactEffect(hit.point, hit.normal);

                // Check if we hit an enemy
                var enemy = hit.collider.GetComponent<Enemies.AlienBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(currentDamage, hit.point);
                    currentDamage *= penetrationDamageMultiplier;
                    penetrations++;
                }
                else
                {
                    // Hit a wall, stop penetration
                    break;
                }
            }

            // Create rail trail effect
            CreateRailTrail(firePoint.position, endPoint);
        }

        private void CreateRailTrail(Vector3 start, Vector3 end)
        {
            railBeam.SetPosition(0, start);
            railBeam.SetPosition(1, end);
            railBeam.enabled = true;

            // Create spiral effect particles along the rail
            StartCoroutine(FadeRail());
        }

        private System.Collections.IEnumerator FadeRail()
        {
            float elapsed = 0f;
            Color startColor = railColor;

            while (elapsed < railDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / railDuration);
                railBeam.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
                railBeam.endColor = new Color(startColor.r, startColor.g, startColor.b, alpha * 0.3f);
                yield return null;
            }

            railBeam.enabled = false;
        }
    }
}

