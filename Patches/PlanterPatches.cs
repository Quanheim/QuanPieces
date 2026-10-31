using System;
using HarmonyLib;
using QuanPieces.Pieces;
using UnityEngine;

namespace QuanPieces.Patches
{
    // Planter soil as cultivated ground. Vanilla only ever checks the terrain for cultivation, so these patches
    // let plants be placed on a planter's soil and grow there, in any biome, under a roof and next to walls.
    // Each one acts only for points on planter soil (PlanterSoil.IsOverSoil) and leaves everything else vanilla.

    /// <summary>Planter soil counts as cultivated: lets seeds be placed and keeps plants on it growing.</summary>
    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.IsCultivated))]
    internal static class HeightmapIsCultivatedPatch
    {
        private static void Postfix(Vector3 worldPos, ref bool __result)
        {
            if (!__result && PlanterSoil.IsOverSoil(worldPos))
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// Placing a seed needs the aim to land on terrain (a Heightmap). When the cultivator is aimed at planter soil,
    /// report the soil point as terrain, so the vanilla checks (then the IsCultivated patch) accept it.
    /// </summary>
    [HarmonyPatch(typeof(Player), "PieceRayTest")]
    internal static class PlayerPieceRayTestPatch
    {
        private static void Postfix(Player __instance, bool __result, ref Vector3 point, ref Vector3 normal, ref Piece piece,
                                    ref Heightmap heightmap)
        {
            if (!__result || piece == null || heightmap != null || !PlacementGhost.IsPlanting(__instance))
            {
                return;
            }

            var soil = piece.GetComponent<PlanterSoil>();
            var camera = GameCamera.instance;
            if (soil == null || camera == null)
            {
                return;
            }

            var ray = new Ray(camera.transform.position, camera.transform.forward);
            if (!soil.TryHitSoil(ray, point, out var soilPoint))
            {
                return;
            }

            var terrain = Heightmap.FindHeightmap(soilPoint);
            if (terrain == null)
            {
                return;
            }

            point = soilPoint;
            normal = Vector3.up;
            heightmap = terrain;
            piece = null;
        }
    }

    /// <summary>
    /// In build mode the game tints the piece you aim at to show its support. While planting into a planter's
    /// soil that tint covers the whole planter and looks as if the planter, not the soil, is the target, so it's
    /// skipped then. Aiming at the rim still shows it.
    /// </summary>
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Highlight))]
    internal static class WearNTearHighlightPatch
    {
        private static readonly AccessTools.FieldRef<Player, int> RemoveRayMask =
            AccessTools.FieldRefAccess<Player, int>("m_removeRayMask");

        private static bool Prefix(WearNTear __instance)
        {
            var soil = __instance.GetComponent<PlanterSoil>();
            var player = Player.m_localPlayer;
            var camera = GameCamera.instance;
            if (soil == null || player == null || camera == null || !PlacementGhost.IsPlanting(player))
            {
                return true;
            }

            var ray = new Ray(camera.transform.position, camera.transform.forward);
            var aimingAtSoil = Physics.Raycast(ray, out var hit, 50f, RemoveRayMask(player))
                               && soil.TryHitSoil(ray, hit.point, out _);
            return !aimingAtSoil;
        }
    }

    /// <summary>Seeds can be placed in a planter whatever the biome, and in deep snow.</summary>
    [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
    internal static class PlayerUpdatePlacementGhostPatch
    {
        private static void Postfix(Player __instance)
        {
            var status = PlacementGhost.Status(__instance);
            if ((status != Player.PlacementStatus.WrongBiome && status != Player.PlacementStatus.DeepSnow)
                || !PlacementGhost.IsPlanting(__instance)
                || !PlanterSoil.IsOverSoil(PlacementGhost.Get(__instance).transform.position))
            {
                return;
            }

            PlacementGhost.Status(__instance) = Player.PlacementStatus.Valid;
            PlacementGhost.SetValid(__instance, true);
        }
    }

    /// <summary>Plants in a planter grow in any biome, and don't suffer heat or cold.</summary>
    [HarmonyPatch(typeof(Plant), nameof(Plant.UpdateHealth))]
    internal static class PlantUpdateHealthPatch
    {
        // A struct rather than a tuple: Valheim's runtime has no System.ValueTuple for net462 plugins.
        internal struct SavedRules
        {
            public bool Changed;
            public Heightmap.Biome Biome;
            public bool TolerateHeat;
            public bool TolerateCold;
        }

        private static void Prefix(Plant __instance, out SavedRules __state)
        {
            __state = default;
            if (!PlanterSoil.IsOverSoil(__instance.transform.position))
            {
                return;
            }

            __state = new SavedRules
            {
                Changed = true,
                Biome = __instance.m_biome,
                TolerateHeat = __instance.m_tolerateHeat,
                TolerateCold = __instance.m_tolerateCold
            };
            __instance.m_biome = (Heightmap.Biome)(-1);
            __instance.m_tolerateHeat = true;
            __instance.m_tolerateCold = true;
        }

        private static Exception Finalizer(Plant __instance, SavedRules __state, Exception __exception)
        {
            if (__state.Changed && __instance != null)
            {
                __instance.m_biome = __state.Biome;
                __instance.m_tolerateHeat = __state.TolerateHeat;
                __instance.m_tolerateCold = __state.TolerateCold;
            }
            return __exception;
        }
    }

    /// <summary>Plants in a planter grow under a roof, so planters work in greenhouses.</summary>
    [HarmonyPatch(typeof(Plant), "HaveRoof")]
    internal static class PlantHaveRoofPatch
    {
        private static bool Prefix(Plant __instance, ref bool __result)
        {
            if (!PlanterSoil.IsOverSoil(__instance.transform.position))
            {
                return true;
            }
            __result = false;
            return false;
        }
    }

    /// <summary>
    /// Plants in a planter always have room, so they can be packed in as tightly as you like: nothing crowds
    /// them, not other seedlings, not grown crops, not the planter's frame or a greenhouse's walls and roof.
    /// (Grown crops matter most: they're not plants, so counting them as obstacles left neighbours that hadn't
    /// grown yet unhealthy, and the game destroys a crop that's unhealthy when it's due to grow.) Rocks and
    /// trees can't be inside a planter, so there's nothing left to check except vines, which still need room
    /// from other vines as in vanilla.
    /// </summary>
    [HarmonyPatch(typeof(Plant), "HaveGrowSpace")]
    internal static class PlantHaveGrowSpacePatch
    {
        private static readonly Collider[] Colliders = new Collider[64];
        private static int _spaceMask;

        private static bool Prefix(Plant __instance, ref bool __result)
        {
            var position = __instance.transform.position;
            if (!PlanterSoil.IsOverSoil(position))
            {
                return true;
            }

            if (_spaceMask == 0)
            {
                _spaceMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "piece_nonsolid");
            }

            __result = true;
            if (__instance.m_growRadiusVines > 0f)
            {
                var count = Physics.OverlapSphereNonAlloc(position, __instance.m_growRadiusVines, Colliders, _spaceMask);
                for (var i = 0; i < count; i++)
                {
                    if (Colliders[i].GetComponentInParent<Vine>() != null)
                    {
                        __result = false;
                        return false;
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Grown crops have StaticPhysics, which drops them to the terrain below. On planter soil they stay put.
    /// </summary>
    [HarmonyPatch(typeof(StaticPhysics), "Fall")]
    internal static class StaticPhysicsFallPatch
    {
        private static bool Prefix(StaticPhysics __instance)
        {
            return !PlanterSoil.IsOverSoil(__instance.transform.position);
        }
    }

    /// <summary>Access to the player's private placement ghost state.</summary>
    internal static class PlacementGhost
    {
        private static readonly AccessTools.FieldRef<Player, GameObject> GhostField =
            AccessTools.FieldRefAccess<Player, GameObject>("m_placementGhost");

        internal static readonly AccessTools.FieldRef<Player, Player.PlacementStatus> Status =
            AccessTools.FieldRefAccess<Player, Player.PlacementStatus>("m_placementStatus");

        private static readonly Action<Player, bool> SetValidMethod =
            AccessTools.MethodDelegate<Action<Player, bool>>(AccessTools.Method(typeof(Player), "SetPlacementGhostValid"));

        private static readonly AccessTools.FieldRef<Player, PieceTable> BuildPiecesField =
            AccessTools.FieldRefAccess<Player, PieceTable>("m_buildPieces");

        // The cultivator's piece table (Jotunn's PieceTables.Cultivator).
        private const string CultivatorTable = "_CultivatorPieceTable";

        internal static GameObject Get(Player player) => GhostField(player);

        /// <summary>
        /// True while placing something that grows in soil: a seedling (vanilla's Plant), or anything else placed
        /// with the cultivator, such as PlantEverything's mushrooms, berry bushes and flowers (Pickables, not Plants).
        /// </summary>
        internal static bool IsPlanting(Player player)
        {
            var ghost = GhostField(player);
            if (ghost == null)
            {
                return false;
            }
            if (ghost.GetComponent<Plant>() != null)
            {
                return true;
            }
            // The cultivator's own terrain tools (Cultivate, and any other mod's) work the real ground, never a planter.
            if (ghost.GetComponentInChildren<TerrainOp>(true) != null || ghost.GetComponentInChildren<TerrainModifier>(true) != null)
            {
                return false;
            }
            var table = BuildPiecesField(player);
            return table != null && table.name.StartsWith(CultivatorTable, StringComparison.Ordinal);
        }

        internal static void SetValid(Player player, bool valid) => SetValidMethod(player, valid);
    }
}
