// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

using DisplayName = EnhancedEditor.DisplayNameAttribute;
#endif

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// Base <see cref="ConversationNode"/> line class.
    /// <br/> Inherit from this to create your own lines.
    /// </summary>
    /// <typeparam name="T">This line content type.</typeparam>
    [Serializable]
    public abstract class ConversationLine<T> : ConversationNode {
        #region Global Members
        /// <summary>
        /// This line content.
        /// </summary>
        [Enhanced, Block] public T Line = Activator.CreateInstance<T>();

        [Space(10f)]

        [Tooltip("This line speaker")]
        [SerializeField, Enhanced, DisplayName("Speaker"), Popup("Speakers")] protected int speakerIndex = 0;

        [Tooltip("Allow this line to only be played once")]
        [SerializeField] protected bool onlyOnce = false;

        #if UNITY_EDITOR
        [Space(15f)]

        [SerializeField, TextArea(2, 7)] internal string comment = string.Empty;
        #endif

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Required flags for this line to be available to play")]
        public FlagValueGroup RequiredFlags = new FlagValueGroup();

        [Tooltip("Modified flags when this line is played")]
        public FlagValueGroup AfterFlags = new FlagValueGroup();

        // -----------------------

        [NonSerialized] private bool wasPlayed = false;

        /// <summary>
        /// The duration of this line (in seconds).
        /// </summary>
        public virtual float Duration {
            get { return 0f; }
        }

        // -----------------------

        public override int SpeakerIndex {
            get { return speakerIndex; }
        }

        public override bool IsAvailable {
            get {
                if (onlyOnce && wasPlayed) {
                    return false;
                }

                return base.IsAvailable && RequiredFlags.Valid;
            }
        }
        #endregion

        #region Behaviour
        public override void Play(ConversationPlayer _player) {
            base.Play(_player);

            // Automatically play the next node if already played.
            if (onlyOnce && wasPlayed) {

                _player.PlayNextNode();
                return;
            }

            // Update flag values on play (safer than on exit).
            AfterFlags.SetValues();
            wasPlayed = true;
        }

        protected internal override void Reset() {
            base.Reset();

            wasPlayed = false;
        }
        #endregion

        #region Editor Utility
        protected internal override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "console.infoicon.sml";
                    break;

                case 1:
                    _iconName = "winbtn_win_close@2x";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return (nodes.Length == 0) ? 2 : 1;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="ConversationLine{T}"/> node class with a single text and an associated audio file.
    /// </summary>
    [Serializable, DisplayName("Base/Line [Standard]")]
    public class ConversationTextLine : ConversationLine<ConversationTextLine.Content> {
        /// <summary>
        /// Wrapper for the <see cref="ConversationTextLine"/> line content.
        /// </summary>
        [Serializable]
        public class Content {
            #region Content
            [Tooltip("Text of this line")]
            [Enhanced, EnhancedTextArea(true)] public string Text = DefaultText;

            [Tooltip("Audio asset of this line")]
            public AudioAsset Audio = null;
            #endregion
        }

        #region Global Members
        public override string Text {
            get { return Line.Text; }
            set { Line.Text = value; }
        }

        public override float Duration {
            get {
                return Line.Audio.IsValid()
                     ? Line.Audio.Duration
                     : (Text.Length * .05f);
            }
        }
        #endregion
    }

    #if LOCALIZATION_ENABLED
    /// <summary>
    /// <see cref="ConversationLine{T}"/> node class with a localized text and an associated localized audio file.
    /// </summary>
    [Serializable, DisplayName("Base/Line [Localized]")]
    public class ConversationLocalizedLine : ConversationLine<ConversationLocalizedLine.Content> {
        /// <summary>
        /// Wrapper for the <see cref="ConversationLocalizedLine"/> line content.
        /// </summary>
        [Serializable]
        public class Content {
            #region Content
            [Tooltip("Localized text of this line")]
            public LocalizedString Text = new LocalizedString();

            [Tooltip("Localized audio of this line")]
            public LocalizedAsset<AudioAsset> Audio = new LocalizedAsset<AudioAsset>();
            #endregion
        }

        #region Global Members
        public override string Text {
            get { return Line.Text.GetLocalizedValue(); }
            set { Line.Text.SetLocalizedValue(value); }
        }

        public override float Duration {
            get {
                if (GetAudioFile(out AudioAsset _audio) && _audio.IsValid()) {
                    return _audio.Duration;
                }

                return Text.Length * .05f;
            }
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        public override void GetLocalizationTables(Set<TableReference> _stringTables, Set<TableReference> _assetTables) {
            base.GetLocalizationTables(_stringTables, _assetTables);

            // Get all localization tables used by this node.
            if (Line.Text.GetLocalizedTable(out TableReference _table)) {
                _stringTables.Add(_table);
            }

            if (Line.Audio.GetLocalizedTable(out _table)) {
                _assetTables.Add(_table);
            }
        }
        #endif
        #endregion

        #region Utility
        /// <summary>
        /// Get this line audio asset.
        /// </summary>
        /// <param name="_audio">This line audio asset.</param>
        /// <returns>True if an audio asset was successfully found and loaded, false otherwise.</returns>
        public bool GetAudioFile(out AudioAsset _audio) {
            return Line.Audio.GetLocalizedValue(out _audio);
        }
        #endregion
    }
    #endif
}
