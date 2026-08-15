# Shuriken VR — Sprint Checklist (12h)

Read AGENTS.md first if you haven't this session. This file is the live state — update the
block below every time you switch tasks or open a fresh Codex session.

## Right now
- Last completed: Item 2 (Enemy health bar asset swap)
- In progress: —
- Next: Item 4 (Locomotion)

## Run two tracks in parallel, not serial
**Track A (you, Unity Editor/art)** — items 2, 3, 7 visuals: health bar asset swap, fireball/ice
FBX + hit VFX, environment art.
**Track B (Codex, C#/systems)** — items 1, 4, 5, 6: bug fix, locomotion, dragon, ranged enemy.

These touch almost entirely different files. Start Codex on Item 1 now, and while it runs, go
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

## 1. [x] [CODEX] Mutant enemy — rotation / stuck / abrupt animation switching

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
needed if the new asset is still a Slider. If its structure is different, flag before Codex
touches EnemyHealthBarUI.cs.

---

## 3. [x] [PINJU] Fireball/Ice Shard FBX + hit VFX
Asset-only: swap prefab references in the existing SpellData assets / projectile prefabs. No
script changes expected if collider/rigidbody setup carries over onto the new model.

---

## 4. [CODEX, you steer] Locomotion — continuous air-pull (Drakheir-style)
Not anchor-based climbing — nothing needs to exist to grab. While a hand is gripping, track its
world-position delta frame to frame; move the rig by the inverse of that delta. Pull your hand
toward you, you move forward; push it away, you move back. Both hands work simultaneously.
`PhysicsGrabbable.cs`/`RigidbodyKinematicLocker.cs` are NOT the base for this — those are for
grabbing actual rigidbody objects, not relevant here.
This is a movement input, not a spell-cast gesture — it must NOT route through IGestureProvider
or GestureManager (locked, casting-only). Grip detection should be its own thing: check first
whether IHand/HandRef already exposes a pinch/grab-strength value to use directly, rather than
building new pose-recognition wiring for it.
Rig exception applies (see AGENTS.md). Set your own time cutoff before starting — write it in
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

## 5. [CODEX] Dragon enemy — 4 types, always flying
Assets already live at Assets\FourEvilDragonsHP\Prefab\{DragonNightmare,DragonSoulEater,
DragonTerrorBringer,DragonUsurper}\ — each folder is one type, color subfolders inside it are
reskins, not separate types. Do not touch Assets\FourEvilDragonsHP\Scene\ — those are the asset
pack's own demo scenes, reference only.
Confirmed: dragons always fly, never NavMeshAgent — direct transform/physics-based flight.

Prompt:
```
Wire up the dragon enemy using Assets\FourEvilDragonsHP\Prefab\DragonNightmare\Blue.prefab as
the first reference type. Dragons always fly — do not use NavMeshAgent, use direct
transform/physics-based flight toward/around the player, matching the pattern other enemies use
for FSM structure (reuse EnemyMove's Idle/Chase/Attack/Dead enum-switch if it fits) but not for
movement.
Reuse Health.cs unmodified for damage/death — this is Assets\Scripts\Health.cs specifically, NOT
Assets\Scripts\ProgBasics\HealthComponent.cs, which is unrelated legacy code, ignore it entirely.
Once DragonNightmare/Blue is fully working, wire the other 3 types (DragonSoulEater,
DragonTerrorBringer, DragonUsurper) as data/prefab variants of the same pattern — color variants
within each type just need the model swapped, not new logic.
Deliverable: at least DragonNightmare confirmed fully working; other 3 types wired via the same
pattern.
Test: spawn each type manually in Game_Scene.unity, confirm damage/death/flight behaviour, check
console for errors.
```

---

## 6. [CODEX] Ranged enemy — spells, VFX, health bar
Health bar: reuse the existing EnemyHealthBarUI/BillboardUI Canvas prefab directly — drop-in,
no new code expected.
**Trap:** don't let Codex reuse FireballProjectile.cs untouched for enemy-fired projectiles. It
currently damages any Health it hits with no shooter-exclusion or faction check — reused
naively, an enemy's shot can hit other enemies or itself on spawn.

Prompt:
```
Integrate the ranged enemy (model + animations provided): attack, health bar, wiring.
Health bar: use the existing EnemyHealthBarUI.cs / BillboardUI.cs Canvas prefab as-is — this
should require no code changes, just adding the prefab to the new enemy.
Attack: on Attack-state entry, spawn a projectile toward the player. If reusing
FireballProjectile.cs, add a guard so the projectile does not damage the enemy that fired it or
other enemies — only the player's Health should take damage from an enemy-fired projectile.
State explicitly how this is guarded (tag/layer check or spawn-frame self-ignore) in the
changelog.
VFX: placeholder/minimal is fine this pass — cosmetic VFX is stretch, not required for the
wiring to count as done.
Deliverable: ranged enemy prefab, fires at player from range, correct health bar, no
friendly-fire.
Test: spawn near another enemy and near the player, confirm only the player takes damage,
confirm health bar reflects damage taken.
```

---

## 7. [PINJU primary, CODEX if stuck] Environment + lighting
Own task. If stuck on something narrow (a NavMesh rebake issue after adding geometry, a
post-processing volume not applying), bring Codex in for that specific piece — don't hand over
the whole pass.

---

## 8. [PINJU] Hand tracking optimization, profiling, build settings
Own task, last. 72fps floor is the target. Profiler pass for draw calls/static batching.
Confirm the final build actually switches to IL2CPP — Mono is for dev-iteration speed only.
