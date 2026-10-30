using System;
using HarmonyLib;
using UnityEngine;

namespace QuanPieces.Patches
{
    /// <summary>
    /// Marks a piece whose bars you can look through: the hover (what [E] picks up or uses, and its text) passes
    /// through it to whatever is behind, such as an egg inside a coop. The piece stays solid for everything
    /// else: animals, players, arrows and the hammer.
    /// </summary>
    internal sealed class SeeThrough : MonoBehaviour
    {
    }

    /// <summary>
    /// Vanilla hovers the first thing the camera ray hits. When that's a <see cref="SeeThrough"/> piece, the
    /// same choice is made again, skipping see-through pieces.
    /// </summary>
    [HarmonyPatch(typeof(Player), "FindHoverObject")]
    internal static class PlayerFindHoverObjectPatch
    {
        private static readonly RaycastHit[] Hits = new RaycastHit[64];
        private static int _interactMask;

        private static void Postfix(Player __instance, ref GameObject hover, ref Character hoverCreature)
        {
            if (hover == null || hover.GetComponentInParent<SeeThrough>() == null || GameCamera.instance == null)
            {
                return;
            }

            if (_interactMask == 0)
            {
                // The same layers vanilla's hover uses (Player.m_interactMask).
                _interactMask = LayerMask.GetMask("item", "piece", "piece_nonsolid", "Default", "static_solid", "Default_small",
                                                  "character", "character_net", "terrain", "vehicle", "character_ghost");
            }

            hover = null;
            var camera = GameCamera.instance.transform;
            var count = Physics.RaycastNonAlloc(camera.position, camera.forward, Hits, 50f, _interactMask);
            Array.Sort(Hits, 0, count, Nearest.Instance);
            for (var i = 0; i < count; i++)
            {
                var hit = Hits[i];
                var body = hit.collider.attachedRigidbody;
                if ((body != null && body.gameObject == __instance.gameObject) || hit.collider.GetComponentInParent<SeeThrough>() != null)
                {
                    continue;
                }

                if (hoverCreature == null)
                {
                    hoverCreature = body ? body.GetComponent<Character>() : hit.collider.GetComponent<Character>();
                }

                var hoverable = hit.collider.GetComponentInParent<Hoverable>();
                var reach = __instance.m_maxInteractDistance + (hoverable?.GetHoverOffset() ?? 0f);
                if (Vector3.Distance(__instance.m_eye.position, hit.point) < reach)
                {
                    hover = hit.collider.GetComponent<Hoverable>() != null ? hit.collider.gameObject
                        : body != null ? body.gameObject
                        : hit.collider.gameObject;
                }
                break;
            }
        }

        private sealed class Nearest : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly Nearest Instance = new Nearest();

            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }
    }
}
