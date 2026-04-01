using UnityEngine;

namespace QuakeAliens
{
    /// <summary>
    /// Helper script for setting up prefabs in the Unity Editor
    /// Provides methods to create player, enemy, and weapon prefabs with proper components
    /// </summary>
    public class PrefabSetup : MonoBehaviour
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("QuakeAliens/Setup/Create Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            // Create player root
            GameObject player = new GameObject("Player");
            
            // Add components
            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0, 1f, 0);
            
            player.AddComponent<Player.PlayerController>();
            
            // Create camera holder
            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(player.transform);
            cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);
            cameraHolder.AddComponent<Player.PlayerCamera>();
            
            // Create main camera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.transform.SetParent(cameraHolder.transform);
            cameraObj.transform.localPosition = Vector3.zero;
            cameraObj.tag = "MainCamera";
            
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.fieldOfView = 90f;
            
            cameraObj.AddComponent<AudioListener>();
            
            // Create weapon holder
            GameObject weaponHolder = new GameObject("WeaponHolder");
            weaponHolder.transform.SetParent(cameraObj.transform);
            weaponHolder.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            
            var weaponManager = player.AddComponent<Weapons.WeaponManager>();
            
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");
            
            Debug.Log("Player prefab created! Drag it to Assets/Prefabs folder.");
            UnityEditor.Selection.activeGameObject = player;
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Create Alien Grunt Prefab")]
        public static void CreateAlienGruntPrefab()
        {
            GameObject enemy = CreateBaseEnemy("Alien Grunt");
            enemy.AddComponent<Enemies.AlienGrunt>();
            
            // Add rigidbody for leap ability
            var rb = enemy.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            // Make it look more aggressive (scale up slightly)
            enemy.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            
            Debug.Log("Alien Grunt prefab created!");
            UnityEditor.Selection.activeGameObject = enemy;
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Create Alien Shooter Prefab")]
        public static void CreateAlienShooterPrefab()
        {
            GameObject enemy = CreateBaseEnemy("Alien Shooter");
            enemy.AddComponent<Enemies.AlienShooter>();
            
            // Create fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(enemy.transform);
            firePoint.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
            
            Debug.Log("Alien Shooter prefab created!");
            UnityEditor.Selection.activeGameObject = enemy;
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Create Alien Brute Prefab")]
        public static void CreateAlienBrutePrefab()
        {
            GameObject enemy = CreateBaseEnemy("Alien Brute");
            enemy.AddComponent<Enemies.AlienBrute>();
            
            // Make it bigger
            enemy.transform.localScale = new Vector3(1.5f, 1.8f, 1.5f);
            
            // Update NavMeshAgent for larger size
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            agent.radius = 0.8f;
            agent.height = 3f;
            
            Debug.Log("Alien Brute prefab created!");
            UnityEditor.Selection.activeGameObject = enemy;
        }

        private static GameObject CreateBaseEnemy(string name)
        {
            // Create enemy root with capsule body
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            
            // Add NavMeshAgent
            var agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = 3.5f;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 1f;
            agent.radius = 0.5f;
            agent.height = 2f;
            
            // Add audio source
            enemy.AddComponent<AudioSource>();
            
            // Set layer
            enemy.layer = LayerMask.NameToLayer("Default");
            
            // Give it an alien-ish color
            var renderer = enemy.GetComponent<Renderer>();
            renderer.material.color = new Color(0.3f, 0.8f, 0.4f); // Alien green
            
            return enemy;
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Create Projectile Prefab")]
        public static void CreateProjectilePrefab()
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Alien Projectile";
            projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            
            // Add projectile component
            projectile.AddComponent<Enemies.AlienProjectile>();
            
            // Setup collider as trigger
            var collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            
            // Add trail
            var trail = projectile.AddComponent<TrailRenderer>();
            trail.startWidth = 0.2f;
            trail.endWidth = 0f;
            trail.time = 0.3f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(1f, 0.3f, 0.1f);
            trail.endColor = new Color(1f, 0.3f, 0.1f, 0f);
            
            // Add light
            var light = projectile.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.5f, 0.2f);
            light.intensity = 2f;
            light.range = 5f;
            
            // Set material color
            var renderer = projectile.GetComponent<Renderer>();
            renderer.material.color = new Color(1f, 0.5f, 0.2f);
            renderer.material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.2f) * 2f);
            
            Debug.Log("Projectile prefab created!");
            UnityEditor.Selection.activeGameObject = projectile;
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Create Weapon Prefabs")]
        public static void CreateWeaponPrefabs()
        {
            CreateWeaponPrefab<Weapons.Pistol>("Pistol", new Color(0.3f, 0.3f, 0.3f));
            CreateWeaponPrefab<Weapons.Shotgun>("Shotgun", new Color(0.4f, 0.3f, 0.2f));
            CreateWeaponPrefab<Weapons.Railgun>("Railgun", new Color(0.2f, 0.3f, 0.5f));
            CreateWeaponPrefab<Weapons.Laser>("Laser", new Color(0.5f, 0.2f, 0.2f));
            
            Debug.Log("All weapon prefabs created!");
        }

        private static void CreateWeaponPrefab<T>(string name, Color color) where T : Weapons.WeaponBase
        {
            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weapon.name = name;
            weapon.transform.localScale = new Vector3(0.1f, 0.1f, 0.5f);
            
            // Remove collider (weapons don't need collision)
            DestroyImmediate(weapon.GetComponent<BoxCollider>());
            
            // Add weapon component
            weapon.AddComponent<T>();
            
            // Create fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(weapon.transform);
            firePoint.transform.localPosition = new Vector3(0, 0, 0.3f);
            
            // Set material color
            var renderer = weapon.GetComponent<Renderer>();
            renderer.material.color = color;
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Create Game Manager")]
        public static void CreateGameManager()
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            gm.AddComponent<AudioManager>();
            gm.AddComponent<EnemySpawner>();
            
            Debug.Log("GameManager created!");
            UnityEditor.Selection.activeGameObject = gm;
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Create UI Canvas")]
        public static void CreateUICanvas()
        {
            // Create canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Add UI script
            canvasObj.AddComponent<UI.GameUI>();
            
            // Create HUD panel
            GameObject hudPanel = CreateUIPanel(canvasObj.transform, "HUD Panel");
            
            // Create health display
            GameObject healthGroup = new GameObject("Health Group");
            healthGroup.transform.SetParent(hudPanel.transform);
            var healthRect = healthGroup.AddComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0, 0);
            healthRect.anchorMax = new Vector2(0, 0);
            healthRect.pivot = new Vector2(0, 0);
            healthRect.anchoredPosition = new Vector2(20, 20);
            healthRect.sizeDelta = new Vector2(200, 50);
            
            // Health text
            CreateUIText(healthGroup.transform, "Health Text", "100", new Vector2(0, 0));
            
            // Create ammo display
            GameObject ammoGroup = new GameObject("Ammo Group");
            ammoGroup.transform.SetParent(hudPanel.transform);
            var ammoRect = ammoGroup.AddComponent<RectTransform>();
            ammoRect.anchorMin = new Vector2(1, 0);
            ammoRect.anchorMax = new Vector2(1, 0);
            ammoRect.pivot = new Vector2(1, 0);
            ammoRect.anchoredPosition = new Vector2(-20, 20);
            ammoRect.sizeDelta = new Vector2(200, 50);
            
            CreateUIText(ammoGroup.transform, "Ammo Text", "30/100", new Vector2(0, 0));
            
            // Create crosshair
            GameObject crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(hudPanel.transform);
            var crossRect = crosshair.AddComponent<RectTransform>();
            crossRect.anchorMin = new Vector2(0.5f, 0.5f);
            crossRect.anchorMax = new Vector2(0.5f, 0.5f);
            crossRect.sizeDelta = new Vector2(20, 20);
            var crossImage = crosshair.AddComponent<UnityEngine.UI.Image>();
            crossImage.color = Color.white;
            
            Debug.Log("UI Canvas created!");
            UnityEditor.Selection.activeGameObject = canvasObj;
        }

        private static GameObject CreateUIPanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }

        private static void CreateUIText(Transform parent, string name, string text, Vector2 position)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200, 50);
            
            var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
            textComp.text = text;
            textComp.fontSize = 24;
            textComp.color = Color.white;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        [UnityEditor.MenuItem("QuakeAliens/Setup/Setup Complete Scene")]
        public static void SetupCompleteScene()
        {
            // Create arena
            GameObject arenaObj = new GameObject("Arena");
            arenaObj.AddComponent<ArenaSetup>();
            
            // Create game manager
            CreateGameManager();
            
            // Create UI
            CreateUICanvas();
            
            // Create player
            CreatePlayerPrefab();
            
            Debug.Log("Complete scene setup done! Don't forget to:");
            Debug.Log("1. Bake NavMesh (Window > AI > Navigation)");
            Debug.Log("2. Create enemy prefabs and assign to EnemySpawner");
            Debug.Log("3. Link UI references in GameUI component");
        }
        #endif
    }
}

