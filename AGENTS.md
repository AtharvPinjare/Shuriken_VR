# Shuriken VR — AGENTS.md

Sprint mode. Shipping in ~12 hours. Read this in full before touching anything. Don't re-derive
architecture by scanning the project — everything you need is here or in CHECKLIST.md.

## Project
Unity 6 (6000.3.8f1), Meta Interaction SDK v201.0.0, Quest 3, Android/IL2CPP final build.
Repo root: C:\Shuriken_VR\Shuriken_VR

**[PINJU — do this before the first session]** Paste your actual Assets scripts/prefabs tree
below. Ground-truth paths beat described ones — this is the single biggest lever against Codex
inventing files that don't exist or guessing wrong paths.

PowerShell, from repo root:
```
Get-ChildItem -Recurse -Include *.cs,*.prefab,*.asset "Assets\<your scripts/prefabs root>" | Resolve-Path -Relative
```

```
<PASTE TREE HERE>
```

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
_(fill in once done: root cause + what fixed it)_

### Locomotion system
_(fill in once done: grab-based or continuous, what's now on the rig, what was reused from the SDK samples)_

### Dragon enemy
_(fill in once done: grounded or flight, data pattern used for the 4 variants)_

### Ranged enemy
_(fill in once done: attack implementation, friendly-fire guard used)_
