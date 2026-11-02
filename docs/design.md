# QuanPieces: design (pieces to come)

Pieces planned for QuanPieces that aren't built yet. Built pieces are described in the [README](../README.md);
the game code each one relies on is in [valheim-dependencies.md](valheim-dependencies.md). When a piece here is
built, move its description to the README and remove it from this file.

## Eating troughs

Status: idea, 2 Nov 2026 (Quandru). Nothing built.

### What Quandru asked for

- One or more **eating trough** pieces for animal pens.
- Each works as a **plain chest**: a container, nothing more.
- Probably a **clone of the vanilla early wooden chest** (`piece_chest_wood`; name to confirm in game), keeping its
  cost of **10 Wood**.
- Visuals of a feeding trough instead of a chest. Quandru will **kitbash** the look later.
- **A two-part set:** a **food trough** and a **water trough** that sit side by side (agreed 2 Nov 2026).
- **The trough looks full when it has something in it** (agreed 2 Nov 2026; see below).

### Scope: pieces only

QuanPieces is a pieces mod, not a gameplay mod. It adds prefabs and nothing else: **no patches to vanilla
behaviour**. The troughs are plain chests with a different look. Animals eating from them is left to other mods.

### Points to know before building

- **Vanilla tames don't eat from containers.** `MonsterAI.FindClosestConsumableItem` only finds food items lying
  on the ground (the `item` layer) within `m_consumeSearchRange` (5 m by default). On their own, the troughs are
  storage that looks like a trough.
- **No mod on the server feeds from chests yet.** Checked the Default Gale profile (74 mods) on 2 Nov 2026: nothing
  touches tame hunger and containers together. OttoFuel (the auto-fuel mod) only refuels fireplaces, smelters,
  ovens and ballistas from nearby chests; LetMeTameYou uses the vanilla ground-eating code. If a feeding mod is
  added, test it with the troughs (see Testing).
- **The "full" look stays within scope.** A small component on our own piece that shows or hides a fill model
  (grain or hay in the food trough, a water surface in the water trough) when the trough's own inventory changes.
  It reads only its own container and patches nothing.
- **The chest's lid.** The vanilla chest opens and closes a lid (`Container` swaps its `m_open` and `m_closed`
  objects). A trough has no lid, so the clone needs both pointed at the trough model, or the open/closed objects
  removed, or the lid will still pop up.
- **Clone audit.** As for every cloned vanilla prefab, check the tags, layers and materials the clone inherits from the chest (collider layer for hovering, the
  `Container` name and size, guard-stone check, place/destroy effects).
- **Name and description** in the usual QuanPieces style, so the build card says what it is (for example
  "Quan's Feeding Trough", "Holds feed for your animals.").

### Suggestions (for Quandru to pick from)

1. **Two sizes.** A short trough (1x2 m, chest-sized inventory, 10 Wood) and a long one (4 m, which snaps end to end
   for long pens, a bigger inventory, around 20 Wood). Long rows of troughs suit pens better than single boxes.
2. **The pair as one piece or two.** Either a single piece with food on one side and water on the other, or two
   pieces that snap side by side. Two pieces are more flexible (water only in some pens); one piece is quicker to place.
3. **Kitbash parts to try.** Wood planks or `wood_beam` cubes for the sides and ends, `wood_pole` for the legs,
   the straw from `wood_roof` (already used by the chicken coop) or barley meshes for the food fill, and a water
   surface material for the water fill.

### Testing (beyond the usual build and placement checks)

- **OttoFuel:** it pulls fuel and ore from any placed chest in range (5 m for fireplaces, 15 m for smelters). Check
  it treats a trough like any other chest, and doesn't empty a trough next to a fire or smelter unexpectedly.
- **AzuAutoStore:** it moves items lying on the ground into nearby chests. Food dropped in a pen for animals may get
  pulled into a trough (or any nearby chest). Check what it does near a trough.
- **AzuCraftyBoxes:** it crafts from nearby chests, so trough contents may be used for crafting nearby. Check.
- **Any feeding mod added later:** check it finds the troughs. Most look for a vanilla `Container`, which the clone
  keeps.
- **The full look:** shows on placing an item, hides on taking the last one, and is right for every player after a
  relog (it must follow the synced inventory, not one player's actions).

### Open questions

- One size or two (or more)?
- Food and water as one piece or two?
- Which crafting station: none (like the chest) or the Workbench?
- Does the water trough hold anything, or is it a decorative piece with no inventory?
