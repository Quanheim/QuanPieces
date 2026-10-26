using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// Lights a lantern's glass while its Fireplace is burning. Fireplace only switches objects on and off, and the
    /// lantern's glow is the material's emission, so this swaps the renderer between the lit material and a copy
    /// with emission off (the same difference as the vanilla "off" Dvergr lantern material).
    /// </summary>
    public class LanternGlass : MonoBehaviour
    {
        public GameObject m_litWhileActive;
        public MeshRenderer m_renderer;
        public Material m_lit;
        public Material m_unlit;

        private bool m_known;
        private bool m_wasLit;

        private void Update()
        {
            if (m_litWhileActive == null || m_renderer == null)
            {
                return;
            }

            var lit = m_litWhileActive.activeInHierarchy;
            if (m_known && lit == m_wasLit)
            {
                return;
            }

            m_known = true;
            m_wasLit = lit;
            m_renderer.sharedMaterial = lit ? m_lit : m_unlit;
        }
    }
}
