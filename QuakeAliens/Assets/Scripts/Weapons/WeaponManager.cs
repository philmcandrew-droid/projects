using UnityEngine;
using System.Collections.Generic;

namespace QuakeAliens.Weapons
{
    /// <summary>
    /// Manages weapon switching, input, and inventory
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private List<WeaponBase> weapons = new List<WeaponBase>();
        [SerializeField] private Transform weaponHolder;

        [Header("Settings")]
        [SerializeField] private float switchCooldown = 0.3f;

        private int currentWeaponIndex = 0;
        private float lastSwitchTime;
        private WeaponBase currentWeapon;

        public WeaponBase CurrentWeapon => currentWeapon;
        public List<WeaponBase> Weapons => weapons;
        public static WeaponManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Initialize all weapons as inactive
            foreach (var weapon in weapons)
            {
                if (weapon != null)
                    weapon.gameObject.SetActive(false);
            }

            // Equip first weapon
            if (weapons.Count > 0)
            {
                EquipWeapon(0);
            }
        }

        private void Update()
        {
            HandleWeaponInput();
            HandleWeaponSwitching();
        }

        private void HandleWeaponInput()
        {
            if (currentWeapon == null) return;

            // Primary fire
            if (Input.GetButton("Fire1"))
            {
                currentWeapon.TryFire();
            }
            else if (currentWeapon is Laser laser)
            {
                laser.StopFiring();
            }

            // Reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentWeapon.StartReload();
            }
        }

        private void HandleWeaponSwitching()
        {
            if (Time.time - lastSwitchTime < switchCooldown) return;

            // Number keys 1-4
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToWeapon(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToWeapon(3);

            // Mouse wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                SwitchToNextWeapon();
            }
            else if (scroll < 0f)
            {
                SwitchToPreviousWeapon();
            }
        }

        private void SwitchToWeapon(int index)
        {
            if (index >= 0 && index < weapons.Count && index != currentWeaponIndex)
            {
                EquipWeapon(index);
            }
        }

        private void SwitchToNextWeapon()
        {
            int nextIndex = (currentWeaponIndex + 1) % weapons.Count;
            EquipWeapon(nextIndex);
        }

        private void SwitchToPreviousWeapon()
        {
            int prevIndex = currentWeaponIndex - 1;
            if (prevIndex < 0) prevIndex = weapons.Count - 1;
            EquipWeapon(prevIndex);
        }

        private void EquipWeapon(int index)
        {
            if (index < 0 || index >= weapons.Count) return;

            // Unequip current weapon
            if (currentWeapon != null)
            {
                currentWeapon.OnUnequip();
            }

            // Equip new weapon
            currentWeaponIndex = index;
            currentWeapon = weapons[index];
            
            if (currentWeapon != null)
            {
                currentWeapon.OnEquip();
            }

            lastSwitchTime = Time.time;
        }

        public void AddWeapon(WeaponBase weapon)
        {
            if (!weapons.Contains(weapon))
            {
                weapons.Add(weapon);
                weapon.transform.SetParent(weaponHolder);
                weapon.gameObject.SetActive(false);
            }
        }

        public void AddAmmoToCurrentWeapon(int amount)
        {
            currentWeapon?.AddAmmo(amount);
        }

        public void AddAmmoToWeapon(int weaponIndex, int amount)
        {
            if (weaponIndex >= 0 && weaponIndex < weapons.Count)
            {
                weapons[weaponIndex]?.AddAmmo(amount);
            }
        }
    }
}

