# Valheim Code Dependency

Everything this mod relies on from Valheim, so a game update can be checked quickly. `check-mods.ps1` in the
`.decompiles` repo reads the **Game code** tables and flags any entry the game changed. See "Valheim updates" in
`docs/DEVELOPMENT.md` for the whole check.

**Checked against Valheim:** 1.0.21 (2026-11-02)

Update this file in the same commit as any code that patches, calls or relies on something new in the game, and
update the version line above each time the mod passes the Valheim update check.

## Game code

The first column holds the game class or member in backticks, as `Class` or `Class.Member`. Nested types use their
parent's name, e.g. `Player.PlacementStatus`. Only the first column is read by the script; the rest is for people.

### Harmony patches

What the mod patches. A changed signature stops the patch applying; changed logic can make it wrong.

| Game code | Patch | What it does | If the game changes it |
|---|---|---|---|
| `Heightmap.IsCultivated` | Postfix | Planter soil counts as cultivated ground. | Check it still takes a world position and is what seed placement and plant growth ask. |
| `Player.PieceRayTest` | Postfix | Aiming the cultivator at planter soil reports the soil point as terrain. | Signature (`out` point, normal, piece, heightmap) and its use by placement. |
| `WearNTear.Highlight` | Prefix | Skips the support tint on a planter while planting into its soil. | Only cosmetic; check it still exists. |
| `Player.UpdatePlacementGhost` | Postfix | Turns WrongBiome and DeepSnow into Valid for seeds over planter soil. | Placement status rules and the `PlacementStatus` values. |
| `Plant.UpdateHealth` | Prefix and Finalizer | Plants on soil grow in any biome and ignore heat and cold, by swapping the plant's rules for the call. | Which fields it reads for biome and temperature. |
| `Plant.HaveRoof` | Prefix | Plants on soil grow under a roof. | Still a separate check called from growth. |
| `Plant.HaveGrowSpace` | Prefix | Plants on soil always have room, except vines from other vines. | Layers and radius it checks; vine rule. |
| `StaticPhysics.Fall` | Prefix | Grown crops on soil don't drop to the terrain. | Still the method that drops pieces. |
| `Player.FindHoverObject` | Postfix | Hover passes through see-through coop bars to what's behind. | Signature (`out` hover, hoverCreature) and the interact layers (see `Player.m_interactMask`). |

### Private members reached by reflection

Fields, properties and methods read or called through `AccessTools`, `Traverse` or `System.Reflection`. These fail
at runtime, not compile time, if renamed.

| Game code | Kind | Used for |
|---|---|---|
| `Player.m_placementGhost` | Field | The ghost being placed: is it a seed, is it over soil. |
| `Player.m_placementStatus` | Field | Reading and overriding the placement status. |
| `Player.SetPlacementGhostValid` | Method | Turning the ghost green after overriding the status. |
| `Player.m_buildPieces` | Field | Whether the cultivator's piece table is in use. |
| `Player.m_removeRayMask` | Field | Raycasting the planter soil like the hammer does. |

### Public game code called or read

Methods, fields and components used directly. A rename or signature change breaks the build, which is easy to spot;
a change in behaviour isn't, so list the ones whose behaviour the mod depends on.

| Game code | Used for |
|---|---|
| `Player.PlacementStatus` | WrongBiome, DeepSnow and Valid values. |
| `Player.m_interactMask` | The hover patch copies its layer list; keep them in step. |
| `Player.m_maxInteractDistance` | Hover reach. |
| `Player.m_eye` | Hover reach is measured from it. |
| `Hoverable.GetHoverOffset` | Hover reach. |
| `GameCamera.instance` | Aim ray for soil hits and hover. |
| `Heightmap.FindHeightmap` | Terrain under the soil point. |
| `Heightmap.Biome` | Setting "any biome" on a plant for the health check. |
| `Plant.m_biome` | Swapped for the health check. |
| `Plant.m_tolerateHeat` | Swapped for the health check. |
| `Plant.m_tolerateCold` | Swapped for the health check. |
| `Plant.m_growRadiusVines` | Vine spacing on soil. |
| `Vine` | Finding other vines. |
| `TerrainOp` | Cultivator terrain tools aren't planting. |
| `TerrainModifier` | Cultivator terrain tools aren't planting. |
| `Pickable.GetPicked` | Ripe crops drop their harvest when a planter is removed. |
| `Pickable.m_itemPrefab` | Harvest drop, worked out as vanilla picking does. |
| `Pickable.m_amount` | Harvest drop. |
| `Pickable.m_minAmountScaled` | Harvest drop. |
| `Pickable.m_dontScale` | Harvest drop. |
| `Pickable.m_extraDrops` | Harvest drop. |
| `Game.ScaleDrops` | Harvest drop scaling. |
| `DropTable.GetDropListItems` | Extra harvest drops. |
| `Piece.DropResources` | Refund of things growing in a removed planter. |
| `Piece.m_resources` | Seed refund for seedlings; the plank walls copy the vanilla walls' cost. |
| `Piece.m_craftingStation` | The plank walls use the vanilla walls' crafting station. |
| `Piece.m_placeEffect` | Copied from source pieces. |
| `Piece.m_groundOnly` | Placement rules copied from source pieces (with the other placement flags beside it). |
| `Piece.m_clipEverything` | Placement rules copied from source pieces. |
| `EffectArea` | Coop floor warmth: a Heat area, switched on and off by `CoopFloorWarmth`. |
| `EffectArea.Type` | Heat. |
| `Interactable` | Floor heater on/off switch. |
| `Hoverable` | Floor heater hover text. |
| `PrivateArea.CheckAccess` | A ward stops strangers switching a floor heater. |
| `ZNetView.Register` | Floor heater on/off RPC. |
| `ZNetView.InvokeRPC` | Floor heater on/off RPC. |
| `ZDO.GetBool` | Floor heater on/off state. |
| `Localization.Localize` | Floor heater hover text (assembly_guiutils). |
| `WearNTear.MaterialType` | Street lights, flue and coop pieces use wood support. |
| `WearNTear.m_materialType` | Set to wood. |
| `Fireplace.m_smokeSpawner` | Street lights' smoke. |
| `Fireplace.m_enabledObjectHigh` | Cleared on the lantern street light. |
| `Fireplace.m_enabledObjectLow` | Cleared on the lantern street light. |
| `Fireplace.m_fuelItem` | Lantern burns wisps. |
| `LightLod.m_lightDistance` | Lantern light switch-off distance. |
| `Door.m_openEffects` | Coop door sounds. |
| `Door.m_closeEffects` | Coop door sounds. |
| `ZNetView.ClaimOwnership` | Removing things growing in a destroyed planter. |
| `ZNetScene.Destroy` | Removing things growing in a destroyed planter. |
| `ObjectDB.instance` | Seed refund. |

### Behaviour the mod assumes

Things the game does that the mod counts on without calling them, e.g. "only the owner runs
`Pickable.UpdateRespawn`". These break silently, so name the code where the behaviour lives.

| Game code | Assumption |
|---|---|
| `Plant.UpdateHealth` | Biome and temperature checks read `m_biome`, `m_tolerateHeat` and `m_tolerateCold` during this call only. |
| `Pickable.RPC_Pick` | The planter's harvest refund copies how picking works out its drops; if picking changes, the refund drifts. |
| `Fireplace` | The street lights are working Fireplaces, which is why NoSmokeStayLit (and QuanDupeLightsources) can time them. |
| `StaticPhysics` | Grown crops carry it and it's what drops them to the ground. |
| `EggGrow.CanGrow` | An egg grows only inside a Heat `EffectArea` (and under a roof), and losing heat resets it; that's what the coop floor's switchable warmth and the floor heater rely on. |
| `EffectArea.IsPointInsideArea` | Finds areas with a physics overlap, so a deactivated warmth object stops counting at once. |
| `Bed.CheckFire` | Also accepts a Heat area: a bed on a warm coop floor counts as near a fire. |

## Prefabs and assets

Not game code, so no script can check these. They come from the game's asset bundles: vanilla prefabs the mod
clones or reads, child objects it finds by name, materials, shader properties and layer names. Test them in game
after each update.

| Prefab or asset | Used for | Breaks if |
|---|---|---|
| `piece_brazierceiling01` | Base of both street lights: its Fireplace. Children `New`, `_enabled_high/Point light`, `_enabled_high/FireWarmth`, `collider (5)`, a SmokeSpawner. | Renamed, or those children move. |
| `wood_pole2` | Brazier street light post: mesh, material, placement rules, place effect. | Renamed or re-meshed. |
| `wood_beam` | Brazier street light arm, planter rim, floor heater box: cube mesh, woodwall material. | Renamed. |
| `piece_dvergr_lantern_pole` | Lantern street light: lantern, chain, light, flare (children under `New/`), recipe, `_enabled`. | Renamed or children move. |
| `Piece_grausten_pillarbase_tapered` | Lantern street light base, place effect, support. | Renamed. |
| `Piece_grausten_pillarbase_small` | Lantern street light frame material. | Renamed. |
| `ashwood_pole_2m` | Lantern street light post. | Renamed. |
| `ashwood_arch_big` | Lantern street light arch (`New`). | Renamed. |
| `Wisp` | Lantern street light fuel. | Item renamed. |
| `iron_grate` | Coop door base (`door`, `New`). | Renamed or children move. |
| `wood_floor` | Coop floor base. | Renamed. |
| `wood_roof` | Coop floor straw meshes, material, cost. | Renamed. |
| `fire_pit` | Floor heater: ash bed `New/Quad`, glowing ash `_enabled_high/Quad (1)` (meshes and materials), light `_enabled_low/Point light` (copied). | Renamed or those children move. |
| `Stone` | Floor heater cost. | Item renamed. |
| `iron_wall_2x2` | Coop wall base. | Renamed. |
| `iron_floor_2x2` | Chimney flue base (`New`). | Renamed. |
| `stone_floor_2x2` | Chimney flue texture and sounds. | Renamed. |
| `wood_pole` | Coop and planter wood materials (`New`, `WornBroken`). | Renamed. |
| `woodwall` | Base of Quan's Wood Wall and Wood Wall 2x3. Children `New`, `Worn`, `Broken` (each a `LODGroup`: LOD0 the combined plank-and-beam mesh, LOD1 a slab; `high` holds the unused source boxes), snap points, `beam_2m_snow`; root `BoxCollider`. Beams are the mesh islands under 0.5 m tall. | Renamed, the states or LODs restructured, or the beams no longer separate islands (the piece logs and skips). |
| `wood_wall_half` | Base of Quan's Wood Wall Half. Same layout as `woodwall`. | As `woodwall`. |
| `wood_wall_quarter` | Base of Quan's Wood Wall 1x1 and Wood Wall 1x3. Same layout as `woodwall`, pivot at its foot; all three states share one mesh. | As `woodwall`. |
| `woodwall` material texture | `Planks5c_low` tiles, with the material showing 0.56 of its width along a plank: the 3 m walls' UVs run past 0..1 and rely on it repeating. | The texture stops tiling, or the material's tiling changes (the 3 m walls' grain scale shifts). |
| `wood_door` | Coop wood support, hit/break/place effects, door sounds. | Renamed. |
| `wood_floor_1x1` | Planter and floor heater base: piece setup, plank mesh, woodpole material. Base of the triangular floors: meshes under `New`, `Worn`, `Broken` (cut), `collider` (its box gives the wedge's thickness), corner snap points, `floor_1x1_snow`; cost and station. | Renamed, or `collider` stops being a box. |
| `piece_chest_wood` | *Planned* (eating troughs, see [design.md](design.md)): base of the trough; its `Container`, cost and lid (`m_open`/`m_closed`). Name to confirm in game. | Renamed, or the lid objects change. |
| `Custom/Piece` shader | Properties `_RippleDistance`, `_TriplanarMap`, `_TriplanarScale`, `_TriplanarLocalPos`, `_BumpMap`, `_Glossiness`, `_Cutoff`, keywords `_EMISSION`, `_NORMALMAP`. | Shader or property renamed. |
| Terrain material arrays | `terrain_d_array` and `terrain_n_array` for planter soil. | Renamed. |
| Layers | `piece`, `piece_nonsolid`, `blocker`, `character_trigger`, `Default`, `static_solid`, `Default_small`, and the hover list in `Player.m_interactMask`. | Renamed or reassigned. |
| `_CultivatorPieceTable` | Name prefix of the cultivator's piece table. | Renamed. |
