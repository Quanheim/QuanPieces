using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// A street light: the vanilla hanging brazier on a wooden post and arm.
    ///
    /// It clones the hanging brazier so its working Fireplace comes along: Coal fuel, the add-fuel interaction, and
    /// the switching of light, flames, warmth and smoke. That Fireplace is what NoSmokeStayLit patches, so its
    /// timer and keep-lit lists work on this piece by prefab name (Odin Architect's lanterns have no Fireplace,
    /// which is why the timer ignores them).
    ///
    /// The brazier's parts move into a child hung from the arm, and the piece's origin becomes the foot of the
    /// post, so it places like a post. The wooden frame reuses the vanilla pole and beam: the game's unit cube with
    /// the pole and beam materials, placed and scaled exactly, so it matches placed vanilla pieces (including
    /// their vertex wobble).
    /// </summary>
    internal static class BrazierStreetlight
    {
        private const string PrefabName = "Quan_streetlight_brazier";
        private const string BasePrefab = "piece_brazierceiling01"; // vanilla hanging brazier
        private const string PostPrefab = "wood_pole2";             // 2 m pole: cube mesh, woodpole2 material, placement rules
        private const string BeamPrefab = "wood_beam";              // 2 m beam: woodwall material

        // Frame, in metres from the foot of the post. The arm runs along +x from the top of the post.
        private const float PoleWidth = 0.4f;
        private const float PostHeight = 4f;
        private const float ArmLength = 2f;
        private const float BraceScale = 0.75f;      // brace thickness relative to the post, so it sits inside it
        private const float FootHeight = 2f;
        private const float FootOverlap = 0.75f;     // share of each foot pole's width hidden inside the post
        private const float FootInset = 0.002f;      // keeps foot poles' side faces off the post's faces (no flicker)
        private const float BrazierOut = 1.6f;       // brazier distance along the arm from the post's centre

        // The brazier's mesh, in its own space: the vertical chain is 12 links on the centre line, above the point
        // where the four diagonal chains begin. Links whose lowest point is above KeepChainBelow are removed, which
        // keeps the bottom three. ChainHangPoint (the middle of the top kept link) sits at the arm's underside, so
        // that link half disappears into the wood.
        private const float KeepChainBelow = -0.70f;
        private const float ChainHangPoint = -0.6845f;
        private const float ChainAxisTolerance = 0.02f;

        // Light reach while burning normally (the hanging brazier's own is 10 m). Its low, wet flame keeps its own.
        private const float LightRange = 12f;

        public static void Register()
        {
            var basePrefab = PrefabManager.Instance.GetPrefab(BasePrefab);
            var basePiece = basePrefab ? basePrefab.GetComponent<Piece>() : null;
            var post = PrefabManager.Instance.GetPrefab(PostPrefab);
            var beam = PrefabManager.Instance.GetPrefab(BeamPrefab);
            if (basePiece == null || post == null || beam == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"'{BasePrefab}', '{PostPrefab}' or '{BeamPrefab}' not found; brazier street light skipped.");
                return;
            }

            var requirements = basePiece.m_resources
                .Where(r => r != null && r.m_resItem != null)
                .Select(r => new RequirementConfig(r.m_resItem.name, r.m_amount, r.m_amountPerLevel, r.m_recover))
                .Concat(new[] { new RequirementConfig("Wood", 4, 0, true) }) // the frame
                .ToArray();

            var config = new PieceConfig
            {
                Name = "Quan's Brazier Street Light",
                Description = "A coal-fired brazier hung from a wooden post, for lighting paths and streets.",
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = basePiece.m_craftingStation ? basePiece.m_craftingStation.name : string.Empty,
                Requirements = requirements
            };

            var customPiece = new CustomPiece(PrefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, post, beam))
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
                QuanPiecesPlugin.Log.LogWarning("Brazier street light: icon render failed; using the hanging brazier's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, GameObject post, GameObject beam)
        {
            var fireplace = prefab.GetComponent<Fireplace>();
            var postVisual = FindVisual(post);
            var beamVisual = FindVisual(beam);
            if (fireplace == null || postVisual == null || beamVisual == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Brazier street light: Fireplace or the pole/beam visuals not found; skipped.");
                return false;
            }

            // Move everything the brazier had into a child hung from the arm. Fireplace and WearNTear refer to
            // these objects directly, so they keep working; only their positions change.
            var armUnderside = PostHeight - PoleWidth / 2f;
            var brazier = new GameObject("brazier").transform;
            var children = prefab.transform.Cast<Transform>().ToList();
            brazier.SetParent(prefab.transform, false);
            foreach (var child in children)
            {
                child.SetParent(brazier, false);
            }

            var brazierMesh = brazier.Find("New");
            if (brazierMesh == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Brazier street light: the brazier's 'New' mesh wasn't found; skipped.");
                return false;
            }
            brazier.localPosition = new Vector3(BrazierOut, armUnderside - (brazierMesh.localPosition.y + ChainHangPoint), 0f);
            ShortenChain(brazierMesh);

            var light = brazier.Find("_enabled_high/Point light");
            if (light != null && light.GetComponent<Light>() is Light lamp)
            {
                lamp.range = LightRange;
            }

            // The brazier's collider along its top chain would now poke up through the arm.
            var chainCollider = brazier.Find("collider (5)");
            if (chainCollider != null)
            {
                Object.DestroyImmediate(chainCollider.gameObject);
            }

            var renderers = BuildFrame(prefab.transform, postVisual, beamVisual);
            AddFrameColliders(prefab.transform);
            IncludeInLods(prefab, renderers);
            CopyPlacementRules(prefab.GetComponent<Piece>(), post.GetComponent<Piece>());

            // The Fireplace's "something solid just above me" check starts at the piece's origin, which is now the
            // foot of the post, so it would always hit the post and never light. The rain check is separate and
            // still runs: the arm above counts as a roof, so only strong wind dims it, like any covered fire.
            fireplace.m_disableCoverCheck = true;

            // NoSmokeStayLit's no-smoke option finds a brazier's smoke by name ("piece_brazier...") and otherwise
            // uses Fireplace.m_smokeSpawner, which the hanging brazier leaves empty. Point it at the brazier's own.
            if (fireplace.m_smokeSpawner == null)
            {
                fireplace.m_smokeSpawner = brazier.GetComponentInChildren<SmokeSpawner>(true);
            }

            // Mostly wood now: wood support rules, and the pole's sounds for placing, hitting and breaking it.
            var wearNTear = prefab.GetComponent<WearNTear>();
            var postWear = post.GetComponent<WearNTear>();
            if (wearNTear != null)
            {
                wearNTear.m_materialType = WearNTear.MaterialType.Wood;
                if (postWear != null)
                {
                    wearNTear.m_hitEffect = postWear.m_hitEffect;
                    wearNTear.m_destroyedEffect = postWear.m_destroyedEffect;
                }
            }
            prefab.GetComponent<Piece>().m_placeEffect = post.GetComponent<Piece>().m_placeEffect;

            return true;
        }

        /// <summary>
        /// Replaces the brazier's mesh with a copy missing the top of its vertical chain. Each chain link is a
        /// separate island of triangles, so the mesh is split into islands and the high ones on the centre line
        /// are dropped. Vertices are joined by position, because a link's own vertices are split along UV seams.
        /// </summary>
        private static void ShortenChain(Transform meshObject)
        {
            var filter = meshObject.GetComponent<MeshFilter>();
            var source = filter ? filter.sharedMesh : null;
            if (source == null || !source.isReadable)
            {
                QuanPiecesPlugin.Log.LogWarning("Brazier street light: the brazier mesh can't be read; keeping its full chain.");
                return;
            }

            var vertices = source.vertices;
            var parent = Enumerable.Range(0, vertices.Length).ToArray();
            int Root(int i)
            {
                while (parent[i] != i)
                {
                    parent[i] = parent[parent[i]];
                    i = parent[i];
                }
                return i;
            }
            void Join(int a, int b) => parent[Root(a)] = Root(b);

            var byPosition = new Dictionary<Vector3Int, int>();
            for (var i = 0; i < vertices.Length; i++)
            {
                var key = Vector3Int.RoundToInt(vertices[i] * 10000f);
                if (byPosition.TryGetValue(key, out var other))
                {
                    Join(i, other);
                }
                else
                {
                    byPosition[key] = i;
                }
            }

            var submeshTriangles = Enumerable.Range(0, source.subMeshCount).Select(source.GetTriangles).ToList();
            foreach (var triangles in submeshTriangles)
            {
                for (var t = 0; t < triangles.Length; t += 3)
                {
                    Join(triangles[t], triangles[t + 1]);
                    Join(triangles[t], triangles[t + 2]);
                }
            }

            var islands = new Dictionary<int, Bounds>();
            for (var i = 0; i < vertices.Length; i++)
            {
                var root = Root(i);
                if (islands.TryGetValue(root, out var bounds))
                {
                    bounds.Encapsulate(vertices[i]);
                    islands[root] = bounds;
                }
                else
                {
                    islands[root] = new Bounds(vertices[i], Vector3.zero);
                }
            }
            var dropped = new HashSet<int>(islands
                .Where(island => island.Value.min.y > KeepChainBelow
                                 && Mathf.Abs(island.Value.center.x) < ChainAxisTolerance
                                 && Mathf.Abs(island.Value.center.z) < ChainAxisTolerance)
                .Select(island => island.Key));

            var mesh = Object.Instantiate(source);
            mesh.name = source.name + "_quan_shortchain";
            for (var s = 0; s < submeshTriangles.Count; s++)
            {
                var kept = new List<int>(submeshTriangles[s].Length);
                var triangles = submeshTriangles[s];
                for (var t = 0; t < triangles.Length; t += 3)
                {
                    if (!dropped.Contains(Root(triangles[t])))
                    {
                        kept.Add(triangles[t]);
                        kept.Add(triangles[t + 1]);
                        kept.Add(triangles[t + 2]);
                    }
                }
                mesh.SetTriangles(kept, s);
            }
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;

            QuanPiecesPlugin.Log.LogDebug($"Brazier street light: removed {dropped.Count} chain links from the brazier mesh.");
        }

        /// <summary>The post, arm, brace and four foot poles, as copies of the vanilla pole and beam visuals.</summary>
        private static List<Renderer> BuildFrame(Transform parent, Transform postVisual, Transform beamVisual)
        {
            var renderers = new List<Renderer>();
            var upright = Quaternion.Euler(0f, 0f, 90f); // the pole mesh runs along x; vanilla stands it up like this
            var half = PoleWidth / 2f;

            // Post: two 2 m poles stacked, as in game.
            AddPart(parent, "post_lower", postVisual, new Vector3(0f, 1f, 0f), upright, new Vector3(2f, PoleWidth, PoleWidth), renderers);
            AddPart(parent, "post_upper", postVisual, new Vector3(0f, 3f, 0f), upright, new Vector3(2f, PoleWidth, PoleWidth), renderers);

            // Arm: a 2 m beam centred on the top of the post's centre line, running out along +x.
            AddPart(parent, "arm", beamVisual, new Vector3(ArmLength / 2f, PostHeight, 0f), Quaternion.identity,
                    new Vector3(ArmLength, PoleWidth, PoleWidth), renderers);

            // Brace: one pole from the post 1 m below the top to the arm's midpoint, slimmer so it sits inside both.
            var from = new Vector2(0f, PostHeight - ArmLength / 2f);
            var to = new Vector2(ArmLength / 2f, PostHeight);
            var span = to - from;
            var braceWidth = PoleWidth * BraceScale;
            AddPart(parent, "brace", postVisual, (Vector3)((from + to) / 2f), Quaternion.Euler(0f, 0f, Mathf.Atan2(span.y, span.x) * Mathf.Rad2Deg),
                    new Vector3(span.magnitude, braceWidth, braceWidth), renderers);

            // Foot: a 2 m pole against each face, three quarters inside the post. Each is a hair narrower across
            // the post's face so their side faces don't sit exactly on the post's and flicker.
            var offset = half - PoleWidth * FootOverlap + half;
            var narrow = PoleWidth - FootInset * 2f;
            // With the upright rotation, scale is (height, width along x, width along z).
            AddPart(parent, "foot_east", postVisual, new Vector3(offset, FootHeight / 2f, 0f), upright, new Vector3(FootHeight, PoleWidth, narrow), renderers);
            AddPart(parent, "foot_west", postVisual, new Vector3(-offset, FootHeight / 2f, 0f), upright, new Vector3(FootHeight, PoleWidth, narrow), renderers);
            AddPart(parent, "foot_north", postVisual, new Vector3(0f, FootHeight / 2f, offset), upright, new Vector3(FootHeight, narrow, PoleWidth), renderers);
            AddPart(parent, "foot_south", postVisual, new Vector3(0f, FootHeight / 2f, -offset), upright, new Vector3(FootHeight, narrow, PoleWidth), renderers);

            return renderers;
        }

        /// <summary>Solid colliders for the frame, so it has support, blocks movement and can be hit and removed.</summary>
        private static void AddFrameColliders(Transform parent)
        {
            var footWidth = PoleWidth + 2f * (PoleWidth * (1f - FootOverlap));
            AddCollider(parent, "collider_post", new Vector3(0f, PostHeight / 2f, 0f), Quaternion.identity, new Vector3(PoleWidth, PostHeight, PoleWidth));
            AddCollider(parent, "collider_foot", new Vector3(0f, FootHeight / 2f, 0f), Quaternion.identity, new Vector3(footWidth, FootHeight, footWidth));
            // The arm's collider also covers the top of the post, so the fire's roof check (which looks straight up
            // from the post's centre line) always finds it.
            var armStart = -PoleWidth / 2f;
            AddCollider(parent, "collider_arm", new Vector3((armStart + ArmLength) / 2f, PostHeight, 0f), Quaternion.identity,
                        new Vector3(ArmLength - armStart, PoleWidth, PoleWidth));
        }

        private static void AddPart(Transform parent, string name, Transform visual, Vector3 position, Quaternion rotation,
                                    Vector3 scale, List<Renderer> renderers)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            part.AddComponent<MeshFilter>().sharedMesh = visual.GetComponent<MeshFilter>().sharedMesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = visual.GetComponent<MeshRenderer>().sharedMaterials;
            renderers.Add(renderer);
        }

        private static void AddCollider(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 size)
        {
            var part = new GameObject(name) { layer = parent.gameObject.layer };
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.AddComponent<BoxCollider>().size = size;
        }

        /// <summary>Adds the frame to every level of the brazier's LODGroup, so the whole light fades out together.</summary>
        private static void IncludeInLods(GameObject prefab, List<Renderer> frame)
        {
            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                return;
            }

            var lods = lodGroup.GetLODs();
            for (var i = 0; i < lods.Length; i++)
            {
                lods[i].renderers = lods[i].renderers.Where(r => r != null).Concat(frame).ToArray();
            }
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
        }

        /// <summary>The hanging brazier is meant for ceilings; take the 2 m pole's placement rules instead.</summary>
        private static void CopyPlacementRules(Piece piece, Piece post)
        {
            piece.m_groundPiece = post.m_groundPiece;
            piece.m_allowAltGroundPlacement = post.m_allowAltGroundPlacement;
            piece.m_groundOnly = post.m_groundOnly;
            piece.m_noInWater = post.m_noInWater;
            piece.m_notOnWood = post.m_notOnWood;
            piece.m_notOnTiltingSurface = post.m_notOnTiltingSurface;
            piece.m_inCeilingOnly = post.m_inCeilingOnly;
            piece.m_notOnFloor = post.m_notOnFloor;
            piece.m_noClipping = post.m_noClipping;
            piece.m_clipGround = post.m_clipGround;
            piece.m_clipEverything = post.m_clipEverything;
            piece.m_mustBeAboveConnected = post.m_mustBeAboveConnected;
        }

        /// <summary>The visible mesh object of a vanilla piece: its first child with a MeshFilter and a renderer.</summary>
        private static Transform FindVisual(GameObject piece)
        {
            return piece.GetComponentsInChildren<MeshFilter>(true)
                .Select(f => f.transform)
                .FirstOrDefault(t => (t.name == "New" || t.name == "new") && t.GetComponent<MeshRenderer>() != null);
        }
    }
}
