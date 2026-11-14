# QuanPieces: design (pieces to come)

Pieces planned for QuanPieces that aren't built yet. Built pieces are described in the [README](../README.md);
the game code each one relies on is in [valheim-dependencies.md](valheim-dependencies.md). When a piece here is
built, move its description to the README and remove it from this file.

## ▶ Test run: plank walls (1.5.0)

Built 14 Nov 2026, not yet tested: Quan's Wood Wall, Wood Wall Half, Wood Wall 1x1, Wood Wall 2x3 and Wood Wall
1x3, Quan's Triangular Wood Floor 1x1 (1) and (2), and Quan's Item Stand (wall and floor) (described in the [README](../README.md)). Test build: `dist/Quandru-QuanPieces-1.5.0.zip`.

| Test | OK | # | Do | Expect / Result |
|---|---|---|---|---|
| | | | **Done** | |
| ✅ | ✅ | 1 | **Prerequisite:** in Gale's QuanPieces profile, remove `Quandru-QuanPieces` if it's there, import the 1.5.0 zip, start the game and load a world. | **Passed (14 Nov):** QuanPieces 1.5.0 loaded; no "Couldn't add" or "skipped" lines, no errors; Jotunn added 18 pieces (9 existing + 9 new). |
| ✅ | ✅ | 2 | Open the hammer's QuanPieces tab. | **Passed (14 Nov):** all nine new pieces in the tab; costs 2, 1, 1, 3, 2 Wood (walls), 1 Wood (triangles), 4 Finewood + 1 Bronze Nails (item stands, vanilla's), all at a Workbench. Triangle (2)'s icon shows it nearly edge-on (same render angle as (1)); cosmetic, offered a turned icon. |
| ✅ | ✅ | 3 | Place Quan's Wood Wall beside a vanilla wood wall; look at both from front and back. | **Passed (14 Nov):** same planks and colour from the front; from behind, vanilla's two beams show and Quan's has none. |
| | | | &nbsp; | |
| | | | **Remaining** | |
| ⬜ | ⬜ | 4 | Same for the half and the 1x1, each beside its vanilla twin. | Same as 3. |
| ⬜ | ⬜ | 5 | Place a 2x3 beside a 2x2 (Quan's or vanilla) on the same floor. | 3 m tall; no beams, front or back; grain runs the full height at the 2x2's scale, not stretched; no seam or visible repeat. |
| ⬜ | ⬜ | 6 | Place a 1x3 beside the 2x3. | Same height and grain scale as the 2x3. |
| ⬜ | ⬜ | 7 | Snap the 3 m walls: to a floor's edge, side by side, and one wall on top of another. | They snap at floor level and at 3 m; no gaps or overlaps. |
| ⬜ | ⬜ | 8 | Hit one of each wall until it's worn, then until it's broken (or damage with devcommands). | Worn and broken looks have no beams; on the 3 m walls they're 3 m tall. |
| ⬜ | ⬜ | 9 | Walk away from a 2x3 and 1x3 until they switch to their distant look. | The distant slab is the full 3 m, not 2 m or 1 m. |
| ⬜ | ⬜ | 10 | Turn on the hammer's support view on a wall of stacked 3 m walls. | Support colours as for vanilla wood walls; the walls don't fall. |
| ⬜ | ⬜ | 11 | Walk into the top metre of a 3 m wall; aim the hammer at its top. | You can't walk through it; the hammer targets it all the way up. |
| ⬜ | ⬜ | 12 | Remove one of each wall with the hammer. | Full cost back: 2, 1, 1, 3, 2 Wood. |
| ⬜ | ⬜ | 13 | Place each triangular floor on the ground; walk round it and look at the cut edge from the side and above. | Half a 1x1 floor, cut corner to corner; the cut edge is closed (wood, no see-through gaps); (1) and (2) have their planks along different short sides. |
| ⬜ | ⬜ | 14 | **Shelf test:** make a 1x1 floor shelf halfway up a Quan's Wood Wall (your usual way), then snap a triangle to the shelf's end with its long edge to the wall. | It snaps at the shelf's corners; the 45° edge meets the wall, and nothing pokes out of the wall's back. |
| ⬜ | ⬜ | 15 | Walk onto a triangle on the ground and step off its long edge; then walk through the missing half, on the ground and past a triangle on a shelf at chest height; drop an item on it. | You stand on the triangle; the missing half doesn't block you anywhere (no invisible corner); the item rests on the triangle. |
| ⬜ | ⬜ | 16 | Damage a triangle to worn and broken; walk away until it switches to its distant look; remove it. | Worn, broken and distant looks are triangles too; 1 Wood back. |
| ⬜ | ⬜ | 17 | Place a Quan's Item Stand on a wall beside a vanilla one; look from the side. | Only the nub shows, flush on the wall; no backplate. |
| ⬜ | ⬜ | 18 | Attach the same items to both: a weapon, a shield and a trophy. Look from the side. | Each item sits in the same pose as on vanilla's, 5 cm nearer the wall, and hides the nub; nothing sinks into the wall. |
| ⬜ | ⬜ | 19 | Same with the floor stand beside a vanilla floor stand, on a floor and on a 1x1 shelf. | Only the nub shows, on the surface; items sit 2.5 cm lower than on vanilla's, resting on or just above the surface, not sunk into it. |
| ⬜ | ⬜ | 20 | Aim at each bare stand, and at one holding an item; use it to take the item off. | Hover text appears without fiddly aiming; the item comes off and drops in front, as vanilla. |
| ⬜ | ⬜ | 21 | Damage a stand until it looks worn; remove both with the hammer. | Worn look is a nub alone too; vanilla's cost back. |

## Eating troughs

Status: idea, 2 Nov 2026 (Quandru). Nothing built.

### What Quandru asked for

- A **feeding trough** piece for animal pens: **one piece, part food trough and part water trough** (agreed 2 Nov
  2026).
- **Look:** a **wooden food trough with a metal bucket** for the water. One size, **2 to 3 m long** (agreed 2 Nov
  2026).
- **Built at the Workbench** (agreed 2 Nov 2026).
- It works as a **plain chest** and holds items. That's the point of cloning the chest: a vanilla `Container` is
  what other mods look for when they feed animals from chests, so the trough works with them. Without a feeding mod
  it's simply a display piece that stores things.
- A **clone of the vanilla early wooden chest** (`piece_chest_wood`; name to confirm in game), keeping its cost of
  **10 Wood**.
- Visuals of a feeding trough instead of a chest. Quandru will **kitbash** the look later.
- **Empty or full together:** both halves look empty while the chest is empty. As soon as it holds anything, both
  show full: feed in the food half, water in the water half (agreed 2 Nov 2026; see below).

### Scope: pieces only

QuanPieces is a pieces mod, not a gameplay mod. It adds prefabs and nothing else: **no patches to vanilla
behaviour**. The troughs are plain chests with a different look. Animals eating from them is left to other mods.

### Points to know before building

- **Vanilla tames don't eat from containers.** `MonsterAI.FindClosestConsumableItem` only finds food items lying
  on the ground (the `item` layer) within `m_consumeSearchRange` (5 m by default). On their own, the troughs are
  storage that looks like a trough, until a feeding mod uses it.
- **No mod on the server feeds from chests yet.** Checked the Default Gale profile (74 mods) on 2 Nov 2026: nothing
  touches tame hunger and containers together. OttoFuel (the auto-fuel mod) only refuels fireplaces, smelters,
  ovens and ballistas from nearby chests; LetMeTameYou uses the vanilla ground-eating code. Feeding from chests came
  from **BetterTames**, which was removed; **QuanCompanions** takes it over (its feature list, A5.1 to A5.4, with a
  note about these troughs). Test the troughs with it (see Testing).
- **The "full" look stays within scope.** A small component on our own piece shows both fill models (grain or hay
  in the food half, a water surface in the water half) when the trough's own inventory has anything in it, and hides
  both when it's empty. It reads only its own container and patches nothing. The water is purely a look: nothing
  in the chest has to be water.
- **The chest's lid.** The vanilla chest opens and closes a lid (`Container` swaps its `m_open` and `m_closed`
  objects). A trough has no lid, so the clone needs both pointed at the trough model, or the open/closed objects
  removed, or the lid will still pop up.
- **Clone audit.** As for every cloned vanilla prefab, check the tags, layers and materials the clone inherits from the chest (collider layer for hovering, the
  `Container` name and size, guard-stone check, place/destroy effects).
- **Name and description** in the usual QuanPieces style, so the build card says what it is (for example
  "Quan's Feeding Trough", "Holds feed for your animals.").

### Suggestions (for Quandru to pick from)

1. **Kitbash parts to try** (prefab names to confirm in game):
   - **Trough:** wood planks or `wood_beam` cubes for the sides and ends, `wood_pole` for the legs.
   - **Food fill:** the straw from `wood_roof` (already used by the chicken coop) or barley meshes.
   - **Metal bucket:** a scaled-down cauldron (`piece_cauldron`) or the iron bands of a barrel; the iron material
     from `iron_grate` or the iron walls (already used by the coop and flue) for a plain metal look.
   - **Water fill:** the water surface in the hot tub (`piece_bathtub`), scaled to the top of the bucket.
2. **Snapping end to end**, so a row of troughs lines a pen neatly. Cheap to add with snap points at each end.

### Testing (beyond the usual build and placement checks)

- **OttoFuel:** it pulls fuel and ore from any placed chest in range (5 m for fireplaces, 15 m for smelters). Check
  it treats a trough like any other chest, and doesn't empty a trough next to a fire or smelter unexpectedly.
- **AzuAutoStore:** it moves items lying on the ground into nearby chests. Food dropped in a pen for animals may get
  pulled into a trough (or any nearby chest). Check what it does near a trough.
- **AzuCraftyBoxes:** it crafts from nearby chests, so trough contents may be used for crafting nearby. Check.
- **QuanCompanions feeding (A5):** once built, check tames eat from the troughs like any player-built chest. It will
  look for a vanilla `Container`, which the clone keeps.
- **The full look:** both halves fill on placing an item and empty on taking the last one, including when a feeding
  mod takes it. Right for every player after a relog (it must follow the synced inventory, not one player's actions).

### Open questions

- Exact length (2 or 3 m) and inventory size: settle when kitbashing.
- Cost: 10 Wood like the chest, or a little iron for the bucket?

## Tar and Clay extractors

Status: idea, 6 Nov 2026 (Quandru). Nothing built or designed yet.

### What Quandru asked for

Two extractor pieces: one for tar and one for clay.

### Open questions

- How each works: placed on a source (a tar pit, a clay deposit) and producing over time, like vanilla's sap
  collector, or something else.
- Where clay comes from in the current game and modpack, so the extractor fits it.
- Cost, crafting station, output rate and capacity, and whether they need fuel.
