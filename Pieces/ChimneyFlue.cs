using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using QuanPieces.Meshes;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// A 2x2 flue that keeps rain off a fire below it without stopping the fire's smoke.
    /// Clones the vanilla iron cage floor for its collider, snapping and piece setup, and replaces its bars with a
    /// stone frame holding angled louvres.
    ///
    /// Why it works, using only vanilla behaviour (no patches):
    /// - A fire is "wet" (low flame: no heat, no cooking, no smoke) when Cover.IsUnderRoof's thin sphere cast
    ///   straight up from the fire hits nothing. Valheim's physics settings have queriesHitTriggers on,
    ///   so a trigger collider on the piece layer counts as a roof.
    /// - Smoke puffs are rigidbodies about 0.84 m across that collide with pieces, but never with triggers,
    ///   so a trigger covering the tile lets smoke straight through.
    /// - Fireplace counts anything (triggers included) within about 0.5 m above its cover check point
    ///   as a blockage, so the flue has to sit roughly 1.5 m or more above the fire.
    ///
    /// WearNTear ignores triggers when working out support, so thin solid strips along the edges give it
    /// something to rest on walls and neighbouring tiles with, and give the hammer something to hit,
    /// while leaving the opening clear. Tiles snap together for wider chimneys.
    /// </summary>
    internal static class ChimneyFlue
    {
        private const string PrefabName = "Quan_chimney_flue";
        private const string BasePrefab = "iron_floor_2x2";      // vanilla iron cage floor
        private const string StoneSource = "stone_floor_2x2";    // its texture goes on the bars, its sounds on the piece
        private const string PieceShader = "Custom/Piece";       // snow overlays use a different shader and keep theirs

        private const float HalfTile = 1f;
        private const float StripWidth = 0.1f;
        private const float StripHeight = 0.1f;

        // Visual side plates and louvres. Their top sits just above the snap points and they hang down from there.
        // They stay within the cage floor's 2 m footprint, so they never show through the walls of a 2x2 chimney.
        // Nothing visual has a collider, so none of it affects smoke or the fire's checks.
        private const float FrameWidth = 0.08f;
        private const float FrameTop = 0.12f;     // height of the plates' and slats' tops above the snap points
        private const int SlatCount = 6;
        private const float SlatAngle = 40f;      // degrees from horizontal
        private const float SlatChord = 0.28f;    // width across the slat
        private const float SlatThickness = 0.04f;

        // Opaque regions of the stone floor's texture atlas, taken from the stone floor's own UVs. The rest of the
        // atlas belongs to other pieces and is partly transparent, which the piece shader cuts out as holes.
        private static readonly Rect StoneEdgeBlocks = new Rect(0.01f, 0.01f, 0.98f, 0.11f); // its side strip, 4 blocks long
        private static readonly Rect StoneTile = new Rect(0.01f, 0.145f, 0.24f, 0.24f);      // one square of its top face

        public static void Register()
        {
            var basePrefab = PrefabManager.Instance.GetPrefab(BasePrefab);
            var basePiece = basePrefab ? basePrefab.GetComponent<Piece>() : null;
            if (basePiece == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"Base prefab '{BasePrefab}' not found or has no Piece component; chimney flue skipped.");
                return;
            }

            var stonePrefab = PrefabManager.Instance.GetPrefab(StoneSource);
            var stoneMaterial = FindPieceMaterial(stonePrefab);
            if (stoneMaterial == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"No '{PieceShader}' material found on '{StoneSource}'; chimney flue keeps the iron look.");
            }

            var config = new PieceConfig
            {
                Name = "Quan's Chimney Flue",
                Description = "Blocks rain but not smoke. Must be 1.5m or more above a fireplace. For Valyyn.",
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                // Workbench rather than the stonecutter: it's rough stone bars, not cut stone.
                CraftingStation = CraftingStations.Workbench,
                Requirements = new[] { new RequirementConfig("Stone", 5, 0, true) }
            };

            // CustomPiece(name, baseName, config) deep-clones the vanilla prefab; the clone is then reshaped in place.
            var customPiece = new CustomPiece(PrefabName, BasePrefab, config);
            if (!Reshape(customPiece.PiecePrefab, stoneMaterial))
            {
                return;
            }
            UseStoneEffects(customPiece.PiecePrefab, stonePrefab);

            // The cloned icon shows the iron cage; render one from the reskinned prefab instead.
            var icon = RenderManager.Instance.Render(customPiece.PiecePrefab, RenderManager.IsometricRotation);
            if (icon != null)
            {
                customPiece.Piece.m_icon = icon;
            }
            else
            {
                QuanPiecesPlugin.Log.LogWarning("Chimney flue: icon render failed; using the iron cage floor's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Reshape(GameObject prefab, Material stoneMaterial)
        {
            // The cage floor's only collider is a solid 2 x 0.1 x 2 slab on the root, which would cap the chimney.
            // As a trigger it still blocks the fire's roof check but no longer stops smoke.
            var roof = prefab.GetComponent<BoxCollider>();
            if (roof == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}' has no root BoxCollider any more; chimney flue skipped.");
                return false;
            }
            roof.isTrigger = true;

            // The cage floor is tagged "leaky" because rain falls through its bars, and Cover.IsUnderRoof skips
            // leaky colliders. Clear the tag (as on the vanilla wood floor) so the slab counts as a roof.
            prefab.tag = "Untagged";

            AddSupportStrips(prefab, roof);
            if (!ReplaceVisual(prefab, stoneMaterial))
            {
                return false;
            }

            // Wood for support purposes, despite the stone look: stone needs at least 100 support, which wooden
            // walls can never supply, so a stone flue would fall out of a wooden chimney. Wood needs only 10,
            // which both wood and stone walls provide.
            var wearNTear = prefab.GetComponent<WearNTear>();
            if (wearNTear != null)
            {
                wearNTear.m_materialType = WearNTear.MaterialType.Wood;
            }

            return true;
        }

        /// <summary>
        /// The clone keeps the cage floor's iron sounds and particles. Swap in the stone floor's for placing,
        /// hitting and destroying, so the piece sounds like what it looks like.
        /// </summary>
        private static void UseStoneEffects(GameObject prefab, GameObject stonePrefab)
        {
            var stonePiece = stonePrefab ? stonePrefab.GetComponent<Piece>() : null;
            var stoneWear = stonePrefab ? stonePrefab.GetComponent<WearNTear>() : null;
            if (stonePiece == null || stoneWear == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{StoneSource}' has no Piece or WearNTear; chimney flue keeps the iron sounds.");
                return;
            }

            prefab.GetComponent<Piece>().m_placeEffect = stonePiece.m_placeEffect;

            var wearNTear = prefab.GetComponent<WearNTear>();
            if (wearNTear != null)
            {
                wearNTear.m_hitEffect = stoneWear.m_hitEffect;
                wearNTear.m_destroyedEffect = stoneWear.m_destroyedEffect;
                wearNTear.m_switchEffect = stoneWear.m_switchEffect;
            }
        }

        /// <summary>Four thin solid strips along the tile's edges, flush with the trigger slab.</summary>
        private static void AddSupportStrips(GameObject prefab, BoxCollider roof)
        {
            var inset = HalfTile - StripWidth / 2f;
            var length = HalfTile * 2f;
            AddStrip(prefab, roof, "support_north", new Vector3(0f, 0f, inset), new Vector3(length, StripHeight, StripWidth));
            AddStrip(prefab, roof, "support_south", new Vector3(0f, 0f, -inset), new Vector3(length, StripHeight, StripWidth));
            AddStrip(prefab, roof, "support_east", new Vector3(inset, 0f, 0f), new Vector3(StripWidth, StripHeight, length));
            AddStrip(prefab, roof, "support_west", new Vector3(-inset, 0f, 0f), new Vector3(StripWidth, StripHeight, length));
        }

        private static void AddStrip(GameObject prefab, BoxCollider roof, string name, Vector3 position, Vector3 size)
        {
            var strip = new GameObject(name) { layer = prefab.layer };
            strip.transform.SetParent(prefab.transform, false);
            strip.transform.localPosition = roof.center + position;
            strip.AddComponent<BoxCollider>().size = size;
        }

        /// <summary>
        /// Swaps the cage's bars for a stone frame and louvres, as the only visual. WearNTear swaps between New,
        /// Worn and Broken as the piece takes damage; pointing all three at New keeps one look throughout
        /// (SetHealthVisual explicitly allows them to be the same object).
        /// </summary>
        private static bool ReplaceVisual(GameObject prefab, Material stoneMaterial)
        {
            var wearNTear = prefab.GetComponent<WearNTear>();
            var visual = prefab.transform.Find("New");
            if (wearNTear == null || visual == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Chimney flue: couldn't find WearNTear or the 'New' visual; chimney flue skipped.");
                return false;
            }

            // Fall back to the cage's own material if the stone one wasn't found.
            var baseMaterial = stoneMaterial;
            if (baseMaterial == null)
            {
                var cageRenderer = visual.GetComponentInChildren<MeshRenderer>(true);
                baseMaterial = cageRenderer ? cageRenderer.sharedMaterial : null;
            }
            var material = WithoutRipple(baseMaterial);

            // Worn and Broken go; so does Destruction, the cage's pre-cut debris. With no fragment roots,
            // WearNTear breaks the visible meshes (the louvres) into debris instead.
            foreach (var unused in new[] { "Worn", "Broken", "Destruction" })
            {
                var child = prefab.transform.Find(unused);
                if (child != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
            wearNTear.m_fragmentRoots = null;

            var oldParts = new List<GameObject>();
            foreach (Transform child in visual)
            {
                oldParts.Add(child.gameObject);
            }
            oldParts.ForEach(Object.DestroyImmediate);

            var renderers = BuildFrameAndLouvres(visual, material);

            // The cage's LODGroup still lists its old renderers. Replace it with one level holding the new parts,
            // culled at the same distance as the cage's last level.
            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                var cullHeight = lods.Length > 0 ? lods[lods.Length - 1].screenRelativeTransitionHeight : 0.02f;
                lodGroup.SetLODs(new[] { new LOD(cullHeight, renderers.ToArray()) });
                lodGroup.RecalculateBounds();
            }

            wearNTear.m_new = visual.gameObject;
            wearNTear.m_worn = visual.gameObject;
            wearNTear.m_broken = visual.gameObject;
            return true;
        }

        /// <summary>
        /// Two side plates running corner to corner along the east and west edges of the 2 m footprint, holding
        /// slats tilted about their length like louvres. The north and south sides are open, so tiles placed
        /// north-south of each other form one continuous run of louvres, and tiles placed east-west meet plate to
        /// plate with no gap.
        /// </summary>
        private static List<Renderer> BuildFrameAndLouvres(Transform parent, Material material)
        {
            var renderers = new List<Renderer>();
            var outer = HalfTile * 2f;
            var inner = outer - FrameWidth * 2f;
            var plateOffset = HalfTile - FrameWidth / 2f;

            var top = FrameTop;
            var angle = SlatAngle * Mathf.Deg2Rad;
            var slatDepth = SlatChord * Mathf.Sin(angle) + SlatThickness * Mathf.Cos(angle); // vertical extent
            var bottom = top - slatDepth;

            // East and west: plates the full depth of the louvres and the full 2 m length of the tile.
            var sidePlate = BoxMesh.Create("quan_flue_side_plate", new Vector3(FrameWidth, top - bottom, outer), StoneEdgeBlocks, StoneTile);
            var plateY = (top + bottom) / 2f;
            AddPart(parent, "frame_east", sidePlate, material, new Vector3(plateOffset, plateY, 0f), Quaternion.identity, renderers);
            AddPart(parent, "frame_west", sidePlate, material, new Vector3(-plateOffset, plateY, 0f), Quaternion.identity, renderers);

            // Slats span between the plates, each centred in an equal share of the tile's length. The gap at each
            // open edge is then half the gap between slats, so tiles placed edge to edge space out like one long piece.
            var slat = BoxMesh.Create("quan_flue_slat", new Vector3(inner, SlatThickness, SlatChord), StoneEdgeBlocks, StoneTile);
            var pitch = outer / SlatCount;
            var tilt = Quaternion.Euler(SlatAngle, 0f, 0f);
            var slatY = top - slatDepth / 2f;
            for (var i = 0; i < SlatCount; i++)
            {
                var z = -HalfTile + pitch * (i + 0.5f);
                AddPart(parent, $"louvre_{i + 1}", slat, material, new Vector3(0f, slatY, z), tilt, renderers);
            }

            return renderers;
        }

        private static void AddPart(Transform parent, string name, Mesh mesh, Material material,
                                    Vector3 position, Quaternion rotation, List<Renderer> renderers)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderers.Add(renderer);
        }

        /// <summary>
        /// A copy of <paramref name="material"/> with the piece shader's vertex ripple turned off. Stone pieces
        /// wobble each vertex by up to _RippleDistance (0.1 m on the stone floor) based on its world position, for
        /// a hand-built look. Our boxes don't share vertices where they meet, so the wobble pulls the joins apart.
        /// The game itself zeroes it on placement ghosts and debris.
        /// </summary>
        private static Material WithoutRipple(Material material)
        {
            if (material == null || !material.HasProperty("_RippleDistance"))
            {
                return material;
            }

            var copy = new Material(material) { name = material.name + "_quan_noripple" };
            copy.SetFloat("_RippleDistance", 0f);
            return copy;
        }

        private static Material FindPieceMaterial(GameObject source)
        {
            if (source == null)
            {
                return null;
            }

            return source.GetComponentsInChildren<MeshRenderer>(true)
                .Select(r => r.sharedMaterial)
                .FirstOrDefault(m => m != null && m.shader.name == PieceShader);
        }
    }
}
