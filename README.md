# QuanPieces

A BepInEx/Jotunn Valheim mod adding small custom building pieces. Each piece lives in its own class under
`Pieces/`. The player-facing description is `Package/README.md`. Build and release steps are in
`docs/DEVELOPMENT.md`.

## Pieces

### Quan's Chimney Flue (`Quan_chimney_flue`)

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
- `Fireplace` treats anything, triggers included, within about 0.5 m above its cover check point as a
  blockage, hence the 1.5 m placement rule.
