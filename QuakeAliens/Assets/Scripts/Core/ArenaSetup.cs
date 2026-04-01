using UnityEngine;

namespace QuakeAliens
{
    /// <summary>
    /// Sets up the arena level with spawn points, pickups, and patrol routes
    /// Attach to an empty GameObject in the scene
    /// </summary>
    public class ArenaSetup : MonoBehaviour
    {
        [Header("Arena Dimensions")]
        [SerializeField] private float arenaWidth = 50f;
        [SerializeField] private float arenaLength = 50f;
        [SerializeField] private float wallHeight = 8f;

        [Header("Spawn Points")]
        [SerializeField] private int enemySpawnPointCount = 8;
        [SerializeField] private int pickupSpawnPointCount = 6;

        [Header("References")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material pillarMaterial;

        private Transform[] enemySpawnPoints;
        private Transform[] patrolPoints;
        private Transform playerSpawn;

        public Transform[] EnemySpawnPoints => enemySpawnPoints;
        public Transform[] PatrolPoints => patrolPoints;
        public Transform PlayerSpawn => playerSpawn;

        private void Awake()
        {
            GenerateArena();
        }

        [ContextMenu("Generate Arena")]
        public void GenerateArena()
        {
            // Clear existing children
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            CreateFloor();
            CreateWalls();
            CreatePillars();
            CreateSpawnPoints();
            CreatePatrolPoints();
            CreatePlayerSpawn();
            CreateLighting();
        }

        private void CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(transform);
            floor.transform.localPosition = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(arenaWidth, 1f, arenaLength);

            // Add NavMesh surface component marker
            floor.layer = LayerMask.NameToLayer("Default");
            floor.isStatic = true;

            if (floorMaterial != null)
            {
                floor.GetComponent<Renderer>().material = floorMaterial;
            }
            else
            {
                // Create a default industrial floor material
                var renderer = floor.GetComponent<Renderer>();
                renderer.material.color = new Color(0.3f, 0.3f, 0.35f);
            }
        }

        private void CreateWalls()
        {
            // Four walls around the arena
            CreateWall("North Wall", new Vector3(0, wallHeight / 2, arenaLength / 2), new Vector3(arenaWidth, wallHeight, 1));
            CreateWall("South Wall", new Vector3(0, wallHeight / 2, -arenaLength / 2), new Vector3(arenaWidth, wallHeight, 1));
            CreateWall("East Wall", new Vector3(arenaWidth / 2, wallHeight / 2, 0), new Vector3(1, wallHeight, arenaLength));
            CreateWall("West Wall", new Vector3(-arenaWidth / 2, wallHeight / 2, 0), new Vector3(1, wallHeight, arenaLength));
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.isStatic = true;

            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }
            else
            {
                var renderer = wall.GetComponent<Renderer>();
                renderer.material.color = new Color(0.4f, 0.35f, 0.3f);
            }
        }

        private void CreatePillars()
        {
            // Create some pillars for cover - Quake-style
            float pillarHeight = 4f;
            float pillarSize = 2f;

            Vector3[] pillarPositions = new Vector3[]
            {
                new Vector3(-10, pillarHeight / 2, -10),
                new Vector3(10, pillarHeight / 2, -10),
                new Vector3(-10, pillarHeight / 2, 10),
                new Vector3(10, pillarHeight / 2, 10),
                new Vector3(0, pillarHeight / 2, 0),
                new Vector3(-15, pillarHeight / 2, 0),
                new Vector3(15, pillarHeight / 2, 0),
                new Vector3(0, pillarHeight / 2, 15),
                new Vector3(0, pillarHeight / 2, -15),
            };

            GameObject pillarsParent = new GameObject("Pillars");
            pillarsParent.transform.SetParent(transform);

            foreach (var pos in pillarPositions)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = "Pillar";
                pillar.transform.SetParent(pillarsParent.transform);
                pillar.transform.localPosition = pos;
                pillar.transform.localScale = new Vector3(pillarSize, pillarHeight, pillarSize);
                pillar.isStatic = true;

                if (pillarMaterial != null)
                {
                    pillar.GetComponent<Renderer>().material = pillarMaterial;
                }
                else
                {
                    var renderer = pillar.GetComponent<Renderer>();
                    renderer.material.color = new Color(0.5f, 0.45f, 0.4f);
                }
            }

            // Create some elevated platforms
            CreatePlatform(new Vector3(-18, 2, -18), new Vector3(6, 0.5f, 6));
            CreatePlatform(new Vector3(18, 2, -18), new Vector3(6, 0.5f, 6));
            CreatePlatform(new Vector3(-18, 2, 18), new Vector3(6, 0.5f, 6));
            CreatePlatform(new Vector3(18, 2, 18), new Vector3(6, 0.5f, 6));
        }

        private void CreatePlatform(Vector3 position, Vector3 scale)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.SetParent(transform);
            platform.transform.localPosition = position;
            platform.transform.localScale = scale;
            platform.isStatic = true;

            var renderer = platform.GetComponent<Renderer>();
            renderer.material.color = new Color(0.35f, 0.35f, 0.4f);

            // Add ramp
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Ramp";
            ramp.transform.SetParent(platform.transform);
            ramp.transform.localPosition = new Vector3(0, -0.5f, 1.5f);
            ramp.transform.localScale = new Vector3(0.5f, 4f, 0.3f);
            ramp.transform.localRotation = Quaternion.Euler(45, 0, 0);
            ramp.isStatic = true;

            ramp.GetComponent<Renderer>().material.color = new Color(0.35f, 0.35f, 0.4f);
        }

        private void CreateSpawnPoints()
        {
            GameObject spawnParent = new GameObject("Enemy Spawn Points");
            spawnParent.transform.SetParent(transform);

            enemySpawnPoints = new Transform[enemySpawnPointCount];

            // Create spawn points around the edges of the arena
            for (int i = 0; i < enemySpawnPointCount; i++)
            {
                GameObject spawn = new GameObject($"Spawn Point {i + 1}");
                spawn.transform.SetParent(spawnParent.transform);

                float angle = (360f / enemySpawnPointCount) * i;
                float radius = Mathf.Min(arenaWidth, arenaLength) * 0.4f;
                
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                
                spawn.transform.localPosition = new Vector3(x, 0.5f, z);
                spawn.transform.LookAt(Vector3.zero);

                enemySpawnPoints[i] = spawn.transform;
            }
        }

        private void CreatePatrolPoints()
        {
            GameObject patrolParent = new GameObject("Patrol Points");
            patrolParent.transform.SetParent(transform);

            int patrolCount = 12;
            patrolPoints = new Transform[patrolCount];

            // Create patrol points in a pattern
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-15, 0.5f, -15),
                new Vector3(0, 0.5f, -15),
                new Vector3(15, 0.5f, -15),
                new Vector3(15, 0.5f, 0),
                new Vector3(15, 0.5f, 15),
                new Vector3(0, 0.5f, 15),
                new Vector3(-15, 0.5f, 15),
                new Vector3(-15, 0.5f, 0),
                new Vector3(-8, 0.5f, -8),
                new Vector3(8, 0.5f, -8),
                new Vector3(8, 0.5f, 8),
                new Vector3(-8, 0.5f, 8),
            };

            for (int i = 0; i < patrolCount && i < positions.Length; i++)
            {
                GameObject patrol = new GameObject($"Patrol Point {i + 1}");
                patrol.transform.SetParent(patrolParent.transform);
                patrol.transform.localPosition = positions[i];
                patrolPoints[i] = patrol.transform;
            }
        }

        private void CreatePlayerSpawn()
        {
            GameObject spawn = new GameObject("Player Spawn");
            spawn.transform.SetParent(transform);
            spawn.transform.localPosition = new Vector3(0, 1f, -20);
            spawn.transform.rotation = Quaternion.identity;
            playerSpawn = spawn.transform;
        }

        private void CreateLighting()
        {
            // Create ambient lighting
            GameObject lightingParent = new GameObject("Lighting");
            lightingParent.transform.SetParent(transform);

            // Main directional light
            GameObject mainLight = new GameObject("Main Light");
            mainLight.transform.SetParent(lightingParent.transform);
            mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.8f);
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;

            // Add some point lights for atmosphere
            CreatePointLight(lightingParent.transform, new Vector3(-15, 6, -15), new Color(1f, 0.5f, 0.3f));
            CreatePointLight(lightingParent.transform, new Vector3(15, 6, -15), new Color(0.3f, 0.5f, 1f));
            CreatePointLight(lightingParent.transform, new Vector3(-15, 6, 15), new Color(0.3f, 1f, 0.5f));
            CreatePointLight(lightingParent.transform, new Vector3(15, 6, 15), new Color(1f, 0.3f, 0.5f));
            CreatePointLight(lightingParent.transform, new Vector3(0, 8, 0), new Color(1f, 1f, 1f), 15f);
        }

        private void CreatePointLight(Transform parent, Vector3 position, Color color, float range = 10f)
        {
            GameObject lightObj = new GameObject("Point Light");
            lightObj.transform.SetParent(parent);
            lightObj.transform.localPosition = position;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 2f;
            light.range = range;
        }

        private void OnDrawGizmos()
        {
            // Draw arena bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(arenaWidth, wallHeight, arenaLength));

            // Draw spawn points
            if (enemySpawnPoints != null)
            {
                Gizmos.color = Color.red;
                foreach (var spawn in enemySpawnPoints)
                {
                    if (spawn != null)
                    {
                        Gizmos.DrawWireSphere(spawn.position, 1f);
                    }
                }
            }

            // Draw patrol points
            if (patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                        
                        // Draw lines between patrol points
                        int next = (i + 1) % patrolPoints.Length;
                        if (patrolPoints[next] != null)
                        {
                            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
                        }
                    }
                }
            }

            // Draw player spawn
            if (playerSpawn != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerSpawn.position, 1f);
            }
        }
    }
}

