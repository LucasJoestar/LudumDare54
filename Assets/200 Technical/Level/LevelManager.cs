// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedFramework.Core;
using System.Collections.Generic;
using UnityEngine;

namespace LudumDare54
{
    [DefaultExecutionOrder(-99)]
    public class LevelManager : EnhancedSingleton<LevelManager>
    {
        #region Global Members
        //[Section("Level Manager")]
        #endregion

        #region Registration
        private static readonly List<Block> blocks = new List<Block>();

        // -----------------------

        public static void RegisterBlock(Block _block) {
            blocks.Add(_block);
        }

        public static void UnregisterBlock(Block _block) {
            blocks.Remove(_block);
        }
        #endregion

        #region Utility
        public bool IsAvailableCoords(Vector2 _coords) {

            _coords = ProjectUtility.GetCoords(_coords);

            foreach (Block _block in blocks) {
                if (_block.IsOnCoords(_coords))
                    return false;
            }

            return true;
        }
        #endregion
    }
}
