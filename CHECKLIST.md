# Shuriken VR — Sprint Checklist (12h)

Read AGENTS.md first if you haven't this session. This file is the live state — update the
block below every time you switch tasks or open a fresh Codex session.

## Right now
- Last completed: Item 1 (Mutant fix)
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
2. Locomotion: pre-committed fallback to continuous/teleport if grab-based isn't converging by
   your own cutoff (pick one, e.g. 90 minutes — write it here: ______).
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

## 2. [PINJU] Enemy health bar asset swap
Slots into the existing EnemyHealthBarUI.cs / BillboardUI.cs Canvas pattern — no code changes
needed if the new asset is still a Slider. If its structure is different, flag before Codex
touches EnemyHealthBarUI.cs.

---

## 3. [PINJU] Fireball/Ice Shard FBX + hit VFX
Asset-only: swap prefab references in the existing SpellData assets / projectile prefabs. No
script changes expected if collider/rigidbody setup carries over onto the new model.

---

## 4. [CODEX, you steer] Locomotion — grab-based preferred, continuous as fallback
Check `PhysicsGrabbable.cs` and `RigidbodyKinematicLocker.cs` first — both are already in the
project as Meta Interaction SDK samples and may cover most of the grab plumbing. Also check
whether the imported SDK package already ships a climbing/locomotion sample before writing
anything new.
Rig exception applies (see AGENTS.md). Set your own time cutoff before starting — written above
in the ship-line section.

Prompt (grab-based):
```
Implement grab-based locomotion (pull yourself through the world by grabbing static anchors,
Drakheir/Boneworks-style). Check PhysicsGrabbable.cs / RigidbodyKinematicLocker.cs first as a
possible base, and check the imported Meta Interaction SDK samples for an existing
climbing/locomotion component before writing one from scratch.
This is the one task allowed to add components to the OVRCameraRig root. Do not touch anything
under the rig related to hand-tracking data sources, the gesture provider components, or
camera/head anchor setup — casting must keep working exactly as before.
Deliverable: player can pull themselves around the arena by grabbing fixed points.
Test: cast a spell immediately after moving via the new locomotion, confirm gesture detection
is unaffected.
```
Fallback prompt (if pivoting at your cutoff):
```
Implement continuous joystick locomotion using Meta Interaction SDK's standard locomotion
sample as the base, added to the OVRCameraRig root only. Same rig-exception constraint —
hand-tracking/gesture wiring untouched.
```

---

## 5. [CODEX] Dragon enemy — 4 types
**Resolve first:** do the dragons fly or move on the ground? Grounded reuses NavMeshAgent like
existing enemies; flight needs a different, non-NavMesh movement controller. Decide before
running the prompt.
Use a data-driven pattern for the 4 variants (a DragonType SO or prefab-variant approach,
matching how SpellData/WaveData already work) — get ONE type fully wired end-to-end first, the
other 3 should be near-zero-cost swaps once the pattern is proven.

Prompt:
```
Integrate the dragon enemy (models + animations already provided). Movement: [GROUNDED via
NavMeshAgent, matching EnemyMove's pattern / FLYING — needs a new movement approach. State
which before starting.]
Build one fully wired dragon type first: Health integration (reuse Health.cs unmodified), FSM
matching the complexity actually needed (reuse EnemyMove's Idle/Chase/Attack/Dead enum-switch
pattern if it fits; only diverge if the dragon genuinely needs more states), damage/death
events wired the same way existing enemies do it.
Once one type works, add the other 3 as data/prefab variants of the same pattern — do not write
4 separate scripts unless behaviour actually diverges per type.
Deliverable: 4 dragon prefabs, at least one confirmed fully working standalone.
Test: spawn each type manually, confirm damage/death/animation cycle, check console for errors.
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
