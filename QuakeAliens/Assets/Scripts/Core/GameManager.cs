using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace QuakeAliens
{
    /// <summary>
    /// Main game manager - handles game state, spawning, and progression
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Menu,
            Playing,
            Paused,
            GameOver,
            Victory
        }

        [Header("Game Settings")]
        [SerializeField] private int totalWaves = 5;
        [SerializeField] private float waveCooldown = 5f;
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private float enemyScalingPerWave = 1.5f;

        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private float spawnDelay = 0.5f;

        [Header("Player")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private GameObject playerPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip gameOverMusic;

        private GameState currentState = GameState.Menu;
        private int currentWave = 0;
        private int enemiesRemaining;
        private int totalKills;
        private float gameTime;
        private List<Enemies.AlienBase> activeEnemies = new List<Enemies.AlienBase>();
        private AudioSource musicSource;

        public static GameManager Instance { get; private set; }
        
        public GameState CurrentState => currentState;
        public int CurrentWave => currentWave;
        public int EnemiesRemaining => enemiesRemaining;
        public int TotalKills => totalKills;
        public float GameTime => gameTime;

        // Events
        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action<int> OnEnemyCountChanged;
        public System.Action OnGameOver;
        public System.Action OnVictory;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
        }

        private void Start()
        {
            StartGame();
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                gameTime += Time.deltaTime;

                // Handle pause
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    TogglePause();
                }
            }
            else if (currentState == GameState.Paused)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    TogglePause();
                }
            }
            else if (currentState == GameState.GameOver || currentState == GameState.Victory)
            {
                // Restart on R key
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }
            }
        }

        public void StartGame()
        {
            currentState = GameState.Playing;
            currentWave = 0;
            totalKills = 0;
            gameTime = 0f;
            activeEnemies.Clear();

            // Play background music
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
            }

            // Spawn player if needed
            if (Player.PlayerController.Instance == null && playerPrefab != null && playerSpawnPoint != null)
            {
                Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            }

            // Start first wave
            StartNextWave();
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (currentState == GameState.Paused)
            {
                currentState = GameState.Playing;
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void StartNextWave()
        {
            if (currentWave >= totalWaves)
            {
                Victory();
                return;
            }

            currentWave++;
            int enemyCount = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(enemyScalingPerWave, currentWave - 1));
            
            OnWaveStarted?.Invoke(currentWave);
            
            StartCoroutine(SpawnWave(enemyCount));
        }

        private System.Collections.IEnumerator SpawnWave(int count)
        {
            enemiesRemaining = count;
            OnEnemyCountChanged?.Invoke(enemiesRemaining);

            for (int i = 0; i < count; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            // Pick random enemy type
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            // Pick random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            var alienBase = enemy.GetComponent<Enemies.AlienBase>();
            if (alienBase != null)
            {
                activeEnemies.Add(alienBase);
            }
        }

        public void OnEnemyKilled(Enemies.AlienBase enemy)
        {
            activeEnemies.Remove(enemy);
            enemiesRemaining--;
            totalKills++;
            
            OnEnemyCountChanged?.Invoke(enemiesRemaining);

            // Check if wave is complete
            if (enemiesRemaining <= 0)
            {
                OnWaveCompleted?.Invoke(currentWave);
                
                if (currentWave >= totalWaves)
                {
                    Victory();
                }
                else
                {
                    // Start next wave after cooldown
                    StartCoroutine(WaveCooldown());
                }
            }
        }

        private System.Collections.IEnumerator WaveCooldown()
        {
            yield return new WaitForSeconds(waveCooldown);
            StartNextWave();
        }

        public void GameOver()
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0.3f; // Slow-mo death

            if (gameOverMusic != null)
            {
                musicSource.clip = gameOverMusic;
                musicSource.Play();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnGameOver?.Invoke();
        }

        private void Victory()
        {
            currentState = GameState.Victory;

            if (victoryMusic != null)
            {
                musicSource.clip = victoryMusic;
                musicSource.Play();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnVictory?.Invoke();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
