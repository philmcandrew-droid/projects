using UnityEngine;

namespace QuakeAliens.Weapons
{
    /// <summary>
    /// Base class for all weapons - handles common functionality
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Info")]
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected int weaponIndex = 0;
        [SerializeField] protected Sprite weaponIcon;

        [Header("Stats")]
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected float fireRate = 0.5f;
        [SerializeField] protected float range = 100f;
        [SerializeField] protected int maxAmmo = 100;
        [SerializeField] protected int clipSize = 30;
        [SerializeField] protected float reloadTime = 2f;

        [Header("Recoil")]
        [SerializeField] protected Vector3 recoilKick = new Vector3(-2f, 0.5f, 0);

        [Header("Effects")]
        [SerializeField] protected GameObject muzzleFlashPrefab;
        [SerializeField] protected GameObject impactEffectPrefab;
        [SerializeField] protected Transform firePoint;
        [SerializeField] protected AudioClip fireSound;
        [SerializeField] protected AudioClip reloadSound;
        [SerializeField] protected AudioClip emptySound;

        protected int currentAmmo;
        protected int currentClip;
        protected float nextFireTime;
        protected bool isReloading;
        protected AudioSource audioSource;
        protected Camera playerCamera;

        public string WeaponName => weaponName;
        public int WeaponIndex => weaponIndex;
        public int CurrentAmmo => currentAmmo;
        public int CurrentClip => currentClip;
        public int ClipSize => clipSize;
        public bool IsReloading => isReloading;
        public Sprite WeaponIcon => weaponIcon;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound for weapon

            playerCamera = Camera.main;
        }

        protected virtual void Start()
        {
            currentAmmo = maxAmmo;
            currentClip = clipSize;
        }

        public virtual void OnEquip()
        {
            gameObject.SetActive(true);
            // Play equip animation/sound
        }

        public virtual void OnUnequip()
        {
            gameObject.SetActive(false);
            CancelReload();
        }

        public virtual bool CanFire()
        {
            return Time.time >= nextFireTime && !isReloading && currentClip > 0;
        }

        public virtual void TryFire()
        {
            if (CanFire())
            {
                Fire();
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

        protected abstract void Fire();

        protected virtual void ApplyRecoil()
        {
            var playerCamera = FindObjectOfType<Player.PlayerCamera>();
            if (playerCamera != null)
            {
                playerCamera.ApplyKick(recoilKick);
            }
        }

        protected virtual void CreateMuzzleFlash()
        {
            if (muzzleFlashPrefab != null && firePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
                Destroy(flash, 0.1f);
            }
        }

        protected virtual void CreateImpactEffect(Vector3 position, Vector3 normal)
        {
            if (impactEffectPrefab != null)
            {
                GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(impact, 2f);
            }
        }

        public virtual void StartReload()
        {
            if (isReloading || currentAmmo <= 0 || currentClip >= clipSize)
                return;

            isReloading = true;
            PlaySound(reloadSound);
            Invoke(nameof(FinishReload), reloadTime);
        }

        protected virtual void FinishReload()
        {
            int ammoNeeded = clipSize - currentClip;
            int ammoToReload = Mathf.Min(ammoNeeded, currentAmmo);
            
            currentClip += ammoToReload;
            currentAmmo -= ammoToReload;
            isReloading = false;
        }

        protected virtual void CancelReload()
        {
            if (isReloading)
            {
                CancelInvoke(nameof(FinishReload));
                isReloading = false;
            }
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public void AddAmmo(int amount)
        {
            currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        }

        protected bool Raycast(out RaycastHit hit)
        {
            Vector3 origin = playerCamera.transform.position;
            Vector3 direction = playerCamera.transform.forward;
            return Physics.Raycast(origin, direction, out hit, range);
        }
    }
}

