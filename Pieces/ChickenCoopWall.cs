using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using QuanPieces.Meshes;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// A 2x2 wall of wooden bars: the vanilla Cage Wall 2x2 remade in wood. Its meshes keep their shape; they get
    /// wood-grain UVs and the wood pole's materials (worn wood for its damaged states), and the wood door's
    /// support, sounds and effects (<see cref="CoopWood"/>).
    /// </summary>
    internal static class ChickenCoopWall
    {
        private const string PrefabName = "Quan_chicken_coop_wall";
        private const string BasePrefab = "iron_wall_2x2"; // vanilla Cage Wall 2x2

        private const string Description =
            "A 2x2 wall of wooden bars for the coop: plenty of light and air for Hen Solo and Cluckbacca, and no " +
            "way in for the wolves. For Valyyn.";

        public static void Register()
        {
            if (PrefabManager.Instance.GetPrefab(BasePrefab) == null || !CoopWood.TryGetMaterials(out var wood, out var worn))
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}' or the wood pole's material not found; chicken coop wall skipped.");
                return;
            }

            var config = new PieceConfig
            {
                Name = "Quan's Chicken Coop Wall",
                Description = Description,
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = CoopWood.Station,
                Requirements = CoopWood.Cost
            };

            var customPiece = new CustomPiece(PrefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, wood, worn))
            {
                return;
            }
            CoopWood.MakeWooden(customPiece.PiecePrefab);
            // You can look through the bars to pick up eggs inside the coop.
            customPiece.PiecePrefab.AddComponent<Patches.SeeThrough>();

            var icon = RenderManager.Instance.Render(customPiece.PiecePrefab, RenderManager.IsometricRotation);
            if (icon != null)
            {
                customPiece.Piece.m_icon = icon;
            }
            else
            {
                QuanPiecesPlugin.Log.LogWarning("Chicken coop wall: icon render failed; using the Cage Wall's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, Material wood, Material worn)
        {
            // The new, worn and broken states share some meshes; convert each mesh once.
            var converted = new Dictionary<Mesh, Mesh>();
            foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null || filter.sharedMesh == null)
                {
                    continue;
                }

                if (!converted.TryGetValue(filter.sharedMesh, out var mesh))
                {
                    var data = MeshData.From(filter.sharedMesh);
                    if (data == null)
                    {
                        QuanPiecesPlugin.Log.LogWarning($"Chicken coop wall: '{filter.name}' can't be read; skipped.");
                        return false;
                    }
                    mesh = WoodGrain.Rewrap(data).ToMesh(filter.sharedMesh.name + "_quan_wood");
                    converted[filter.sharedMesh] = mesh;
                }

                filter.sharedMesh = mesh;
                var damaged = filter.transform.parent != null && filter.transform.parent.name != "New";
                renderer.sharedMaterial = damaged ? worn : wood;
            }
            return converted.Count > 0;
        }
    }
}
