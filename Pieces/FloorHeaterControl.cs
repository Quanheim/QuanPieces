using System.Collections.Generic;
using UnityEngine;

namespace QuanPieces.Pieces
{
    /// <summary>
    /// The floor heater's switch. Use it to turn the heater on or off; the state is saved on the piece, so everyone
    /// sees the same. While it's on, its embers glow and every chicken coop floor within <see cref="Range"/> is warm
    /// (see <see cref="CoopFloorWarmth"/>). It needs no fuel, and rain, wind, smoke or open sky make no difference.
    /// </summary>
    internal sealed class FloorHeaterControl : MonoBehaviour, Interactable, Hoverable
    {
        /// <summary>How far a lit heater warms coop floors, measured to the middle of each floor.</summary>
        internal const float Range = 10f;

        private const string LitKey = "Quan_floor_heater_lit";
        private const string SetLitRpc = "Quan_floor_heater_set_lit";

        private static readonly List<FloorHeaterControl> Loaded = new List<FloorHeaterControl>();

        // The glowing ash and its light, shown while lit.
        public GameObject m_embers;

        private ZNetView _nview;
        private Piece _piece;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _piece = GetComponent<Piece>();
            if (_nview != null && _nview.GetZDO() != null)
            {
                _nview.Register<bool>(SetLitRpc, RPC_SetLit);
            }
        }

        private void OnEnable()
        {
            Loaded.Add(this);
            InvokeRepeating(nameof(UpdateEmbers), 0f, 0.5f);
        }

        private void OnDisable()
        {
            Loaded.Remove(this);
            CancelInvoke(nameof(UpdateEmbers));
        }

        /// <summary>On ("lit"). A new heater starts on.</summary>
        internal bool IsLit => _nview != null && _nview.IsValid() && _nview.GetZDO().GetBool(LitKey, true);

        internal static bool IsLitHeaterNear(Vector3 point)
        {
            foreach (var heater in Loaded)
            {
                if (heater != null && (heater.transform.position - point).sqrMagnitude <= Range * Range && heater.IsLit)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateEmbers()
        {
            var lit = IsLit;
            if (m_embers != null && m_embers.activeSelf != lit)
            {
                m_embers.SetActive(lit);
            }
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || _nview == null || !_nview.IsValid())
            {
                return false;
            }
            if (!PrivateArea.CheckAccess(transform.position))
            {
                return true;
            }
            _nview.InvokeRPC(SetLitRpc, !IsLit);
            return true;
        }

        private void RPC_SetLit(long sender, bool lit)
        {
            if (_nview.IsOwner())
            {
                _nview.GetZDO().Set(LitKey, lit);
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

        public string GetHoverText()
        {
            if (_nview == null || !_nview.IsValid())
            {
                return string.Empty;
            }
            var lit = IsLit;
            var floors = CountFloorsInRange();
            var state = lit
                ? $"On: warming {floors} coop floor{(floors == 1 ? "" : "s")} within {Range:0}m"
                : $"Off ({floors} coop floor{(floors == 1 ? "" : "s")} in range)";
            return Localization.instance.Localize(
                $"{GetHoverName()}\n{state}\n[<color=yellow><b>$KEY_Use</b></color>] {(lit ? "Turn off" : "Turn on")}");
        }

        public string GetHoverName() => _piece != null ? Localization.instance.Localize(_piece.m_name) : string.Empty;

        public float GetHoverOffset() => 0f;

        private int CountFloorsInRange()
        {
            var count = 0;
            foreach (var floor in CoopFloorWarmth.Loaded)
            {
                if (floor != null && (floor.transform.position - transform.position).sqrMagnitude <= Range * Range)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
