using UnityEngine;
using System.Collections.Generic;

namespace QuakeAliens
{
    /// <summary>
    /// Handles enemy spawning and wave management
    /// Works with GameManager for wave-based gameplay
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject alienGruntPrefab;
        [SerializeField] private GameObject alienShooterPrefab;
        [SerializeField] private GameObject alienBrutePrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform[] patrolPoints;

        [Header("Wave Composition")]
        [SerializeField] private float gruntChance = 0.5f;
        [SerializeField] private float shooterChance = 0.35f;
        [SerializeField] private float bruteChance = 0.15f;

        private List<Enemies.AlienBase> activeEnemies = new List<Enemies.AlienBase>();

        public static EnemySpawner Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Try to get spawn points from ArenaSetup if not set
            if ((spawnPoints == null || spawnPoints.Length == 0))
            {
                var arenaSetup = FindObjectOfType<ArenaSetup>();
                if (arenaSetup != null)
                {
                    spawnPoints = arenaSetup.EnemySpawnPoints;
                    patrolPoints = arenaSetup.PatrolPoints;
                }
            }
        }

        public Enemies.AlienBase SpawnEnemy(EnemyType type, Vector3 position)
        {
            GameObject prefab = GetPrefabForType(type);
            if (prefab == null) return null;

            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
            var alienBase = enemy.GetComponent<Enemies.AlienBase>();

            if (alienBase != null)
            {
                activeEnemies.Add(alienBase);
                
                // Assign patrol points
                AssignPatrolPoints(alienBase);
            }

            return alienBase;
        }

        public Enemies.AlienBase SpawnRandomEnemy()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return null;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            EnemyType type = GetRandomEnemyType();

            return SpawnEnemy(type, spawnPoint.position);
        }

        public Enemies.AlienBase SpawnRandomEnemyAtPoint(Transform spawnPoint)
        {
            EnemyType type = GetRandomEnemyType();
            return SpawnEnemy(type, spawnPoint.position);
        }

        private EnemyType GetRandomEnemyType()
        {
            float roll = Random.value;
            
            if (roll < gruntChance)
                return EnemyType.Grunt;
            else if (roll < gruntChance + shooterChance)
                return EnemyType.Shooter;
            else
                return EnemyType.Brute;
        }

        private GameObject GetPrefabForType(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Grunt:
                    return alienGruntPrefab;
                case EnemyType.Shooter:
                    return alienShooterPrefab;
                case EnemyType.Brute:
                    return alienBrutePrefab;
                default:
                    return alienGruntPrefab;
            }
        }

        private void AssignPatrolPoints(Enemies.AlienBase alien)
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            // Assign random subset of patrol points
            int pointCount = Mathf.Min(4, patrolPoints.Length);
            Transform[] assignedPoints = new Transform[pointCount];
            
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                availableIndices.Add(i);
            }

            for (int i = 0; i < pointCount; i++)
            {
                int randomIndex = Random.Range(0, availableIndices.Count);
                assignedPoints[i] = patrolPoints[availableIndices[randomIndex]];
                availableIndices.RemoveAt(randomIndex);
            }

            // Use reflection or serialized field to assign patrol points
            // This would require the AlienBase to have a public method or property
            // For now, enemies will use their default patrol behavior
        }

        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            activeEnemies.Clear();
        }

        public int GetActiveEnemyCount()
        {
            // Clean up null references
            activeEnemies.RemoveAll(e => e == null);
            return activeEnemies.Count;
        }
    }

    public enum EnemyType
    {
        Grunt,
        Shooter,
        Brute
    }
}

