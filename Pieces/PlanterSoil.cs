using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// Makes a planter's soil count as cultivated ground. The soil is a solid collider named
    /// <see cref="SoilColliderName"/>; the patches in Patches/PlanterPatches.cs ask <see cref="IsOverSoil"/> whether a
    /// point sits on one, and treat it as cultivated, sunlit, any-biome ground with room to grow.
    ///
    /// Also supplies the soil's material (the terrain's own cultivated texture), and removes seedlings when the
    /// planter is destroyed, returning their seeds.
    /// </summary>
    internal sealed class PlanterSoil : MonoBehaviour
    {
        internal const string SoilColliderName = "Quan_planter_soil";
        internal const float SoilColliderDepth = 0.05f;

        // The terrain's diffuse texture array, and the slice that is cultivated soil.
        private const string TerrainDiffuseArray = "terrain_d_array";
        private const int CultivatedSlice = 10;
        private const string TerrainNormalArray = "terrain_n_array";
        private const int CultivatedNormalSlice = 5;

        // Shown until the terrain texture can be copied: roughly the cultivated soil's average colour.
        private static readonly Color FallbackSoilColour = new Color(0.22f, 0.16f, 0.08f);

        // A point counts as on the soil if this small box around it touches a soil collider.
        private static readonly Vector3 SoilProbe = new Vector3(0.05f, 0.12f, 0.05f);

        private static readonly Collider[] Hits = new Collider[32];
        private static readonly Collider[] Found = new Collider[64];
        private static int _pieceMask;
        private static int _active;
        private static Material _soilMaterial;
        private static bool _textureDone;

        private Collider _soil;

        private static int PieceMask => _pieceMask != 0 ? _pieceMask : _pieceMask = LayerMask.GetMask("piece", "piece_nonsolid");

        private void Awake()
        {
            foreach (var collider in GetComponentsInChildren<Collider>(true))
            {
                if (collider.name == SoilColliderName)
                {
                    _soil = collider;
                }
            }

            var wearNTear = GetComponent<WearNTear>();
            if (wearNTear != null)
            {
                wearNTear.m_onDestroyed += RemoveSeedlings;
            }

            ApplySoilTexture();
        }

        private void OnEnable() => _active++;

        private void OnDisable() => _active--;

        /// <summary>True if <paramref name="point"/> is on the soil of a planter.</summary>
        internal static bool IsOverSoil(Vector3 point)
        {
            // Called by Heightmap.IsCultivated and every plant's health check; skip the physics query when no
            // planter is loaded.
            if (_active <= 0)
            {
                return false;
            }

            var count = Physics.OverlapBoxNonAlloc(point, SoilProbe, Hits, Quaternion.identity, PieceMask, QueryTriggerInteraction.Collide);
            for (var i = 0; i < count; i++)
            {
                if (Hits[i] != null && Hits[i].name == SoilColliderName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Where <paramref name="ray"/> meets this planter's soil, if that's the surface the game's own ray hit at
        /// <paramref name="hitPoint"/> (and not the rim in front of it).
        /// </summary>
        internal bool TryHitSoil(Ray ray, Vector3 hitPoint, out Vector3 soilPoint)
        {
            soilPoint = hitPoint;
            if (_soil == null || !_soil.Raycast(ray, out var hit, 60f) || (hit.point - hitPoint).sqrMagnitude > 0.0025f)
            {
                return false;
            }
            soilPoint = hit.point;
            return true;
        }

        /// <summary>
        /// When the planter is destroyed or removed, its seedlings would be left floating. Remove them and drop
        /// their seeds. Grown crops are left: with no soil under them they fall to the ground like any other.
        /// </summary>
        private void RemoveSeedlings()
        {
            if (_soil == null)
            {
                return;
            }

            var bounds = _soil.bounds;
            bounds.Expand(new Vector3(0f, 0.6f, 0f));
            var count = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, Found, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
            var plants = new HashSet<Plant>();
            for (var i = 0; i < count; i++)
            {
                var plant = Found[i] != null ? Found[i].GetComponentInParent<Plant>() : null;
                if (plant != null)
                {
                    plants.Add(plant);
                }
            }

            foreach (var plant in plants)
            {
                var nview = plant.GetComponent<ZNetView>();
                if (nview == null || !nview.IsValid())
                {
                    continue;
                }
                DropSeeds(plant);
                nview.ClaimOwnership();
                ZNetScene.instance.Destroy(plant.gameObject);
            }
        }

        /// <summary>
        /// Drops what the seedling was planted with. Piece.DropResources won't: saplings' costs aren't marked
        /// recoverable, because vanilla never lets you remove one.
        /// </summary>
        private static void DropSeeds(Plant plant)
        {
            var piece = plant.GetComponent<Piece>();
            if (piece == null || piece.m_resources == null || ObjectDB.instance == null)
            {
                return;
            }

            var position = plant.transform.position + Vector3.up * 0.3f;
            foreach (var requirement in piece.m_resources)
            {
                if (requirement == null || requirement.m_resItem == null || requirement.m_amount <= 0)
                {
                    continue;
                }
                var itemPrefab = ObjectDB.instance.GetItemPrefab(requirement.m_resItem.name);
                if (itemPrefab == null)
                {
                    continue;
                }
                var drop = Instantiate(itemPrefab, position, Quaternion.identity).GetComponent<ItemDrop>();
                if (drop != null)
                {
                    drop.SetStack(requirement.m_amount);
                    ItemDrop.OnCreateNew(drop);
                }
            }
        }

        /// <summary>
        /// The soil material, shared by every planter: a copy of the wood floor's piece material (so it takes
        /// light, rain and wetness like other pieces) with the cultivated soil texture and no vertex wobble.
        /// </summary>
        internal static Material GetSoilMaterial(Material pieceMaterial)
        {
            if (_soilMaterial != null)
            {
                return _soilMaterial;
            }

            _soilMaterial = new Material(pieceMaterial) { name = "Quan_planter_soil" };
            if (_soilMaterial.HasProperty("_BumpMap"))
            {
                _soilMaterial.SetTexture("_BumpMap", null);
            }
            if (_soilMaterial.HasProperty("_RippleDistance"))
            {
                _soilMaterial.SetFloat("_RippleDistance", 0f);
            }
            if (_soilMaterial.HasProperty("_Glossiness"))
            {
                _soilMaterial.SetFloat("_Glossiness", 0.1f);
            }
            // The terrain texture's alpha holds other data, not transparency, but the piece shader cuts out
            // pixels below _Cutoff alpha, which punched holes in the soil. Keep every pixel.
            if (_soilMaterial.HasProperty("_Cutoff"))
            {
                _soilMaterial.SetFloat("_Cutoff", 0f);
            }
            _soilMaterial.mainTexture = null;
            _soilMaterial.color = FallbackSoilColour;
            ApplySoilTexture();
            return _soilMaterial;
        }

        /// <summary>
        /// The cultivated soil's colour and bump (normal) textures exist only as slices of the terrain's texture
        /// arrays, so they're copied out on the GPU into textures of their own. That needs the arrays loaded, which
        /// may not be the case until a world is, so it's retried whenever a planter appears.
        /// </summary>
        private static void ApplySoilTexture()
        {
            if (_textureDone || _soilMaterial == null)
            {
                return;
            }

            Texture2DArray diffuse = null;
            Texture2DArray normals = null;
            foreach (var candidate in Resources.FindObjectsOfTypeAll<Texture2DArray>())
            {
                if (candidate.name == TerrainDiffuseArray)
                {
                    diffuse = candidate;
                }
                else if (candidate.name == TerrainNormalArray)
                {
                    normals = candidate;
                }
            }
            if (diffuse == null)
            {
                return;
            }

            _textureDone = true;
            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
            {
                QuanPiecesPlugin.Log.LogWarning("Planter: this GPU can't copy textures; the soil keeps a plain colour.");
                return;
            }

            var colour = CopySlice(diffuse, CultivatedSlice, "Quan_planter_soil");
            if (colour == null)
            {
                return;
            }
            _soilMaterial.mainTexture = colour;
            _soilMaterial.color = Color.white;

            // The clods and pocks. Without it the soil looks flat; with no normal array the soil just stays flat.
            var bump = normals != null ? CopySlice(normals, CultivatedNormalSlice, "Quan_planter_soil_n") : null;
            if (bump != null && _soilMaterial.HasProperty("_BumpMap"))
            {
                _soilMaterial.SetTexture("_BumpMap", bump);
                _soilMaterial.EnableKeyword("_NORMALMAP");
            }
        }

        private static Texture2D CopySlice(Texture2DArray array, int slice, string name)
        {
            if (array.depth <= slice)
            {
                QuanPiecesPlugin.Log.LogWarning($"Planter: '{array.name}' has no slice {slice}; the soil is missing part of its texture.");
                return null;
            }

            try
            {
                var texture = new Texture2D(array.width, array.height, array.graphicsFormat, TextureCreationFlags.None)
                {
                    name = name,
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = array.filterMode,
                    anisoLevel = array.anisoLevel
                };
                Graphics.CopyTexture(array, slice, 0, texture, 0, 0);
                return texture;
            }
            catch (System.Exception e)
            {
                QuanPiecesPlugin.Log.LogWarning($"Planter: copying slice {slice} of '{array.name}' failed ({e.Message}); the soil is missing part of its texture.");
                return null;
            }
        }
    }
}
