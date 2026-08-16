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

Assets\Praneet_assets\FlyingEnemy\Vampire A Lusth.prefab   <- RANGED ENEMY (Item 6). Flying vampire
  mage, own scripts FlyingMageEnemy.cs (movement/attack) + EnemySpellProjectile.cs (on
  EnemySpell.prefab). Already shipped with Health.cs + EnemyHealthBarUI/BillboardUI attached
  before Item 6 integration started — see Systems log "Ranged enemy" for what was actually
  broken (material refs, no Animator controller/clips, Apply Root Motion, tag-based hit
  detection) vs already correct (Health wiring, health bar).
Assets\Praneet_assets\FlyingEnemy\EnemySpell.prefab         <- ranged enemy's projectile prefab.
Assets\Praneet_assets\FlyingEnemy\Animations\VampireController.controller  <- Cast/Die triggers,
  already had Any State transitions wired; Item 6 added the missing AnimationClips.
Assets\ScriptableObjects\Waves\Wave_3.asset                 <- CREATED (Item 6). enemyPrefab =
  Vampire A Lusth, enemyCount=3, spawnInterval=10. Appended to WaveManager's _waves list —
  ranged enemies are wave 3, after the two Mutant waves.
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
Assets\Flashy Feather Assets\Lasers - Sample\Prefabs\VFX Laser Fire.prefab  <- USED (Stage 3
  beam VFX). Pack also has "VFX Laser Water"/"Hit Laser Fire"/"Hit Laser Water" variants, unused.
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
  DONE — see Systems log "Dragon enemy — Stage 3." Beam replaces fireball as Attack's default
  (`useBeamAttack` bool on DragonMove, flip to false for an instant fireball-only fallback, no
  code changes needed).
- On Health reaching zero: play whatever death clip Unity MCP confirms exists on the Animator
  (don't assume a clip name), then invoke a public UnityEvent OnDragonDefeated. Consumer of
  that event is not yet decided — expose the hook, don't assume what it triggers.
- Build order is staged: Stage 1 (flight + trigger skeleton, no attack) -> Stage 2 (homing
  fireball + OnDragonDefeated) -> Stage 3 (beam, optional). Each stage MCP-verified working in
  Play mode before the next starts. Prompts live in CHECKLIST.md Item 5. All three stages done.

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
GameManager.cs, WaveManager.cs, SpellCaster.cs, EnemyMove.cs, SpellData.cs, WaveData.cs,
WaveSpawner.cs (added Item 6 — its spawn loop now also checks for FlyingMageEnemy alongside
EnemyMove; treat it with the same additive-only care even though it wasn't in the original list).
Reason: Unity serializes Inspector wiring by field name — a rename silently orphans every
prefab reference to that field.
Health.Faction shooter-exclusion pattern (added Dragon Stage 2, reused Item 6): FireballProjectile.cs
has a shooterFaction field checked before applying damage. Item 6's ranged enemy shipped with its
own EnemySpellProjectile.cs (not FireballProjectile.cs) — per the "own script still checks
Health.Faction" rule, it independently implements the identical shooterFaction-vs-Health.faction
guard rather than inventing a second mechanism. Any future projectile script should do the same.

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

### Dragon enemy — Stage 3 (beam attack, stretch goal, DragonSoulEater/Blue)
VFX: `Assets\Flashy Feather Assets\Lasers - Sample\Prefabs\VFX Laser Fire.prefab`. The pack's
only script, `FF_Laser01_Settings`, is a fire-and-forget config holder (public fields only, no
methods) — it scales `t_main_laser.localScale.x` by its `length_multiplier` field and configures
particle burst timings once in Awake(), then does nothing further (no tracking, no hit-detection,
no auto-destroy since `DESTROY_ON_END` defaults false). Not a real "beam-control script" in the
sense of an API to call — aim/length are one-shot at instantiation, so DragonMove sets both
itself: instantiate at the spawn point with `Quaternion.LookRotation(direction)`, then
immediately overwrite `t_main_laser.localScale.x` (Awake's own scaling already ran by the time
Instantiate() returns, so this is a full override, not an additional multiply). World length
per 1.0 of `length_multiplier` was empirically measured at ~22.336 units (mesh X-extent 6.98*2 *
startSize.x 8 * the "Scale" parent node's own 0.2 localScale) — re-measure if the prefab changes.
`DragonMove` calls `Destroy(beam, beamDuration + 0.2f)` itself since `DESTROY_ON_END` is off.

Damage: per Pinju's note (added to this doc after Stage 2), OVRCameraRig has no Collider
anywhere, so beam damage — like the Stage 2 fireball fix — cannot use physics at all. Implemented
as a scripted line-check: capture beam origin/direction/length once at cast time, then each tick
(`beamTickInterval`, default 0.3s) compute the player's perpendicular distance to that fixed line
segment and call `_playerHealth.TakeDamage()` directly if within `beamHitRadius`. This ticks for
`beamDuration` (1.8s, matching the VFX default) independently of the Attack state's own
telegraph/cooldown timer — `UpdateBeamDamage()` runs unconditionally every Update() frame so a
beam's damage window isn't cut short by a state change mid-tick.

Beam REPLACES the fireball as Attack's default (`useBeamAttack = true` on the live scene
instance) — read well in Play mode testing. Fireball path is untouched and fully intact:
`UpdateAttack()` just branches on `useBeamAttack`, so flipping that one bool back to false in the
Inspector reverts to Stage 2 behavior instantly, no code changes needed, per the "don't delete
the fireball path" instruction. Verified both ways: beam-on cycle dealt 3x(6 ticks x 8dmg)=144
total over ~7.5s before player death; beam-off cycle produced the identical Stage 2 fireball
console output as before, confirming zero regression from adding the branch.

Investigated-then-ruled-out false alarm during MCP-verify: querying a live beam's transform via
Unity MCP several tool-calls after it fired showed position/scale reset to defaults — looked like
a real bug (beam rendering at world origin). Added temporary inline Debug.Log calls immediately
after Instantiate/override and confirmed position+scale were correct at spawn time
(`(19.32, 6.97, 4.73)`, scale `0.31`, exactly as computed). The "reset" values were from querying
an object AFTER its own `Destroy(beam, 2.0f)` had already fired — MCP tool round-trip latency
exceeded the beam's own lifetime, so later queries were reading a torn-down object's stale
field values, not the live object's actual state. Not a bug; a lesson for future verification —
when polling something with a short lifetime, check immediately or don't trust a "wrong" reading
without first confirming you're not looking at a destroyed object.

MCP-verified in Play mode (WaveSpawner temporarily disabled for isolated testing only, restored
after): beam fired on the telegraph/cooldown cadence, 6 damage ticks per beam at 8dmg each,
dragon's own Health unaffected across the whole test (self-damage is structurally impossible —
damage is a direct `_playerHealth.TakeDamage()` call, never a generic Health lookup), full
console output captured and reported in chat.

### Dragon enemy — disabled in Game_Scene for the submitted build
All three stages (flight/trigger skeleton, homing fireball + OnDragonDefeated, beam attack) are
implemented and MCP-verified working, per the Stage 1/2/3 entries above — the system itself is
NOT abandoned or incomplete. Time ran out to also integrate/polish the other 3 dragon types and
give the encounter a full multi-system playtest pass, so `Dragon_SoulEater_Blue`,
`Dragon_LoiterVolume`, and `Dragon_EngageTrigger` were disabled (SetActive false, not deleted)
in `Game_Scene` for this submission — a scene-presence change only. `DragonMove.cs`,
`FireballProjectile.cs`'s homing/faction additions, `Health.cs`'s Faction field,
`DragonFireball.prefab`, `DragonFireballData.asset`, and `SouleaterCTRL.controller`'s triggers
are all untouched and fully intact. Re-enabling is a one-step reversal (SetActive true on those
3 objects) — no code or asset changes needed.

Checked GameManager.cs's `dragons[]` handling before disabling (per the "don't just clear the
reference and assume" instruction): `Start()` already guards with `if (dragon != null)`, so a
null array entry is safely skipped. A reference to a *disabled* (non-null) object still calls
`dragon.InjectPlayerReferences(...)`, but that's a plain C# method call setting two private
fields — it doesn't require the GameObject to be active, throws nothing, and since the disabled
DragonMove's Update() never runs, those fields are simply never read. Verified via a full Play
mode playthrough: zero new console errors, WaveManager/WaveSpawner/Mutant combat all behave
exactly as before dragon work started.

### Ranged enemy (Vampire A Lusth flying mage, Item 6)
Asset audited before touching anything (Phase 1). It arrived much further integrated than a raw
import: `Vampire A Lusth.prefab` already carried `Health.cs` (faction=Enemy, correct default)
with `OnDeath` already wired to `FlyingMageEnemy.TakeDamage()` (misleadingly named — takes no
damage amount, it's a death handler: VFX, disables collider, plays Die, self-destructs), and its
`HealthBar` child already had `EnemyHealthBarUI`/`BillboardUI` attached and self-wiring
correctly. No parallel health system, nothing to remove there.

Four real bugs found and fixed:
1. **Purple/white rendering.** The `Vampire` child's SkinnedMeshRenderer had broken mesh+material
   GUID references (traced in the prefab's raw YAML — pointed at GUIDs matching no current
   asset, left over from before the FBX was reimported). Reassigned to the FBX's current mesh
   sub-asset and `Vampire_MAT1.mat` (Pinju's choice) for both material slots. `Vampire_MAT1.mat`
   itself had no Base Map/Emission Map assigned (`_BaseColor` was plain white) — assigned
   `Vampire_diffuse.png`/`Vampire_emission.png` (the project already had duplicate `" 1"` copies
   of these from an earlier fix attempt; picked the originals since they're identical in
   size/format/sRGB, no reason to prefer the duplicates) and set `_EmissionColor` to white so the
   emission map isn't black-multiplied to invisible.
2. **No Animator wired at all.** Prefab's Animator component had `runtimeAnimatorController=null`.
   Assigned `VampireController.controller`. That controller (unlike DragonNightmare's original
   empty one, closer to SouleaterCTRL's shape) already had `Cast`/`Die` Trigger parameters with
   Any State transitions correctly wired to the matching states — but every state had `motion=
   null`, no AnimationClip assigned despite matching FBX files sitting right there. Wired
   `mixamo_com` <- "Hanging Idle.fbx" clip (picked over the flatter "Idle.fbx" — a hover/float
   pose reads better for a non-grounded flying enemy), `Standing 1H Magic Attack 01` <- its
   matching FBX clip, `VampireBackwardDeath` <- its matching FBX clip, on both the Base and
   UpperBody layers where each state exists.
3. **Apply Root Motion was enabled** — same bug class as the original Mutant fix. Caused the
   enemy to keep drifting via animation-baked root motion even after the
   `GameManager.CurrentState != Playing` freeze guard (added during this integration, same
   pattern as EnemyMove/DragonMove) correctly stopped the script's own movement logic. Looked
   like the freeze guard wasn't working; it was working, root motion was fighting it. Fixed by
   disabling Apply Root Motion on the prefab's Animator, verified by re-checking position was
   bit-identical across repeated polls after Defeat (previously it visibly crept a few
   centimeters between polls).
4. **Broken/crashing hit detection.** `EnemySpellProjectile.cs` used `OnCollisionEnter`/
   `OnTriggerEnter` + `CompareTag(targetTag)`, and `targetTag` was set to `"Collide"` on the
   prefab — not a registered project tag (`Untagged/Respawn/Finish/EditorOnly/MainCamera/Player/
   GameController`). `CompareTag` against an undefined tag throws `UnityException`, so this would
   have crashed on its first collision with anything. Independent of that, OVRCameraRig has no
   Collider (per the Known Discrepancies note added during Dragon Stage 2), so collision-based
   detection could never fire against the player regardless. Rewrote around a shared
   `ResolveHit()` (mirrors `FireballProjectile.cs`'s pattern): a `Health.Faction shooterFaction`
   field (default Enemy) guards against damaging same-faction targets on physical collision, plus
   a `FixedUpdate()` proximity check against a `target` Transform (set by `FlyingMageEnemy` to the
   injected player Transform at spawn) for the player-hit case specifically, since it has no
   Collider to collide with at all.

FSM decision: kept `FlyingMageEnemy.cs`'s existing continuous-loop behavior (maintain
`preferredRange` from the player, fire on a flat cooldown) rather than refactoring into the
Idle/Chase/Attack/Dead enum-switch convention `EnemyMove`/`DragonMove` use — Pinju's call, fastest
path that certainly works; the loop already functionally covers chase+attack, and Dead is already
handled via the existing `isDead` flag. No telegraph delay added either (Pinju: "leave it") —
Item 6 doesn't carry Dragon's VR-comfort mandate.

Player references: replaced the asset's raw public `player` field with private
`_playerTransform`/`_playerHealth` + `InjectPlayerReferences()`, matching EnemyMove/DragonMove
exactly. Also added `initialHoverHeight` (spawns elevated above its spawn point instead of
hovering at ground level, since Mutant spawn points are ground-level and this is a flier).

Spawn method (Pinju's call): wave-spawned only, not manually placed. `WaveSpawner.cs`'s spawn
loop was hard-typed to `GetComponent<EnemyMove>()` — added an `else if FlyingMageEnemy` branch
(additive) so ranged enemies also get `InjectPlayerReferences()` at spawn. Created
`Wave_3.asset` (enemyPrefab=Vampire A Lusth, enemyCount=3, spawnInterval=10) and appended to
WaveManager's `_waves` list — ranged enemies are their own wave, after the two Mutant waves, not
mixed into Wave_1/Wave_2.

MCP-verified in Play mode (WaveSpawner temporarily disabled/re-enabled around manual test spawns
only, to avoid background Mutant-wave interference — restored and scene re-saved after): spawned
via a direct call to the real `WaveSpawner.SpawnWave(Wave_3, ...)` code path (not bypassed),
placed a standalone Mutant nearby — zero friendly-fire damage to it or to the vampire itself
across the whole test. Player HP ticked down in exact 15-damage steps from repeated hits,
existing DEFEAT path fired correctly, vampire froze in place after Defeat (confirming the root
motion fix). Health bar verified separately: direct 40-damage test dropped its Slider from 1.0
to 0.6, matching 60/100 HP exactly. Death sequence verified: `Health.TakeDamage()` to 0 disabled
its Collider and set `IsDead`, object self-destructed after the expected delay. Zero new console
errors throughout (only pre-existing, unrelated XR/OVR sample-script noise).
