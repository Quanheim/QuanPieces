using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// A ring of hot stones that warms chicken coop floors within 10 m, so eggs on them hatch. Switch it off and the
    /// floors go cold, so eggs can be collected instead, without swapping any floors.
    ///
    /// Clones the vanilla campfire (fire_pit) and keeps only its ring of stones and ash bed. The logs, flames,
    /// smoke, sounds and the Fireplace itself go; the glowing ash and the low-burning light are kept as embers,
    /// shown while it's on. It burns no fuel and doesn't care about rain, wind, smoke or open sky, so it can sit
    /// under the edge of a coop or out in the open beside it.
    /// </summary>
    internal static class FloorHeater
    {
        private const string PrefabName = "Quan_floor_heater";
        private const string BasePrefab = "fire_pit";

        private const string Description =
            "A ring of hot stones. While it's on, Quan's Chicken Coop Floors within 10m stay warm enough for eggs " +
            "to hatch. Turn it off to collect eggs instead. Needs no fuel, and doesn't mind rain or open sky. For Valyyn.";

        public static void Register()
        {
            if (PrefabManager.Instance.GetPrefab(BasePrefab) == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}' not found; floor heater skipped.");
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
            if (!Build(customPiece.PiecePrefab))
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
                QuanPiecesPlugin.Log.LogWarning("Floor heater: icon render failed; using the campfire's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab)
        {
            var root = prefab.transform;
            var stones = root.Find("New");
            var glowingAsh = root.Find("_enabled_high/Quad (1)");
            var light = root.Find("_enabled_low/Point light");
            if (stones == null || glowingAsh == null || light == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Floor heater: the campfire's stones, glowing ash or light weren't found; skipped.");
                return false;
            }

            // The embers: the glowing ash bed and the low fire's light, together under one switch.
            var embers = new GameObject("embers");
            embers.transform.SetParent(root, false);
            glowingAsh.SetParent(embers.transform, false);
            light.SetParent(embers.transform, false);

            // Everything else that made it a fire: the Fireplace, its burning and lit states (flames, smoke, sound,
            // heat for players, scaring animals), the base area that stops monsters spawning, and the logs.
            Object.DestroyImmediate(prefab.GetComponent<Fireplace>());
            foreach (var name in new[] { "_enabled", "_enabled_high", "_enabled_low", "PlayerBase" })
            {
                var child = root.Find(name);
                if (child != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
            foreach (var log in stones.Cast<Transform>().Where(t => t.name.StartsWith("Cube")).ToList())
            {
                Object.DestroyImmediate(log.gameObject);
            }

            // A campfire is ground-only and counts towards comfort; the heater can go on a floor and isn't a fire.
            var piece = prefab.GetComponent<Piece>();
            piece.m_groundOnly = false;
            piece.m_groundPiece = false;
            piece.m_comfort = 0;

            prefab.AddComponent<FloorHeaterControl>().m_embers = embers;

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
            return true;
        }
    }
}
