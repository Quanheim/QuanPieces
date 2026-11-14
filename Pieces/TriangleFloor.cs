using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using QuanPieces.Meshes;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// The vanilla 1x1 wood floor cut in half corner to corner: a small triangle for filling gaps, or for ending a
    /// 1x1 shelf on a wall with a 45° corner that doesn't poke out of the wall's back. Two mirror-image versions,
    /// so the planks can run along either short side.
    /// </summary>
    internal static class TriangleFloor
    {
        private const string BasePrefab = "wood_floor_1x1"; // 1 Wood at a Workbench; two planks running along z

        // Each keeps the half where dot(normal, p) <= 0: the triangle with its right angle at (-0.5, -0.5) for (1),
        // at (0.5, -0.5) for (2). The planks run along z, so they're along one short side in (1), the other in (2).
        public static void Register1() => Register("Quan_wood_floor_triangle_1x1_1", "Quan's Triangular Wood Floor 1x1 (1)", new Vector3(1f, 0f, 1f));
        public static void Register2() => Register("Quan_wood_floor_triangle_1x1_2", "Quan's Triangular Wood Floor 1x1 (2)", new Vector3(-1f, 0f, 1f));

        private const string Description =
            "Half a 1x1 wood floor, cut corner to corner. Fills small triangular gaps, or finishes the end of a 1x1 " +
            "shelf with a 45° corner into the wall behind it.";

        private static void Register(string prefabName, string name, Vector3 cutNormal)
        {
            var basePrefab = PrefabManager.Instance.GetPrefab(BasePrefab);
            var basePiece = basePrefab ? basePrefab.GetComponent<Piece>() : null;
            if (basePiece == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}' not found; {name} skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = name,
                Description = Description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = basePiece.m_craftingStation ? basePiece.m_craftingStation.name : CraftingStations.Workbench,
                Requirements = basePiece.m_resources
                    .Where(r => r != null && r.m_resItem != null)
                    .Select(r => new RequirementConfig(r.m_resItem.name, r.m_amount, r.m_amountPerLevel, r.m_recover))
                    .ToArray()
            };

            var customPiece = new CustomPiece(prefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, name, cutNormal.normalized))
            {
                return;
            }

            var icon = RenderManager.Instance.Render(customPiece.PiecePrefab, RenderManager.IsometricRotation);
            if (icon != null)
            {
                customPiece.Piece.m_icon = icon;
            }
            else
            {
                QuanPiecesPlugin.Log.LogWarning($"{name}: icon render failed; using the 1x1 floor's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, string name, Vector3 cutNormal)
        {
            var root = prefab.transform;
            var colliderObject = root.Find("collider");
            var box = colliderObject ? colliderObject.GetComponent<BoxCollider>() : null;
            if (box == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"{name}: '{BasePrefab}' has no 'collider' box; skipped.");
                return false;
            }

            // Every mesh in every wear state, near and distant: cut in the piece's own space and closed.
            var cutAny = false;
            foreach (var state in new[] { "New", "Worn", "Broken" }.Select(n => root.Find(n)).Where(s => s != null))
            {
                foreach (var filter in state.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null))
                {
                    var data = MeshData.From(filter.sharedMesh);
                    if (data == null)
                    {
                        QuanPiecesPlugin.Log.LogWarning($"{name}: '{filter.sharedMesh.name}' can't be read; skipped.");
                        return false;
                    }
                    var toPiece = root.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    var cut = new MeshData().Append(data, toPiece).Cut(cutNormal, 0f);
                    filter.sharedMesh = new MeshData().Append(cut, toPiece.inverse).ToMesh(filter.sharedMesh.name + "_quan_triangle");
                    cutAny = true;
                }
            }
            if (!cutAny)
            {
                QuanPiecesPlugin.Log.LogWarning($"{name}: '{BasePrefab}' has no New, Worn or Broken meshes; skipped.");
                return false;
            }
            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                lodGroup.RecalculateBounds();
            }

            // The square snow cover would hang over the cut; drop it, and the snap point on the cut-off corner.
            foreach (Transform child in root.Cast<Transform>().ToList())
            {
                var local = root.InverseTransformPoint(child.position);
                var cutCorner = child.name.StartsWith("$hud_snappoint") && Vector3.Dot(cutNormal, local) > 0.01f;
                if (cutCorner || child.name.Contains("snow"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // A triangular wedge collider in place of the square box, as thick as the box.
            var bounds = BoxBoundsInPiece(box, root);
            var corners = new[] { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f) }
                .Where(c => Vector3.Dot(cutNormal, new Vector3(c.x, 0f, c.y)) <= 0.01f)
                .ToArray();
            var wedge = new Mesh { name = "Quan_triangle_floor_collider" };
            wedge.SetVertices(corners.SelectMany(c => new[]
            {
                new Vector3(c.x, bounds.min.y, c.y),
                new Vector3(c.x, bounds.max.y, c.y)
            }).ToList());
            // A convex MeshCollider builds its own hull from the points; the triangles only need to be valid.
            wedge.SetTriangles(new[] { 0, 2, 4, 1, 5, 3, 0, 1, 2, 2, 1, 3, 2, 3, 4, 4, 3, 5, 4, 5, 0, 0, 5, 1 }, 0);
            wedge.RecalculateBounds();

            var wedgeObject = new GameObject("collider") { layer = colliderObject.gameObject.layer };
            wedgeObject.transform.SetParent(root, false);
            var meshCollider = wedgeObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = wedge;
            meshCollider.convex = true;
            meshCollider.sharedMaterial = box.sharedMaterial;
            Object.DestroyImmediate(colliderObject.gameObject);
            return true;
        }

        /// <summary>The box collider's bounds in the piece's own space.</summary>
        private static Bounds BoxBoundsInPiece(BoxCollider box, Transform root)
        {
            var toPiece = root.worldToLocalMatrix * box.transform.localToWorldMatrix;
            var half = box.size / 2f;
            var bounds = new Bounds(toPiece.MultiplyPoint3x4(box.center), Vector3.zero);
            foreach (var sx in new[] { -1f, 1f })
            foreach (var sy in new[] { -1f, 1f })
            foreach (var sz in new[] { -1f, 1f })
            {
                bounds.Encapsulate(toPiece.MultiplyPoint3x4(box.center + Vector3.Scale(half, new Vector3(sx, sy, sz))));
            }
            return bounds;
        }
    }
}
