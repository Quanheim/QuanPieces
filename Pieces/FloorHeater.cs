using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using QuanPieces.Meshes;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// A 1x1 wooden box of glowing embers that warms chicken coop floors within 10 m, so eggs on them hatch. Switch
    /// it off and the floors go cold, so eggs can be collected instead, without swapping any floors.
    ///
    /// Kitbashed: the vanilla 1x1 wood floor for its piece setup (wood support, sounds, placement), a square of
    /// half-width beams like the planter's rim, a plank base inside, and the campfire's (fire_pit) ash bed, glowing
    /// ash and low-fire light scaled down to fit. The glowing ash and light are the embers, shown while it's on.
    /// It burns no fuel and doesn't care about rain, wind, smoke or open sky (it isn't a Fireplace), so it can sit
    /// under the edge of a coop or out in the open beside it.
    /// </summary>
    internal static class FloorHeater
    {
        private const string PrefabName = "Quan_floor_heater";
        private const string BasePrefab = "wood_floor_1x1"; // piece setup, plank material
        private const string BeamPrefab = "wood_beam";      // box sides: the beam's cube mesh and woodwall material
        private const string FirePitPrefab = "fire_pit";    // ash bed, glowing ash, light

        private const string Description =
            "A wooden box of glowing embers. While it's on, Quan's Chicken Coop Floors within 10m stay warm enough " +
            "for eggs to hatch. Turn it off to collect eggs instead. Needs no fuel, and doesn't mind rain or open sky. For Valyyn.";

        // The box: 1 m square, sides of half-width beams (0.2 m), 0.25 m tall.
        private const float Size = 1f;
        private const float SideTop = 0.25f;
        private const float SideWidth = 0.2f;
        // The plank base inside, and the ash lying on it.
        private const float BaseTop = 0.08f;
        private const float BaseInset = 0.16f;      // base edge 4 cm inside the sides' inner faces
        private const float AshHeight = BaseTop + 0.01f;
        private const float GlowHeight = BaseTop + 0.02f;
        // The ash patches are round, so they're sized past the 0.6 m opening; their edges hide under the sides.
        private const float AshSize = 0.8f;
        private const float GlowSize = 0.55f;
        private const float LightHeight = 0.5f;

        public static void Register()
        {
            var beam = PrefabManager.Instance.GetPrefab(BeamPrefab);
            var floor = PrefabManager.Instance.GetPrefab(BasePrefab);
            var firePit = PrefabManager.Instance.GetPrefab(FirePitPrefab);
            if (beam == null || floor == null || firePit == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}', '{BeamPrefab}' or '{FirePitPrefab}' not found; floor heater skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = "Quan's Floor Heater",
                Description = Description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = CraftingStations.Workbench,
                Requirements = new[] { new RequirementConfig("Stone", 5, 0, true) }
            };

            var customPiece = new CustomPiece(PrefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, floor, beam, firePit))
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
                QuanPiecesPlugin.Log.LogWarning("Floor heater: icon render failed; using the wood floor's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, GameObject floor, GameObject beam, GameObject firePit)
        {
            var plank = floor.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name.StartsWith("plank"));
            var beamFilter = beam.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == "new");
            var cube = beamFilter ? MeshData.From(beamFilter.sharedMesh) : null;
            var ash = firePit.transform.Find("New/Quad");
            var glow = firePit.transform.Find("_enabled_high/Quad (1)");
            var light = firePit.transform.Find("_enabled_low/Point light");
            if (plank == null || cube == null || ash == null || glow == null || light == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Floor heater: the floor plank, beam, or the campfire's ash or light weren't found; skipped.");
                return false;
            }

            // Start from an empty piece: the floor's planks, colliders and snap points are rebuilt to the box.
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

            var sides = new GameObject("sides");
            sides.transform.SetParent(visual.transform, false);
            sides.AddComponent<MeshFilter>().sharedMesh =
                Planter.BuildRim(cube, Size, Size, SideTop, SideTop, SideWidth).ToMesh("Quan_floor_heater_sides");
            var sidesRenderer = sides.AddComponent<MeshRenderer>();
            sidesRenderer.sharedMaterials = beamFilter.GetComponent<MeshRenderer>().sharedMaterials;

            var baseBoard = new GameObject("base");
            baseBoard.transform.SetParent(visual.transform, false);
            var bounds = beamFilter.sharedMesh.bounds;
            var baseSize = new Vector3(Size - 2f * BaseInset, BaseTop, Size - 2f * BaseInset);
            var baseScale = new Vector3(baseSize.x / bounds.size.x, baseSize.y / bounds.size.y, baseSize.z / bounds.size.z);
            baseBoard.transform.localScale = baseScale;
            baseBoard.transform.localPosition = new Vector3(0f, BaseTop / 2f, 0f) - Vector3.Scale(bounds.center, baseScale);
            baseBoard.AddComponent<MeshFilter>().sharedMesh = beamFilter.sharedMesh;
            var baseRenderer = baseBoard.AddComponent<MeshRenderer>();
            baseRenderer.sharedMaterials = plank.sharedMaterials;

            var ashRenderer = CopyFlat(ash, visual.transform, "ash", AshHeight, AshSize);

            // The embers: glowing ash and the low fire's light, together under one switch.
            var embers = new GameObject("embers");
            embers.transform.SetParent(prefab.transform, false);
            CopyFlat(glow, embers.transform, "glow", GlowHeight, GlowSize);
            var lamp = Object.Instantiate(light.gameObject, embers.transform, false);
            lamp.name = "Point light";
            lamp.transform.localPosition = new Vector3(0f, LightHeight, 0f);

            // One solid box for support, walking and the hammer, and one to aim at for the switch.
            Planter.AddCollider(prefab.transform, "collider", new Vector3(0f, SideTop / 2f, 0f), new Vector3(Size, SideTop, Size));
            Planter.AddSnapPoints(prefab.transform, Size);

            prefab.AddComponent<FloorHeaterControl>().m_embers = embers;

            // One damage state, like the planter: the same visual whether new, worn or broken.
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
                lodGroup.SetLODs(new[] { new LOD(cullHeight, new Renderer[] { sidesRenderer, baseRenderer, ashRenderer }) });
                lodGroup.RecalculateBounds();
            }
            return true;
        }

        /// <summary>
        /// A copy of one of the campfire's flat ash quads, lying at <paramref name="height"/> and scaled so it's
        /// <paramref name="size"/> across, keeping its orientation (the quads are rotated to lie flat).
        /// </summary>
        private static Renderer CopyFlat(Transform source, Transform parent, string name, float height, float size)
        {
            var filter = source.GetComponent<MeshFilter>();
            var renderer = source.GetComponent<MeshRenderer>();
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = new Vector3(0f, height, 0f);
            part.transform.localRotation = source.rotation;
            var across = source.lossyScale.x * filter.sharedMesh.bounds.size.x;
            part.transform.localScale = source.lossyScale * (size / Mathf.Max(0.01f, across));
            part.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            var copy = part.AddComponent<MeshRenderer>();
            copy.sharedMaterials = renderer.sharedMaterials;
            copy.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return copy;
        }
    }
}
