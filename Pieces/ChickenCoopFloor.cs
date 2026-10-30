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
    /// A flat thatch roof on a 2x2 wood floor, low enough that chicks stand above the thatch rather than in it.
    ///
    /// Clones the vanilla 2x2 wood floor (snapping, collider, planks, support) and lays the 26° thatch roof's
    /// straw on top. The roof's three straw layers are taken into the roof's own space, then unrolled flat:
    /// - along the slope, the 2.24 m sloped span is squeezed to the floor's 2 m, while the overhang past each edge
    ///   keeps exactly the roof's length (no extra from the slope);
    /// - their height above the slope is squashed into a thin layer just above the planks.
    /// The floor's collider is raised to the middle of the thatch, so small animals stand on it. It gives off
    /// warmth (a Heat effect area, like a fire) so eggs on it hatch, and it doesn't weather in the rain, like a roof.
    /// </summary>
    internal static class ChickenCoopFloor
    {
        private const string PrefabName = "Quan_chicken_coop_floor";
        private const string BasePrefab = "wood_floor"; // 2x2 wood floor
        private const string RoofPrefab = "wood_roof";  // 26° thatch roof: straw meshes, material, cost

        private const string Description =
            "A wooden floor laid with low, warm thatch: little legs stand tall on it and eggs stay cosy enough to hatch. " +
            "Give Hen Solo and Cluckbacca a proper coop floor to keep them comfy. For Valyyn.";

        // The 26° roof in its own space: it rises towards -z, from (y 0, z 1) to (y 1, z -1).
        private static readonly Vector3 SlopeCentre = new Vector3(0f, 0.5f, 0f);
        private static readonly Vector3 DownSlope = new Vector3(0f, -1f, 2f).normalized;
        private static readonly Vector3 SlopeNormal = new Vector3(0f, 2f, 1f).normalized;
        private static readonly float HalfSlope = Mathf.Sqrt(5f) / 2f; // half the sloped span, 1.118 m
        private const float HalfFloor = 1f;

        // The floor's planks top out at 0.081 m. The thatch lies right on them and is this thick.
        private const float ThatchBase = 0.081f;
        private const float ThatchThickness = 0.05f;
        // Where small animals (and players) stand: halfway up the thatch.
        private const float WalkHeight = ThatchBase + ThatchThickness / 2f;
        // The warm area above the floor reaches this high: eggs, chickens and the player standing on it.
        private const float WarmthHeight = 1f;

        public static void Register()
        {
            var floor = PrefabManager.Instance.GetPrefab(BasePrefab);
            var roof = PrefabManager.Instance.GetPrefab(RoofPrefab);
            var roofPiece = roof ? roof.GetComponent<Piece>() : null;
            if (floor == null || roofPiece == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}' or '{RoofPrefab}' not found; chicken coop floor skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = "Quan's Chicken Coop Floor",
                Description = Description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = roofPiece.m_craftingStation ? roofPiece.m_craftingStation.name : CraftingStations.Workbench,
                Requirements = roofPiece.m_resources
                    .Where(r => r != null && r.m_resItem != null)
                    .Select(r => new RequirementConfig(r.m_resItem.name, r.m_amount, r.m_amountPerLevel, r.m_recover))
                    .ToArray()
            };

            var customPiece = new CustomPiece(PrefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, roof))
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
                QuanPiecesPlugin.Log.LogWarning("Chicken coop floor: icon render failed; using the wood floor's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, GameObject roof)
        {
            var straws = roof.GetComponentsInChildren<MeshFilter>(true)
                .Where(f => f.name.StartsWith("straw") && f.transform.parent != null && f.transform.parent.name == "high"
                            && f.transform.parent.parent != null && f.transform.parent.parent.name == "New")
                .ToList();
            var material = straws.Select(s => s.GetComponent<MeshRenderer>()).FirstOrDefault(r => r != null)?.sharedMaterial;
            if (straws.Count == 0 || material == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Chicken coop floor: the thatch roof's straw wasn't found; skipped.");
                return false;
            }

            // The straw layers in the roof's own space.
            var thatch = new MeshData();
            foreach (var straw in straws)
            {
                var data = MeshData.From(straw.sharedMesh);
                if (data == null)
                {
                    QuanPiecesPlugin.Log.LogWarning("Chicken coop floor: the straw mesh can't be read; skipped.");
                    return false;
                }
                thatch.Append(data, roof.transform.worldToLocalMatrix * straw.transform.localToWorldMatrix);
            }
            Flatten(thatch);

            var part = new GameObject("thatch");
            part.transform.SetParent(prefab.transform, false);
            part.AddComponent<MeshFilter>().sharedMesh = thatch.ToMesh("Quan_chicken_coop_thatch");
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            IncludeInLods(prefab, renderer);

            RaiseWalkSurface(prefab);
            AddWarmth(prefab);

            // A roof, not a floor, as far as the weather goes: it doesn't wear away when nothing covers it.
            var wearNTear = prefab.GetComponent<WearNTear>();
            var roofWear = roof.GetComponent<WearNTear>();
            if (wearNTear != null && roofWear != null)
            {
                wearNTear.m_noRoofWear = roofWear.m_noRoofWear;
            }
            return true;
        }

        /// <summary>
        /// Unrolls the sloped straw flat. Along the slope, the sloped span maps onto the floor's 2 m and anything
        /// past it (the overhang) keeps its length; across the slope nothing changes. Height above the slope is
        /// squashed into <see cref="ThatchThickness"/>, keeping the layers' order.
        /// </summary>
        private static void Flatten(MeshData thatch)
        {
            var heights = thatch.Positions.Select(p => Vector3.Dot(p - SlopeCentre, SlopeNormal)).ToList();
            var lowest = heights.Min();
            var squash = ThatchThickness / Mathf.Max(0.01f, heights.Max() - lowest);

            for (var i = 0; i < thatch.Positions.Count; i++)
            {
                var p = thatch.Positions[i];
                var along = Vector3.Dot(p - SlopeCentre, DownSlope);
                var z = Mathf.Abs(along) <= HalfSlope
                    ? along * (HalfFloor / HalfSlope)
                    : Mathf.Sign(along) * (HalfFloor + Mathf.Abs(along) - HalfSlope);
                thatch.Positions[i] = new Vector3(p.x, ThatchBase + (heights[i] - lowest) * squash, z);

                // Normals: the roof's tilt removed, so the straw takes light as a flat surface would.
                var n = thatch.Normals[i];
                thatch.Normals[i] = new Vector3(n.x, Vector3.Dot(n, SlopeNormal), Vector3.Dot(n, DownSlope)).normalized;
            }
        }

        /// <summary>
        /// Warmth over the floor, so eggs laid on it hatch: an egg grows only inside a Heat area (what fires give
        /// off), and under a roof. The area is the floor's 2x2 square, from the planks to <see cref="WarmthHeight"/>.
        /// It warms anyone standing on it too, like standing by a fire.
        /// </summary>
        private static void AddWarmth(GameObject prefab)
        {
            var warmth = new GameObject("warmth") { layer = LayerMask.NameToLayer("character_trigger") };
            warmth.transform.SetParent(prefab.transform, false);
            var box = warmth.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, (ThatchBase + WarmthHeight) / 2f, 0f);
            box.size = new Vector3(2f * HalfFloor, WarmthHeight - ThatchBase, 2f * HalfFloor);
            warmth.AddComponent<EffectArea>().m_type = EffectArea.Type.Heat;
        }

        /// <summary>The floor's collider, raised so its top is halfway up the thatch.</summary>
        private static void RaiseWalkSurface(GameObject prefab)
        {
            var box = prefab.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(b => b.name == "collider");
            if (box == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Chicken coop floor: the floor's collider wasn't found; the walk surface stays at plank height.");
                return;
            }

            var t = box.transform;
            var scaleY = t.localScale.y;
            var bottom = t.localPosition.y + scaleY * (box.center.y - box.size.y / 2f);
            var size = box.size;
            var centre = box.center;
            size.y = (WalkHeight - bottom) / scaleY;
            centre.y = ((WalkHeight + bottom) / 2f - t.localPosition.y) / scaleY;
            box.size = size;
            box.center = centre;
        }

        private static void IncludeInLods(GameObject prefab, Renderer renderer)
        {
            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                return;
            }
            var lods = lodGroup.GetLODs();
            for (var i = 0; i < lods.Length; i++)
            {
                lods[i].renderers = lods[i].renderers.Where(r => r != null).Concat(new[] { renderer }).ToArray();
            }
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
        }
    }
}
