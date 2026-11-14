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
    /// The vanilla wood walls with only their vertical planks: the horizontal beams across the back are gone. Each
    /// wear state (New, Worn, Broken) draws one combined mesh of planks and beams; the beams are the islands under
    /// half a metre tall, and are dropped. The 3 m walls are then lengthened, with the planks' texture extended up
    /// the extra height rather than stretched, at the 2x2 wall's grain scale.
    /// </summary>
    internal static class PlankWall
    {
        // Beams are 0.18-0.33 m tall (tilted in the broken states); planks 1-2 m.
        private const float BeamMaxHeight = 0.5f;

        private const string Wall2x2 = "woodwall";            // Wood wall: 2x2, 2 Wood, pivot at its centre
        private const string WallHalf = "wood_wall_half";     // Wood wall half: 2x1, 1 Wood
        private const string Wall1x1 = "wood_wall_quarter";   // Wood wall 1x1: 1 Wood, pivot at its foot

        private struct Spec
        {
            public string PrefabName;
            public string BasePrefab;
            public string Name;
            public string Description;
            public float HeightScale;   // 1: as vanilla
            public float Pivot;         // the height (piece space) that stays put when lengthened
            public float GrainScale;    // texture scale up the planks relative to the base wall
            public RequirementConfig[] Cost; // null: the base wall's cost
        }

        public static void RegisterWall() => Register(new Spec
        {
            PrefabName = "Quan_wood_wall",
            BasePrefab = Wall2x2,
            Name = "Quan's Wood Wall",
            Description = "The 2x2 wood wall with nothing but its planks: no beams across the back.",
            HeightScale = 1f
        });

        public static void RegisterHalf() => Register(new Spec
        {
            PrefabName = "Quan_wood_wall_half",
            BasePrefab = WallHalf,
            Name = "Quan's Wood Wall Half",
            Description = "The 2x1 wood wall with nothing but its planks: no beam across the back.",
            HeightScale = 1f
        });

        public static void Register1x1() => Register(new Spec
        {
            PrefabName = "Quan_wood_wall_1x1",
            BasePrefab = Wall1x1,
            Name = "Quan's Wood Wall 1x1",
            Description = "The 1x1 wood wall with nothing but its planks: no beam across the back.",
            HeightScale = 1f
        });

        // 2 m planks lengthened to 3 m about the wall's centre; the grain carries on at the same scale.
        public static void Register2x3() => Register(new Spec
        {
            PrefabName = "Quan_wood_wall_2x3",
            BasePrefab = Wall2x2,
            Name = "Quan's Wood Wall 2x3",
            Description = "A wall of plain planks, 2 m wide and 3 m tall.",
            HeightScale = 1.5f,
            Pivot = 0f,
            GrainScale = 1f,
            Cost = new[] { new RequirementConfig("Wood", 3, 0, true) }
        });

        // 1 m planks lengthened to 3 m from the foot, at half the grain density so it matches the 2x2 and 2x3 walls.
        public static void Register1x3() => Register(new Spec
        {
            PrefabName = "Quan_wood_wall_1x3",
            BasePrefab = Wall1x1,
            Name = "Quan's Wood Wall 1x3",
            Description = "A wall of plain planks, 1 m wide and 3 m tall.",
            HeightScale = 3f,
            Pivot = 0f,
            GrainScale = 0.5f,
            Cost = new[] { new RequirementConfig("Wood", 2, 0, true) }
        });

        private static void Register(Spec spec)
        {
            var basePrefab = PrefabManager.Instance.GetPrefab(spec.BasePrefab);
            var basePiece = basePrefab ? basePrefab.GetComponent<Piece>() : null;
            if (basePiece == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{spec.BasePrefab}' not found; {spec.Name} skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = spec.Name,
                Description = spec.Description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = basePiece.m_craftingStation ? basePiece.m_craftingStation.name : CraftingStations.Workbench,
                Requirements = spec.Cost ?? basePiece.m_resources
                    .Where(r => r != null && r.m_resItem != null)
                    .Select(r => new RequirementConfig(r.m_resItem.name, r.m_amount, r.m_amountPerLevel, r.m_recover))
                    .ToArray()
            };

            var customPiece = new CustomPiece(spec.PrefabName, spec.BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, spec))
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
                QuanPiecesPlugin.Log.LogWarning($"{spec.Name}: icon render failed; using the vanilla wall's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, Spec spec)
        {
            var root = prefab.transform;
            var states = new[] { "New", "Worn", "Broken" }.Select(n => root.Find(n)).ToArray();
            if (states.Any(s => s == null || s.GetComponent<LODGroup>() == null))
            {
                QuanPiecesPlugin.Log.LogWarning($"{spec.Name}: '{spec.BasePrefab}' no longer has New, Worn and Broken LODs; skipped.");
                return false;
            }

            var converted = new Dictionary<Mesh, Mesh>(); // the 1x1 shares one mesh between its states
            foreach (var state in states)
            {
                // The separate plank boxes the combined mesh was made from: renderers off, in no LOD. Remove them.
                foreach (var high in state.Cast<Transform>().Where(c => c.name == "high").ToList())
                {
                    Object.DestroyImmediate(high.gameObject);
                }

                var lodGroup = state.GetComponent<LODGroup>();
                var lods = lodGroup.GetLODs();
                foreach (var filter in lods[0].renderers.Where(r => r != null).Select(r => r.GetComponent<MeshFilter>()).Where(f => f != null && f.sharedMesh != null))
                {
                    if (!converted.TryGetValue(filter.sharedMesh, out var mesh))
                    {
                        mesh = Planks(filter, root, spec);
                        if (mesh == null)
                        {
                            return false;
                        }
                        converted[filter.sharedMesh] = mesh;
                    }
                    filter.sharedMesh = mesh;
                }

                if (spec.HeightScale != 1f)
                {
                    // The distant slab (LOD1) and anything else in the state, e.g. the 2x2's snow on top.
                    var lod0 = new HashSet<Transform>(lods[0].renderers.Where(r => r != null).Select(r => r.transform));
                    var lod1 = new HashSet<Transform>(lods.Skip(1).SelectMany(l => l.renderers).Where(r => r != null).Select(r => r.transform));
                    foreach (Transform child in state)
                    {
                        if (!lod0.Contains(child))
                        {
                            Lift(child, root, spec, stretch: lod1.Contains(child));
                        }
                    }
                    lodGroup.RecalculateBounds();
                }
            }

            if (spec.HeightScale != 1f)
            {
                // Snap points and the snow beam on top.
                foreach (Transform child in root)
                {
                    if (!states.Contains(child))
                    {
                        Lift(child, root, spec, stretch: false);
                    }
                }
                foreach (var box in prefab.GetComponents<BoxCollider>())
                {
                    var centre = box.center;
                    box.center = new Vector3(centre.x, spec.Pivot + (centre.y - spec.Pivot) * spec.HeightScale, centre.z);
                    box.size = Vector3.Scale(box.size, new Vector3(1f, spec.HeightScale, 1f));
                }
            }
            return true;
        }

        /// <summary>The filter's mesh without its beams, lengthened if the spec asks.</summary>
        private static Mesh Planks(MeshFilter filter, Transform root, Spec spec)
        {
            var source = MeshData.From(filter.sharedMesh);
            if (source == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"{spec.Name}: '{filter.sharedMesh.name}' can't be read; skipped.");
                return null;
            }

            // Work in the piece's own space (the combined meshes sit under offset transforms), then go back.
            var toPiece = root.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            var data = new MeshData().Append(source, toPiece);

            var planks = data.KeepIslands(b => b.size.y >= BeamMaxHeight, out var kept);
            if (kept.Count == 0 || planks.TriangleCount == data.TriangleCount)
            {
                QuanPiecesPlugin.Log.LogWarning($"{spec.Name}: '{filter.sharedMesh.name}' has no beams or no planks to tell apart (game update?); skipped.");
                return null;
            }

            if (spec.HeightScale != 1f)
            {
                planks = planks.ExtendY(spec.Pivot, spec.HeightScale, spec.GrainScale);
            }
            return new MeshData().Append(planks, toPiece.inverse).ToMesh(filter.sharedMesh.name + "_quan_planks");
        }

        /// <summary>
        /// Moves a child to its place on the lengthened wall; with <paramref name="stretch"/> (the distant slab)
        /// also makes it as much taller as the wall.
        /// </summary>
        private static void Lift(Transform child, Transform root, Spec spec, bool stretch)
        {
            var p = root.InverseTransformPoint(child.position);
            p.y = spec.Pivot + (p.y - spec.Pivot) * spec.HeightScale;
            child.position = root.TransformPoint(p);
            if (!stretch)
            {
                return;
            }
            // The child's local axis that points up the wall.
            var axis = Enumerable.Range(0, 3)
                .OrderByDescending(i => Mathf.Abs(Vector3.Dot(child.TransformDirection(Axis(i)), root.up)))
                .First();
            var scale = child.localScale;
            scale[axis] *= spec.HeightScale;
            child.localScale = scale;
        }

        private static Vector3 Axis(int i) => i == 0 ? Vector3.right : i == 1 ? Vector3.up : Vector3.forward;
    }
}
