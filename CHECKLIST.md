# Shuriken VR — Sprint Checklist (12h)

Read CLAUDE.md first if you haven't this session. This file is the live state — update the
block below every time you switch tasks or open a fresh Claude session.

## Right now
- Last completed: Item 6 (Ranged enemy — Vampire A Lusth flying mage). Wave-spawned only
  (Wave_3), MCP-verified: no friendly fire, health bar reflects damage, DEFEAT/death sequences
  correct, zero new console errors. See CLAUDE.md Systems log "Ranged enemy" for the 4 real bugs
  found and fixed along the way.
- In progress: —
- Next: all Track B items (1, 4, 5, 6) done. Remaining: Item 7 (environment/lighting, Pinju
  primary) and Item 8 (hand tracking optimization/profiling/build settings, Pinju).

## Run two tracks in parallel, not serial
**Track A (you, Unity Editor/art)** — items 2, 3, 7 visuals: health bar asset swap, fireball/ice
FBX + hit VFX, environment art.
**Track B (Claude, C#/systems)** — items 1, 4, 5, 6: bug fix, locomotion, dragon, ranged enemy.

These touch almost entirely different files. Start Claude on Item 1 now, and while it runs, go
do Item 2 yourself — don't sit and watch it work. Sync points are called out below where a
Track B item needs a Track A asset first.

## Ship-line — decide this now, not at hour 10
1. Dragon: fewer fully-working types beats four half-wired ones.
2. Locomotion: pre-committed fallback to thumbstick continuous locomotion if air-pull isn't
   converging by your own cutoff (pick one, e.g. 90 minutes — write it here: ______).
3. Ranged enemy VFX: functional wiring with placeholder VFX is a complete pass; cosmetic polish
   is stretch.
4. Environment polish is the most cuttable item overall. A plain-but-clean arena ships; a
   broken dragon does not.

---

## 1. [x] [Claude] Mutant enemy — rotation / stuck / abrupt animation switching

Check in this order before broad exploration — all three are grounded in bugs this exact
codebase has already hit once:

- **Apply Root Motion.** Mutant is a Mixamo import driven by NavMeshAgent. If Apply Root Motion
  is on, animation-driven transform changes fight the agent's own control of position/rotation
  — this alone produces both symptoms.
- **Missing hysteresis at the Attack boundary.** Idle↔Chase already uses separate
  detectionRange/loseRange to avoid oscillation. Check whether Chase↔Attack uses a single
  attackRange for both entering and exiting — if so, standing near that boundary flickers the
  FSM every frame, which reads as "won't approach" + "abrupt animation switching."
- **Idle's per-frame ResetPath().** Confirm it's scoped only to the Idle case and isn't also
  firing during Chase (would wipe the agent's destination every frame).

Prompt:
```
Diagnose and fix EnemyMove.cs (Mutant enemy): rotation-to-player looks wrong, sometimes doesn't
reach the player despite being in detection range, animation states switch abruptly.
Check in this order: (1) Animator "Apply Root Motion" fighting NavMeshAgent's transform
control, (2) whether Chase<->Attack uses a single shared range for both entering and exiting
the state (needs the same enter/exit hysteresis already used for Idle<->Chase via
detectionRange/loseRange), (3) whether the Idle-state per-frame agent.ResetPath() call is
correctly scoped and not firing during Chase.
Fix only what's actually wrong — don't refactor the FSM structure. Report which hypothesis was
the actual cause in the changelog.
```
Test: approach from multiple angles, including standing exactly at the attack-range boundary.
Confirm smooth rotation, consistent chase, no animation flicker.

---

## 2. [x] [PINJU] Enemy health bar asset swap
Slots into the existing EnemyHealthBarUI.cs / BillboardUI.cs Canvas pattern — no code changes
needed if the new asset is still a Slider. If its structure is different, flag before Claude
touches EnemyHealthBarUI.cs.

---

## 3. [x] [PINJU] Fireball/Ice Shard FBX + hit VFX
Asset-only: swap prefab references in the existing SpellData assets / projectile prefabs. No
script changes expected if collider/rigidbody setup carries over onto the new model.

---

## 4. [x] [Claude] Locomotion — continuous air-pull (Drakheir-style)
Not anchor-based climbing — nothing needs to exist to grab. While a hand is gripping, track its
world-position delta frame to frame; move the rig by the inverse of that delta. Pull your hand
toward you, you move forward; push it away, you move back. Both hands work simultaneously.
`PhysicsGrabbable.cs`/`RigidbodyKinematicLocker.cs` are NOT the base for this — those are for
grabbing actual rigidbody objects, not relevant here.
This is a movement input, not a spell-cast gesture — it must NOT route through IGestureProvider
or GestureManager (locked, casting-only). Grip detection should be its own thing: check first
whether IHand/HandRef already exposes a pinch/grab-strength value to use directly, rather than
building new pose-recognition wiring for it.
Rig exception applies (see CLAUDE.md). Set your own time cutoff before starting — write it in
the ship-line section above. Note: pure air-pull locomotion (no physical anchor) is more prone
to motion sickness for some players than anchor-based climbing — not worth solving tonight,
just don't be surprised if it comes up in playtesting.

Prompt:
```
Implement continuous air-pull locomotion (Drakheir-style): while a hand is gripping, track its
world-position delta frame-to-frame and move the OVRCameraRig by the inverse of that delta
(times a tunable multiplier) — pulling a hand toward yourself moves you forward in that
direction. Support both hands simultaneously (sum deltas if both gripped).
Grip detection: check first whether IHand/HandRef already exposes a pinch/grab-strength value
to use as the signal, rather than building new pose detection. For controller fallback, use the
physical grip button.
This is a NEW input pathway for movement — do not route it through IGestureProvider or
GestureManager, those stay locked to spell-casting gestures only.
This is the one task allowed to add a component to the OVRCameraRig root that moves it. Do not
touch anything under the rig related to hand-tracking data sources, the gesture provider
components, or camera/head anchor setup — casting must keep working exactly as before.
Deliverable: gripping and moving a hand translates the player through the arena; releasing
stops movement (momentum/drag is a stretch, not required for v1).
Test: cast a spell immediately after moving via the new locomotion, confirm gesture detection
is unaffected. Grip with both hands at once and confirm it doesn't double-speed or fight itself.
```
Fallback prompt (true zero-build fallback — flip to this if the above stalls past your cutoff):
```
Enable Meta Interaction SDK / OVR's standard thumbstick continuous locomotion on the
OVRCameraRig, if the SDK ships one out of the box (check the imported package's samples/
first-party locomotion component before writing anything custom). Same rig-exception
constraint — hand-tracking/gesture wiring untouched.
```

---

## 5. [x] [Claude] Dragon enemy — staged (DragonSoulEater/Blue, other 3 types after)
**All 3 stages implemented and MCP-verified. Disabled (not deleted) in Game_Scene for this
submission — see CLAUDE.md Systems log "Dragon enemy — disabled in Game_Scene for the submitted
build" for the full reasoning and the one-step re-enable.**
Design is locked in CLAUDE.md under "Dragon encounter design" — read that before running any
stage below. Run sequentially, verify each in Play mode before starting the next. Update this
block's status and CLAUDE.md's Systems log after each stage.

**Target swapped mid-Stage-1: DragonNightmare/Blue -> DragonSoulEater/Blue.** All 4
DragonNightmare colors share an Animator Controller with zero flight/hover clips (ground-
locomotion pack only) — confirmed via Unity MCP before writing any state logic, per the prompt
below. DragonSoulEater/TerrorBringer/Usurper all have real flight clips; SoulEater was picked
because its controller also has a "Fly Fireball Shoot" clip matching the Stage 2 attack. Use
SoulEater/TerrorBringer/Usurper for the other 3 types later, not Nightmare.

Prompt (Stage 1 — flight + trigger skeleton, no attack):
```
Build Stage 1 of the dragon encounter per CLAUDE.md's "Dragon encounter design" section, for
Assets\FourEvilDragonsHP\Prefab\DragonNightmare\Blue.prefab only.

Before writing state logic: inspect the prefab's Animator via Unity MCP, list available clips,
tell me what's there before deciding how to drive them.

Idle = ambient loiter inside a bounding volume I'll place (serialized field, don't hardcode).
Chase = triggered by a separate engage-trigger volume I'll place (serialized field), flies
toward the player within an altitude band, never diving to eye-level.
Attack = placeholder, log only, no movement.
Dead = Health.cs (Assets\Scripts\Health.cs, not the ProgBasics legacy one), play whatever death
clip you found, disable movement.

Player references via the existing InjectPlayerReferences() pattern only — no
GetComponentInChildren/GetComponentsInChildren.

MCP-verify: compile clean, Play mode, report actual Console output through Idle->Chase->
Attack(log). List exactly which fields need Inspector assignment.
```

**[x] Stage 1 DONE — DragonSoulEater/Blue.** Files: created
`Assets\Scripts\Dragon\DragonMove.cs`; additive fields on `Assets\Scripts\GameManager.cs`
(`playerTransform`, `dragons[]`, wired in `Start()`); added 4 Trigger params + Any State
transitions to `Assets\FourEvilDragonsHP\Animators\SouleaterCTRL.controller` (shared by all 4
SoulEater colors — stripped its stale auto-cycling demo transitions on the 3 states we drive).
Scene: `Dragon_SoulEater_Blue` + `Dragon_LoiterVolume` + `Dragon_EngageTrigger` added to
Game_Scene, wired to GameManager. Real bug found+fixed: a stuck Animator Trigger (fired while
already on its own target state, blocked by canTransitionToSelf=false) silently ate a later
legitimate trigger — fixed by resetting all triggers before arming the new one; watch for the
same class of bug in Stage 2/3 and Item 6 if they also drive multiple Animator states via
triggers. MCP-verified full Idle->Chase->Attack(log)->Dead in Play mode; console output and
inspector-field list reported in chat. To manually test: enter Play mode, walk/teleport the
player into `Dragon_EngageTrigger`'s bounds, watch the dragon fly toward you and level off in
the altitude band, then approach within 8m to see the Attack placeholder log fire every 2s.

Prompt (Stage 2 — homing fireball + defeat hook, only after Stage 1 verified):
```
Build Stage 2 per CLAUDE.md — dragon's Attack state fires a homing projectile instead of
logging, and death exposes a continue hook.

Add generic homing fields to FireballProjectile.cs (isHoming, turnRateDegreesPerSecond, target)
— additive only, per its YELLOW-zone rule, since Item 6 will also need to extend this file.
Create DragonFireball.prefab as a variant with isHoming=true, reusing the existing Item 3
fireball VFX — no new VFX. Steering re-targets the player's CURRENT position every FixedUpdate,
capped turn rate. Add a shooter-exclusion guard on the projectile (owner/faction field) so it
can't damage the dragon or other enemies — make this generic, Item 6 will reuse it, don't force
me to build a second one.

Telegraph delay (Inspector-tunable) before firing, non-negotiable VR fairness requirement.

On Health reaching zero: invoke public UnityEvent OnDragonDefeated, consumer TBD, just expose it.

MCP-verify: compile clean, Play mode, trigger Attack, confirm homing works, only player Health
takes damage, OnDragonDefeated fires exactly once at zero Health. Report actual Console output.
```

**[x] Stage 2 DONE — DragonSoulEater/Blue.** Files: `DragonMove.cs` Attack state rewritten to
telegraph->fire->cooldown loop; `Assets\Scripts\FireballProjectile.cs` gained isHoming/
turnRateDegreesPerSecond/target, homingHitRadius, shooterFaction (Health.Faction-based friendly-
fire guard), and a shared ResolveHit() used by both physics collision and homing proximity;
`Assets\Scripts\Health.cs` gained a Faction enum + field (generic, reused by Item 6); created
`Assets\Prefab\Fireball\DragonFireball.prefab` (true Prefab Variant of Fireball.prefab, not a
disconnected copy) and `Assets\ScriptableObjects\Spells\DragonFireballData.asset`.
Real bug found+fixed during MCP-verify: every fireball hit the ground instead of the player —
OVRCameraRig has no Collider (every existing damage source hits the player via direct
Health.TakeDamage(), never physics), so OnCollisionEnter could never fire against it. Fixed with
a proximity-based hit resolution in FixedUpdate for the homing case, entirely in
FireballProjectile.cs — did not touch the player rig (red-zone for this task). Item 6 will hit
this same wall if its ranged-enemy projectile ever targets the player by homing/proximity;
straight-line physics-only projectiles aimed at the player have the same problem if the player
truly has no Collider anywhere — confirm before assuming OnCollisionEnter will fire.
Friendly-fire guard verified twice: a fireball spawned with a guaranteed Collider overlap on a
live Mutant did zero damage and wasn't destroyed (passed through); real gameplay across 3 live
Mutants + the dragon itself, zero unintended damage. MCP-verified in Play mode: 5 fireballs
fired on cadence, player HP ticked 100->80->60->40->20->0 exactly (20 dmg/hit), existing DEFEAT
path fired correctly, OnDragonDefeated fired exactly once on the dragon's own death and did not
re-fire on a second TakeDamage call. Full console output reported in chat. To manually test:
enter Play mode, get within ~8m of the dragon horizontally while it's near its altitude band —
watch it telegraph, fire a curving fireball that tracks you, repeat every ~2.5s.

Prompt (Stage 3 — beam, stretch goal, only after Stage 1+2 verified):
```
Only start after Stage 1 and Stage 2 are both confirmed working. Set your own cutoff first
(CLAUDE.md crunch-mode rule) — if this doesn't converge in that window, git reset --hard to the
last commit and confirm the Stage 2 fireball-only version still works. That's a complete pass.

VFX: [path filled in after import — see CLAUDE.md]. Use the pack's own bundled beam-control
script if it has one, don't write LineRenderer/damage-tick logic from scratch.

Beam supplements or replaces the fireball in Attack state — your call based on how it reads in
Play mode, but do not delete the fireball path, it's the fallback.
Same telegraph-delay and no-instant-damage VR constraints as Stage 2.

MCP-verify: compile clean, Play mode, report actual Console output.
```

**[x] Stage 3 DONE — DragonSoulEater/Blue.** VFX:
`Assets\Flashy Feather Assets\Lasers - Sample\Prefabs\VFX Laser Fire.prefab`. The pack's one
script (`FF_Laser01_Settings`) has no public API to call — it's a fire-and-forget config holder
that scales/configures itself once in `Awake()`. No hit-detection in the pack at all, so wrote a
scripted line-proximity check (per Pinju's note: OVRCameraRig has no Collider, physics-based
detection cannot work — same lesson as the Stage 2 fireball fix) that ticks damage against the
player's Transform directly, independent of the Attack state's telegraph/cooldown timer so a
beam's damage window survives a mid-tick state change. `useBeamAttack` bool on `DragonMove`
(currently `true`) switches Attack between beam and fireball — flip to `false` for an instant,
code-free fallback to Stage 2 behavior; verified both paths work with zero regression.
MCP-verified in Play mode (WaveSpawner temporarily disabled for isolated testing, restored
after): beam fires on cadence, 6 damage ticks/beam at 8dmg each, dragon's own Health never
affected (damage is a direct call to the player's injected Health reference, not a generic
lookup — self-damage is structurally impossible). One false alarm investigated and ruled out:
a beam's transform read back as reset-to-origin several tool-calls after firing — turned out to
be querying an already-`Destroy()`'d object (2s lifetime, tool round-trip exceeded it), not a
real positioning bug; confirmed via inline logging that position/scale are correct at spawn
time. Full console output reported in chat. To manually test: get within ~8m of the dragon
horizontally near its altitude band, watch it telegraph then fire a beam that visibly reaches
you, taking tick damage while you stay in its path.

Deliverable for Item 5 overall: at least one dragon type with Stage 1+2 confirmed via MCP-verified
Play mode test (Stage 3 optional). Other 3 types wired as prefab/data variants once this one is
solid — not before. **All three stages done for DragonSoulEater/Blue — disabled (SetActive
false, not deleted) in Game_Scene for this submission due to time constraints on further
multi-type integration/polish. Not abandoned: mechanics are intact and verified for the report,
and re-enabling for a future session is a one-step reversal (see CLAUDE.md Systems log).**
Do not touch Assets\FourEvilDragonsHP\Scene\.

---

## 6. [x] [Claude] Ranged enemy — spells, VFX, health bar
**DONE — Vampire A Lusth flying mage (`Assets\Praneet_assets\FlyingEnemy\`), wave-spawned as
Wave_3 (after the two Mutant waves). MCP-verified: no friendly fire, health bar reflects damage,
DEFEAT/death sequences correct, zero new console errors. Full writeup in CLAUDE.md Systems log
"Ranged enemy" — the asset arrived already carrying Health.cs + EnemyHealthBarUI/BillboardUI
(no parallel health system to remove), but had 4 real bugs: broken mesh/material references
(purple/white render), no Animator controller or clips wired at all, Apply Root Motion fighting
the script's own movement (same bug class as the original Mutant fix), and a tag-based hit-check
that would have thrown on first collision (tag didn't exist) and couldn't have hit the player
anyway (no Collider). All fixed; see the log for specifics.**

Health bar: reuse the existing EnemyHealthBarUI/BillboardUI Canvas prefab directly — drop-in,
no new code expected.
**Trap:** don't let Claude reuse FireballProjectile.cs untouched for enemy-fired projectiles. It
currently damages any Health it hits with no shooter-exclusion or faction check — reused
naively, an enemy's shot can hit other enemies or itself on spawn.

Prompt:
```
Integrate the ranged enemy (model + animations provided): attack, health bar, wiring.

Health bar: use the existing EnemyHealthBarUI.cs / BillboardUI.cs Canvas prefab as-is — wire it
by dragging the prefab reference explicitly, do not search for it via GetComponentInChildren.

Attack: on Attack-state entry, spawn a projectile toward the player. If reusing
FireballProjectile.cs, add a guard so the projectile does not damage the enemy that fired it or
other enemies — only the player's Health should take damage from an enemy-fired projectile.
State explicitly how this is guarded (tag/layer check or spawn-frame self-ignore) in the
changelog.

Any reference to Health, the player Transform, or the health-bar prefab must be wired explicitly
via serialized fields or the existing InjectPlayerReferences() spawn-time pattern — no ambiguous
runtime component search. If the gesture/projectile pipeline needs to distinguish player vs.
enemy Health at runtime, use a tag or layer check, not type-based GetComponent search.

Before reporting done: compile via Unity MCP and confirm zero errors, then enter Play mode,
spawn near another enemy and near the player, and check the Console for exceptions. Report the
actual console output. Confirm via the running scene (not just code review) that only the
player's Health takes damage, and that the health bar reflects damage taken.

VFX: placeholder/minimal is fine this pass — cosmetic VFX is stretch, not required for the
wiring to count as done.

Deliverable: ranged enemy prefab, fires at player from range, correct health bar, no
friendly-fire, MCP-verified clean console.
```

---

## 7. [PINJU primary, Claude if stuck] Environment + lighting
Own task. If stuck on something narrow (a NavMesh rebake issue after adding geometry, a
post-processing volume not applying), bring Claude in for that specific piece — don't hand over
the whole pass.

---

## 8. [PINJU] Hand tracking optimization, profiling, build settings
Own task, last. 72fps floor is the target. Profiler pass for draw calls/static batching.
Confirm the final build actually switches to IL2CPP — Mono is for dev-iteration speed only.
