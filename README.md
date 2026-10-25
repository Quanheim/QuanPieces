# QuanPieces

New building pieces for the things the vanilla build menu can't quite do. Each one exists because it solves a
real building problem, and they all live in their own **QuanPieces** tab on the hammer.

Everyone on a server needs the mod installed: the server and every player connecting to it.

## Pieces

<details>
<summary><b>Quan's Chimney Flue</b></summary>

<p><img src="docs/images/chimney-flue.jpg" alt="Quan's Chimney Flue seen from below, its stone louvres above a lit campfire inside a wooden chimney" width="480"></p>

<p><img src="docs/images/chimney-flue-tooltip.png" alt="The flue's build-menu card: Blocks rain but not smoke. Must be 1.5m or more above a fireplace. For Valyyn. Costs 5 Stone at a Workbench." width="480"></p>

A 2x2 chimney flue that sits above your fireplace, inside your chimney, and stops the rain putting out your
fire whilst still letting the smoke escape. No longer do you need a roof over your chimneys!

| Cost | Crafting station | Size |
|---|---|---|
| 5 Stone | Workbench | 2x2, snaps and tiles with itself to make larger flues |

> **Place it at least 1.5 m above your fire**, so the smoke can form without putting the fire out.

- It only needs to cover the space directly above the fire. For wider chimneys, snap several together to
  fill the opening.
- It doesn't change how smoke behaves: a chimney that backs smoke up without the flue still will with it.
- You can walk through it. Only its edges are solid.

</details>

## For developers

A BepInEx/Jotunn mod. Each piece lives in its own class under `Pieces/`. The Thunderstore page is
`Package/README.md`; keep its player-facing text in step with the sections above. Build and release steps are in
`docs/DEVELOPMENT.md`.

### How the pieces work

<details>
<summary><b>Quan's Chimney Flue</b> (<code>Quan_chimney_flue</code>)</summary>

A clone of the vanilla iron cage floor (`iron_floor_2x2`), with its bars replaced by a generated stone frame and
angled louvres (`Meshes/BoxMesh.cs`, no colliders), that blocks rain from a fire without stopping its smoke.
It relies only on vanilla behaviour, no patches:

- A fire is "wet" when `Cover.IsUnderRoof`'s thin sphere cast straight up from it hits nothing. A wet fire
  shows its low flame only: no warmth, no cooking area and no smoke spawner.
- Valheim's physics settings have `queriesHitTriggers` on, so a trigger collider on the `piece` layer counts as
  a roof. Smoke puffs are rigidbodies about 0.84 m across that collide with pieces but not with triggers.
- `Cover.IsUnderRoof` skips colliders tagged `leaky`, and the cage floor is tagged `leaky` because rain falls
  through its bars. The flue clears that tag so its slab counts as a roof.
- The flue's slab collider is a trigger. Thin solid strips along its edges give `WearNTear` support (it
  ignores triggers) and give the hammer something to hit.
- Its `WearNTear` material type is wood, despite the stone look. Stone needs at least 100 support, which
  wooden walls can never supply, so a stone flue would fall out of a wooden chimney. Its sounds and particles
  come from the stone floor.
- The louvres use a copy of the stone floor's material with `_RippleDistance` set to 0. Stone pieces otherwise
  wobble each vertex by up to 0.1 m, which pulled the generated boxes apart at their joins. Their UVs map into
  the opaque regions of the stone floor's texture atlas, because the rest of the atlas is partly transparent.
- `Fireplace` treats anything, triggers included, within about 0.5 m above its cover check point as a
  blockage, hence the 1.5 m placement rule.

</details>
