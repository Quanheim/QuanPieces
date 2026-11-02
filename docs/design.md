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

### Points to know before building

- **Vanilla tames don't eat from containers.** `MonsterAI.FindClosestConsumableItem` only finds food items lying
  on the ground (the `item` layer) within `m_consumeSearchRange` (5 m by default). So a plain-chest trough is storage
  that looks like a trough; animals won't eat from it. That's fine for version 1 if it's what Quandru wants. Feeding
  from the trough would be a later option (see suggestions below).
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
2. **Animals feed from it (later, optional).** A small patch so tames within range of a trough eat suitable food
   straight from its inventory, the way they would from the ground. It would make the trough actually useful, and
   stop food rolling around the pen. Needs care: server and owner rules for who removes the item, and only food the
   animal can eat (`MonsterAI.CanConsume`). Could be a config switch, off by default.
3. **A look that shows it's full.** A layer of grain or hay inside the trough that shows when the inventory has
   anything in it, and hides when it's empty. A cheap visual cue, done by checking the inventory when it changes.
4. **Kitbash parts to try.** Wood planks or `wood_beam` cubes for the sides and ends, `wood_pole` for the legs,
   and the straw from `wood_roof` (already used by the chicken coop) or barley meshes for the fill.
5. **A water trough twin.** Same model with a water surface instead of feed. Decorative only, but it makes a pen
   look complete.

### Open questions

- One size or two (or more)?
- Plain storage only, or animals feeding from it later?
- Which crafting station: none (like the chest) or the Workbench?
