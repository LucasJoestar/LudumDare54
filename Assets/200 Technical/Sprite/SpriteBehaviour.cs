// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedFramework.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace LudumDare54 {
    [RequireComponent(typeof(SortingGroup))]
    public class SpriteBehaviour : EnhancedBehaviour, IUpdate {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Update;

        #region Enhanced Behaviour
        private SortingGroup group = null;

        public SortingGroup Group {
            get { return group; }
        }

        // -----------------------

        private void Awake() {
            group = GetComponent<SortingGroup>();
        }


        void IUpdate.Update() {
            SetOrder(Mathf.RoundToInt(transform.position.y * -100f));
        }

        public void SetOrder(int order) {
            group.sortingOrder = order;
        }
        #endregion
    }
}
