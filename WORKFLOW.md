# WORKFLOW.md

How the two of us work on this repo without standing on each other. Covers checklist task
**SETUP-05**.

Read it once, agree to it, then treat it as settled. Most of it exists because Unity projects
break in specific, repetitive ways when two people share one.

---

## Machine setup — once per machine

Both machines need all of this before the first pull:

1. **Unity `6000.5.3f1`.** Exactly this version. Opening the project in a different Unity
   upgrades every asset in place, and the resulting diff is unreviewable.
2. **Git LFS:**
   ```bash
   git lfs install
   ```
   Skip this and PNGs arrive as one-line text pointer files. Unity will show broken sprites and
   the cause is not obvious from inside the editor.
3. **The UnityYAMLMerge driver:**
   ```bash
   git config --global merge.unityyamlmerge.name "Unity SmartMerge"
   git config --global merge.unityyamlmerge.driver "'<PATH-TO-UNITY>/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p --force --fallback none %O %B %A %A"
   ```
   Substitute your own install path — they differ between our machines.
4. Confirm with `git lfs ls-files` — it should list the project's `.png` and `.ttf` files.

---

## The rules

### 1. One owner per scene at a time

Scenes are the one thing that genuinely does not merge. Before opening a scene for editing, say
so in chat. When you're done and pushed, say that too.

If you need to work in a scene someone else has open, wait, or do the work somewhere that isn't a
scene — see rule 2.

### 2. Areas are nested prefabs, not loose scene objects

An area's tilemaps, props, triggers, and item placements live inside a prefab under
`Assets/Prefabs/`. The scene contains the prefab instance and very little else.

This is what makes rule 1 survivable. Two people can edit two different area prefabs
simultaneously; two people cannot edit one scene simultaneously. If everything lives loose in
`Overworld.unity`, every task in the project serialises through one file.

### 3. Content lives in prefabs and ScriptableObjects

Items, outfits, quests, enemies, dialogue, event channels — all assets under `Assets/Data/` or
`Assets/Yarn/`, never values typed into a component in a scene.

Content authored as assets can be created by whoever's doing content work, without touching code
or scenes, and reviewed as a readable diff.

### 4. Never move or rename assets outside Unity

Unity identifies assets by the GUID in the paired `.meta` file. Move a `.png` in Explorer without
its `.meta` and every reference breaks — silently, with no error, sometimes not noticed for days.

Use the Unity Project window. If a move genuinely must happen outside Unity, close the editor and
move the file **and** its `.meta` together.

### 5. Commit `.meta` files with their asset, always

A new asset without its `.meta` gives the other person a fresh GUID and a broken reference. Never
`git add` a specific file when adding new assets — stage the directory, or use `git add -A`, so
the metas come along.

### 6. Pull before you start, push when you stop

Long-lived divergence is where Unity merge pain comes from. Both of us work on `main`; pull at the
start of a session, push at the end of it, and don't leave a scene or prefab edit sitting
uncommitted overnight.

If something isn't finishable in one session, commit it in a working state anyway — an ugly commit
is cheaper than a three-day divergence.

### 7. Reference task IDs in commit messages

```
SYS-02.2  WorldItem: add + raise ItemCollected, disable on pickup
ART-04    5 item icons, 32x32
SETUP-06  Freeze PPU and pivot convention
```

Makes `git log` legible against the build checklist, and makes it obvious what was in progress if
a session ends mid-task.

### 8. Done means the checklist's "Done when"

Every task in the build checklist has a *Done when* line. That's the definition — not "the code
compiles," not "it looks right in the inspector." Test the stated behaviour in play mode, then
tick the box.

---

## When a merge conflict happens

**On `.unity` / `.prefab` / `.asset` files:**

```bash
git mergetool
```

UnityYAMLMerge handles most of these. It's configured via `.gitattributes` and the driver from
setup step 3.

If it can't resolve, don't hand-edit the YAML. Take one side whole:

```bash
git checkout --ours   path/to/Scene.unity     # keep mine
git checkout --theirs path/to/Scene.unity     # keep theirs
```

Then redo the lost work by hand. Hand-merged Unity YAML produces corrupted scenes that fail in
ways which look like engine bugs.

**On `.cs` files:** normal text merge, resolve as usual.

**On binary art:** LFS can't merge. One version wins; the other gets redone or renamed.

---

## What never gets committed

`Library/`, `Temp/`, `Logs/`, `Obj/`, `Build/`, `UserSettings/`, `.csproj`, `.sln` — all covered
by `.gitignore`. If any of these show up in `git status`, something is wrong with the ignore
rules; fix that rather than committing around it.

---

## Related documents

- **`ART-SPEC.md`** — PPU, pivots, naming, the frozen Sprite Library vocabulary. Binding on all
  art production.
- **Build checklist** — task IDs, ownership, phase ordering, dependencies.
