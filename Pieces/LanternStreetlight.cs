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
    /// A street light: a Dvergr lantern hung from a Grausten and Ashwood post with an Ashwood arch.
    ///
    /// It clones the vanilla hanging brazier for a working Fireplace (fuel, the add-fuel interaction and on/off
    /// switching, which NoSmokeStayLit needs), strips all of its fire visuals, and switches it to burn Wisps. The
    /// lantern is weatherproof: with the Fireplace's normal/low flame objects removed, it never runs its wet check.
    ///
    /// Everything visible is vanilla geometry reshaped at load time, so no Iron Gate models ship with the mod:
    /// - the Grausten Tapered Pillar as the base;
    /// - the Grausten Small Pillar's sides repeated into one 2.25 m upright and one 2.5 m beam, with no joints,
    ///   meeting in a mitred corner;
    /// - the Ashwood Pole stretched into a 4 m upright core and a 2.2 m beam core, showing through the Grausten;
    /// - the Ashwood Arch, unchanged but scaled down, with its own post and beam hidden inside the frame (its
    ///   mesh is shipped locked, so it can only be scaled and moved, not edited);
    /// - the lantern and chain lifted out of the Dvergr Pole Lantern's mesh.
    /// </summary>
    internal static class LanternStreetlight
    {
        private const string PrefabName = "Quan_streetlight_lantern";
        private const string BasePrefab = "piece_brazierceiling01";        // a working Fireplace to strip down
        private const string LanternPrefab = "piece_dvergr_lantern_pole";  // lantern, chain, light, flare, recipe
        private const string TaperedPrefab = "Piece_grausten_pillarbase_tapered";
        private const string GraustenPrefab = "Piece_grausten_pillarbase_small";
        private const string AshPolePrefab = "ashwood_pole_2m";
        private const string ArchPrefab = "ashwood_arch_big";
        private const string FuelItem = "Wisp";

        // Frame, in metres from the foot of the post; the beam runs along +x.
        private const float TaperedHeight = 2f;   // the tapered pillar's height
        // The tapered pillar's sides run to 1.986 m, then a thin bevel ring closes its top. The ring is trimmed off
        // and the Grausten upright starts there, where the two outlines are closest (6 mm apart on average). The
        // tapered pillar's top edge is then welded onto the upright's bottom edge, taking its normals, so the two
        // meet with no lip for the light to catch.
        private const float TaperedJoin = 1.986f;

        // Grausten's texture is one small repeating stone tile, which each vanilla mesh stretches by a different
        // amount, so the pieces never matched at their joins. The frame instead uses the piece shader's triplanar
        // mapping (as vanilla black marble does): the tile is projected by position in the piece's own space, at
        // the tapered pillar's density, so all the Grausten shares one seamless texture.
        private const float GraustenTilesPerMetre = 0.27f;

        // The tapered pillar's four inner star points (the stepped notches on its diagonals) are drawn in Ashwood,
        // continuing the dark wood that shows through the upright's notches down to the ground. A side face counts
        // as a notch face when its centre is within this many degrees of a diagonal. The wood uses the dark band of
        // the Ashwood texture tile (the band the Ashwood pole uses), with the grain running up the post.
        private const float NotchHalfAngle = 15f;
        private const float AshDarkBandStart = 0.5f;
        private const float AshDarkBandWidth = 0.45f;
        private const float AshGrainPerMetre = 0.5f;
        private const float GraustenHalf = 0.25f; // half the Grausten pillar's width
        private const float BeamY = 4f;           // centre line of the beam
        private const float PostTop = BeamY + GraustenHalf;
        private const float BeamEnd = 1.95f;      // free end of the Grausten beam, just short of the Ashwood core's end
        private const float AshPostHeight = 4f;
        private const float AshBeamStart = -0.2f;
        private const float AshBeamEnd = 2f;

        // The Ashwood Arch, scaled down so its own 0.4 m post and beam fit inside the frame. Measured from its
        // mesh: its post is centred at x = -0.0085 and its beam at y = 2.009 in its own space. Scaled separately
        // front to back, so its curved strut (0.27 m deep at full size) stays within the Grausten beam's raised
        // central panel (0.208 m wide) instead of cutting into the grooves beside it.
        private const float ArchScale = 0.9f;       // along the beam and up the post
        private const float ArchDepthScale = 0.75f; // front to back
        private const float ArchPostCentreX = -0.0085f;
        private const float ArchBeamCentreY = 2.009f;

        // The lantern hangs from the middle of the beam's last Grausten square, its top chain link set slightly
        // into the stone. The Dvergr pole uses its lantern mesh at twice its modelled size.
        private const float LanternScale = 2f;
        private const float LanternInset = 0.03f;

        // Where along the beam's underside the chain hangs: centred across the Grausten's raised central panel
        // (0.208 m wide), and set back from that panel's front edge (behind the 0.023 m end bevel) by the same
        // distance as from its sides.
        private const float PanelHalfWidth = 0.104f;
        private const float EndBevel = 0.023f;
        private const float LanternSetback = EndBevel + PanelHalfWidth;

        // Warmth while lit, from the hanging brazier's own warmth area. Deliberately far larger than vanilla
        // (standing brazier 5 m, sconce 6 m, hanging brazier and campfire 8 m, hearth 13 m) so whole paths stay
        // warm. No cooking area: it's a lantern.
        private const float WarmthRadius = 50f;

        // Light reach. The Dvergr lantern's own light only reaches 6 m, too short for a street light; Odin
        // Architect's lanterns reach 20 m.
        // A point light's lit pool scales with its range but fades steeply well inside it, so a street light needs a
        // long range to visibly reach far; brightness only changes how strong it is near the lamp.
        private const float LightRange = 60f;
        private const float LightIntensity = 1.5f;
        // The game switches a light off beyond 40 m from the player. With a 60 m range a lamp past that would still
        // light ground near the player and visibly pop off, so this light stays on to a greater distance.
        private const float LightSwitchOffDistance = 80f;

        public static void Register()
        {
            var basePrefab = PrefabManager.Instance.GetPrefab(BasePrefab);
            var lanternPrefab = PrefabManager.Instance.GetPrefab(LanternPrefab);
            var lanternPiece = lanternPrefab ? lanternPrefab.GetComponent<Piece>() : null;
            var fuel = PrefabManager.Instance.GetPrefab(FuelItem);
            var sources = new[] { TaperedPrefab, GraustenPrefab, AshPolePrefab, ArchPrefab }
                .ToDictionary(n => n, n => PrefabManager.Instance.GetPrefab(n));
            if (basePrefab == null || lanternPiece == null || fuel == null || sources.Values.Any(p => p == null))
            {
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: a vanilla prefab it's built from wasn't found; skipped.");
                return;
            }

            var requirements = lanternPiece.m_resources
                .Where(r => r != null && r.m_resItem != null)
                .Select(r => new RequirementConfig(r.m_resItem.name, r.m_amount, r.m_amountPerLevel, r.m_recover))
                .Concat(new[]
                {
                    new RequirementConfig("Grausten", 6, 0, true), // the frame
                    new RequirementConfig("Blackwood", 4, 0, true)
                })
                .ToArray();

            var config = new PieceConfig
            {
                Name = "Quan's Dvergr Street Light",
                Description = "A Dvergr lantern hung from a Grausten and Ashwood post. Burns wisps, and shrugs off any weather.",
                PieceTable = PieceTables.Hammer,
                Category = QuanPiecesPlugin.HammerCategory,
                CraftingStation = lanternPiece.m_craftingStation ? lanternPiece.m_craftingStation.name : string.Empty,
                Requirements = requirements
            };

            var customPiece = new CustomPiece(PrefabName, BasePrefab, config);
            if (!Build(customPiece.PiecePrefab, lanternPrefab, fuel.GetComponent<ItemDrop>(), sources))
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
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: icon render failed; using the hanging brazier's icon.");
            }

            PieceManager.Instance.AddPiece(customPiece);
        }

        private static bool Build(GameObject prefab, GameObject lanternPrefab, ItemDrop fuel, Dictionary<string, GameObject> sources)
        {
            var fireplace = prefab.GetComponent<Fireplace>();
            var lit = prefab.transform.Find("_enabled");
            if (fireplace == null || lit == null || fuel == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: the brazier's Fireplace or its '_enabled' object wasn't found; skipped.");
                return false;
            }

            var tapered = Source(sources[TaperedPrefab], "new/default_high", out _);
            var grausten = Source(sources[GraustenPrefab], "new/default_high", out var graustenMaterial);
            var ashPole = Source(sources[AshPolePrefab], "New/default", out var ashMaterial);
            var lantern = Source(lanternPrefab, "New/default", out var lanternMaterial);
            if (tapered == null || grausten == null || ashPole == null || lantern == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: a source mesh is missing or unreadable; skipped.");
                return false;
            }

            StripBrazier(prefab, lit, fireplace, fuel);

            var renderers = new List<Renderer>
            {
                AddMesh(prefab.transform, "frame_grausten", BuildGrausten(tapered, grausten, out var notches), Triplanar(graustenMaterial)),
                AddMesh(prefab.transform, "frame_ashwood", BuildAshwood(ashPole).Append(notches, Matrix4x4.identity), ashMaterial)
            };
            AddArch(prefab.transform, sources[ArchPrefab]);

            var lanternRenderer = AddLantern(prefab, lit, lanternPrefab, lantern, lanternMaterial, out var lanternBounds);
            if (lanternRenderer == null)
            {
                return false;
            }
            renderers.Add(lanternRenderer);
            AddWarmth(lit, lanternBounds.center);

            AddFrameColliders(prefab.transform, lanternBounds);
            SetLods(prefab, renderers);

            var piece = prefab.GetComponent<Piece>();
            var taperedPiece = sources[TaperedPrefab].GetComponent<Piece>();
            CopyPlacementRules(piece, taperedPiece);
            piece.m_placeEffect = taperedPiece.m_placeEffect;

            // Wood support rules, as for the other street light, so it stands on wooden floors as well as stone.
            var wearNTear = prefab.GetComponent<WearNTear>();
            var taperedWear = sources[TaperedPrefab].GetComponent<WearNTear>();
            if (wearNTear != null)
            {
                wearNTear.m_materialType = WearNTear.MaterialType.Wood;
                wearNTear.m_new = null;
                wearNTear.m_worn = null;
                wearNTear.m_broken = null;
                wearNTear.m_fragmentRoots = null;
                if (taperedWear != null)
                {
                    wearNTear.m_hitEffect = taperedWear.m_hitEffect;
                    wearNTear.m_destroyedEffect = taperedWear.m_destroyedEffect;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes every brazier visual (bowl, coals, ash, flames, smoke, its warmth and burn areas) and makes the
        /// Fireplace a weatherproof, wisp-burning lantern. Only "_enabled" (emptied, for the light) and the
        /// PlayerBase area stay.
        /// </summary>
        private static void StripBrazier(GameObject prefab, Transform lit, Fireplace fireplace, ItemDrop fuel)
        {
            foreach (var child in prefab.transform.Cast<Transform>().ToList())
            {
                if (child != lit && child.name != "PlayerBase")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
            foreach (var child in lit.Cast<Transform>().ToList())
            {
                Object.DestroyImmediate(child.gameObject);
            }
            lit.localPosition = Vector3.zero;

            // With no normal/low flame objects the Fireplace never runs its wet check, so neither rain nor wind
            // dims it. The "blocked" check would hit the post (the origin is its foot), so that's off too.
            fireplace.m_enabledObjectHigh = null;
            fireplace.m_enabledObjectLow = null;
            fireplace.m_disableCoverCheck = true;
            fireplace.m_smokeSpawner = null;
            fireplace.m_fuelItem = fuel;
        }

        /// <summary>The tapered pillar, plus the Grausten upright and beam with no joints, mitred at the corner.</summary>
        private static MeshData BuildGrausten(MeshData tapered, MeshData pillar, out MeshData notches)
        {
            // The mitre runs from the corner's outside (-half, PostTop) to its inside (half, BeamY - half).
            const float mitre = PostTop - GraustenHalf;

            // Starts on top of the tapered pillar with no end cap there, so there's no joint between them.
            var upright = new MeshData()
                .Append(pillar.ResizeAlongY(PostTop - TaperedJoin, repeat: true, keepStartCap: false, keepEndCap: true, mirrorAlternate: true),
                        Matrix4x4.Translate(new Vector3(0f, TaperedJoin, 0f)))
                .Clip(new Vector3(1f, 1f, 0f), mitre);
            // Weld both ways: the tapered pillar's edge onto the upright's, then the upright's own corner points
            // (which the tapered pillar's outline lacks) back onto that welded edge, so neither overhangs.
            var taperedBase = WeldEdge(tapered.Clip(Vector3.up, TaperedJoin), upright, TaperedJoin);
            upright = WeldEdge(upright, taperedBase, TaperedJoin);

            // Laid along -x from the free end, so the kept end cap is the free end.
            var beamTransform = Matrix4x4.TRS(new Vector3(BeamEnd, BeamY, 0f), Quaternion.Euler(0f, 0f, 90f), Vector3.one);
            var beam = new MeshData()
                .Append(pillar.ResizeAlongY(BeamEnd + GraustenHalf, repeat: true, keepStartCap: true, keepEndCap: true, mirrorAlternate: true), beamTransform)
                .Clip(new Vector3(-1f, -1f, 0f), -mitre);

            notches = Notches(taperedBase, true);
            return new MeshData()
                .Append(Notches(taperedBase, false), Matrix4x4.identity)
                .Append(upright, Matrix4x4.identity)
                .Append(beam, Matrix4x4.identity);
        }

        /// <summary>
        /// Welds <paramref name="moving"/>'s edge at height <paramref name="y"/> onto <paramref name="target"/>'s edge
        /// at the same height. Each edge vertex snaps onto the nearest edge segment of a target face pointing the same way
        /// (a carved point has several vertex copies, one per face, so matching by distance alone would hand them
        /// another face's position and shading), and takes that face's normal there. Vertices with no matching face
        /// close by are left as they are.
        /// </summary>
        private static MeshData WeldEdge(MeshData moving, MeshData target, float y)
        {
            const float tolerance = 0.0005f;
            const float maxSnap = 0.01f;
            const float minFacing = 0.8f;
            bool OnEdge(Vector3 p) => Mathf.Abs(p.y - y) < tolerance;
            Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z).normalized;

            var segments = new List<int>();
            for (var t = 0; t < target.Triangles.Count; t += 3)
            {
                for (var k = 0; k < 3; k++)
                {
                    var a = target.Triangles[t + k];
                    var b = target.Triangles[t + (k + 1) % 3];
                    if (OnEdge(target.Positions[a]) && OnEdge(target.Positions[b]) &&
                        (target.Positions[a] - target.Positions[b]).sqrMagnitude > 1e-8f)
                    {
                        segments.Add(a);
                        segments.Add(b);
                    }
                }
            }
            if (segments.Count == 0)
            {
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: no edge found to weld the tapered pillar to.");
                return moving;
            }

            var welded = 0;
            for (var i = 0; i < moving.Positions.Count; i++)
            {
                var p = moving.Positions[i];
                if (!OnEdge(p))
                {
                    continue;
                }

                var facing = Flat(moving.Normals[i]);
                var bestDistance = maxSnap * maxSnap;
                var found = false;
                var bestPosition = p;
                var bestNormal = moving.Normals[i];
                for (var s = 0; s < segments.Count; s += 2)
                {
                    int a = segments[s], b = segments[s + 1];
                    var segmentFacing = Flat(target.Normals[a] + target.Normals[b]);
                    if (Vector3.Dot(facing, segmentFacing) < minFacing)
                    {
                        continue;
                    }

                    var pa = target.Positions[a];
                    var ab = target.Positions[b] - pa;
                    var f = Mathf.Clamp01(Vector3.Dot(p - pa, ab) / ab.sqrMagnitude);
                    var q = pa + ab * f;
                    var distance = (q - p).sqrMagnitude;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPosition = q;
                        bestNormal = Vector3.Lerp(target.Normals[a], target.Normals[b], f).normalized;
                        found = true;
                    }
                }

                if (found)
                {
                    moving.Positions[i] = new Vector3(bestPosition.x, y, bestPosition.z);
                    moving.Normals[i] = bestNormal;
                    welded++;
                }
            }
            QuanPiecesPlugin.Log.LogDebug($"Lantern street light: welded {welded} edge vertices at {y} m.");
            return moving;
        }

        /// <summary>
        /// The tapered pillar's notch faces (<paramref name="inNotch"/> true), textured as dark Ashwood grain, or
        /// everything else (false). Flat ledges inside the notches stay stone.
        /// </summary>
        private static MeshData Notches(MeshData pillar, bool inNotch)
        {
            // Decide per whole flat face, not per triangle: a face's two triangles can have centres either side of
            // the angle limit, which split faces half wood, half stone.
            var faces = pillar.FlatFaces();
            var centres = new Dictionary<int, Vector3>();
            var areas = new Dictionary<int, float>();
            var upright = new Dictionary<int, bool>();
            for (var t = 0; t < faces.Length; t++)
            {
                Vector3 a = pillar.Positions[pillar.Triangles[3 * t]];
                Vector3 b = pillar.Positions[pillar.Triangles[3 * t + 1]];
                Vector3 c = pillar.Positions[pillar.Triangles[3 * t + 2]];
                var cross = Vector3.Cross(b - a, c - a);
                var area = cross.magnitude * 0.5f;
                var face = faces[t];
                centres[face] = (centres.TryGetValue(face, out var sum) ? sum : Vector3.zero) + (a + b + c) / 3f * area;
                areas[face] = (areas.TryGetValue(face, out var total) ? total : 0f) + area;
                upright[face] = Mathf.Abs(cross.normalized.y) <= 0.7f;
            }
            var notchFaces = new HashSet<int>();
            foreach (var face in areas.Keys)
            {
                if (!upright[face] || areas[face] <= 0f)
                {
                    continue; // flat ledges stay stone
                }
                var centre = centres[face] / areas[face];
                var angle = Mathf.Repeat(Mathf.Atan2(centre.z, centre.x) * Mathf.Rad2Deg, 90f);
                if (Mathf.Abs(angle - 45f) <= NotchHalfAngle)
                {
                    notchFaces.Add(face);
                }
            }

            var triangle = 0;
            var part = pillar.Where((p, n) => notchFaces.Contains(faces[triangle++]) == inNotch);
            if (!inNotch)
            {
                return part;
            }

            // Across the notch: its angle from the diagonal mapped over the dark band. Along it: height.
            for (var i = 0; i < part.Positions.Count; i++)
            {
                var p = part.Positions[i];
                var angle = Mathf.Repeat(Mathf.Atan2(p.z, p.x) * Mathf.Rad2Deg, 90f);
                var across = Mathf.Clamp01((angle - 45f + NotchHalfAngle) / (2f * NotchHalfAngle));
                part.Uvs[i] = new Vector2(AshDarkBandStart + across * AshDarkBandWidth, p.y * AshGrainPerMetre);
            }
            return part;
        }

        /// <summary>A copy of <paramref name="material"/> projecting its texture by position (see GraustenTilesPerMetre).</summary>
        private static Material Triplanar(Material material)
        {
            var copy = new Material(material) { name = material.name + "_quan_triplanar" };
            copy.SetFloat("_TriplanarMap", 1f);
            copy.SetFloat("_TriplanarScale", GraustenTilesPerMetre);
            copy.SetFloat("_TriplanarLocalPos", 1f); // fixed to the piece, so it doesn't slide when rotated or moved
            return copy;
        }

        /// <summary>The Ashwood cores: a 4 m upright and a beam, stretched so the grain runs unbroken.</summary>
        private static MeshData BuildAshwood(MeshData pole)
        {
            var upright = pole.ResizeAlongY(AshPostHeight, repeat: false, keepStartCap: true, keepEndCap: true);
            var beam = pole.ResizeAlongY(AshBeamEnd - AshBeamStart, repeat: false, keepStartCap: true, keepEndCap: true);
            return new MeshData()
                .Append(upright, Matrix4x4.identity)
                .Append(beam, Matrix4x4.TRS(new Vector3(AshBeamStart, BeamY, 0f), Quaternion.Euler(0f, 0f, -90f), Vector3.one));
        }

        /// <summary>The vanilla Ashwood Arch, scaled so its own post and beam sit inside the frame's.</summary>
        private static void AddArch(Transform parent, GameObject archPrefab)
        {
            var visual = archPrefab.transform.Find("New");
            if (visual == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: the Ashwood Arch's visual wasn't found; built without it.");
                return;
            }

            var anchor = new GameObject("arch").transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = new Vector3(-ArchScale * ArchPostCentreX, BeamY - ArchScale * ArchBeamCentreY, 0f);
            anchor.localScale = new Vector3(ArchScale, ArchScale, ArchDepthScale);

            var copy = Object.Instantiate(visual.gameObject, anchor, false);
            copy.name = "New";
        }

        /// <summary>
        /// The lantern and chain lifted from the Dvergr Pole Lantern's mesh (its pole, arm, brace and base are
        /// separate islands and are dropped), hung from the beam, with the Dvergr light and flare inside it.
        /// </summary>
        private static MeshRenderer AddLantern(GameObject prefab, Transform lit, GameObject lanternPrefab, MeshData source,
                                               Material litMaterial, out Bounds lanternBounds)
        {
            lanternBounds = default;

            // In the lantern mesh's own space the lantern and chain hang at z ~0.3-0.47, above y ~1.1. The pole and
            // brace islands reach back towards z = 0 and the base sits at the bottom.
            var mesh = source.KeepIslands(b => b.min.z >= 0.27f && b.min.y >= 1.1f, out var islands);
            if (islands.Count == 0)
            {
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: couldn't find the lantern in the Dvergr pole lantern's mesh; skipped.");
                return null;
            }

            var chainTop = islands.OrderByDescending(b => b.max.y).First();
            var body = islands.Where(b => b.max.y < 1.46f).Aggregate((a, b) => { a.Encapsulate(b); return a; });

            var hangPoint = new Vector3(BeamEnd - LanternSetback, BeamY - GraustenHalf + LanternInset, 0f);
            var topPoint = new Vector3(chainTop.center.x, chainTop.max.y, chainTop.center.z) * LanternScale;

            var lantern = new GameObject("lantern");
            lantern.transform.SetParent(prefab.transform, false);
            lantern.transform.localPosition = hangPoint - topPoint;
            lantern.transform.localScale = Vector3.one * LanternScale;
            lantern.AddComponent<MeshFilter>().sharedMesh = mesh.ToMesh("quan_streetlight_lantern");
            var renderer = lantern.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = litMaterial;

            var centre = lantern.transform.localPosition + body.center * LanternScale;
            lanternBounds = new Bounds(centre, body.size * LanternScale);

            // The Dvergr light and flare, under the Fireplace's on/off object.
            foreach (var name in new[] { "Point Light", "flare" })
            {
                var effect = lanternPrefab.transform.Find("New/" + name);
                if (effect == null)
                {
                    QuanPiecesPlugin.Log.LogWarning($"Lantern street light: the Dvergr lantern's '{name}' wasn't found.");
                    continue;
                }
                var copy = Object.Instantiate(effect.gameObject, lit, false);
                copy.name = name;
                copy.transform.localPosition = centre;
                copy.transform.localRotation = Quaternion.identity;
                var light = copy.GetComponent<Light>();
                if (light != null)
                {
                    light.range = LightRange;
                    light.intensity = LightIntensity; // LightFlicker takes this as its base when the piece loads
                }
                var lod = copy.GetComponent<LightLod>();
                if (lod != null)
                {
                    lod.m_lightDistance = LightSwitchOffDistance;
                }
            }

            // Glass glows only while lit: the vanilla "off" lantern material is this one with emission turned off.
            var unlit = new Material(litMaterial) { name = litMaterial.name + "_quan_off" };
            unlit.DisableKeyword("_EMISSION");
            unlit.SetColor("_EmissionColor", Color.black);
            var glass = prefab.AddComponent<LanternGlass>();
            glass.m_litWhileActive = lit.gameObject;
            glass.m_renderer = renderer;
            glass.m_lit = litMaterial;
            glass.m_unlit = unlit;

            return renderer;
        }

        /// <summary>
        /// The hanging brazier's warmth area (heat and fire for resting, not shelter), under the Fireplace's on/off
        /// object so it only warms while lit, centred on the lantern with its own radius.
        /// </summary>
        private static void AddWarmth(Transform lit, Vector3 centre)
        {
            var source = PrefabManager.Instance.GetPrefab(BasePrefab).transform.Find("_enabled_high/FireWarmth");
            if (source == null)
            {
                QuanPiecesPlugin.Log.LogWarning("Lantern street light: the hanging brazier's warmth area wasn't found; built without warmth.");
                return;
            }

            var warmth = Object.Instantiate(source.gameObject, lit, false);
            warmth.name = "FireWarmth";
            warmth.transform.localPosition = centre;
            warmth.transform.localScale = Vector3.one;
            var sphere = warmth.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                sphere.radius = WarmthRadius;
            }
        }

        /// <summary>Solid colliders for support, movement and the hammer; one round the lantern for adding fuel.</summary>
        private static void AddFrameColliders(Transform parent, Bounds lantern)
        {
            var width = GraustenHalf * 2f;
            AddCollider(parent, "collider_base", new Vector3(0f, TaperedHeight / 2f, 0f), new Vector3(0.8f, TaperedHeight, 0.8f));
            AddCollider(parent, "collider_post", new Vector3(0f, (TaperedHeight + PostTop) / 2f, 0f), new Vector3(width, PostTop - TaperedHeight, width));
            AddCollider(parent, "collider_beam", new Vector3((BeamEnd - GraustenHalf) / 2f, BeamY, 0f), new Vector3(BeamEnd + GraustenHalf, width, width));
            AddCollider(parent, "collider_lantern", lantern.center, lantern.size);
        }

        private static void AddCollider(Transform parent, string name, Vector3 centre, Vector3 size)
        {
            var part = new GameObject(name) { layer = parent.gameObject.layer };
            part.transform.SetParent(parent, false);
            part.transform.localPosition = centre;
            part.AddComponent<BoxCollider>().size = size;
        }

        private static MeshRenderer AddMesh(Transform parent, string name, MeshData data, Material material)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = data.ToMesh("quan_streetlight_lantern_" + name);
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }

        /// <summary>
        /// The brazier's LODGroup listed its own (now removed) renderers. One level now holds the frame and lantern,
        /// culled at the brazier's distance. The arch keeps its own LODGroup.
        /// </summary>
        private static void SetLods(GameObject prefab, List<Renderer> renderers)
        {
            var lodGroup = prefab.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                return;
            }
            var lods = lodGroup.GetLODs();
            var cullHeight = lods.Length > 0 ? lods[lods.Length - 1].screenRelativeTransitionHeight : 0.02f;
            lodGroup.SetLODs(new[] { new LOD(cullHeight, renderers.ToArray()) });
            lodGroup.RecalculateBounds();
        }

        private static void CopyPlacementRules(Piece piece, Piece from)
        {
            piece.m_groundPiece = from.m_groundPiece;
            piece.m_allowAltGroundPlacement = from.m_allowAltGroundPlacement;
            piece.m_groundOnly = from.m_groundOnly;
            piece.m_noInWater = from.m_noInWater;
            piece.m_notOnWood = from.m_notOnWood;
            piece.m_notOnTiltingSurface = from.m_notOnTiltingSurface;
            piece.m_inCeilingOnly = from.m_inCeilingOnly;
            piece.m_notOnFloor = from.m_notOnFloor;
            piece.m_noClipping = from.m_noClipping;
            piece.m_clipGround = from.m_clipGround;
            piece.m_clipEverything = from.m_clipEverything;
            piece.m_mustBeAboveConnected = from.m_mustBeAboveConnected;
        }

        /// <summary>A readable copy of the mesh at <paramref name="path"/> in a vanilla prefab, and its material.</summary>
        private static MeshData Source(GameObject prefab, string path, out Material material)
        {
            material = null;
            var node = prefab.transform.Find(path);
            var filter = node ? node.GetComponent<MeshFilter>() : null;
            var renderer = node ? node.GetComponent<MeshRenderer>() : null;
            if (filter == null || renderer == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"Lantern street light: '{prefab.name}/{path}' has no mesh.");
                return null;
            }
            var data = MeshData.From(filter.sharedMesh);
            if (data == null)
            {
                QuanPiecesPlugin.Log.LogWarning($"Lantern street light: '{prefab.name}/{path}' mesh is unreadable.");
            }
            material = renderer.sharedMaterial;
            return data;
        }
    }
}
