# Shuriken VR — AGENTS.md

Sprint mode. Shipping in ~12 hours. Read this in full before touching anything. Don't re-derive
architecture by scanning the project — everything you need is here or in CHECKLIST.md.

## Project
Unity 6 (6000.0.x), Meta Interaction SDK v201.0.0, Quest 3, Android/IL2CPP final build.
Repo root: C:\Shuriken_VR\Shuriken_VR

## Real project file paths (ground truth — use these, don't guess or search)
```
Assets\Enemy\Prefab\P_Mutant.prefab                     <- Mutant enemy prefab
Assets\Prefab\Fireball.prefab
Assets\Prefab\Iceshard.prefab
Assets\Prefab\CastTrailParticles.prefab
Assets\Prefab\ImpactParticleSystem.prefab
Assets\ScriptableObjects\Spells\FireballData.asset
Assets\ScriptableObjects\Spells\IceShardData.asset
Assets\ScriptableObjects\Spells\SnakeVenom.asset         <- status unknown, confirm before use
Assets\ScriptableObjects\Waves\Wave_1.asset
Assets\ScriptableObjects\Waves\Wave_2.asset
Assets\StatusEffect\IceSlow_Standard.asset                <- SO instance (class lives in Scripts\IceScripts)

Assets\Scripts\EnemyMove.cs
Assets\Scripts\FireballProjectile.cs
Assets\Scripts\GameManager.cs
Assets\Scripts\Health.cs                                   <- THE canonical shared health/damage component
Assets\Scripts\SpellCaster.cs
Assets\Scripts\SpellData.cs
Assets\Scripts\IceScripts\StatusEffect.cs
Assets\Scripts\IceScripts\IceSlowEffect.cs
Assets\Scripts\KGP\GestureManager.cs                        <- LOCKED
Assets\Scripts\KGP\IGestureProvider.cs                      <- LOCKED
Assets\Scripts\KGP\HandTrackingGestureProvider.cs           <- LOCKED
Assets\Scripts\KGP\KeyboardGestureProvider.cs               <- LOCKED
Assets\Scripts\KGP\EditorMouseLook.cs                        <- LOCKED (editor tool)
Assets\Scripts\UI\BillboardUI.cs
Assets\Scripts\UI\EnemyHealthBarUI.cs
Assets\Scripts\UI\WaveCounterUI.cs
Assets\Scripts\UI\CooldownIndicatorUI.cs
Assets\Scripts\Wave\WaveData.cs
Assets\Scripts\Wave\WaveManager.cs
Assets\Scripts\Wave\WaveSpawner.cs

Assets\FourEvilDragonsHP\Prefab\DragonNightmare\{Albino,Blue,DarkBlue,Green}.prefab
Assets\FourEvilDragonsHP\Prefab\DragonSoulEater\{Blue,Green,Grey,Red}.prefab
Assets\FourEvilDragonsHP\Prefab\DragonTerrorBringer\{Blue,Green,Purple,Red}.prefab
Assets\FourEvilDragonsHP\Prefab\DragonUsurper\{Blue,Green,Purple,Red}.prefab
  -> these ARE the 4 dragon types (color = reskin within a type, not a separate type).
  -> Assets\FourEvilDragonsHP\Scene\*Scene\ are the asset pack's own demo scenes — reference
     only, never edit, never treat as the game's scene.

Assets\Vefects\Trails\VFX\Particles\VFX_Trail_{Fire,Ice,...}.prefab   <- pre-made VFX, Pinju's own task
Assets\Travis Game Assets\Hit Impact Effects\Prefabs\Hits\Hit_0{1-4}.prefab  <- pre-made VFX, Pinju's own task

Assets\Scenes\Game_Scene.unity        <- THE live gameplay scene. This is what Codex tests in.
Assets\Scenes\MainMenu.unity          <- separate, new, unrelated to core sprint items
Assets\Scenes\EnemyNavMesh\, PoseExamples_Test\, Testing\  <- stale test scenes, not ground truth, don't edit

RANGED ENEMY MODEL: not yet located in the tree — [PINJU: fill in exact path here before Item 6]
```

## Known discrepancies — confirm before Codex touches related files
- **`Assets\Scripts\ProgBasics\{BaseEntity,EnemyEntity,HealthComponent,PlayerEntity}.cs`** looks
  like an early teaching scaffold, separate from and older than the real `Health.cs` /
  `EnemyMove.cs` system this whole project is actually built on. Treat as **legacy/dead code —
  ignore, do not extend, do not confuse with `Health.cs`**, unless Pinju confirms it's still live.
- `SnakeVenom.asset` (third spell SO) — status unconfirmed. Don't wire anything to it unless
  Pinju says it's active.
- `Assets\Scripts\Praneet\` (FloatingTitle.cs, TutorialSignActivator.cs) — another contributor's
  MainMenu work, unrelated to the sprint items. Green zone but hands-off unless explicitly asked.

## Locked architecture (do not deviate)
- Data = ScriptableObject (SpellData, WaveData, StatusEffect + subclasses). Behaviour =
  MonoBehaviour. New content = new SO asset / prefab variant wherever a pattern already exists,
  not new code.
- Cross-system comms = UnityEvent, subscribed OnEnable/OnDisable (or Awake where established):
  Health.OnDamaged, Health.OnDeath, WaveManager.OnWaveStarted, WaveManager.OnAllWavesCleared,
  GameManager state listeners.
- Health.cs = single shared damage/death component. Everything damageable gets one — no
  bespoke HP fields.
- SpellCaster.cs: ONE shared cooldown across all player spells. Not a bug, don't "fix" it.
- StatusEffect: abstract SO base, [CreateAssetMenu] on concrete subclasses only.
- EnemyMove.cs FSM: Idle/Chase/Attack/Dead, enum-switch. Guard (_currentState != Dead) before
  touching NavMeshAgent — disabled agents throw on ResetPath().
- IGestureProvider + ControllerGestureProvider / HandTrackingGestureProvider /
  KeyboardGestureProvider + GestureManager: LOCKED. This is casting-input only.

## Zones

**RED — never touch:** Meta Interaction/XR SDK package files. IGestureProvider and its 3
implementations + GestureManager. Existing pose-detection prefabs/wiring for Fireball/Ice
Shard. Hand-tracking data source references anywhere. ProjectSettings/, Packages/manifest.json,
.gitignore.

**RED WITH ONE NAMED EXCEPTION — locomotion task only:** OVRCameraRig / player rig hierarchy is
normally red-zone. For the locomotion system task specifically, you may add movement/
interaction components to the rig root (a locomotion controller, grab anchors, etc). You may
NOT touch anything under the rig related to hand-tracking data sources, the gesture provider
components, or camera/head anchor configuration. If it's ambiguous whether a rig object is
movement-related or gesture-related, stop and ask rather than guessing.

**YELLOW — additive only, never rename/delete an existing public member:** Health.cs,
GameManager.cs, WaveManager.cs, SpellCaster.cs, EnemyMove.cs, SpellData.cs, WaveData.cs.
Reason: Unity serializes Inspector wiring by field name — a rename silently orphans every
prefab reference to that field.

**GREEN:** new scripts, new SO assets, new prefabs, new UI following the existing
BillboardUI / EnemyHealthBarUI / WaveCounterUI / CooldownIndicatorUI patterns, VFX, arena
dressing that doesn't touch the rig.

## Performance budget
Quest 3, 72fps floor. No unbounded particle counts, no new real-time shadows unless explicitly
asked for, no heavy post-process stacks. Default to the cheap option and say so in the summary
rather than silently going expensive.

## Crunch-mode rules
- Never delete-and-recreate an existing .cs file — edit in place. Delete+recreate orphans the
  GUID and silently breaks every reference to it, and it won't error until someone opens the
  prefab later.
- One task = one commit. Commit message states exactly what changed.
- If a change doesn't compile or work within ~15–20 minutes of back-and-forth, stop iterating —
  `git reset --hard` to the last commit and re-scope the task smaller. There is no time budget
  for deep debugging tonight; a smaller working thing beats a bigger broken one.
- End every task with: files touched/created, and the exact thing to manually test in Play
  Mode. No narration of the exploration process.

## Systems log
Append a short entry here as each new system lands. The next Codex session reads this before
starting, so it follows the pattern actually used instead of re-deciding it.

### Mutant bug fix
Root causes: Apply Root Motion was enabled on the NavMeshAgent-driven prefab; the prefab's
Idle/Chase ranges were inverted (25 detection / 10 lose); and Attack used one range for both
entry and exit. Fixed by disabling root motion, setting loseRange to 30, and adding a 2.5m
attack exit range. Idle ResetPath() was already correctly scoped to Idle.

### Enemy health-bar visual quality
Healthbar_BKG and Healthbar_Fill use trilinear filtering, generated mipmaps, and an Android
ASTC 4x4 / quality-100 override; BillboardUI now updates in LateUpdate for steadier head-tracked
facing.

### Fireball hit SFX
FireballProjectile plays FireballHitExplosion at the physics collision contact point with
a temporary 2D AudioSource, so the audio remains audible after the projectile is destroyed.
Its M_Fireball_02_withTrails child has its demo Rigidbody and movement script removed, allowing
the VFX Graph to inherit the projectile's transform throughout flight.

### Locomotion system
Continuous air-pull is attached only to the OVRCameraRig root. It reads IHand pinch strength
for tracked hands, Touch-controller grip when a controller is held, and averages active hand
deltas in tracking space to prevent feedback or two-hand double speed. Gesture wiring is untouched.

### Dragon enemy
_(fill in once done: grounded or flight, data pattern used for the 4 variants)_

### Ranged enemy
_(fill in once done: attack implementation, friendly-fire guard used)_
