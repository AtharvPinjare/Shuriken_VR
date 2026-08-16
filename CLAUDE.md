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
Assets\Scripts\Health.cs                                   <- THE canonical shared health/damage component.
  Now also carries Health.Faction (Player/Enemy enum + faction field, default Enemy) — generic
  friendly-fire guard used by FireballProjectile's shooterFaction check. Player's Health
  (OVRCameraRig) is explicitly set to Player in the scene; everything else defaults to Enemy.
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
  -> CONFIRMED (Stage 1, Unity MCP inspection): all 4 DragonNightmare colors share
     NightmareCTRL.controller, which has NO flight/hover clips (ground-locomotion only:
     Walk/Run/Sleep/Jump + combat/death). DragonSoulEater, DragonTerrorBringer, and
     DragonUsurper all have proper flight clips (Take Off/Fly Float/Fly Forward/Fly
     Glide/Land). Stage 1 target moved from DragonNightmare/Blue to
     **DragonSoulEater/Blue** (Pinju-confirmed) — its SouleaterCTRL controller also has a
     "Fly Fireball Shoot" clip that matches the homing-fireball attack design. If other
     dragon types are wired later, stick to SoulEater/TerrorBringer/Usurper, not Nightmare.

Assets\Vefects\Trails\VFX\Particles\VFX_Trail_{Fire,Ice,...}.prefab   <- pre-made VFX, Pinju's own task
Assets\Travis Game Assets\Hit Impact Effects\Prefabs\Hits\Hit_0{1-4}.prefab  <- pre-made VFX, Pinju's own task

Assets\Scenes\Game_Scene.unity        <- THE live gameplay scene. This is what Claude tests in.
Assets\Scenes\MainMenu.unity          <- separate, new, unrelated to core sprint items
Assets\Scenes\EnemyNavMesh\, PoseExamples_Test\, Testing\  <- stale test scenes, not ground truth, don't edit

RANGED ENEMY MODEL: not yet located in the tree — [PINJU: fill in exact path here before Item 6]
Assets\Scripts\Dragon\DragonMove.cs             <- CREATED (Stage 1+2). Idle/Chase/Attack/Dead enum-switch.
Assets\FourEvilDragonsHP\Animators\SouleaterCTRL.controller  <- Stage 1 added 4 Trigger params
  (TriggerLoiter/TriggerChase/TriggerAttack/TriggerDead) + Any State transitions to
  Fly Float/Fly Forward/Fly Float/Die respectively. Shared by all 4 SoulEater colors.
Assets\Prefab\Fireball\Fireball.prefab          <- CORRECTED PATH (moved under Fireball\ subfolder
  since this doc was first written, as part of Item 3's asset swap). Use this path, not the old
  flat Assets\Prefab\Fireball.prefab.
Assets\Prefab\Fireball\DragonFireball.prefab    <- CREATED (Stage 2). True Unity Prefab Variant of
  Fireball.prefab (PrefabUtility.SaveAsPrefabAsset onto a modified instance — NOT a disconnected
  copy, so it inherits Fireball.prefab's VFX/mesh/materials automatically if Pinju's Item 3 art
  changes later). Overrides: FireballProjectile.isHoming=true, shooterFaction=Enemy,
  turnRateDegreesPerSecond=60.
Assets\ScriptableObjects\Spells\DragonFireballData.asset  <- CREATED (Stage 2). SpellData:
  damage=20, projectileSpeed=15, ImpactPrefabVFX=Explosion (reused, no new VFX),
  projectilePrefab=DragonFireball.prefab.
Assets\[fill in after import]\...               <- [PINJU: beam VFX path, once imported, before Stage 3]
```

## Known discrepancies — confirm before Claude touches related files
- **`Assets\Scripts\ProgBasics\{BaseEntity,EnemyEntity,HealthComponent,PlayerEntity}.cs`** looks
  like an early teaching scaffold, separate from and older than the real `Health.cs` /
  `EnemyMove.cs` system this whole project is actually built on. Treat as **legacy/dead code —
  ignore, do not extend, do not confuse with `Health.cs`**, unless Pinju confirms it's still live.
- `SnakeVenom.asset` (third spell SO) — status unconfirmed. Don't wire anything to it unless
  Pinju says it's active.
- `Assets\Scripts\Praneet\` (FloatingTitle.cs, TutorialSignActivator.cs) — another contributor's
  MainMenu work, unrelated to the sprint items. Green zone but hands-off unless explicitly asked.
- **OVRCameraRig (the player) has no Collider anywhere in its hierarchy.** Every existing
  player-damage path (Mutant melee) calls Health.TakeDamage() directly, never via physics. This
  means OnCollisionEnter/OnTriggerEnter against the player structurally cannot fire — confirmed
  the hard way during Dragon Stage 2 (fireballs hit the ground until fixed with a
  proximity-based check in FixedUpdate instead of collision events). Any future
  projectile/beam/AoE that needs to hit the player must use a proximity or raycast check, not
  collision/trigger events. Do not add a Collider to the rig to "fix" this — rig hierarchy is
  red-zone.


## Locked architecture (do not deviate)
- Data = ScriptableObject (SpellData, WaveData, StatusEffect + subclasses). Behaviour =
  MonoBehaviour. New content = new SO asset / prefab variant wherever a pattern already exists,
  not new code.
- Dragon.OnDragonDefeated (once it exists — consumer TBD).
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
  ### Dragon encounter design (locked)
Redesigned from the original "wire it up like a ground enemy" approach — do not revert to that.
- Engagement is trigger-volume based, not distance/aggro based. Two separate trigger volumes
  exist per dragon instance: a loiter bounding volume (ambient flight before combat) and an
  engage trigger (player entering it starts Chase). Both are scene objects Pinju places and
  wires via serialized fields — never search for them at runtime.
- State names reuse EnemyMove.cs's Idle/Chase/Attack/Dead enum-switch pattern, but the BEHAVIOR
  is new and lives in its own file — Assets\Scripts\Dragon\DragonMove.cs (created, Stage 1). Do
  not edit EnemyMove.cs to add flight support.
- VR comfort constraints, non-negotiable: dragon stays in an altitude band above the player
  during Chase (no diving to eye-level), and every attack has an Inspector-tunable telegraph
  delay before it fires — no instant unavoidable damage.
- Attack payload: a homing variant of the existing Fireball, not a new projectile type.
  Implementation: add generic homing fields (isHoming, turnRateDegreesPerSecond, target) to
  FireballProjectile.cs (now YELLOW, see below) and create a DragonFireball.prefab variant with
  isHoming=true — do not fork a new projectile script. Reuses the Item 3 fireball VFX as-is, no
  new VFX needed for this attack.
- Beam attack (Stage 3) is a stretch goal gated behind Stage 1+2 both verified working first.
  VFX source: see Real project file paths once imported. If beam doesn't converge within a
  session's crunch-mode cutoff, ship fireball-only — pre-agreed ship-line fallback, not a
  failure.
- On Health reaching zero: play whatever death clip Unity MCP confirms exists on the Animator
  (don't assume a clip name), then invoke a public UnityEvent OnDragonDefeated. Consumer of
  that event is not yet decided — expose the hook, don't assume what it triggers.
- Build order is staged: Stage 1 (flight + trigger skeleton, no attack) -> Stage 2 (homing
  fireball + OnDragonDefeated) -> Stage 3 (beam, optional). Each stage MCP-verified working in
  Play mode before the next starts. Prompts live in CHECKLIST.md Item 5.

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
FireballProjectile.cs shooter-exclusion pattern (added in Dragon Stage 2, reuse for Item 6,
don't reinvent): Health.cs now has a Faction enum + faction field (Player/Enemy).
FireballProjectile.cs has a shooterFaction field checked in ResolveHit() before applying damage.
Item 6 must reuse these exact names, not add a second mechanism..

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
Append a short entry here as each new system lands. The next Claude session reads this before
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

### Dragon enemy — Stage 1 (flight + trigger skeleton, DragonSoulEater/Blue)
Flight, not grounded — no NavMeshAgent, moved directly via Transform in Update(). Target
swapped from DragonNightmare/Blue to **DragonSoulEater/Blue**: all 4 DragonNightmare colors
share NightmareCTRL.controller, which has zero flight clips (ground-locomotion pack only).
SoulEater/TerrorBringer/Usurper all have real flight clips; SoulEater's controller additionally
has a "Fly Fireball Shoot" clip matching the Stage 2 attack design, so that's the type to reuse
for the other 3 dragon types later too.

SouleaterCTRL.controller shipped with 16 states/clips and **zero Animator parameters** — it's a
raw demo showcase that auto-cycles through every clip via unconditioned exit-time transitions.
Added 4 Trigger params (TriggerLoiter/TriggerChase/TriggerAttack/TriggerDead), wired as Any
State transitions to Fly Float / Fly Forward / Fly Float / Die, and stripped the stale
unconditioned auto-transitions off those 3 destination states so they stop auto-advancing into
the demo chain. Default state changed to Fly Float.

Real bug hit and fixed: a Trigger parameter that fires while its own destination is already the
current state never gets consumed (Animator's canTransitionToSelf=false blocks the self-jump),
so it stays armed indefinitely and can steal priority from a later legitimate trigger — Chase's
TriggerChase was silently never taking visual effect because a stuck TriggerAttack from an
earlier frame kept winning. Fixed with the standard pattern: reset all 4 triggers before arming
the one for the state actually being entered (see DragonMove.EnterStateIfChanged /
ResetAllAnimatorTriggers). Look out for this same class of bug in Stage 2/3 and in Item 6's
ranged enemy if it also drives multiple Animator states via triggers.

Idle = random point inside a serialized `loiterVolume` Collider (bounds-based, not physics
triggers). Chase = `engageTrigger` Collider checked via `.bounds.Contains(player.position)` each
frame (deliberately not a real OnTriggerEnter — avoids depending on the OVRCameraRig having a
correctly configured Collider/Rigidbody, which is uncertain/red-zone-adjacent). Chase never
reverts to Idle on its own (trigger-volume engagement is sticky by design, per the locked
design doc) — only Attack has a distance-based exit range back to Chase. Player refs come in via
`InjectPlayerReferences(Transform, Health)`, called once from GameManager.Start() over a new
additive `dragons[]`/`playerTransform` serialized field pair (mirrors WaveSpawner's existing
explicit-wiring pattern). Added a `GameManager.CurrentState != Playing` freeze guard to
DragonMove.Update(), matching EnemyMove — without it the dragon kept flying/attacking after a
DEFEAT/VICTORY state change.

Also found: this Editor's Console window had both Log and Warning severity filters toggled off
(pre-existing, unrelated to any of tonight's changes) — every Debug.Log/LogWarning from any
system, not just Dragon, was invisible to console queries until re-enabled via
LogEntries.consoleFlags. If a future session sees suspiciously empty console output during
MCP-verify, check this first before assuming the code isn't logging.

MCP-verified in Play mode: Idle (loiter, Fly Float) -> Chase (Fly Forward, altitude-band
tracking) -> Attack (Fly Float, placeholder log firing on a 2s interval, confirmed in console)
-> Dead (Health.TakeDamage -> Die clip, movement frozen). Scene test rig: Dragon_SoulEater_Blue
+ Dragon_LoiterVolume + Dragon_EngageTrigger, all in Game_Scene, wired to GameManager.

Post-Stage-1 fix (found via manual playtest, not the earlier trigger bug): Chase->Attack used
raw 3D distance against attackRange, which includes the altitude-band vertical offset — a player
walking near/under the dragon's spawn column could push 3D distance under attackRange while the
dragon was still high overhead mid-descent, freezing it into the (correct, by-design) zero-
movement Attack/Fly-Float hover well before it visually looked "arrived." Read as "stuck /
flapping in place, not closing distance" in manual testing. Horizontal-only distance was
considered and rejected — verified against a captured trace that it would trigger even sooner
(removes the vertical gap from the check entirely). Fixed with two conditions together:
HorizontalDistanceToPlayer() < attackRange AND HasReachedAltitudeBand() (current height above
player within [min,max] altitude band +/- altitudeArrivalTolerance). Re-verified via the same
continuous-Update-driven movement trace (teleport-based tests don't reproduce this class of
issue reliably — prefer continuous movement for future Dragon/enemy verification).

### Dragon enemy — Stage 2 (homing fireball + OnDragonDefeated, DragonSoulEater/Blue)
Attack state is now telegraph -> fire -> cooldown -> telegraph, repeating for as long as the
player stays in range (every shot gets its own telegraph delay, not just the first — the VR
fairness requirement is non-negotiable per-shot, not one-time). DragonFireball.prefab is a true
Unity Prefab Variant of Fireball.prefab (via PrefabUtility.SaveAsPrefabAsset on a modified
instance, confirmed via PrefabUtility.GetPrefabAssetType==Variant — NOT a disconnected copy) with
FireballProjectile.isHoming=true, shooterFaction=Enemy, turnRateDegreesPerSecond=60 as overrides.
DragonMove fires it the same way SpellCaster casts the player's fireball (Initialize(data) +
rb.AddForce(direction * data.projectileSpeed, VelocityChange)), reading everything from a new
DragonFireballData SpellData asset rather than a parallel prefab-reference field on DragonMove.

Real bug found and fixed during MCP-verify: every homing fireball hit the ground (Plane) instead
of the player — player HP never moved. Root cause: OVRCameraRig (the player rig) has no Collider
at all; every existing damage source (Mutant melee) hits the player via a direct
Health.TakeDamage() call, never physics, so this was never noticed before. FireballProjectile's
OnCollisionEnter can structurally never fire against a target with no Collider. Fixed without
touching the player rig (red-zone for this task) — added a proximity-based hit resolution path
in FixedUpdate for the isHoming case (hits once within homingHitRadius of target), refactored the
shared hit logic (damage, faction guard, VFX, audio, destroy) out of OnCollisionEnter into a
ResolveHit() both paths call, guarded by a _hasResolved flag so a coincidental physics collision
and the proximity check can't double-resolve the same projectile.

Friendly-fire guard verified two ways: (1) a fireball spawned dead-center inside a live Mutant's
Collider bounds (guaranteed overlap) did zero damage and was not destroyed — passed through, as
designed; (2) real gameplay across 3 live Mutants + the dragon itself, zero unintended damage.
Note: the guard only skips damage/destroy/VFX — it does not prevent the physical bounce/deflect
PhysX still applies on contact before OnCollisionEnter fires (would need a Project Settings
physics layer-collision-matrix change, which is red-zone; not attempted).

MCP-verified in Play mode (WaveSpawner temporarily disabled during the test only, to stop
background Mutant combat from killing the player mid-verification — re-enabled and scene
re-saved after): 5 fireballs fired on the telegraph/cooldown cadence, each logged
"[Fireball] Hit: OVRCameraRig | Damage: 20", player HP ticked 100->80->60->40->20->0 exactly,
GameManager's existing DEFEAT path fired correctly. OnDragonDefeated fires exactly once when the
dragon's own Health is driven to zero (confirmed via a temporary listener), and a second
TakeDamage call afterward does not re-fire it (Health.IsDead guard).

### Ranged enemy
_(fill in once done: attack implementation, friendly-fire guard used)_

### Ranged enemy
_(fill in once done: attack implementation, friendly-fire guard used)_
