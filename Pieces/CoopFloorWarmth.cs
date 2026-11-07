using System.Collections.Generic;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// On each chicken coop floor: switches the floor's warmth (its Heat area) on while a lit floor heater is within
    /// <see cref="FloorHeaterControl.Range"/>, and off otherwise. Every game that has the floor loaded works this out
    /// for itself from the heaters' shared on/off state, so they all agree.
    /// </summary>
    internal sealed class CoopFloorWarmth : MonoBehaviour
    {
        private const float CheckInterval = 1f;

        internal static readonly List<CoopFloorWarmth> Loaded = new List<CoopFloorWarmth>();

        public GameObject m_warmth;

        private void OnEnable()
        {
            Loaded.Add(this);
            // Spread the checks out so a big coop's floors don't all check in the same frame.
            InvokeRepeating(nameof(Check), Random.Range(0f, CheckInterval), CheckInterval);
        }

        private void OnDisable()
        {
            Loaded.Remove(this);
            CancelInvoke(nameof(Check));
        }

        private void Check()
        {
            if (m_warmth == null)
            {
                return;
            }
            var warm = FloorHeaterControl.IsLitHeaterNear(transform.position);
            if (m_warmth.activeSelf != warm)
            {
                m_warmth.SetActive(warm);
            }
        }
    }
}
