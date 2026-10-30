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
    /// A 2x2 wooden gate: the vanilla 2x3 Iron Gate made 1 m shorter and remade in wood, with iron fittings.
    ///
    /// Clones the Iron Gate (its Door and hinge animation) and reshapes its meshes rather than stretching them:
    /// - the gate's frame: the top bar drops 1 m and the uprights shorten to match;
    /// - the swinging leaf: its outer frame and vertical bars lose 1 m from the middle (the rails at their ends
    ///   keep their thickness), its hinges keep their shape near the top and bottom, and the two cross bars with
    ///   the latch between them move together, unchanged, to the middle of the shorter leaf.
    /// The frame, the leaf's outer frame and its vertical bars are wood; the cross bars, latch and hinges stay
    /// iron. It has the wood door's support, sounds and effects (<see cref="CoopWood"/>).
    /// </summary>
    internal static class ChickenCoopDoor
    {
        private const string PrefabName = "Quan_chicken_coop_door";
        private const string BasePrefab = "iron_grate"; // vanilla Iron Gate, 2x3

        private const string Description =
            "A 2x2 gate, just the right size for a coop. Keep Hen Solo and Cluckbacca safe from meddling wolves " +
            "and other predators. For Valyyn.";

        private const float Shorten = 1f;       // metres taken off the gate's height
        private const float SplitHeight = 1.5f; // frame parts above this (in their mesh space) move down
        private const float EndBand = 0.1f;     // leaf: this much at each end keeps its shape (the rails)

        public static void Register()
        {
            if (PrefabManager.Instance.GetPrefab(BasePrefab) == null || !CoopWood.TryGetMaterials(out var wood, out _))
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}' or the wood pole's material not found; chicken coop door skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = "Quan's Chicken Coop Door",
                Description = Description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = CoopWood.Station,
                Requirements = CoopWood.Cost
            };

            var customPiece = new CustomPiece(PrefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, wood))
            {
                return;
            }
            CoopWood.MakeWooden(customPiece.PiecePrefab);

            var icon = RenderManager.Instance.Render(customPiece.PiecePrefab, RenderManager.IsometricRotation);
            if (icon != null)
            {
                customPiece.Piece.m_icon = icon;
            }
            else
            {
                QuanPiecesPlugin.Log.LogWarning("Chicken coop door: icon render failed; using the Iron Gate's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, Material wood)
        {
            var door = prefab.transform.Find("door");
            var frame = prefab.transform.Find("New");
            var leaf = door ? door.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault() : null;
            if (door == null || frame == null || leaf == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Chicken coop door: the Iron Gate's door or frame wasn't found; skipped.");
                return false;
            }

            var fittings = ShortenLeaf(leaf, wood);
            if (fittings == null)
            {
                return false;
            }

            // The gate's frame: the top bar moves down, the uprights' tops come down with it. All wood.
            foreach (var part in frame.GetComponentsInChildren<MeshFilter>(true))
            {
                var data = MeshData.From(part.sharedMesh);
                if (data == null)
                {
                    QuanPiecesPlugin.Log.LogWarning($"Chicken coop door: '{part.name}' can't be read; skipped.");
                    return false;
                }
                var shift = Shorten / part.transform.lossyScale.y;
                for (var i = 0; i < data.Positions.Count; i++)
                {
                    var p = data.Positions[i];
                    if (p.y > SplitHeight)
                    {
                        data.Positions[i] = new Vector3(p.x, p.y - shift, p.z);
                    }
                }
                var mesh = WoodGrain.Rewrap(data).ToMesh(part.sharedMesh.name + "_quan_2x2");
                part.sharedMesh = mesh;
                var renderer = part.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = wood;
                }
                var box = part.GetComponent<BoxCollider>();
                if (box != null)
                {
                    box.center = mesh.bounds.center;
                    box.size = mesh.bounds.size;
                }
            }

            // The swinging leaf's collider loses the same height from its top.
            var doorBox = door.GetComponent<BoxCollider>();
            if (doorBox != null)
            {
                var size = doorBox.size;
                var centre = doorBox.center;
                size.y -= Shorten;
                centre.y -= Shorten / 2f;
                doorBox.size = size;
                doorBox.center = centre;
            }

            // Snap points along the top come down to the new top.
            foreach (Transform child in prefab.transform)
            {
                if (child.CompareTag("snappoint") && child.name.Contains("top"))
                {
                    child.localPosition -= new Vector3(0f, Shorten, 0f);
                }
            }

            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                for (var i = 0; i < lods.Length; i++)
                {
                    if (lods[i].renderers.Contains(leaf.GetComponent<Renderer>()))
                    {
                        lods[i].renderers = lods[i].renderers.Concat(new[] { fittings }).ToArray();
                    }
                }
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }
            return true;
        }

        /// <summary>
        /// Shortens the leaf mesh by <see cref="Shorten"/> without squashing its details, and splits it into its
        /// wooden frame and bars (kept on the leaf) and its iron fittings (a new renderer beside it, returned).
        /// In the mesh's own space the leaf stands from y = 0 to its height; its parts are separate islands.
        /// </summary>
        private static Renderer ShortenLeaf(MeshFilter leaf, Material wood)
        {
            var data = MeshData.From(leaf.sharedMesh);
            if (data == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Chicken coop door: the gate leaf mesh can't be read; skipped.");
                return null;
            }

            var height = data.Positions.Max(p => p.y);
            var shift = Shorten / leaf.transform.lossyScale.y; // the leaf mesh is scaled up in the prefab
            var newHeight = height - shift;
            var middle = height / 2f;

            // Long parts (outer frame, vertical bars): the middle section shortens, the ends keep their shape.
            float Remap(float y)
            {
                if (y <= EndBand)
                {
                    return y;
                }
                if (y >= height - EndBand)
                {
                    return y - shift;
                }
                return EndBand + (y - EndBand) * (newHeight - 2f * EndBand) / (height - 2f * EndBand);
            }

            var longParts = data.KeepIslands(b => b.size.y > middle, out _);
            for (var i = 0; i < longParts.Positions.Count; i++)
            {
                var p = longParts.Positions[i];
                longParts.Positions[i] = new Vector3(p.x, Remap(p.y), p.z);
            }

            // Hinges, near each end: moved, not reshaped.
            var lowerHinge = data.KeepIslands(b => b.size.y <= middle && b.center.y < height * 0.3f, out var lowerBounds);
            var upperHinge = data.KeepIslands(b => b.size.y <= middle && b.center.y > height * 0.7f, out var upperBounds);
            Move(lowerHinge, lowerBounds, Remap);
            Move(upperHinge, upperBounds, Remap);

            // The cross bars and latch: moved together, unchanged, to the middle of the shorter leaf.
            var crossbars = data.KeepIslands(b => b.size.y <= middle && b.center.y >= height * 0.3f && b.center.y <= height * 0.7f,
                                             out var crossBounds);
            if (crossBounds.Count > 0)
            {
                Translate(crossbars, newHeight / 2f - Union(crossBounds).center.y);
            }

            var leafRenderer = leaf.GetComponent<MeshRenderer>();
            var iron = leafRenderer.sharedMaterials;

            // Wood: the leaf's outer frame and vertical bars, on the leaf itself.
            leaf.sharedMesh = WoodGrain.Rewrap(longParts).ToMesh(leaf.sharedMesh.name + "_quan_wood");
            leafRenderer.sharedMaterial = wood;

            // Iron: the hinges, cross bars and latch, on a new object in the same place.
            var fittings = new GameObject("fittings");
            fittings.transform.SetParent(leaf.transform, false);
            fittings.layer = leaf.gameObject.layer;
            var metal = new MeshData()
                .Append(lowerHinge, Matrix4x4.identity)
                .Append(upperHinge, Matrix4x4.identity)
                .Append(crossbars, Matrix4x4.identity);
            fittings.AddComponent<MeshFilter>().sharedMesh = metal.ToMesh("Quan_chicken_coop_door_fittings");
            var renderer = fittings.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = iron;
            return renderer;
        }

        private static void Move(MeshData part, List<Bounds> bounds, System.Func<float, float> remap)
        {
            if (bounds.Count == 0)
            {
                return;
            }
            var centre = Union(bounds).center.y;
            Translate(part, remap(centre) - centre);
        }

        private static Bounds Union(List<Bounds> bounds)
        {
            var union = bounds[0];
            foreach (var b in bounds)
            {
                union.Encapsulate(b);
            }
            return union;
        }

        private static void Translate(MeshData part, float dy)
        {
            for (var i = 0; i < part.Positions.Count; i++)
            {
                var p = part.Positions[i];
                part.Positions[i] = new Vector3(p.x, p.y + dy, p.z);
            }
        }
    }
}
