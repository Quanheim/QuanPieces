# QuanPieces

New building pieces for the things the vanilla build menu can't quite do. Each one exists because it solves a
real building problem, and they all live in their own **QuanPieces** tab on the hammer.

Everyone on a server needs the mod installed: the server and every player connecting to it.

## Pieces

<details>
<summary><b>Quan's Chimney Flue</b></summary>

<p><img src="docs/images/chimney-flue.jpg" alt="Quan's Chimney Flue seen from below, its stone louvres above a lit campfire inside a wooden chimney" width="480"></p>

<p><img src="docs/images/chimney-flue-tooltip.png" alt="The flue's build-menu card: Blocks rain but not smoke. Must be 2m or more above a fireplace. For Valyyn. Costs 5 Stone at a Workbench." width="480"></p>

A 2x2 chimney flue that sits above your fireplace, inside your chimney, and stops the rain putting out your
fire whilst still letting the smoke escape. No longer do you need a roof over your chimneys!

| Cost | Crafting station | Size |
|---|---|---|
| 5 Stone | Workbench | 2x2, snaps and tiles with itself to make larger flues |

> **Place it at least 2 m above your fire**, so the smoke can form without putting the fire out.

- It only needs to cover the space directly above the fire. For wider chimneys, snap several together to
  fill the opening.
- It doesn't change how smoke behaves: a chimney that backs smoke up without the flue still will with it.
- You can walk on it like a grate, and so can creatures; items dropped on it stay there. Smoke still rises
  straight through.

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

<details>
<summary><b>Quan's Planters</b></summary>

<p><img src="docs/images/planter-greenhouse.jpg" alt="A Large Planter full of crops under a roofed walkway, tucked in front of a half wall" width="480"></p>

<table>
<tr>
<td><img src="docs/images/planter-small.jpg" alt="Quan's Planter: a 1x2 raised bed of cultivated soil in a mitred wooden frame" width="320"></td>
<td><img src="docs/images/planter-large-bloom.jpg" alt="Quan's Large Planter packed full and fully grown: flowering onions, carrots, turnips, Jotun Puffs and Magecaps side by side" width="320"></td>
</tr>
<tr>
<td><img src="docs/images/planter-small-card.png" alt="Build-menu card: 10 Wood at a Workbench" width="320"></td>
<td><img src="docs/images/planter-large-card.png" alt="Build-menu card: 16 Wood at a Workbench" width="320"></td>
</tr>
</table>

Raised beds of cultivated soil for growing crops wherever you build: in a greenhouse, along a terrace, or tucked
under the eaves in front of a half wall. Plant straight into them with the cultivator, no hoe needed.

| | Planter | Large Planter |
|---|---|---|
| **Size** | 1x2 | 1x4 |
| **Cost** | 10 Wood | 16 Wood |
| **Crafting station** | Workbench | Workbench |

- Anything you can plant with the cultivator grows in them, in any biome, with no heat or cold damage.
- They work under a roof, so you can farm indoors.
- Pack your crops in as tightly as you like: plants in a planter don't crowd each other, and nearby walls and
  other building pieces don't crowd them either.
- They're 80 cm tall, low enough to sit under a 1 m half wall. They snap to the floor grid and to each other, and
  need the same support as a wooden floor.
- Removing a planter clears it: seedlings give back their seeds, and anything ready to pick drops its harvest.
  Other mods' plants (such as PlantEverything's) give back whatever that mod gives when you remove them.

</details>

<details>
<summary><b>Quan's Chicken Coop</b></summary>

<p><img src="docs/images/chicken-coop-built.jpg" alt="A finished chicken coop: wooden bar walls and a gate on a raised, thatched floor under a thatch roof" width="480"></p>

<table>
<tr>
<td><img src="docs/images/chicken-coop.jpg" alt="Hen Solo and Cluckbacca safe inside their coop" width="240"></td>
<td><img src="docs/images/chicken-coop-door-wall.jpg" alt="The Chicken Coop Wall and the Chicken Coop Door, wooden bars with an iron latch on the gate" width="240"></td>
<td><img src="docs/images/chicken-coop-egg.jpg" alt="Picking up a warm egg through the coop wall's bars" width="240"></td>
<td><img src="docs/images/floor-heater.jpg" alt="Quan's Floor Heater glowing on the ground beside a raised coop, warming its floors" width="240"></td>
</tr>
<tr>
<td><img src="docs/images/chicken-coop-floor-card.png" alt="Build-menu card: Quan's Chicken Coop Floor, 2 Wood at a Workbench" width="240"></td>
<td><img src="docs/images/chicken-coop-wall-card.png" alt="Build-menu card: Quan's Chicken Coop Wall, 2 Wood at a Workbench" width="240"></td>
<td><img src="docs/images/chicken-coop-door-card.png" alt="Build-menu card: Quan's Chicken Coop Door, 2 Wood at a Workbench" width="240"></td>
<td><img src="docs/images/floor-heater-card.png" alt="Build-menu card: Quan's Floor Heater, 5 Stone at a Workbench" width="240"></td>
</tr>
</table>

Pieces for giving your chickens a proper home: a thatched floor, a heater to warm it, walls of wooden bars and a
gate to match.

| | Coop Floor | Floor Heater | Coop Wall | Coop Door |
|---|---|---|---|---|
| **Size** | 2x2 | 1x1 | 2x2 | 2x2 |
| **Cost** | 2 Wood | 5 Stone | 2 Wood | 2 Wood |
| **Crafting station** | Workbench | Workbench | Workbench | Workbench |

- **Floor**: a wooden floor with the thatch laid low, so even chicks stand tall on it. While a floor heater
  that's on is within 10 m, it gives off warmth like a fire, so eggs laid on it hatch. As in vanilla, they also
  need a roof over them. Without a heater nearby it stays cold, and eggs on it wait to be collected.
- **Floor Heater**: a small wooden box of glowing embers. It's built switched off; use it to turn it on or off: on,
  the coop floors around it are warm and eggs hatch; off, they go cold, eggs stop hatching (an egg that's already
  growing starts again from scratch), and you can collect them. It needs no fuel and doesn't mind rain, wind, smoke
  or open sky, so it can sit under the edge of a raised coop or out in the open beside it. Its hover text shows how
  many coop floors it's warming.
- **Wall**: wooden bars that let the light and air in and keep animals in or out. You can see and reach through
  them, so you can pick up eggs from outside the coop.
- **Door**: a 2x2 gate of wooden bars with an iron latch. It opens and closes like any door.
- They snap like vanilla 2x2 floors and walls, and need the same support as other wooden pieces.

</details>

<details>
<summary><b>Quan's Building Pieces</b></summary>

<table>
<tr>
<td><img src="docs/images/wood-wall-card.png" alt="Build-menu card: Quan's Wood Wall, 2 Wood at a Workbench" width="240"></td>
<td><img src="docs/images/wood-wall-half-card.png" alt="Build-menu card: Quan's Wood Wall Half, 1 Wood at a Workbench" width="240"></td>
<td><img src="docs/images/wood-wall-1x1-card.png" alt="Build-menu card: Quan's Wood Wall 1x1, 1 Wood at a Workbench" width="240"></td>
</tr>
<tr>
<td><img src="docs/images/wood-wall-2x3-card.png" alt="Build-menu card: Quan's Wood Wall 2x3, 3 Wood at a Workbench" width="240"></td>
<td><img src="docs/images/wood-wall-1x3-card.png" alt="Build-menu card: Quan's Wood Wall 1x3, 2 Wood at a Workbench" width="240"></td>
<td><img src="docs/images/triangular-floor-1-card.png" alt="Build-menu card: Quan's Triangular Wood Floor 1x1 (1), 1 Wood at a Workbench" width="240"></td>
</tr>
<tr>
<td><img src="docs/images/triangular-floor-2-card.png" alt="Build-menu card: Quan's Triangular Wood Floor 1x1 (2), 1 Wood at a Workbench" width="240"></td>
</tr>
</table>

Plain wooden pieces that fill the gaps in vanilla's building set: walls with no beams across the back, taller
walls, and a small triangular floor.

| | Size | Cost | Crafting station |
|---|---|---|---|
| **Wood Wall** | 2x2 | 2 Wood | Workbench |
| **Wood Wall Half** | 2x1 | 1 Wood | Workbench |
| **Wood Wall 1x1** | 1x1 | 1 Wood | Workbench |
| **Wood Wall 2x3** | 2 m wide, 3 m tall | 3 Wood | Workbench |
| **Wood Wall 1x3** | 1 m wide, 3 m tall | 2 Wood | Workbench |
| **Triangular Wood Floor 1x1 (1) and (2)** | half a 1x1, cut corner to corner | 1 Wood | Workbench |

- **Beamless walls:** the vanilla wood walls with nothing but their vertical planks, no horizontal beams across
  the back. Otherwise vanilla's: the same look, wear as they take damage, support and snapping, so they sit happily
  beside vanilla walls.
- **3 m walls:** beamless too. Their planks are longer, not stretched: the wood grain carries on up the extra
  height at the same scale as the 2x2 wall's, so a 2x3 next to a 2x2 matches.
- **Triangular floors:** fill small triangular gaps, or finish the end of a 1x1 shelf. Snap one to the shelf's end
  and it meets the wall behind at a clean 45°, without a corner poking out through the back of a thin wooden wall.
  The missing half is empty space: you walk straight through it. (1) and (2) are mirror images, with the planks
  along one short side or the other, so you can match the floor or shelf beside it. They snap at their three
  corners.

</details>

<details>
<summary><b>Quan's Decorative Pieces</b></summary>

<table>
<tr>
<td><img src="docs/images/itemstand-wall-card.png" alt="Build-menu card: Quan's Item Stand for the wall, 4 Finewood and 1 Bronze Nails at a Workbench" width="240"></td>
<td><img src="docs/images/itemstand-floor-card.png" alt="Build-menu card: Quan's Item Stand for the floor, 4 Finewood and 1 Bronze Nails at a Workbench" width="240"></td>
</tr>
</table>

Small pieces for finishing touches: backless item stands and a bevelled shelf edge.

### Backless item stands

**Decorators, this one's for you.** Every vanilla item stand comes with a chunky wooden plate. Behind a shield
or a trophy you barely notice it, but put a knife, a cup or a bottle on a table or a shelf and there's an
unsightly box sitting under it, every time. Not any more.

These are the two vanilla item stands, on the wall and on the floor, with only their little nub: no backplate or
underplate. The item sits right on the table, the shelf or the floor, or hangs right against the wall, and hides
the nub completely, so your displays look placed, not mounted.

| | Cost | Crafting station |
|---|---|---|
| **Item Stand (wall)** | 4 Finewood, 1 Bronze Nails | Workbench |
| **Item Stand (floor)** | 4 Finewood, 1 Bronze Nails | Workbench |

- They work exactly like vanilla's: use one to attach an item or trophy, use it again to take it off, and they
  accept the same items. They cost the same as vanilla's.
- Both are called "Quan's Item Stand", as vanilla's two are both "Item stand"; the icons show which is which.
- The nub is small, so its hover area is a little bigger than the nub to make it easier to aim at.
- With an item on it, the nub disappears altogether; it comes back when you take the item off.

### Bevelled shelf edge

<p><img src="docs/images/bevelled-shelf-edge.png" alt="A 1x1 floor shelf on a plank wall, finished at both ends with Quan's Bevelled Shelf Edges: the planks turn the corner at 45° into the wall, their seams meeting the shelf's" width="640"></p>

Half a 1x1 wood floor, cut corner to corner, with its planks running along the long edge. Snap it to the end of a
1x1 shelf and the shelf finishes in a neat bevelled corner into the wall, with the long edge a plank's own rounded
edge, and nothing poking out through the back of a thin wall. It's the same both ways round, so one piece does
both ends of a shelf.

| | Size | Cost | Crafting station |
|---|---|---|---|
| **Bevelled Shelf Edge** | half a 1x1, cut corner to corner | 1 Wood | Workbench |

</details>

## For developers

A BepInEx/Jotunn mod. Each piece lives in its own class under `Pieces/`, Harmony patches under `Patches/` and shared mesh code under `Meshes/`. The Thunderstore page is
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
- A second, solid copy of the slab sits on the `blocker` layer, the layer vanilla uses for invisible walls. In
  Valheim's layer collision matrix, characters and items collide with `blocker` (and `Character` counts it as
  ground) but smoke doesn't, so the flue can be walked on without stopping smoke.
- Its `WearNTear` material type is wood, despite the stone look. Stone needs at least 100 support, which
  wooden walls can never supply, so a stone flue would fall out of a wooden chimney. Its sounds and particles
  come from the stone floor.
- The louvres use a copy of the stone floor's material with `_RippleDistance` set to 0. Stone pieces otherwise
  wobble each vertex by up to 0.1 m, which pulled the generated boxes apart at their joins. Their UVs map into
  the opaque regions of the stone floor's texture atlas, because the rest of the atlas is partly transparent.
- `Fireplace` treats anything, triggers included, within about 0.5 m above its cover check point as a
  blockage, hence the 2 m placement rule (with room to spare for taller fires such as the hearth).

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

<details>
<summary><b>Quan's Planters</b> (<code>Quan_planter_1x2</code>, <code>Quan_planter_1x4</code>)</summary>

Both clone the vanilla 1x1 wood floor (`wood_floor_1x1`) for its piece setup (wood support, sounds, placement)
and rebuild it (`Pieces/Planter.cs`). Their only snap points are the four bottom corners.

- The rim is the 2 m beam's cube at half width (0.2 x 0.4 m). Each long side is exact vanilla 2 m beams end to
  end, so their joint looks like vanilla's. At the corners each beam's end is pushed out past the corner and cut
  along the diagonal (`MeshData.Clip`), which makes the mitre and removes that end's bevel.
- The bed is the same cube in the floor's material, one solid block 50 cm deep, reaching 4 cm into the beams. (The
  floor's own mesh is separate planks, which showed gaps when stretched.)
- The soil uses the terrain's own cultivated textures. They exist at runtime only as slices of the terrain's
  texture arrays (`terrain_d_array` slice 10, `terrain_n_array` slice 5), so `PlanterSoil` copies them out on the
  GPU (`Graphics.CopyTexture`) into a copy of the floor's material. That copy has `_Cutoff` at 0, because the
  terrain texture's alpha holds other data and would otherwise punch holes in the soil, and no vertex wobble.

Planting relies on Harmony patches (`Patches/PlanterPatches.cs`), each acting only on points within a few cm of
a planter's soil collider (`Quan_planter_soil`, checked by `PlanterSoil.IsOverSoil`). Vanilla only ever asks the
terrain whether ground is cultivated, so:

- `Heightmap.IsCultivated` returns true on planter soil.
- `Player.PieceRayTest`: when a seedling, or anything else from the cultivator apart from its terrain tools (such
  as PlantEverything's pickable mushrooms and bushes), is being placed and the aim hits the soil, the hit is
  reported as terrain, so the vanilla placement checks (and then `IsCultivated`) accept it.
- `Player.UpdatePlacementGhost`: wrong biome and deep snow don't block planting in a planter.
- `Plant.UpdateHealth`: biome, heat and cold are ignored. `Plant.HaveRoof` reports no roof.
- `Plant.HaveGrowSpace`: always has room, apart from vines needing room from other vines. Grown crops must not
  count as obstacles: the game destroys a crop that's unhealthy when it's due to grow.
- `StaticPhysics.Fall`: grown crops on the soil stay put instead of dropping to the terrain.
- `WearNTear.Highlight`: the build-mode support tint is skipped on a planter while planting into its soil.

Each patch class is applied on its own (`QuanPiecesPlugin.ApplyPatches`), so one that stops fitting the game is
logged and skipped without affecting the rest. When a planter is destroyed, `PlanterSoil` (via
`WearNTear.m_onDestroyed`) clears the soil itself. Seedlings drop their seeds directly (`Piece.DropResources`
won't, because saplings' costs aren't marked recoverable). Pickables go as if removed by hand: `Piece.DropResources`,
so the owning mod's refund rules apply, plus the harvest if ripe.

</details>

<details>
<summary><b>Quan's Chicken Coop</b> (<code>Quan_chicken_coop_floor</code>, <code>Quan_floor_heater</code>, <code>Quan_chicken_coop_wall</code>, <code>Quan_chicken_coop_door</code>)</summary>

**Floor** (`Pieces/ChickenCoopFloor.cs`): the vanilla 2x2 wood floor (`wood_floor`) with the 26° thatch roof's
(`wood_roof`) three straw layers laid flat on top. The straw is taken into the roof's own space and unrolled:
along the slope, the 2.24 m sloped span maps onto the floor's 2 m while the overhang past each edge keeps the
roof's exact length; height above the slope is squashed into a 5 cm layer lying on the planks. The floor's
collider is raised to halfway up the thatch, so small animals stand on it. A trigger box over the floor on the
`character_trigger` layer carries an `EffectArea` of type Heat, which is what `EggGrow` checks for (eggs also need
a roof, checked separately). The box starts inactive; `CoopFloorWarmth` checks once a second (staggered per floor)
for a loaded heater that's on within 10 m of the floor's centre and switches the box to match. Every client works
this out for itself from the heaters' shared state. It costs the same as the thatch roof, read from its recipe.

**Floor heater** (`Pieces/FloorHeater.cs`, `Pieces/FloorHeaterControl.cs`): kitbashed on the vanilla 1x1 wood
floor (`wood_floor_1x1`) for its piece setup, wood support and placement. Its planks are replaced by a 1 m square
of 0.2 m beams (the planter's mitred rim, 0.25 m tall), a plank-material base inside, and the campfire's
(`fire_pit`) ash bed (`New/Quad`) scaled down onto it. The campfire's glowing ash (`_enabled_high/Quad (1)`) and
low-fire light (`_enabled_low/Point light`) are copied into an `embers` child shown while it's on. It's not a
`Fireplace`, so nothing checks fuel, rain, cover or smoke. `FloorHeaterControl` is the switch: the on/off state is
a ZDO bool (`Quan_floor_heater_lit`, off by default), set by the owner through an RPC, and wards are respected.
Version 1.3.0's unheated floor (`Quan_chicken_coop_floor_unheated`) was removed in 1.4.0.

**Door** (`Pieces/ChickenCoopDoor.cs`): the vanilla Iron Gate (`iron_grate`), 2x3, made 1 m shorter by reshaping
rather than scaling. The frame's top bar drops 1 m and the uprights shorten. The swinging leaf is split into
islands: its outer frame and vertical bars lose 1 m from the middle (their end rails keep their thickness), the
hinges move with the ends, and the cross bars and latch move together, unchanged, to the middle. The frame and
bars are wood; the hinges, cross bars and latch stay iron, on a second renderer beside the leaf.

**Wall** (`Pieces/ChickenCoopWall.cs`): the vanilla Cage Wall 2x2 (`iron_wall_2x2`) with the same meshes in
wood. Its single solid collider keeps animals in and out, so `Patches/SeeThroughPatches.cs` lets the hover look
past it: `Player.FindHoverObject` is re-run, skipping pieces marked `SeeThrough`, when the first thing hit is one.
Hammer aiming uses its own ray and isn't affected.

The door and wall share `Pieces/CoopWood.cs`: 2 Wood at a Workbench (the Cage Wall's 2 Iron, in wood), the wood
pole's materials without vertex wobble (which visibly bends thin bars), and the wood door's support, sounds and
effects. `Meshes/WoodGrain.cs` gives the iron meshes new UVs, mapping each face flat from the side it faces with
the grain along the bar, since their UVs point into a metal texture's layout.

</details>

<details>
<summary><b>Quan's Plank Walls</b> (<code>Quan_wood_wall</code>, <code>Quan_wood_wall_half</code>, <code>Quan_wood_wall_1x1</code>, <code>Quan_wood_wall_2x3</code>, <code>Quan_wood_wall_1x3</code>)</summary>

`Pieces/PlankWall.cs`. Clones of the vanilla `woodwall` (2x2), `wood_wall_half` and `wood_wall_quarter` (1x1), with
the beams taken out of their meshes. No patches.

- Each wear state (`New`, `Worn`, `Broken`) is a `LODGroup`. Up close it draws one combined mesh of planks and
  beams, baked in the editor by SimpleMeshCombine from the boxes under `high` (their renderers are off, and they're
  in no LOD). From a distance it draws a plain slab, which has no beams anyway.
- The `high` boxes are what the wall breaks into when removed or destroyed: `WearNTear.m_fragmentRoots` points at
  each state's `high`. So they stay, minus the beam boxes; on the 3 m walls the plank boxes are lengthened too.
  (Deleting `high` made `RPC_CreateFragments` throw after the refund, leaving the wall standing.)
- The combined mesh is split into islands (`MeshData.KeepIslands`), and the islands under 0.5 m tall are dropped:
  the beams are 0.18-0.33 m, the planks 1-2 m. The mesh is handled in the piece's own space, since the combined
  meshes sit under offset transforms (some at y = 50 or 51).
- The 3 m walls are then lengthened with `MeshData.ExtendY`: about the centre for the 2x3 (the 2x2's pivot), and
  from the foot for the 1x3 (the 1x1's). Every face's UVs grow with it, so the texture carries on instead of
  stretching. The wood texture (`Planks5c_low`, tiling) shows 0.56 of its width along a plank, so a 3 m plank at
  the 2x2's grain scale uses 0.84 of it and nothing visibly repeats. Vanilla maps the same texture span onto 1 m
  and 2 m planks, so the 1x3 takes half the 1x1's grain density to match the 2x2 and 2x3.
- On the 3 m walls, the snap points, snow, distant slab and box collider move or grow with the extra height.

</details>

<details>
<summary><b>Quan's Triangular Wood Floors 1x1</b> (<code>Quan_wood_floor_triangle_1x1_1</code>, <code>Quan_wood_floor_triangle_1x1_2</code>, <code>Quan_bevelled_shelf_edge</code>)</summary>

`Pieces/TriangleFloor.cs`. Clones of the vanilla 1x1 wood floor (`wood_floor_1x1`, two planks running along z),
cut on a diagonal plane through its centre. No patches.

- Every mesh in `New`, `Worn` and `Broken` (the planks and the distant box) is cut with `MeshData.Cut`: a `Clip`,
  then each plank's cross-section on the plane (the convex hull of its points there) is closed with a flat face,
  so the cut planks stay solid. The mesh is handled in the piece's own space and moved back.
- The two versions cut on opposite diagonals, keeping the right angle at a different corner. With the planks
  along z, that makes them mirror images, which no rotation can swap.
- The square box collider (`collider`) becomes a convex `MeshCollider` wedge of the same thickness. The snap point
  on the cut-off corner and the square snow cover (`floor_1x1_snow`) are removed.
- The Bevelled Shelf Edge (`Quan_bevelled_shelf_edge`) is (1)'s triangle with the planks along the long edge: the
  planks are lengthened along their grain with `MeshData.ExtendY` (via a z-to-y turn, so the texture extends rather
  than stretches), narrowed across by 1/√2 (texture cropped, not squashed) so the two planks exactly fill the
  triangle's depth and their seam meets each short side halfway, where a 1x1 floor's seam meets it, then turned 45°
  with the outermost plank's edge on the long edge and cut along the two short sides.

</details>

<details>
<summary><b>Quan's Item Stands</b> (<code>Quan_itemstand</code>, <code>Quan_itemstandh</code>)</summary>

`Pieces/NubItemStand.cs`. Clones of the vanilla `itemstand` (wall) and `itemstandh` (floor), so the attaching is
the game's own `ItemStand`. No patches.

- Each look (`New`, `WornBroken`) is two boxes: the plate (the bigger) and the nub. The plates are deleted, and the
  `LODGroup` drops their empty entries.
- "Out" is the plate's face normal (its thinnest axis), towards the nub. The floor stand's parts are rotated 90°
  and its nub sits 5.4 cm to one side of the plate's centre, so the plate-to-nub direction would point sideways.
- The plate's face stands off the piece's origin (5 cm on the wall stand, 2.5 cm on the floor stand). The nub,
  `m_attachOther` and `m_dropSpawnPoint` all move back by that much. The game places an attached item relative to
  `m_attachOther` (plus the item's own `attach` offset), so every item keeps its vanilla position relative to the
  plate's face, now the surface itself.
- On the floor stand the nub also slides sideways under the attach point (vanilla attaches items at the plate's
  centre, beside the nub), so an attached item hides it.
- The `collider` box is remade round the nub, starting exactly at the origin and going out, at least 10 cm across.
  Placement (`Player.UpdatePlacementGhost`) puts the collider's nearest point on the surface, so the box's start
  is where the surface ends up: it must be the origin, or the stand floats (found in testing on the floor stand).
- `NubHider` (on both stands) switches the nub's renderers off while `ItemStand.HaveAttachment()` is true, read
  every frame from the stand's ZDO, so every player sees the same. Only renderers change; the hover box stays.

</details>
