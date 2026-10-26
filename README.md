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

<details>
<summary><b>Quan's Street Lights</b></summary>

<table>
<tr>
<td><img src="docs/images/streetlight-brazier.jpg" alt="Quan's Brazier Street Light: a coal brazier hung from a wooden post and arm" width="320"></td>
<td><img src="docs/images/streetlight-lantern.jpg" alt="Quan's Dvergr Street Light: a Dvergr lantern hung from a Grausten and Ashwood post with a carved arch" width="320"></td>
</tr>
<tr>
<td><img src="docs/images/streetlight-brazier-card.png" alt="Build-menu card: 5 Bronze, 2 Coal, 1 Chain and 4 Wood at a Forge" width="320"></td>
<td><img src="docs/images/streetlight-lantern-card.png" alt="Build-menu card: 3 Copper, 1 Dvergr Lantern, 1 Chain, 6 Grausten and 4 Ashwood at a Black Forge" width="320"></td>
</tr>
</table>

Two street lights for lighting paths, roads and the spaces between buildings: a coal brazier for the middle of
the game and a Dvergr lantern for later on. Both work like any other fire, so you refuel them by hand, and with
[NoSmokeStayLit](https://thunderstore.io/c/valheim/p/TastyChickenLegs/NoSmokeStayLit/) you can put them on its
day/night timer or keep them lit.

| | Brazier Street Light | Dvergr Street Light |
|---|---|---|
| **Cost** | 5 Bronze, 2 Coal, 1 Chain, 4 Wood | 3 Copper, 1 Dvergr Lantern, 1 Chain, 6 Grausten, 4 Ashwood |
| **Crafting station** | Forge | Black Forge |
| **Fuel** | Coal | Wisps |
| **Rain** | Stays lit, sheltered by its arm | Stays lit |
| **Strong wind** | Burns low, like any open fire | Stays lit, it's enclosed |
| **Light** | A little brighter and wider than a hanging brazier | A wide pool: lamps about 30 m apart light the whole path |
| **Warmth** | Up to 8 m away, like a hanging brazier | Up to 50 m away, so whole paths stay warm |

- Both stand on the ground or on floors, and need the same support as a wooden post.
- They're 4 m tall, with the light hanging 2.5 to 3 m up.
- To use them with NoSmokeStayLit's timer, add `Quan_streetlight_brazier` and `Quan_streetlight_lantern` to its
  **Custom Items on Timers** setting.

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

<details>
<summary><b>Quan's Street Lights</b> (<code>Quan_streetlight_brazier</code>, <code>Quan_streetlight_lantern</code>)</summary>

Both clone the vanilla hanging brazier (`piece_brazierceiling01`), so they're built on the game's own `Fireplace`
component, the same one every vanilla fire and torch uses: fuel, the add-fuel interaction, and the on/off
switching of light, warmth and effects. Many modded light sources are just a light with no `Fireplace`, so they
sit outside the game's fire behaviour. These street lights behave like vanilla light sources, so any mod that
works with vanilla fires and torches (fuel, lit state, timers and so on) can work with them too. The piece origin
moves to the foot of the post, so `m_disableCoverCheck` is on: the blocked check would otherwise hit the post
itself.

**Brazier street light** (`Pieces/BrazierStreetlight.cs`):

- The frame is the vanilla pole and beam meshes, placed and scaled: two stacked 2 m poles, a 2 m arm, a 45°
  brace at 75% thickness, and four foot poles 75% inside the post.
- The brazier hangs from the arm. Its mesh is copied with the top nine links of its vertical chain removed
  (islands on the centre line above a cut height), so its chain meets the arm.
- The arm's collider is added by the mod and untagged, so it counts as a roof: rain doesn't dim it, but strong
  wind still does (vanilla rule). Vanilla beams are tagged `leaky` and don't shelter.
- Light range 12 m (the brazier's own is 10 m).

**Dvergr street light** (`Pieces/LanternStreetlight.cs`):

- All brazier visuals are stripped. With no high/low flame objects the `Fireplace` never runs its wet check, so
  it's weatherproof. Fuel is Wisp, at the brazier's burn rate.
- The Grausten frame is built from the Grausten Small Pillar, repeated with every other repeat mirrored (no
  joints), mitred at the corner, and welded both ways onto the Tapered Pillar, matching faces by direction so
  the carved star keeps its shading. All the Grausten uses a triplanar copy of its material, so the texture is
  continuous across every piece. The Tapered Pillar's inner notch faces are split off (whole flat faces, not
  triangles) and textured as Ashwood.
- The Ashwood cores are the Ashwood pole stretched to 4 m and 2.2 m.
- The Ashwood Arch is shipped unreadable, so it's used whole, scaled 90% along the beam and post and 75% front to
  back, with its own post and beam hidden inside the cores.
- The lantern and chain are islands lifted from the Dvergr Pole Lantern's mesh. `LanternGlass` swaps its
  material for a copy with emission off while the `Fireplace` is out.
- The light is the Dvergr lantern's, at range 60 m and intensity 1.5, with its `LightLod` switch-off distance
  raised to 80 m. Warmth is the hanging brazier's area at 50 m.
- `Meshes/MeshData.cs` holds the reusable mesh operations (islands, resize, clip, slice, flat faces).

</details>
