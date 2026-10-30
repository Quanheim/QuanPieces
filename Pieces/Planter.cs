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
    /// Raised planters: a bed of cultivated soil in a wooden frame on short legs, low enough to sit under a half wall.
    ///
    /// Clones the vanilla 1x1 wood floor for its piece setup (wood support, sounds, placement) and rebuilds it:
    /// - a rim of half-width wood beams (2 m lengths along the sides), mitred at the corners, its outer faces on
    ///   the piece's footprint;
    /// - a bed (one solid block in the floor's wood) hanging below the rim, its edges just inside the beams so
    ///   there's no gap inside and nothing sticks out;
    /// - a soil surface with the terrain's cultivated texture, which <see cref="PlanterSoil"/> makes count as
    ///   cultivated ground for planting and growing;
    /// - 1 m wood pole legs.
    /// Its only snap points are the four bottom corners, so it lines up with the floor grid.
    /// </summary>
    internal static class Planter
    {
        private const string BasePrefab = "wood_floor_1x1"; // piece setup, plank mesh and woodpole material
        private const string BeamPrefab = "wood_beam";      // rim: the beam's cube mesh and woodwall material
        private const string PolePrefab = "wood_pole";      // legs: the pole's woodpole material

        private const string Description =
            "A raised bed of cultivated soil in a wooden frame, low enough to sit under a half wall. " +
            "Plant anything in it, anywhere, even under a roof. For Tech'kai.";

        // Footprint: Length along x, Width along z, origin at the centre of the bottom face.
        private const float Width = 1f;

        // Rim: vanilla beams are 0.4 m square; these are half as wide, as tall, their outer faces on the footprint.
        private const float RimTop = 0.8f;
        private const float RimHeight = 0.4f;
        private const float RimWidth = 0.2f;
        private const float MitreOverrun = 0.5f; // beams are built this much past each corner, then cut at 45°
        private const float SideBeamLength = 2f; // long sides are vanilla-length beams, joined end to end

        // Soil 15 cm below the rim; the bed hangs 50 cm from just below it.
        private const float SoilHeight = 0.65f;
        private const float BedTop = 0.615f;     // just under the soil, clear of the beams' vertex wobble
        private const float BedDepth = 0.5f;
        private const float BedInset = 0.16f;    // bed edge 4 cm inside the beams' inner faces

        // Legs: 1 m wood poles, 0.4 m square, standing on the ground and hidden in the bed above it.
        private const float LegWidth = 0.4f;
        private const float LegTop = 0.5f;

        // Cultivated soil tiles every 2 m, as on the terrain.
        private const float SoilTilesPerMetre = 0.5f;

        public static void RegisterLarge()
        {
            Register("Quan_planter_1x4", "Quan's Large Planter", 4f, 16, new[] { -1f, 0f, 1f });
        }

        public static void RegisterSmall()
        {
            Register("Quan_planter_1x2", "Quan's Planter", 2f, 10, new[] { -1f, 1f });
        }

        /// <param name="legPositions">-1 and 1 put a leg at each end of the bed; 0 is the middle.</param>
        private static void Register(string prefabName, string displayName, float length, int wood, float[] legPositions)
        {
            var floor = PrefabManager.Instance.GetPrefab(BasePrefab);
            var beam = PrefabManager.Instance.GetPrefab(BeamPrefab);
            var pole = PrefabManager.Instance.GetPrefab(PolePrefab);
            if (floor == null || beam == null || pole == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}', '{BeamPrefab}' or '{PolePrefab}' not found; {displayName} skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = displayName,
                Description = Description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = CraftingStations.Workbench,
                Requirements = new[] { new RequirementConfig("Wood", wood, 0, true) }
            };

            var customPiece = new CustomPiece(prefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, floor, beam, pole, length, legPositions, displayName))
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
                QuanPiecesPlugin.Log.LogWarning($"{displayName}: icon render failed; using the wood floor's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, GameObject floor, GameObject beam, GameObject pole, float length,
                                  float[] legPositions, string displayName)
        {
            var plank = floor.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name.StartsWith("plank"));
            var beamFilter = beam.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == "new");
            var poleRenderer = pole.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name == "New");
            var cube = beamFilter ? MeshData.From(beamFilter.sharedMesh) : null;
            if (plank == null || beamFilter == null || poleRenderer == null || cube == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"{displayName}: the floor plank, beam or pole visuals weren't found or can't be read; skipped.");
                return false;
            }

            // Start from an empty piece: everything the floor had (planks, colliders, snap points, snow, wet
            // particles) is rebuilt to the planter's shape.
            foreach (var child in prefab.transform.Cast<Transform>().ToList())
            {
                Object.DestroyImmediate(child.gameObject);
            }
            foreach (var collider in prefab.GetComponents<Collider>())
            {
                Object.DestroyImmediate(collider);
            }

            var visual = new GameObject("New");
            visual.transform.SetParent(prefab.transform, false);
            var renderers = new List<Renderer>
            {
                AddMesh(visual.transform, "rim", BuildRim(cube, length), beamFilter.GetComponent<MeshRenderer>().sharedMaterials),
                AddBed(visual.transform, beamFilter.sharedMesh, plank.GetComponent<MeshRenderer>(), length),
                AddMesh(visual.transform, "soil", BuildSoil(length), new[] { PlanterSoil.GetSoilMaterial(plank.GetComponent<MeshRenderer>().sharedMaterial) })
            };
            renderers.AddRange(AddLegs(visual.transform, beamFilter.sharedMesh, poleRenderer.sharedMaterials, length, legPositions));

            AddColliders(prefab, length, legPositions);
            AddSnapPoints(prefab.transform, length);
            prefab.AddComponent<PlanterSoil>();

            // One damage state, like the flue: the same visual whether new, worn or broken.
            var wearNTear = prefab.GetComponent<WearNTear>();
            if (wearNTear != null)
            {
                wearNTear.m_new = visual;
                wearNTear.m_worn = visual;
                wearNTear.m_broken = visual;
                wearNTear.m_wet = null;
                wearNTear.m_snow = null;
                wearNTear.m_snowWorn = null;
                wearNTear.m_snowBroken = null;
                wearNTear.m_fragmentRoots = null;
            }

            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                var cullHeight = lods.Length > 0 ? lods[lods.Length - 1].screenRelativeTransitionHeight : 0.01f;
                lodGroup.SetLODs(new[] { new LOD(cullHeight, renderers.ToArray()) });
                lodGroup.RecalculateBounds();
            }

            return true;
        }

        /// <summary>
        /// The four rim beams as one mesh: each is the vanilla beam's cube, made half as wide, built past both
        /// corners and cut along the corner diagonals, so neighbouring beams meet in a mitre.
        /// </summary>
        private static MeshData BuildRim(MeshData cube, float length)
        {
            var halfL = length / 2f;
            var halfW = Width / 2f;
            var y = RimTop - RimHeight / 2f;
            var rim = new MeshData();

            // Long sides: 2 m beams end to end, like vanilla beams, with a plain joint where they meet. At a corner
            // a beam keeps the points nearer its own face than the end's.
            var segments = Mathf.Max(1, Mathf.RoundToInt(length / SideBeamLength));
            var segmentLength = length / segments;
            foreach (var sz in new[] { -1f, 1f })
            {
                for (var k = 0; k < segments; k++)
                {
                    var atStart = k == 0;
                    var atEnd = k == segments - 1;
                    var centreX = -halfL + (k + 0.5f) * segmentLength;

                    // Exactly a vanilla 2 m beam (at half width), so the joint between two looks like vanilla's.
                    // Only its corner end is then pushed out past the mitre, so the cut removes that end's bevel
                    // and the outer corner stays square.
                    var side = new MeshData().Append(cube, Matrix4x4.TRS(new Vector3(centreX, y, sz * (halfW - RimWidth / 2f)),
                                                                         Quaternion.identity, new Vector3(segmentLength, RimHeight, RimWidth)));
                    for (var i = 0; i < side.Positions.Count; i++)
                    {
                        var p = side.Positions[i];
                        if (atStart && p.x < centreX)
                        {
                            side.Positions[i] = new Vector3(p.x - MitreOverrun, p.y, p.z);
                        }
                        else if (atEnd && p.x > centreX)
                        {
                            side.Positions[i] = new Vector3(p.x + MitreOverrun, p.y, p.z);
                        }
                    }
                    if (atStart)
                    {
                        side = side.Clip(new Vector3(-1f, 0f, -sz), halfL - halfW);
                    }
                    if (atEnd)
                    {
                        side = side.Clip(new Vector3(1f, 0f, -sz), halfL - halfW);
                    }
                    rim.Append(side, Matrix4x4.identity);
                }
            }

            foreach (var sx in new[] { -1f, 1f })
            {
                var end = new MeshData().Append(cube, Matrix4x4.TRS(new Vector3(sx * (halfL - RimWidth / 2f), y, 0f), Quaternion.identity,
                                                                    new Vector3(RimWidth, RimHeight, Width + 2f * MitreOverrun)));
                foreach (var sz in new[] { -1f, 1f })
                {
                    end = end.Clip(new Vector3(-sx, 0f, sz), halfW - halfL);
                }
                rim.Append(end, Matrix4x4.identity);
            }
            return rim;
        }

        /// <summary>
        /// The bed: one solid block with the 1x1 floor's material, stretched to the bed's size (the floor's plank
        /// mesh is separate planks, which showed gaps when stretched). It reaches 4 cm into the beams on every side.
        /// </summary>
        private static Renderer AddBed(Transform parent, Mesh cube, MeshRenderer plank, float length)
        {
            var bounds = cube.bounds;
            var size = new Vector3(length - 2f * BedInset, BedDepth, Width - 2f * BedInset);
            var scale = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, size.z / bounds.size.z);
            var centre = new Vector3(0f, BedTop - BedDepth / 2f, 0f);

            var bed = new GameObject("bed");
            bed.transform.SetParent(parent, false);
            bed.transform.localScale = scale;
            bed.transform.localPosition = centre - Vector3.Scale(bounds.center, scale);
            bed.AddComponent<MeshFilter>().sharedMesh = cube;
            var renderer = bed.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = plank.sharedMaterials;
            return renderer;
        }

        /// <summary>
        /// The soil: a flat surface over the whole bed top. Only the part inside the rim shows; the rest is hidden
        /// in the beams, so the beams' vertex wobble can never open a gap at its edges.
        /// </summary>
        private static MeshData BuildSoil(float length)
        {
            var halfL = length / 2f - BedInset;
            var halfW = Width / 2f - BedInset;
            var corners = new[]
            {
                new Vector3(-halfL, SoilHeight, -halfW), new Vector3(-halfL, SoilHeight, halfW),
                new Vector3(halfL, SoilHeight, halfW), new Vector3(halfL, SoilHeight, -halfW)
            };
            var soil = new MeshData();
            foreach (var corner in corners)
            {
                soil.Positions.Add(corner);
                soil.Normals.Add(Vector3.up);
                soil.Uvs.Add(new Vector2(corner.x, corner.z) * SoilTilesPerMetre);
            }
            soil.Triangles.AddRange(new[] { 0, 1, 2, 0, 2, 3 });
            return soil;
        }

        /// <summary>Legs: the pole's cube and material, standing on the ground along the bed's centre line.</summary>
        private static IEnumerable<Renderer> AddLegs(Transform parent, Mesh cube, Material[] materials, float length, float[] legPositions)
        {
            var endX = length / 2f - BedInset - LegWidth / 2f;
            foreach (var position in legPositions)
            {
                var leg = new GameObject("leg");
                leg.transform.SetParent(parent, false);
                leg.transform.localPosition = new Vector3(position * endX, LegTop / 2f, 0f);
                // The cube's grain runs along x; vanilla stands its poles up like this.
                leg.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                leg.transform.localScale = new Vector3(LegTop, LegWidth, LegWidth);
                leg.AddComponent<MeshFilter>().sharedMesh = cube;
                var renderer = leg.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = materials;
                yield return renderer;
            }
        }

        private static Renderer AddMesh(Transform parent, string name, MeshData data, Material[] materials)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = data.ToMesh("Quan_planter_" + name);
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            return renderer;
        }

        /// <summary>
        /// Solid colliders for the rim, bed and legs (support, movement, hammer), and the soil collider, a thin
        /// slab inside the rim whose top is the soil surface. The bed's collider stops just below it, so a
        /// look at the soil always hits the soil collider.
        /// </summary>
        private static void AddColliders(GameObject prefab, float length, float[] legPositions)
        {
            var parent = prefab.transform;
            var halfL = length / 2f;
            var halfW = Width / 2f;
            var rimY = RimTop - RimHeight / 2f;

            AddCollider(parent, "collider_rim_south", new Vector3(0f, rimY, -(halfW - RimWidth / 2f)), new Vector3(length, RimHeight, RimWidth));
            AddCollider(parent, "collider_rim_north", new Vector3(0f, rimY, halfW - RimWidth / 2f), new Vector3(length, RimHeight, RimWidth));
            AddCollider(parent, "collider_rim_west", new Vector3(-(halfL - RimWidth / 2f), rimY, 0f), new Vector3(RimWidth, RimHeight, Width));
            AddCollider(parent, "collider_rim_east", new Vector3(halfL - RimWidth / 2f, rimY, 0f), new Vector3(RimWidth, RimHeight, Width));

            var bedBottom = BedTop - BedDepth;
            var bedTop = SoilHeight - PlanterSoil.SoilColliderDepth;
            AddCollider(parent, "collider_bed", new Vector3(0f, (bedBottom + bedTop) / 2f, 0f),
                        new Vector3(length - 2f * BedInset, bedTop - bedBottom, Width - 2f * BedInset));

            AddCollider(parent, PlanterSoil.SoilColliderName, new Vector3(0f, SoilHeight - PlanterSoil.SoilColliderDepth / 2f, 0f),
                        new Vector3(length - 2f * RimWidth, PlanterSoil.SoilColliderDepth, Width - 2f * RimWidth));

            var endX = halfL - BedInset - LegWidth / 2f;
            foreach (var position in legPositions)
            {
                AddCollider(parent, "collider_leg", new Vector3(position * endX, (bedBottom + 0.05f) / 2f, 0f),
                            new Vector3(LegWidth, bedBottom + 0.05f, LegWidth));
            }
        }

        private static void AddCollider(Transform parent, string name, Vector3 centre, Vector3 size)
        {
            var part = new GameObject(name) { layer = parent.gameObject.layer };
            part.transform.SetParent(parent, false);
            part.transform.localPosition = centre;
            part.AddComponent<BoxCollider>().size = size;
        }

        /// <summary>The four bottom corners, like a floor's, so it snaps to the floor grid and to other planters.</summary>
        private static void AddSnapPoints(Transform parent, float length)
        {
            var i = 1;
            foreach (var sx in new[] { -1f, 1f })
            {
                foreach (var sz in new[] { -1f, 1f })
                {
                    var snap = new GameObject($"$hud_snappoint_corner {i++}") { tag = "snappoint", layer = parent.gameObject.layer };
                    snap.SetActive(false);
                    snap.transform.SetParent(parent, false);
                    snap.transform.localPosition = new Vector3(sx * length / 2f, 0f, sz * Width / 2f);
                }
            }
        }
    }
}
