using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuanPieces.Meshes
{
    /// <summary>
    /// A plain, editable copy of a mesh's positions, normals, first UV set and triangles (all submeshes merged),
    /// for reshaping readable vanilla meshes into kitbashed pieces: trimming islands, lengthening extrusions,
    /// cutting along a plane and merging parts into one mesh.
    /// </summary>
    internal sealed class MeshData
    {
        public readonly List<Vector3> Positions = new List<Vector3>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public readonly List<Vector2> Uvs = new List<Vector2>();
        public readonly List<int> Triangles = new List<int>();

        public int TriangleCount => Triangles.Count / 3;

        // A plain struct rather than a tuple: Valheim's runtime has no System.ValueTuple.
        private struct Corner
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 Uv;

            public Corner(Vector3 position, Vector3 normal, Vector2 uv)
            {
                Position = position;
                Normal = normal;
                Uv = uv;
            }
        }

        /// <summary>Copies a readable mesh. Returns null if the game ships it unreadable.</summary>
        public static MeshData From(Mesh mesh)
        {
            if (mesh == null || !mesh.isReadable)
            {
                return null;
            }

            var data = new MeshData();
            data.Positions.AddRange(mesh.vertices);
            var normals = mesh.normals;
            data.Normals.AddRange(normals.Length == data.Positions.Count ? normals : new Vector3[data.Positions.Count]);
            var uvs = mesh.uv;
            data.Uvs.AddRange(uvs.Length == data.Positions.Count ? uvs : new Vector2[data.Positions.Count]);
            for (var s = 0; s < mesh.subMeshCount; s++)
            {
                data.Triangles.AddRange(mesh.GetTriangles(s));
            }
            return data;
        }

        private int AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            Positions.Add(position);
            Normals.Add(normal);
            Uvs.Add(uv);
            return Positions.Count - 1;
        }

        /// <summary>Appends <paramref name="other"/>, moved by <paramref name="transform"/>, into this mesh.</summary>
        public MeshData Append(MeshData other, Matrix4x4 transform)
        {
            var normalTransform = transform.inverse.transpose;
            var offset = Positions.Count;
            for (var i = 0; i < other.Positions.Count; i++)
            {
                AddVertex(transform.MultiplyPoint3x4(other.Positions[i]),
                          normalTransform.MultiplyVector(other.Normals[i]).normalized,
                          other.Uvs[i]);
            }
            Triangles.AddRange(other.Triangles.Select(t => t + offset));
            return this;
        }

        public Mesh ToMesh(string name)
        {
            var mesh = new Mesh { name = name };
            if (Positions.Count > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.SetVertices(Positions);
            mesh.SetNormals(Normals);
            mesh.SetUVs(0, Uvs);
            mesh.SetTriangles(Triangles, 0);
            mesh.RecalculateTangents(); // the piece shader uses normal maps
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Splits the mesh into islands (triangles joined by shared vertices or shared positions, since a single
        /// part's vertices are often split along UV seams) and keeps only the islands whose bounds pass
        /// <paramref name="keep"/>. The kept islands' bounds are returned through <paramref name="kept"/>.
        /// </summary>
        public MeshData KeepIslands(Func<Bounds, bool> keep, out List<Bounds> kept)
        {
            var roots = IslandRoots();
            int Root(int i) => roots[i];

            var islands = new Dictionary<int, Bounds>();
            for (var i = 0; i < Positions.Count; i++)
            {
                var root = Root(i);
                if (islands.TryGetValue(root, out var bounds))
                {
                    bounds.Encapsulate(Positions[i]);
                    islands[root] = bounds;
                }
                else
                {
                    islands[root] = new Bounds(Positions[i], Vector3.zero);
                }
            }

            var keptRoots = new HashSet<int>(islands.Where(island => keep(island.Value)).Select(island => island.Key));
            kept = keptRoots.Select(root => islands[root]).ToList();

            var result = new MeshData();
            var remap = new Dictionary<int, int>();
            for (var t = 0; t < Triangles.Count; t += 3)
            {
                if (!keptRoots.Contains(Root(Triangles[t])))
                {
                    continue;
                }
                for (var k = 0; k < 3; k++)
                {
                    var source = Triangles[t + k];
                    if (!remap.TryGetValue(source, out var index))
                    {
                        index = result.AddVertex(Positions[source], Normals[source], Uvs[source]);
                        remap[source] = index;
                    }
                    result.Triangles.Add(index);
                }
            }
            return result;
        }

        /// <summary>
        /// Like <see cref="Clip"/>, but closes the cut: each island's cross-section on the plane gets a flat face
        /// pointing out of the cut, so a cut box stays solid. The new faces' UVs map the wood texture flat across
        /// the cut (end grain needs no particular direction). Each island must be convex, as planks and boxes are.
        /// </summary>
        public MeshData Cut(Vector3 normal, float distance)
        {
            normal = normal.normalized;
            var result = Clip(normal, distance);
            var roots = result.IslandRoots();

            // A basis on the plane: along it horizontally, and up it.
            var along = Vector3.Cross(Vector3.up, normal);
            along = along.sqrMagnitude > 1e-6f ? along.normalized : Vector3.right;
            var up = Vector3.Cross(normal, along);

            var onPlane = new Dictionary<int, List<Vector2>>();
            for (var i = 0; i < result.Positions.Count; i++)
            {
                var p = result.Positions[i];
                if (Mathf.Abs(Vector3.Dot(normal, p) - distance) > 1e-4f)
                {
                    continue;
                }
                if (!onPlane.TryGetValue(roots[i], out var points))
                {
                    onPlane[roots[i]] = points = new List<Vector2>();
                }
                points.Add(new Vector2(Vector3.Dot(p, along), Vector3.Dot(p, up)));
            }

            var origin = normal * distance;
            foreach (var points in onPlane.Values)
            {
                var hull = ConvexHull(points);
                if (hull.Count < 3)
                {
                    continue;
                }
                var corners = hull.Select(h => origin + along * h.x + up * h.y).ToList();
                for (var k = 1; k + 1 < corners.Count; k++)
                {
                    var a = corners[0];
                    var b = corners[k];
                    var c = corners[k + 1];
                    if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0f)
                    {
                        var swap = b;
                        b = c;
                        c = swap;
                    }
                    foreach (var p in new[] { a, b, c })
                    {
                        result.Triangles.Add(result.AddVertex(p, normal,
                            new Vector2(Vector3.Dot(p, along) / 2f, Vector3.Dot(p, up) / 0.4f)));
                    }
                }
            }
            return result;
        }

        /// <summary>The convex hull of 2D points (Andrew's monotone chain), in order around it.</summary>
        private static List<Vector2> ConvexHull(List<Vector2> points)
        {
            var sorted = points.Distinct().OrderBy(p => p.x).ThenBy(p => p.y).ToList();
            if (sorted.Count < 3)
            {
                return sorted;
            }
            float Turn(Vector2 o, Vector2 a, Vector2 b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
            var hull = new List<Vector2>();
            foreach (var pass in new[] { sorted, Enumerable.Reverse(sorted).ToList() })
            {
                var start = hull.Count;
                foreach (var p in pass)
                {
                    while (hull.Count >= start + 2 && Turn(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 1e-9f)
                    {
                        hull.RemoveAt(hull.Count - 1);
                    }
                    hull.Add(p);
                }
                hull.RemoveAt(hull.Count - 1);
            }
            return hull;
        }

        /// <summary>
        /// Each vertex's island, as a representative vertex index: vertices are joined by shared triangles and by
        /// shared positions (a part's vertices are often split along UV seams).
        /// </summary>
        private int[] IslandRoots()
        {
            var parent = Enumerable.Range(0, Positions.Count).ToArray();
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
            for (var i = 0; i < Positions.Count; i++)
            {
                var key = Vector3Int.RoundToInt(Positions[i] * 10000f);
                if (byPosition.TryGetValue(key, out var other))
                {
                    Join(i, other);
                }
                else
                {
                    byPosition[key] = i;
                }
            }
            for (var t = 0; t < Triangles.Count; t += 3)
            {
                Join(Triangles[t], Triangles[t + 1]);
                Join(Triangles[t], Triangles[t + 2]);
            }

            return Enumerable.Range(0, Positions.Count).Select(Root).ToArray();
        }

        /// <summary>
        /// Resizes a mesh that is extruded along its local Y axis (end caps in thin bands at each end, side faces
        /// spanning the whole length) to <paramref name="length"/>, starting at y = 0. With
        /// <paramref name="repeat"/>, the side faces are repeated end to end so their texture keeps its scale
        /// (stone panels); otherwise they are stretched (wood grain). Either end cap can be dropped, which removes
        /// the joint where another part continues the shape. With <paramref name="mirrorAlternate"/>, every other
        /// repeat is flipped end for end, so the texture always meets a copy of its own edge and no line shows
        /// where repeats meet.
        /// </summary>
        public MeshData ResizeAlongY(float length, bool repeat, bool keepStartCap, bool keepEndCap,
                                     bool mirrorAlternate = false, float capBand = 0.05f)
        {
            var minY = Positions.Min(p => p.y);
            var maxY = Positions.Max(p => p.y);

            var startCap = new List<int>();
            var endCap = new List<int>();
            var middle = new List<int>();
            for (var t = 0; t < Triangles.Count; t += 3)
            {
                var ys = new[] { Positions[Triangles[t]].y, Positions[Triangles[t + 1]].y, Positions[Triangles[t + 2]].y };
                var list = ys.Max() <= minY + capBand ? startCap : ys.Min() >= maxY - capBand ? endCap : middle;
                list.Add(t);
            }
            if (middle.Count == 0)
            {
                throw new InvalidOperationException("Mesh has no faces spanning its length; it isn't a simple extrusion.");
            }

            var a = middle.SelectMany(t => new[] { Positions[Triangles[t]].y, Positions[Triangles[t + 1]].y, Positions[Triangles[t + 2]].y }).Min();
            var b = middle.SelectMany(t => new[] { Positions[Triangles[t]].y, Positions[Triangles[t + 1]].y, Positions[Triangles[t + 2]].y }).Max();
            var period = b - a;
            var start = keepStartCap ? a - minY : 0f;
            var middleLength = length - start - (keepEndCap ? maxY - b : 0f);
            var copies = repeat ? Mathf.Max(1, Mathf.RoundToInt(middleLength / period)) : 1;
            var scale = middleLength / (copies * period);

            var result = new MeshData();
            void AddTriangle(int t, Func<float, float> mapY, bool flipped = false)
            {
                // A flipped copy is mirrored in y, so its winding and its normals' y must flip too.
                foreach (var k in flipped ? new[] { 0, 2, 1 } : new[] { 0, 1, 2 })
                {
                    var i = Triangles[t + k];
                    var p = Positions[i];
                    var n = Normals[i];
                    result.Triangles.Add(result.AddVertex(new Vector3(p.x, mapY(p.y), p.z),
                                                          flipped ? new Vector3(n.x, -n.y, n.z) : n, Uvs[i]));
                }
            }

            if (keepStartCap)
            {
                startCap.ForEach(t => AddTriangle(t, y => y - minY));
            }
            for (var copy = 0; copy < copies; copy++)
            {
                var offset = copy * period;
                if (mirrorAlternate && copy % 2 == 1)
                {
                    middle.ForEach(t => AddTriangle(t, y => start + (offset + b - y) * scale, flipped: true));
                }
                else
                {
                    middle.ForEach(t => AddTriangle(t, y => start + (offset + y - a) * scale));
                }
            }
            if (keepEndCap)
            {
                endCap.ForEach(t => AddTriangle(t, y => start + middleLength + (y - b)));
            }
            return result;
        }

        /// <summary>
        /// Lengthens the mesh along Y by <paramref name="scale"/>, about the height <paramref name="pivot"/>, without
        /// stretching its texture: each face's UVs grow with it, so the texture carries on rather than being pulled
        /// longer. <paramref name="grainScale"/> then sets the texture's scale along Y relative to the original
        /// (0.5: half as dense, to match a mesh twice the original's length). Works face by face, so any box-like
        /// mesh works, tilted faces included. The UVs can run past 0..1, so the texture must tile.
        /// </summary>
        public MeshData ExtendY(float pivot, float scale, float grainScale = 1f)
        {
            // Every triangle gets its own vertices, so each face's UVs can change on their own.
            var result = Where((positions, normals) => true);
            for (var t = 0; t < result.Triangles.Count; t += 3)
            {
                var i0 = result.Triangles[t];
                var i1 = result.Triangles[t + 1];
                var i2 = result.Triangles[t + 2];

                // How the UVs change per metre of height across this face: the UV gradient within the triangle's
                // plane, G = alpha*e1 + beta*e2, solved from its edges, then read along Y.
                var e1 = result.Positions[i1] - result.Positions[i0];
                var e2 = result.Positions[i2] - result.Positions[i0];
                var d1 = result.Uvs[i1] - result.Uvs[i0];
                var d2 = result.Uvs[i2] - result.Uvs[i0];
                var a = Vector3.Dot(e1, e1);
                var b = Vector3.Dot(e1, e2);
                var c = Vector3.Dot(e2, e2);
                var det = a * c - b * b;
                var perMetreY = Vector2.zero;
                if (det > 1e-12f)
                {
                    var alpha = (c * d1 - b * d2) / det;
                    var beta = (a * d2 - b * d1) / det;
                    perMetreY = alpha * e1.y + beta * e2.y;
                }

                foreach (var i in new[] { i0, i1, i2 })
                {
                    var p = result.Positions[i];
                    var oldY = p.y - pivot;
                    var newY = oldY * scale;
                    result.Positions[i] = new Vector3(p.x, pivot + newY, p.z);
                    result.Uvs[i] += perMetreY * (newY * grainScale - oldY);
                }
            }

            // Stretching in Y tilts the normals of sloping faces (the inverse transpose of the scaling).
            for (var i = 0; i < result.Normals.Count; i++)
            {
                var n = result.Normals[i];
                result.Normals[i] = new Vector3(n.x, n.y / scale, n.z).normalized;
            }
            return result;
        }

        /// <summary>
        /// Groups triangles into flat faces: triangles sharing an edge (matched by position) whose normals point the
        /// same way. Returns each triangle's group number, indexed by triangle (index / 3).
        /// </summary>
        public int[] FlatFaces(float minAlignment = 0.999f)
        {
            var count = Triangles.Count / 3;
            var parent = Enumerable.Range(0, count).ToArray();
            int Root(int i)
            {
                while (parent[i] != i)
                {
                    parent[i] = parent[parent[i]];
                    i = parent[i];
                }
                return i;
            }

            var normals = new Vector3[count];
            var edges = new Dictionary<string, int>();
            var corners = new Vector3[3];
            for (var t = 0; t < count; t++)
            {
                for (var k = 0; k < 3; k++)
                {
                    corners[k] = Positions[Triangles[3 * t + k]];
                }
                normals[t] = Vector3.Cross(corners[1] - corners[0], corners[2] - corners[0]).normalized;
                for (var k = 0; k < 3; k++)
                {
                    var key = EdgeKey(corners[k], corners[(k + 1) % 3]);
                    if (edges.TryGetValue(key, out var other))
                    {
                        if (Vector3.Dot(normals[t], normals[other]) >= minAlignment)
                        {
                            parent[Root(t)] = Root(other);
                        }
                    }
                    else
                    {
                        edges[key] = t;
                    }
                }
            }
            return Enumerable.Range(0, count).Select(Root).ToArray();
        }

        /// <summary>An order-independent key for the edge between two positions (to 0.1 mm).</summary>
        private static string EdgeKey(Vector3 a, Vector3 b)
        {
            var ka = Vector3Int.RoundToInt(a * 10000f);
            var kb = Vector3Int.RoundToInt(b * 10000f);
            var aFirst = ka.x < kb.x || (ka.x == kb.x && (ka.y < kb.y || (ka.y == kb.y && ka.z <= kb.z)));
            var first = aFirst ? ka : kb;
            var second = aFirst ? kb : ka;
            return $"{first.x},{first.y},{first.z}|{second.x},{second.y},{second.z}";
        }

        /// <summary>
        /// The triangles for which <paramref name="keep"/> (given their three positions and normals) is true, as a
        /// new mesh with its own vertices.
        /// </summary>
        public MeshData Where(Func<Vector3[], Vector3[], bool> keep)
        {
            var result = new MeshData();
            var positions = new Vector3[3];
            var normals = new Vector3[3];
            for (var t = 0; t < Triangles.Count; t += 3)
            {
                for (var k = 0; k < 3; k++)
                {
                    positions[k] = Positions[Triangles[t + k]];
                    normals[k] = Normals[Triangles[t + k]];
                }
                if (!keep(positions, normals))
                {
                    continue;
                }
                for (var k = 0; k < 3; k++)
                {
                    var i = Triangles[t + k];
                    result.Triangles.Add(result.AddVertex(Positions[i], Normals[i], Uvs[i]));
                }
            }
            return result;
        }

        /// <summary>
        /// Splits every triangle that crosses the horizontal plane at <paramref name="y"/> into pieces on each side,
        /// keeping both, so later per-height reshaping has vertices on that line.
        /// </summary>
        public MeshData SliceY(float y)
        {
            return new MeshData()
                .Append(Clip(Vector3.up, y), Matrix4x4.identity)
                .Append(Clip(Vector3.down, -y), Matrix4x4.identity);
        }

        /// <summary>
        /// Cuts the mesh along the plane dot(<paramref name="normal"/>, p) = <paramref name="distance"/>, keeping the
        /// side where dot(normal, p) &lt;= distance. Triangles crossing the plane are clipped, with new positions,
        /// normals and UVs interpolated along their edges.
        /// </summary>
        public MeshData Clip(Vector3 normal, float distance)
        {
            var result = new MeshData();
            var polygon = new List<Corner>(4);
            var clipped = new List<Corner>(5);
            const float epsilon = 1e-5f;

            for (var t = 0; t < Triangles.Count; t += 3)
            {
                polygon.Clear();
                for (var k = 0; k < 3; k++)
                {
                    var i = Triangles[t + k];
                    polygon.Add(new Corner(Positions[i], Normals[i], Uvs[i]));
                }

                clipped.Clear();
                for (var k = 0; k < polygon.Count; k++)
                {
                    var current = polygon[k];
                    var previous = polygon[(k + polygon.Count - 1) % polygon.Count];
                    var currentSide = Vector3.Dot(normal, current.Position) - distance;
                    var previousSide = Vector3.Dot(normal, previous.Position) - distance;
                    var currentIn = currentSide <= epsilon;
                    var previousIn = previousSide <= epsilon;
                    if (currentIn != previousIn)
                    {
                        var f = previousSide / (previousSide - currentSide);
                        clipped.Add(new Corner(Vector3.Lerp(previous.Position, current.Position, f),
                                               Vector3.Lerp(previous.Normal, current.Normal, f).normalized,
                                               Vector2.Lerp(previous.Uv, current.Uv, f)));
                    }
                    if (currentIn)
                    {
                        clipped.Add(current);
                    }
                }

                for (var k = 1; k + 1 < clipped.Count; k++)
                {
                    foreach (var corner in new[] { clipped[0], clipped[k], clipped[k + 1] })
                    {
                        result.Triangles.Add(result.AddVertex(corner.Position, corner.Normal, corner.Uv));
                    }
                }
            }
            return result;
        }
    }
}
