using System.Collections.Generic;
using UnityEngine;

namespace QuanPieces.Meshes
{
    /// <summary>
    /// Turns iron bars into wooden ones. Iron meshes' UVs point into a metal texture's layout, which means
    /// nothing on a wood texture, so their faces are given new UVs: each face is mapped flat from the side it
    /// faces, with the wood's grain running along the bar's length (whichever of its axes is longest).
    /// </summary>
    internal static class WoodGrain
    {
        // The wood texture's scale on a vanilla 2 m pole: once along its length, once across its 0.4 m width.
        private const float TileLength = 2f;
        private const float TileWidth = 0.4f;

        // Set if the texture's grain turns out to run along v rather than u.
        private const bool GrainAlongU = true;

        private static readonly Dictionary<Material, Material> Copies = new Dictionary<Material, Material>();

        /// <summary>A copy of <paramref name="mesh"/> with every face mapped for wood grain.</summary>
        public static MeshData Rewrap(MeshData mesh)
        {
            // Every triangle gets its own vertices, so faces pointing different ways can have different UVs.
            var result = mesh.Where((positions, normals) => true);
            var corner = new Vector3[3];
            for (var t = 0; t < result.Triangles.Count; t += 3)
            {
                for (var k = 0; k < 3; k++)
                {
                    corner[k] = result.Positions[result.Triangles[t + k]];
                }

                // The axis the face points along is not used; of the other two, the one the triangle spans
                // further is the bar's length.
                var normal = Vector3.Cross(corner[1] - corner[0], corner[2] - corner[0]);
                var facing = Dominant(new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z)));
                var span = Vector3.Max(Vector3.Max(corner[0], corner[1]), corner[2]) - Vector3.Min(Vector3.Min(corner[0], corner[1]), corner[2]);
                var a = (facing + 1) % 3;
                var b = (facing + 2) % 3;
                var along = span[a] >= span[b] ? a : b;
                var across = along == a ? b : a;

                for (var k = 0; k < 3; k++)
                {
                    var p = corner[k];
                    var u = p[along] / TileLength;
                    var v = p[across] / TileWidth;
                    result.Uvs[result.Triangles[t + k]] = GrainAlongU ? new Vector2(u, v) : new Vector2(v, u);
                }
            }
            return result;
        }

        /// <summary>
        /// A shared copy of a wood piece material with no vertex wobble: the vanilla wobble (a few cm) is
        /// invisible on thick beams but bends thin bars visibly.
        /// </summary>
        public static Material Steady(Material wood)
        {
            if (wood == null)
            {
                return null;
            }
            if (Copies.TryGetValue(wood, out var copy))
            {
                return copy;
            }
            copy = new Material(wood) { name = wood.name + "_quan_steady" };
            if (copy.HasProperty("_RippleDistance"))
            {
                copy.SetFloat("_RippleDistance", 0f);
            }
            Copies[wood] = copy;
            return copy;
        }

        private static int Dominant(Vector3 v)
        {
            return v.x >= v.y && v.x >= v.z ? 0 : v.y >= v.z ? 1 : 2;
        }
    }
}
