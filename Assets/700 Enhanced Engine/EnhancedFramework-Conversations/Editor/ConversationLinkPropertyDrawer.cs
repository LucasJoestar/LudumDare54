// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedEditor.Editor;
using UnityEditor;
using UnityEngine;

namespace EnhancedFramework.Conversations.Editor {
    /// <summary>
    /// Custom <see cref="ConversationLink"/> drawer, used to display the class content when using the <see cref="SerializeReference"/> attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConversationLink), true)]
    public class ConversationLinkPropertyDrawer : ConversationNodePropertyDrawer {
        #region Drawer Content
        private const float ButtonWidth = 125f;
        private const float ButtonHeight = 25f;

        private const float ButtonSpacing = 10f;
        private const float NodeSpacing = 10f;

        private const string NullLinkLabel = "NULL";

        private static readonly GUIContent linkNodeGUI = new GUIContent("Link Node", "The GUID and the label of the associated linked node.");
        private static readonly GUIContent selectLinkGUI = new GUIContent("Select Link", "Select the associated linked node.");
        private static readonly Color selectLinkColor = SuperColor.Lime.Get();

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            float _height = base.OnEnhancedGUI(_position, _property, _label) + NodeSpacing;
            _position.y += _height;

            _position.height = 1f;
            EnhancedEditorGUI.HorizontalLine(_position, SuperColor.Grey.Get());

            _height += _position.height + NodeSpacing;
            _position.y += NodeSpacing;
            _position.height += EditorGUIUtility.singleLineHeight;

            ConversationNode _node = ConversationEditorWindow.GetSelectedNode();

            // Null link management.
            if ((_node == null) || !(_node is ConversationLink _linkNode)) {
                return _height;
            }

            ConversationNode _link = _linkNode.Link;

            // Null link info.
            if (_link == null) {
                using (var _scope = EnhancedGUI.GUIEnabled.Scope(false)) {
                    EditorGUI.TextField(_position, linkNodeGUI, NullLinkLabel);
                }

                return _height + _position.height;
            }

            // Debug.
            if (_node.nodes.Length > 1) {
                Debug.LogWarning("Link Length => " + _node.nodes.Length);
            }

            // Link node informations.
            using (var _scope = EnhancedGUI.GUIEnabled.Scope(false)) {
                EditorGUI.IntField(_position, linkNodeGUI, _link.guid);

                _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
                _height += _position.height + EditorGUIUtility.standardVerticalSpacing;

                EnhancedEditorGUI.TextArea(_position, GUIContent.none, _link.Text, out float _areaHeight);

                _position.y += _areaHeight + EditorGUIUtility.standardVerticalSpacing;
                _height += _areaHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // Select link button.
            _height += ButtonSpacing;
            _position.y += ButtonSpacing;

            _position.width = ButtonWidth;
            _position.height = ButtonHeight;

            using (var _scope = EnhancedGUI.GUIColor.Scope(selectLinkColor)) {
                if (GUI.Button(_position, selectLinkGUI)) {
                    ConversationEditorWindow.GetWindow().SelectNode(_link, true);
                }
            }

            return _height + _position.height;
        }
        #endregion
    }
}
