using System.Linq;
using Jotunn.Configs;
using Jotunn.Managers;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// What the chicken coop's door and wall share: they're iron pieces remade in wood. Wood materials come from
    /// the vanilla wood pole, and wood support, sounds and break effects from the vanilla wood door.
    /// </summary>
    internal static class CoopWood
    {
        private const string PolePrefab = "wood_pole"; // its 'New' and 'WornBroken' renderers: woodpole materials
        private const string DoorPrefab = "wood_door"; // wood support, hit/break/place effects, door sounds

        // Same as the Cage Wall 2x2 (2 Iron at a Forge), in wood.
        public static RequirementConfig[] Cost => new[] { new RequirementConfig("Wood", 2, 0, true) };
        public static string Station => CraftingStations.Workbench;

        /// <summary>The wood pole's material, for new pieces, and its worn one, for damaged ones.</summary>
        public static bool TryGetMaterials(out Material wood, out Material worn)
        {
            var pole = PrefabManager.Instance.GetPrefab(PolePrefab);
            var renderers = pole ? pole.GetComponentsInChildren<MeshRenderer>(true) : new MeshRenderer[0];
            wood = Meshes.WoodGrain.Steady(renderers.FirstOrDefault(r => r.name == "New")?.sharedMaterial);
            worn = Meshes.WoodGrain.Steady(renderers.FirstOrDefault(r => r.name == "WornBroken")?.sharedMaterial) ?? wood;
            return wood != null;
        }

        /// <summary>
        /// Wood rules for support (iron needs far more support than wooden walls give) and the wood door's
        /// sounds and effects for placing, hitting, breaking and (for doors) opening and closing.
        /// </summary>
        public static void MakeWooden(GameObject prefab)
        {
            var source = PrefabManager.Instance.GetPrefab(DoorPrefab);
            var wearNTear = prefab.GetComponent<WearNTear>();
            var sourceWear = source ? source.GetComponent<WearNTear>() : null;
            if (wearNTear != null)
            {
                wearNTear.m_materialType = WearNTear.MaterialType.Wood;
                if (sourceWear != null)
                {
                    wearNTear.m_hitEffect = sourceWear.m_hitEffect;
                    wearNTear.m_destroyedEffect = sourceWear.m_destroyedEffect;
                    wearNTear.m_damages = sourceWear.m_damages;
                }
            }

            var piece = prefab.GetComponent<Piece>();
            var sourcePiece = source ? source.GetComponent<Piece>() : null;
            if (piece != null && sourcePiece != null)
            {
                piece.m_placeEffect = sourcePiece.m_placeEffect;
            }

            var door = prefab.GetComponent<Door>();
            var sourceDoor = source ? source.GetComponent<Door>() : null;
            if (door != null && sourceDoor != null)
            {
                door.m_openEffects = sourceDoor.m_openEffects;
                door.m_closeEffects = sourceDoor.m_closeEffects;
            }
        }
    }
}
