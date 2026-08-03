# ART-SPEC.md

The frozen art conventions for this project. Covers checklist tasks **SETUP-06**, **SETUP-07**,
and **SETUP-08**.

Everything in this document is a **gate**: cheap to decide now, expensive to change once art
production is underway. Nothing here changes without both of us agreeing and re-committing this
file. See [Change control](#change-control) at the bottom.

---

## ⚠️ UNRESOLVED — decide before ART-07

**Is `left` a mirror of `right`, or drawn separately?**

This is the one decision in this document that has not been made. It must be settled before the
first real character sheet (ART-07), because it changes both the Label vocabulary and the
Animator.

| | Draw both sides (default below) | Mirror right → left |
|---|---|---|
| Labels per outfit | 20 | 12 |
| Frames per outfit | 20 | 12 |
| Asymmetric designs | Supported — satchel on one shoulder, scar on one cheek, side-parted hair | Not possible; details flip with the sprite |
| Implementation | Nothing extra | Flip `localScale.x` on the sprite child, never the root |

The vocabulary below assumes **both sides drawn**, matching the original spec. If we choose
mirroring, delete the `*_left*` Labels from the list and note the flip convention here.

**Decision:** _____________  **Agreed by:** _____________  **Date:** _____________

> If mirroring, flip a **child** transform, not the character root. Flipping the root inverts
> child offsets and will send colliders and interaction origins to the wrong side.

---

## 1. Import settings

Every sprite in `Assets/Art/` uses these unless explicitly noted:

| Setting | Value |
|---|---|
| Pixels Per Unit (PPU) | **32** |
| Tile size | **32 × 32** |
| Filter Mode | **Point (no filter)** |
| Compression | **None** |
| Generate Mip Maps | Off |
| Wrap Mode | Clamp |
| sRGB (Color Texture) | On |
| Max Size | ≥ the sheet's native size — never let Unity downscale |
| Mesh Type | Full Rect |

**Max Size is the quiet one.** Unity defaults to 2048. A sprite sheet wider than that gets
silently downscaled on import, which destroys pixel art without any error message. Check it on
every sheet import.

### Legacy assets

The parallax backgrounds inherited from the earlier build (`forest.png`, `foreground.png`,
`Cave BG.png`, `branch.png`, `bush.png`, `drip 1–3.png`, `menu_bg.png`) predate this spec and do
**not** conform to PPU 32. They live in `Assets/Art/Backgrounds/` and are exempt. Do not use them
as a reference for new work, and do not copy their import settings.

---

## 2. Pivots and sorting

- **Pivot: bottom-centre, at the character's feet.** Not the sprite's bounding box centre, not
  the bottom of the canvas — the point the character actually stands on. For a sprite with empty
  headroom, the pivot still sits at the feet.
- **Sort Point = Pivot** on every character `SpriteRenderer`.
- **Transparency Sort Mode = Custom Axis, Axis = (0, 1, 0)** in Project Settings → Graphics
  (checklist task SYS-00.2).

Together these three make a character standing lower on screen draw in front of one standing
higher, which is what sells depth in top-down. Get any one of them wrong and the sorting looks
broken in ways that are hard to diagnose.

> ⚠️ Changing pivots or PPU after art production means re-slicing **every** sprite in the
> project and re-checking every animation. This is one of the four gates in the build checklist.

---

## 3. Camera

| Setting | Value |
|---|---|
| Reference Resolution | **320 × 180** |
| Assets PPU | **32** |
| Upscale Render Texture | Off |
| Pixel Snapping | On |
| Crop Frame | Stretch Fill |

At 320 × 180 with PPU 32, the visible play area is 10 × 5.625 tiles. Design rooms and encounter
spacing around that figure — it's smaller than it sounds, and layouts that read fine in the Scene
view often feel cramped in Game view.

---

## 4. Folder structure

```
Assets/
├── Art/
│   ├── Characters/     char_* sprite sheets, Sprite Library Assets
│   ├── Tilesets/       tileset sheets, Tile assets, Rule Tiles, Palettes
│   ├── UI/             9-slice frames, buttons, prompts, dialogue boxes
│   ├── Items/          icon_* item icons
│   └── Backgrounds/    legacy parallax art (exempt from this spec)
├── Data/
│   ├── Items/          ItemDefinition assets
│   ├── Outfits/        OutfitDefinition assets
│   ├── Quests/         QuestDefinition assets
│   ├── Events/         event channel assets
│   └── Runtime/        InventoryRuntime, PlayerRuntime, EquipmentRuntime, QuestFlagStore
├── Prefabs/
├── Scenes/
├── Scripts/
│   ├── Runtime/
│   └── Editor/
├── Yarn/               .yarn dialogue files
├── Fonts/
└── Settings/
```

**Move and rename assets from inside the Unity Project window only.** Unity tracks assets by the
GUID in the `.meta` file; move a `.png` in Explorer without its `.meta` and every reference to it
breaks silently. If a move must happen outside Unity, close the editor and move the file and its
`.meta` together.

---

## 5. Naming conventions

| Kind | Pattern | Example |
|---|---|---|
| Character frame | `char_<name>_<state>_<dir>_<frame>` | `char_kira_walk_down_2` |
| Item icon | `icon_<itemid>` | `icon_brass_locket` |
| Sprite Library Asset | `outfit_<name>.spriteLib` | `outfit_travelling_cloak.spriteLib` |
| Tileset sheet | `tiles_<area>` | `tiles_forest` |
| Rule Tile | `rule_<area>_<kind>` | `rule_forest_wall` |
| UI element | `ui_<element>_<state>` | `ui_slot_selected` |

Rules:

- **Lowercase, underscores, no spaces.** Spaces in filenames complicate every script, shell
  command, and build pipeline that touches them.
- `<frame>` is zero-indexed: `_0`, `_1`, `_2`, `_3`.
- `icon_<itemid>` must match the `id` field on the corresponding `ItemDefinition` exactly. The
  planned Outfit Checker tooling (POLISH-05) relies on this to find orphaned icons.

---

## 6. Sprite Library vocabulary — FROZEN (SETUP-08)

Every outfit's Sprite Library Asset must define **exactly** these Categories and Labels. Same
names, same spelling, every time.

**Category:** `body`

**Labels:**

```
idle_down
idle_up
idle_left
idle_right
walk_down_0    walk_down_1    walk_down_2    walk_down_3
walk_up_0      walk_up_1      walk_up_2      walk_up_3
walk_left_0    walk_left_1    walk_left_2    walk_left_3
walk_right_0   walk_right_1   walk_right_2   walk_right_3
```

20 Labels, 20 frames per outfit.

> ⚠️ Renaming a Label later breaks the shared Animator across **every outfit ever made**. This is
> a gate. A missing Label doesn't error at import — it fails at runtime, as an invisible or
> wrong-frame character, usually in the outfit you tested least.

### Animation clips

Animation clips must animate the **Sprite Resolver's Label key**, never `SpriteRenderer.sprite`
directly. Mixing the two breaks the paper-doll upgrade path and produces outfits that partially
revert to the base character mid-animation.

### Adding a new outfit

1. Duplicate the base outfit's Sprite Library Asset.
2. Replace all 20 sprites, keeping every Label name identical.
3. Create the `OutfitDefinition` and its `ItemDefinition` (category Outfit).
4. Equip it in play mode and walk in all four directions before considering it done.

---

## 7. Character sheets

| | Canvas | Frames |
|---|---|---|
| Player / NPC | 32 × 32 | 20 (4 idle + 16 walk) |
| Boss | ~96 × 96 | front-on, no directional set |

- Character sprites may exceed 32 × 32 vertically if a design needs headroom (a hat, tall hair).
  The **pivot stays at the feet** and the tile grid is unaffected.
- Walk cycles are 4 frames at 12 fps unless a specific character needs otherwise.
- Deliver as a single horizontal strip per character, sliced by Grid By Cell Size.

**Delivery checklist** — a sheet isn't done until all five pass:

- [ ] Sliced, with every pivot verified at the feet (check a few individually, not just the first)
- [ ] Filter Point, Compression None, Max Size ≥ native
- [ ] All 20 Labels present in the Sprite Library Asset, spelled exactly as above
- [ ] Named per section 5
- [ ] Walks correctly in all four directions in play mode

---

## 8. UI assets

Every UI element ships with all its states (ART-05):

| Element | States |
|---|---|
| Inventory slot frame | normal / selected / empty |
| Interaction prompt | hidden / shown |
| Dialogue box | line / awaiting-choice / choices |

9-slice elements (slot frames, dialogue box) need borders set in the Sprite Editor at import.
Keep corners inside the border region and centre areas flat, or stretching will smear the
detail — the failure is only visible at sizes other than the one it was authored at, so check a
wide box and a tall one.

---

## Change control

Sections 1, 2, and 6 are **gates**. Changing them after art production has begun means re-slicing
sprites, re-rigging libraries, or rebuilding the Animator.

To change a gate: both of us agree, this file is updated and committed in the same session, and
the migration work is added to the board as an explicit task. Not "we'll fix it as we go."

Everything else here can be revised by whoever needs it revised, provided the change is committed
to this file at the same time. A convention that lives only in someone's head isn't a convention.
