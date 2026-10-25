using BepInEx;
using BepInEx.Logging;
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
            PrefabManager.OnVanillaPrefabsAvailable += AddPieces;
        }

        private void AddPieces()
        {
            // Only ever register once per session.
            PrefabManager.OnVanillaPrefabsAvailable -= AddPieces;

            ChimneyFlue.Register();
        }
    }
}
