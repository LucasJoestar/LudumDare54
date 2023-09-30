// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Contraints value for selecting axises.
    /// </summary>
    [Flags]
    public enum AxisConstraints {

        [Ethereal]
        None = 0,

        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,

        [Ethereal]
        All = X | Y | Z,
    }
}
