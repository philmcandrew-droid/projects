using UnityEngine;

namespace QuakeAliens
{
    /// <summary>
    /// Helper class for creating visual effects at runtime
    /// </summary>
    public static class EffectsHelper
    {
        /// <summary>
        /// Create a simple muzzle flash effect
        /// </summary>
        public static GameObject CreateMuzzleFlash(Vector3 position, Color color)
        {
            GameObject flash = new GameObject("MuzzleFlash");
            flash.transform.position = position;

            // Add light
            Light light = flash.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 3f;
            light.range = 5f;

            // Auto destroy
            Object.Destroy(flash, 0.05f);

            return flash;
        }

        /// <summary>
        /// Create a bullet impact effect
        /// </summary>
        public static GameObject CreateImpactEffect(Vector3 position, Vector3 normal, Color color)
        {
            GameObject impact = new GameObject("Impact");
            impact.transform.position = position;
            impact.transform.rotation = Quaternion.LookRotation(normal);

            // Add particle system
            ParticleSystem ps = impact.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = 0.2f;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startColor = color;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1f;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 10) });
            emission.rateOverTime = 0;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.1f;

            // Auto destroy
            Object.Destroy(impact, 2f);

            return impact;
        }

        /// <summary>
        /// Create a bullet trail line
        /// </summary>
        public static GameObject CreateBulletTrail(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
        {
            GameObject trail = new GameObject("BulletTrail");
            
            LineRenderer lr = trail.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;

            Object.Destroy(trail, duration);

            return trail;
        }

        /// <summary>
        /// Create an explosion effect
        /// </summary>
        public static GameObject CreateExplosion(Vector3 position, float radius, Color color)
        {
            GameObject explosion = new GameObject("Explosion");
            explosion.transform.position = position;

            // Light flash
            Light light = explosion.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 5f;
            light.range = radius * 2f;

            // Particle system
            ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = radius * 0.5f;
            main.startLifetime = 1f;
            main.startSpeed = radius;
            main.startColor = color;
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 30) });
            emission.rateOverTime = 0;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0), new GradientColorKey(Color.gray, 1) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }
            );
            colorOverLifetime.color = grad;

            Object.Destroy(explosion, 3f);

            return explosion;
        }

        /// <summary>
        /// Create a death effect for enemies
        /// </summary>
        public static GameObject CreateDeathEffect(Vector3 position, Color primaryColor)
        {
            GameObject death = new GameObject("DeathEffect");
            death.transform.position = position;

            // Blood/goo splatter
            ParticleSystem ps = death.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startLifetime = 2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 8f);
            main.startColor = primaryColor;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1f;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 50) });
            emission.rateOverTime = 0;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            // Add light flash
            Light light = death.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = primaryColor;
            light.intensity = 2f;
            light.range = 5f;

            Object.Destroy(death, 3f);

            return death;
        }

        /// <summary>
        /// Create floating damage number
        /// </summary>
        public static void CreateDamageNumber(Vector3 position, float damage, bool isCritical = false)
        {
            // This would require TextMeshPro or Unity UI
            // For now, just debug log
            Debug.Log($"Damage: {damage}{(isCritical ? " CRITICAL!" : "")} at {position}");
        }
    }
}

