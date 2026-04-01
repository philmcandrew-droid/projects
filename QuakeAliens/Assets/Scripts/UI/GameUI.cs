using UnityEngine;
using UnityEngine.UI;

namespace QuakeAliens.UI
{
    /// <summary>
    /// Main game UI - HUD, health, ammo, crosshair
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private Text healthText;
        [SerializeField] private Image healthBar;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text clipText;
        [SerializeField] private Text weaponNameText;
        [SerializeField] private Image weaponIcon;

        [Header("Crosshair")]
        [SerializeField] private Image crosshair;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hitColor = Color.red;

        [Header("Wave Info")]
        [SerializeField] private Text waveText;
        [SerializeField] private Text enemiesText;
        [SerializeField] private GameObject waveAnnouncement;
        [SerializeField] private Text waveAnnouncementText;

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverText;
        [SerializeField] private Text statsText;

        [Header("Victory")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Text victoryStatsText;

        [Header("Pause")]
        [SerializeField] private GameObject pausePanel;

        [Header("Damage Indicator")]
        [SerializeField] private Image damageVignette;
        [SerializeField] private float damageFlashDuration = 0.3f;

        private float lastHealth;
        private float crosshairHitTimer;

        public static GameUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted += OnWaveStarted;
                GameManager.Instance.OnWaveCompleted += OnWaveCompleted;
                GameManager.Instance.OnEnemyCountChanged += OnEnemyCountChanged;
                GameManager.Instance.OnGameOver += OnGameOver;
                GameManager.Instance.OnVictory += OnVictory;
            }

            // Hide panels
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (waveAnnouncement != null) waveAnnouncement.SetActive(false);

            lastHealth = 100f;
        }

        private void Update()
        {
            UpdateHUD();
            UpdateCrosshair();
            UpdatePauseMenu();
            UpdateDamageIndicator();
        }

        private void UpdateHUD()
        {
            var player = Player.PlayerController.Instance;
            var weaponManager = Weapons.WeaponManager.Instance;

            if (player != null)
            {
                // Health
                if (healthText != null)
                {
                    healthText.text = Mathf.CeilToInt(player.Health).ToString();
                }

                if (healthBar != null)
                {
                    healthBar.fillAmount = player.Health / player.MaxHealth;
                    
                    // Color based on health
                    if (player.Health > 60)
                        healthBar.color = Color.green;
                    else if (player.Health > 30)
                        healthBar.color = Color.yellow;
                    else
                        healthBar.color = Color.red;
                }

                // Check for damage taken
                if (player.Health < lastHealth)
                {
                    ShowDamageFlash();
                }
                lastHealth = player.Health;
            }

            if (weaponManager != null && weaponManager.CurrentWeapon != null)
            {
                var weapon = weaponManager.CurrentWeapon;

                // Weapon name
                if (weaponNameText != null)
                {
                    weaponNameText.text = weapon.WeaponName;
                }

                // Ammo
                if (ammoText != null)
                {
                    ammoText.text = weapon.CurrentAmmo.ToString();
                }

                if (clipText != null)
                {
                    clipText.text = $"{weapon.CurrentClip}/{weapon.ClipSize}";
                }

                // Weapon icon
                if (weaponIcon != null && weapon.WeaponIcon != null)
                {
                    weaponIcon.sprite = weapon.WeaponIcon;
                }
            }

            // Game info
            if (GameManager.Instance != null)
            {
                if (waveText != null)
                {
                    waveText.text = $"Wave {GameManager.Instance.CurrentWave}";
                }

                if (enemiesText != null)
                {
                    enemiesText.text = $"Enemies: {GameManager.Instance.EnemiesRemaining}";
                }
            }
        }

        private void UpdateCrosshair()
        {
            if (crosshair == null) return;

            // Fade back from hit color
            if (crosshairHitTimer > 0)
            {
                crosshairHitTimer -= Time.deltaTime;
                crosshair.color = Color.Lerp(normalColor, hitColor, crosshairHitTimer / 0.1f);
            }
        }

        private void UpdatePauseMenu()
        {
            if (pausePanel == null) return;

            if (GameManager.Instance != null)
            {
                pausePanel.SetActive(GameManager.Instance.CurrentState == GameManager.GameState.Paused);
            }
        }

        private void UpdateDamageIndicator()
        {
            if (damageVignette == null) return;

            // Fade out damage vignette
            Color c = damageVignette.color;
            c.a = Mathf.Lerp(c.a, 0, Time.deltaTime * 3f);
            damageVignette.color = c;
        }

        private void ShowDamageFlash()
        {
            if (damageVignette != null)
            {
                Color c = damageVignette.color;
                c.a = 0.5f;
                damageVignette.color = c;
            }
        }

        public void ShowHitMarker()
        {
            crosshairHitTimer = 0.1f;
            if (crosshair != null)
            {
                crosshair.color = hitColor;
            }
        }

        private void OnWaveStarted(int wave)
        {
            if (waveAnnouncement != null && waveAnnouncementText != null)
            {
                waveAnnouncementText.text = $"WAVE {wave}";
                waveAnnouncement.SetActive(true);
                Invoke(nameof(HideWaveAnnouncement), 3f);
            }
        }

        private void HideWaveAnnouncement()
        {
            if (waveAnnouncement != null)
            {
                waveAnnouncement.SetActive(false);
            }
        }

        private void OnWaveCompleted(int wave)
        {
            // Could show wave complete message
        }

        private void OnEnemyCountChanged(int count)
        {
            // Already handled in Update
        }

        private void OnGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameOverText != null)
            {
                gameOverText.text = "YOU DIED";
            }

            if (statsText != null && GameManager.Instance != null)
            {
                statsText.text = $"Wave Reached: {GameManager.Instance.CurrentWave}\n" +
                                $"Total Kills: {GameManager.Instance.TotalKills}\n" +
                                $"Time Survived: {FormatTime(GameManager.Instance.GameTime)}\n\n" +
                                "Press R to Restart";
            }
        }

        private void OnVictory()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            if (victoryStatsText != null && GameManager.Instance != null)
            {
                victoryStatsText.text = $"VICTORY!\n\n" +
                                       $"Total Kills: {GameManager.Instance.TotalKills}\n" +
                                       $"Time: {FormatTime(GameManager.Instance.GameTime)}\n\n" +
                                       "Press R to Play Again";
            }
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            return $"{minutes:00}:{seconds:00}";
        }

        public void OnResumeClicked()
        {
            GameManager.Instance?.TogglePause();
        }

        public void OnRestartClicked()
        {
            GameManager.Instance?.RestartGame();
        }

        public void OnQuitClicked()
        {
            GameManager.Instance?.QuitGame();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted -= OnWaveStarted;
                GameManager.Instance.OnWaveCompleted -= OnWaveCompleted;
                GameManager.Instance.OnEnemyCountChanged -= OnEnemyCountChanged;
                GameManager.Instance.OnGameOver -= OnGameOver;
                GameManager.Instance.OnVictory -= OnVictory;
            }
        }
    }
}
