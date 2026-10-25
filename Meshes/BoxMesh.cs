using System.Collections.Generic;
using UnityEngine;

namespace QuanPieces.Meshes
{
    /// <summary>
    /// Builds simple box meshes for kitbashed pieces that borrow a vanilla material. Vanilla piece textures are
    /// atlases with transparent gaps (which the piece shader cuts out), so each face is mapped into a chosen opaque
    /// region of the atlas rather than tiling the whole texture.
    /// </summary>
    internal static class BoxMesh
    {
        private static readonly Vector3[] FaceNormals =
        {
            Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
        };

        /// <summary>
        /// An axis-aligned box of <paramref name="size"/> centred on the origin. Faces at least
        /// <paramref name="elongation"/> times longer than they are wide map onto <paramref name="longRegion"/>,
        /// the rest onto <paramref name="squareRegion"/>. A face's long side always runs along a region's width.
        /// </summary>
        public static Mesh Create(string name, Vector3 size, Rect longRegion, Rect squareRegion, float elongation = 3f)
        {
            var half = size / 2f;
            var vertices = new List<Vector3>(24);
            var normals = new List<Vector3>(24);
            var uvs = new List<Vector2>(24);
            var triangles = new List<int>(36);

            foreach (var normal in FaceNormals)
            {
                // Two axes spanning the face: a permutation of the normal is always perpendicular to it.
                var u = new Vector3(normal.y, normal.z, normal.x);
                var v = Vector3.Cross(normal, u);

                var lengthU = Mathf.Abs(Vector3.Dot(size, u));
                var lengthV = Mathf.Abs(Vector3.Dot(size, v));
                var uIsLong = lengthU >= lengthV;
                var isLong = Mathf.Max(lengthU, lengthV) >= elongation * Mathf.Min(lengthU, lengthV);
                var region = isLong ? longRegion : squareRegion;

                var centre = Vector3.Scale(normal, half);
                var du = Vector3.Scale(u, half);
                var dv = Vector3.Scale(v, half);
                var corners = new[] { centre - du - dv, centre + du - dv, centre + du + dv, centre - du + dv };

                var first = vertices.Count;
                foreach (var corner in corners)
                {
                    vertices.Add(corner);
                    normals.Add(normal);

                    // Position across the face, 0 to 1 on each axis.
                    var s = Vector3.Dot(corner, u) / lengthU + 0.5f;
                    var t = Vector3.Dot(corner, v) / lengthV + 0.5f;
                    var along = uIsLong ? s : t;
                    var across = uIsLong ? t : s;
                    uvs.Add(new Vector2(region.x + along * region.width, region.y + across * region.height));
                }

                // Unity treats a triangle as front-facing along Cross(b - a, c - a); wind each face to point outwards.
                var outward = Vector3.Dot(Vector3.Cross(corners[1] - corners[0], corners[2] - corners[0]), normal) > 0f;
                triangles.AddRange(outward
                    ? new[] { first, first + 1, first + 2, first, first + 2, first + 3 }
                    : new[] { first, first + 2, first + 1, first, first + 3, first + 2 });
            }

            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents(); // the piece shader uses normal maps
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
