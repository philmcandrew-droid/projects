# Quake Aliens - FPS Shooter Game

A fast-paced arena FPS shooter inspired by the classic **Quake 2**, built in Unity. Battle waves of alien invaders with an arsenal of devastating weapons!

![Quake Aliens](https://img.shields.io/badge/Unity-2021.3+-blue) ![License](https://img.shields.io/badge/License-MIT-green)

## 🎮 Features

### Weapons
- **Pistol** - Reliable sidearm with high accuracy
- **Shotgun** - Devastating at close range, fires 8 pellets
- **Railgun** - High-powered penetrating beam, classic Quake weapon!
- **Laser** - Continuous beam weapon with damage over time

### Enemies
- **Alien Grunt** - Fast melee attacker with leap ability
- **Alien Shooter** - Ranged enemy with burst-fire projectiles  
- **Alien Brute** - Heavy tank with charge attack and ground slam

### Gameplay
- Quake-style movement (strafe jumping, bunny hopping, air control)
- Wave-based survival gameplay
- Health and ammo pickups
- Single arena level with cover and platforms

## 🚀 Quick Start

### Requirements
- Unity 2021.3 LTS or newer
- NavMesh Components package

### Setup Instructions

1. **Create a new Unity project** (3D template)

2. **Import the scripts**
   - Copy the `Assets/Scripts` folder to your Unity project

3. **Setup the scene using the menu**
   ```
   QuakeAliens > Setup > Setup Complete Scene
   ```
   This creates:
   - Arena with walls, pillars, and platforms
   - Player prefab with controller and weapons
   - Game Manager with wave spawning
   - UI canvas with HUD

4. **Create Enemy Prefabs**
   ```
   QuakeAliens > Setup > Create Alien Grunt Prefab
   QuakeAliens > Setup > Create Alien Shooter Prefab
   QuakeAliens > Setup > Create Alien Brute Prefab
   QuakeAliens > Setup > Create Projectile Prefab
   ```
   Drag each to `Assets/Prefabs` folder

5. **Create Weapon Prefabs**
   ```
   QuakeAliens > Setup > Create Weapon Prefabs
   ```
   Drag each to `Assets/Prefabs` folder

6. **Bake NavMesh**
   - Open `Window > AI > Navigation`
   - Select the Arena floor and walls, mark as "Navigation Static"
   - Click "Bake" in the Navigation window

7. **Wire up references**
   - Select GameManager and assign enemy prefabs to EnemySpawner
   - Select Player and assign weapon prefabs to WeaponManager
   - Link UI elements in GameUI component

8. **Play!**

## 🎯 Controls

| Action | Key |
|--------|-----|
| Move | WASD |
| Look | Mouse |
| Jump | Space |
| Sprint | Left Shift |
| Fire | Left Mouse |
| Reload | R |
| Weapon 1 (Pistol) | 1 |
| Weapon 2 (Shotgun) | 2 |
| Weapon 3 (Railgun) | 3 |
| Weapon 4 (Laser) | 4 |
| Switch Weapon | Mouse Wheel |
| Pause | Escape |
| Restart (after death) | R |

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs    # Quake-style FPS movement
│   │   └── PlayerCamera.cs        # Head bob, view kick, screen shake
│   ├── Weapons/
│   │   ├── WeaponBase.cs          # Base weapon class
│   │   ├── WeaponManager.cs       # Weapon switching & input
│   │   ├── Pistol.cs              # Standard pistol
│   │   ├── Shotgun.cs             # Spread shotgun
│   │   ├── Railgun.cs             # Penetrating beam
│   │   └── Laser.cs               # Continuous beam
│   ├── Enemies/
│   │   ├── AlienBase.cs           # Base enemy AI (patrol/chase/attack)
│   │   ├── AlienGrunt.cs          # Melee attacker
│   │   ├── AlienShooter.cs        # Ranged attacker
│   │   ├── AlienBrute.cs          # Heavy tank
│   │   └── AlienProjectile.cs     # Enemy projectile
│   ├── Core/
│   │   ├── GameManager.cs         # Game state & waves
│   │   ├── EnemySpawner.cs        # Wave spawning
│   │   ├── ArenaSetup.cs          # Level generation
│   │   ├── Pickup.cs              # Health/ammo pickups
│   │   ├── AudioManager.cs        # Sound management
│   │   └── PrefabSetup.cs         # Editor setup helpers
│   └── UI/
│       └── GameUI.cs              # HUD, menus, crosshair
├── Prefabs/
├── Materials/
└── Scenes/
```

## 🛠️ Customization

### Adjusting Difficulty
In `GameManager.cs`:
```csharp
[SerializeField] private int totalWaves = 5;
[SerializeField] private int baseEnemiesPerWave = 5;
[SerializeField] private float enemyScalingPerWave = 1.5f;
```

### Weapon Balancing
Each weapon has configurable stats in its script:
- `damage` - Damage per hit
- `fireRate` - Time between shots
- `clipSize` - Ammo per magazine
- `reloadTime` - Reload duration
- `range` - Maximum range
- `recoilKick` - Camera kick on fire

### Enemy Stats
In each enemy script:
- `maxHealth` - Hit points
- `damage` - Damage dealt
- `attackRange` - Attack distance
- `detectionRange` - Player detection radius
- `walkSpeed/runSpeed` - Movement speed

### Arena Size
In `ArenaSetup.cs`:
```csharp
[SerializeField] private float arenaWidth = 50f;
[SerializeField] private float arenaLength = 50f;
```

## 🎨 Adding Visuals

The scripts work with placeholder geometry. To make it look better:

1. **Player Model** - Replace capsule with FPS arms model
2. **Weapon Models** - Import/create 3D weapon models
3. **Enemy Models** - Create or import alien character models
4. **Materials** - Create PBR materials for arena surfaces
5. **Particles** - Add muzzle flashes, impacts, explosions
6. **Skybox** - Create atmospheric sci-fi skybox

## 🔊 Adding Audio

Assign audio clips in the Inspector for:
- Weapon fire/reload/empty sounds
- Enemy idle/attack/hurt/death sounds
- Background music
- UI sounds
- Ambient arena sounds

## 📝 Tips

1. **Movement**: Master strafe jumping for speed boosts
2. **Railgun**: Save for multiple enemies in a line
3. **Shotgun**: Get close for maximum damage
4. **Laser**: Great for consistent damage on single targets
5. **Use cover**: Pillars block projectiles!

## 🐛 Troubleshooting

**Enemies not moving?**
- Make sure NavMesh is baked
- Check that floor is marked Navigation Static

**Weapons not firing?**
- Ensure weapons are assigned to WeaponManager
- Check that fire point transform is set

**UI not updating?**
- Link all UI references in GameUI component
- Verify GameManager events are connected

## 📜 License

MIT License - Feel free to use and modify!

---

*Inspired by id Software's Quake 2 (1997)*

