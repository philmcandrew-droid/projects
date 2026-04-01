using UnityEngine;

namespace QuakeAliens.Weapons
{
    /// <summary>
    /// Laser weapon - continuous beam that deals damage over time
    /// </summary>
    public class Laser : WeaponBase
    {
        [Header("Laser Settings")]
        [SerializeField] private float damagePerSecond = 50f;
        [SerializeField] private Color laserColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private float laserWidth = 0.05f;
        [SerializeField] private float ammoConsumeRate = 10f; // Ammo consumed per second

        private LineRenderer laserBeam;
        private bool isFiring;
        private float ammoTimer;
        private ParticleSystem impactParticles;

        protected override void Awake()
        {
            base.Awake();
            weaponName = "Laser";
            weaponIndex = 4;
            damage = damagePerSecond;
            fireRate = 0f; // Continuous fire
            range = 100f;
            maxAmmo = 200;
            clipSize = 100;
            reloadTime = 3f;
            recoilKick = new Vector3(-0.5f, 0.1f, 0);

            SetupLaserBeam();
            SetupImpactParticles();
        }

        private void SetupLaserBeam()
        {
            laserBeam = gameObject.AddComponent<LineRenderer>();
            laserBeam.startWidth = laserWidth;
            laserBeam.endWidth = laserWidth;
            laserBeam.material = new Material(Shader.Find("Sprites/Default"));
            laserBeam.startColor = laserColor;
            laserBeam.endColor = laserColor;
            laserBeam.positionCount = 2;
            laserBeam.enabled = false;
        }

        private void SetupImpactParticles()
        {
            GameObject particleObj = new GameObject("LaserImpact");
            particleObj.transform.SetParent(transform);
            impactParticles = particleObj.AddComponent<ParticleSystem>();
            
            var main = impactParticles.main;
            main.startSize = 0.2f;
            main.startLifetime = 0.2f;
            main.startSpeed = 2f;
            main.startColor = laserColor;
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = impactParticles.emission;
            emission.rateOverTime = 50f;

            var shape = impactParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            impactParticles.Stop();
        }

        private void Update()
        {
            if (isFiring && currentClip > 0 && !isReloading)
            {
                FireContinuous();
            }
            else
            {
                StopFiring();
            }
        }

        public override bool CanFire()
        {
            return !isReloading && currentClip > 0;
        }

        public override void TryFire()
        {
            if (CanFire())
            {
                isFiring = true;
            }
            else if (currentClip <= 0 && !isReloading)
            {
                if (currentAmmo > 0)
                {
                    StartReload();
                }
                else
                {
                    PlaySound(emptySound);
                }
            }
        }

        protected override void Fire()
        {
            // Laser uses continuous fire instead
        }

        private void FireContinuous()
        {
            laserBeam.enabled = true;

            Vector3 origin = playerCamera.transform.position;
            Vector3 direction = playerCamera.transform.forward;
            Vector3 endPoint = origin + direction * range;

            laserBeam.SetPosition(0, firePoint.position);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
            {
                endPoint = hit.point;
                laserBeam.SetPosition(1, hit.point);

                // Position impact particles
                impactParticles.transform.position = hit.point;
                impactParticles.transform.rotation = Quaternion.LookRotation(hit.normal);
                if (!impactParticles.isPlaying)
                    impactParticles.Play();

                // Deal continuous damage
                var enemy = hit.collider.GetComponent<Enemies.AlienBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerSecond * Time.deltaTime, hit.point);
                }
            }
            else
            {
                laserBeam.SetPosition(1, endPoint);
                impactParticles.Stop();
            }

            // Consume ammo over time
            ammoTimer += Time.deltaTime;
            if (ammoTimer >= 1f / ammoConsumeRate)
            {
                ammoTimer = 0f;
                currentClip--;
                
                if (currentClip <= 0)
                {
                    StopFiring();
                }
            }

            // Apply slight continuous recoil
            ApplyRecoil();

            // Play continuous fire sound
            if (!audioSource.isPlaying && fireSound != null)
            {
                audioSource.clip = fireSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        public void StopFiring()
        {
            isFiring = false;
            laserBeam.enabled = false;
            impactParticles.Stop();
            
            if (audioSource.isPlaying && audioSource.loop)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
        }

        public override void OnUnequip()
        {
            StopFiring();
            base.OnUnequip();
        }
    }
}

