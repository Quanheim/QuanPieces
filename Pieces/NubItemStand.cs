using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// The vanilla item stands (wall and floor) with only their nub: the backplate or underplate is gone, and the
    /// nub, attach point and drop point move back by the plate's thickness, so the nub sits on the surface and the
    /// item hangs as close to it as the item's own attach offset allows. Everything else is the vanilla
    /// <see cref="ItemStand"/>: attaching, taking off, what it accepts.
    /// </summary>
    internal static class NubItemStand
    {
        private const string Name = "Quan's Item Stand";

        // The collider is the nub's box, made at least this big across, so the tiny nub is still easy to aim at.
        private const float MinColliderSize = 0.1f;

        public static void RegisterWall() => Register("Quan_itemstand", "itemstand",
            "The wall item stand without its backplate: just the nub, so the item sits right against the wall.",
            nubUnderItem: false);

        // Vanilla's floor stand attaches the item at the plate's centre, 5 cm to one side of its nub; with no plate
        // the nub would show beside the item, so it moves under the item.
        public static void RegisterFloor() => Register("Quan_itemstandh", "itemstandh",
            "The floor item stand without its underplate: just the nub, so the item sits right on the floor.",
            nubUnderItem: true);

        private static void Register(string prefabName, string basePrefabName, string description, bool nubUnderItem)
        {
            var basePrefab = PrefabManager.Instance.GetPrefab(basePrefabName);
            var basePiece = basePrefab ? basePrefab.GetComponent<Piece>() : null;
            if (basePiece == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{basePrefabName}' not found; {Name} ({basePrefabName}) skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = Name,
                Description = description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = basePiece.m_craftingStation ? basePiece.m_craftingStation.name : CraftingStations.Workbench,
                Requirements = basePiece.m_resources
                    .Where(r => r != null && r.m_resItem != null)
                    .Select(r => new RequirementConfig(r.m_resItem.name, r.m_amount, r.m_amountPerLevel, r.m_recover))
                    .ToArray()
            };

            var customPiece = new CustomPiece(prefabName, basePrefabName, config);
            if (!Build(customPiece.PiecePrefab, basePrefabName, nubUnderItem))
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
                QuanPiecesPlugin.Log.LogWarning($"{Name} ({basePrefabName}): icon render failed; using the vanilla icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, string baseName, bool nubUnderItem)
        {
            var root = prefab.transform;
            var stand = prefab.GetComponent<ItemStand>();
            var box = root.Find("collider")?.GetComponent<BoxCollider>();
            if (stand == null || stand.m_attachOther == null || box == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"{Name} ({baseName}): no ItemStand, attach point or 'collider' box; skipped.");
                return false;
            }

            // Each look (New, WornBroken) is two boxes: the plate (the bigger) and the nub.
            var looks = root.Cast<Transform>()
                .Select(t => t.GetComponentsInChildren<MeshRenderer>(true))
                .Where(r => r.Length == 2)
                .ToList();
            if (looks.Count == 0)
            {
                QuanPiecesPlugin.Log.LogWarning($"{Name} ({baseName}): no plate-and-nub looks found (game update?); skipped.");
                return false;
            }

            // Which way the nub sticks out: the plate's face normal (its thinnest axis), towards the nub. Not simply
            // plate-to-nub: the floor stand's nub sits further to one side of the plate's centre than above it.
            // Then how far the plate's face stands off the surface (the piece's origin).
            var first = looks[0].OrderByDescending(r => Volume(Bounds(r, root))).ToArray();
            var plate = Bounds(first[0], root);
            var nub = Bounds(first[1], root);
            var thin = plate.size.x <= plate.size.y && plate.size.x <= plate.size.z ? 0 : plate.size.y <= plate.size.z ? 1 : 2;
            var outward = Vector3.zero;
            outward[thin] = Mathf.Sign(nub.center[thin] - plate.center[thin]);
            var shift = -outward * Far(plate, outward);

            // On the floor stand the nub also slides sideways to sit under the attach point, where the item hides it.
            var nubShift = shift;
            if (nubUnderItem)
            {
                var sideways = root.InverseTransformPoint(stand.m_attachOther.position) - nub.center;
                sideways[thin] = 0f;
                nubShift += sideways;
            }

            foreach (var look in looks)
            {
                var ordered = look.OrderByDescending(r => Volume(Bounds(r, root))).ToArray();
                Object.DestroyImmediate(ordered[0].gameObject);
                ordered[1].transform.position += root.TransformVector(nubShift);
            }
            stand.m_attachOther.position += root.TransformVector(shift);
            if (stand.m_dropSpawnPoint != null)
            {
                stand.m_dropSpawnPoint.position += root.TransformVector(shift);
            }

            // The LODGroup still lists the plates; keep only what's left.
            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                for (var i = 0; i < lods.Length; i++)
                {
                    lods[i].renderers = lods[i].renderers.Where(r => r != null).ToArray();
                }
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }

            // A collider round the nub, from the surface outwards, at least MinColliderSize across.
            var nubNow = new Bounds(nub.center + nubShift, nub.size);
            var size = Vector3.Max(nubNow.size, Vector3.one * MinColliderSize);
            var axis = thin;
            size[axis] = Mathf.Max(Far(nubNow, outward), MinColliderSize);
            var centre = nubNow.center;
            centre[axis] = outward[axis] * size[axis] / 2f;
            var boxObject = box.gameObject;
            boxObject.transform.localPosition = Vector3.zero;
            boxObject.transform.localRotation = Quaternion.identity;
            boxObject.transform.localScale = Vector3.one;
            box.center = centre;
            box.size = size;
            return true;
        }

        private static Bounds Bounds(Renderer renderer, Transform root)
        {
            var filter = renderer.GetComponent<MeshFilter>();
            var local = filter && filter.sharedMesh ? filter.sharedMesh.bounds : new Bounds();
            var toPiece = root.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
            var bounds = new Bounds(toPiece.MultiplyPoint3x4(local.center), Vector3.zero);
            foreach (var sx in new[] { -1f, 1f })
            foreach (var sy in new[] { -1f, 1f })
            foreach (var sz in new[] { -1f, 1f })
            {
                bounds.Encapsulate(toPiece.MultiplyPoint3x4(local.center + Vector3.Scale(local.extents, new Vector3(sx, sy, sz))));
            }
            return bounds;
        }

        private static float Volume(Bounds b) => b.size.x * b.size.y * b.size.z;

        /// <summary>How far the bounds reach from the origin along the unit axis <paramref name="outward"/>.</summary>
        private static float Far(Bounds b, Vector3 outward) =>
            Vector3.Dot(outward, outward.x + outward.y + outward.z > 0f ? b.max : b.min);
    }
}
