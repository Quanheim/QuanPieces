using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using QuanPieces.Pieces;

namespace QuanPieces
{
    /// <summary>
    /// A collection of small custom building pieces. Each piece lives in its own class under Pieces/
    /// and registers itself once the vanilla prefabs it clones are available.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class QuanPiecesPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "quandru.QuanPieces";
        public const string PluginName = "QuanPieces";
        public const string PluginVersion = PluginInfo.Version; // generated from <Version> in the .csproj

        // Hammer tab for every piece in this mod. Jotunn creates the tab when a piece names a category it doesn't know.
        internal const string HammerCategory = "QuanPieces";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            ApplyPatches();
            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
        }

        /// <summary>
        /// Applies each Harmony patch class on its own, so one that no longer fits the game (after an update)
        /// is logged and skipped instead of stopping the others and the pieces.
        /// </summary>
        private static void ApplyPatches()
        {
            var harmony = new Harmony(PluginGuid);
            foreach (var type in typeof(QuanPiecesPlugin).Assembly.GetTypes())
            {
                if (!type.IsDefined(typeof(HarmonyPatch), false))
                {
                    continue;
                }
                try
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
                catch (System.Exception e)
                {
                    Log.LogError($"Couldn't apply {type.Name}: {e}");
                }
            }
        }

        private void AddPieces()
        {
            // Only ever register once per session.
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;

            // Each piece registers on its own, so one failing (e.g. after a game update) can't stop the others.
            Register("chimney flue", ChimneyFlue.Register);
            Register("brazier street light", BrazierStreetlight.Register);
            Register("lantern street light", LanternStreetlight.Register);
            Register("large planter", Planter.RegisterLarge);
            Register("planter", Planter.RegisterSmall);
            Register("chicken coop floor", ChickenCoopFloor.Register);
            Register("floor heater", FloorHeater.Register);
            Register("chicken coop door", ChickenCoopDoor.Register);
            Register("chicken coop wall", ChickenCoopWall.Register);
            Register("wood wall", PlankWall.RegisterWall);
            Register("wood wall half", PlankWall.RegisterHalf);
            Register("wood wall 1x1", PlankWall.Register1x1);
            Register("wood wall 2x3", PlankWall.Register2x3);
            Register("wood wall 1x3", PlankWall.Register1x3);
            Register("triangular wood floor 1x1 (1)", TriangleFloor.Register1);
            Register("triangular wood floor 1x1 (2)", TriangleFloor.Register2);
            Register("bevelled shelf edge", TriangleFloor.RegisterBevelled);
            Register("wall item stand", NubItemStand.RegisterWall);
            Register("floor item stand", NubItemStand.RegisterFloor);
        }

        private static void Register(string piece, System.Action register)
        {
            try
            {
                register();
            }
            catch (System.Exception e)
            {
                Log.LogError($"Couldn't add the {piece}: {e}");
            }
        }
    }
}
