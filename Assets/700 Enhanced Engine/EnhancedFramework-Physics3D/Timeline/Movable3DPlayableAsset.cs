// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedFramework.Timeline;
using System;
using UnityEngine.Playables;

namespace EnhancedFramework.Physics3D.Timeline {
    /// <summary>
    /// Base interface to inherit any <see cref="Movable3D"/> <see cref="PlayableAsset"/> from.
    /// </summary>
    public interface IMovable3DPlayableAsset { }

    /// <summary>
    /// Base non-generic <see cref="Movable3D"/> <see cref="PlayableAsset"/> class.
    /// </summary>
    public abstract class Movable3DPlayableAsset : EnhancedPlayableAsset, IMovable3DPlayableAsset { }

    /// <summary>
    /// Base generic class for every <see cref="Movable3D"/> <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class Movable3DPlayableAsset<T> : EnhancedPlayableAsset<T, Movable3D>, IMovable3DPlayableAsset
                                                      where T : EnhancedPlayableBehaviour<Movable3D>, new() { }

    /// <summary>
    /// Base generic class for every <see cref="CreatureMovable3D"/> <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class CreatureMovable3DPlayableAsset<T> : EnhancedPlayableAsset<T, CreatureMovable3D>, IMovable3DPlayableAsset
                                                              where T : EnhancedPlayableBehaviour<CreatureMovable3D>, new() { }

    // -------------------------------------------
    // Behaviours
    // -------------------------------------------

    /// <summary>
    /// Base <see cref="PlayableBehaviour"/> class for a <see cref="Movable3D"/>.
    /// </summary>
    [Serializable]
    public abstract class Movable3DPlayableBehaviour : EnhancedPlayableBehaviour<Movable3D> {
        #region Global Members
        /// <summary>
        /// <see cref="Movable3D"/> instance to use.
        /// </summary>
        public virtual Movable3D Movable {
            get { return bindingObject; }
        }
        #endregion
    }

    /// <summary>
    /// Base <see cref="PlayableBehaviour"/> class for a <see cref="CreatureMovable3D"/>.
    /// </summary>
    [Serializable]
    public abstract class CreatureMovable3DPlayableBehaviour : EnhancedPlayableBehaviour<CreatureMovable3D> {
        #region Global Members
        /// <summary>
        /// <see cref="CreatureMovable3D"/> instance to use.
        /// </summary>
        public virtual CreatureMovable3D Creature {
            get { return bindingObject; }
        }
        #endregion
    }
}
