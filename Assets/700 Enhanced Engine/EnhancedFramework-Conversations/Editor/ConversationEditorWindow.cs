// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedEditor.Editor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;

using ArrayUtility = EnhancedEditor.ArrayUtility;
using Object = UnityEngine.Object;

namespace EnhancedFramework.Conversations.Editor {
    /// <summary>
    /// Editor window used to edit <see cref="Conversations.Conversation"/> assets.
    /// <para/>
    /// This is where the game writers and designers can write and configure the dialogues to be used in the game.
    /// </summary>
    public class ConversationEditorWindow : EditorWindow {
        #region Styles
        private class Styles {
            public static readonly GUIStyle LeftButton = new GUIStyle(EditorStyles.miniButtonLeft) {
                fontSize = 8
            };

            public static readonly GUIStyle RightButton = new GUIStyle(EditorStyles.miniButtonRight) {
                fontSize = 8
            };
        }
        #endregion

        #region Window GUI
        /// <summary>
        /// Returns the first <see cref="ConversationEditorWindow"/> currently on screen.
        /// <br/> Creates and shows a new instance if there is none.
        /// </summary>
        /// <returns><see cref="ConversationEditorWindow"/> instance on screen.</returns>
        [MenuItem(FrameworkUtility.MenuItemPath + "Conversation Editor", false, 10)]
        public static ConversationEditorWindow GetWindow() {
            ConversationEditorWindow _window = GetWindow<ConversationEditorWindow>("Conversation Editor");
            _window.Show();

            return _window;
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private const string UndoRecordTitle = "Conversation Change";
        private const string PreferencesKey = "ConversationPreferences";
        private const string NoConversationMessage = "Open a conversation asset to start editing its convent.";

        private static Conversation Conversation = null;
        private static bool isActive = false;

        private static List<int> selectedNodeIndexes = new List<int>();
        private static SerializedProperty selectedProperty = null;

        [SerializeReference] private static ConversationNode selectedNode = null;
        [SerializeReference] private static List<ConversationNode> nodes = new List<ConversationNode>();

        private readonly GUIContent openGUI = new GUIContent(" Open", "Select a new conversation asset to edit.");
        private readonly GUIContent newGUI = new GUIContent(" New", "Creates a new conversation asset and open it.");
        private readonly GUIContent createGUI = new GUIContent(" Create Node", "Creates a new node from the selected one.");
        private readonly GUIContent toggleGridGUI = new GUIContent(string.Empty, "Toggle the visibility of the grid.");
        private readonly GUIContent speakerColorGUI = new GUIContent(string.Empty, "Toggle the visibility of the configuration section, used to edit the colors used to display each speaker node.");

        private readonly GUIContent addSpeakerColorGUI = new GUIContent(string.Empty, "Add a new color to configure for a specific speaker.");
        private readonly GUIContent removeSpeakerColorGUI = new GUIContent(string.Empty, "Remove this speaker configured color from the list.");

        private SerializedObject serializedObject = null;
        private bool doFocusSelection = false;

        [SerializeField] private Conversation conversation = null;
        [SerializeField] private string conversationPath = string.Empty;

        [SerializeField] private Pair<string, Color>[] speakerColors = new Pair<string, Color>[] { };
        [SerializeField] private Color defaultSpeakerColor = new Color(.9f, .9f, .9f);
        [SerializeField] private Vector2 scroll = Vector2.zero;
        [SerializeField] private bool useGrid = false;

        // -----------------------

        private void OnEnable() {
            // Load preferences.
            string _json = EditorPrefs.GetString(PreferencesKey, string.Empty);
            if (!string.IsNullOrEmpty(_json)) {
                JsonUtility.FromJsonOverwrite(_json, this);
                
                if (conversation == null) {
                    conversation = AssetDatabase.LoadAssetAtPath<Conversation>(conversationPath);
                }
            }

            titleContent.image = EditorGUIUtility.IconContent("align_horizontally_left").image;

            openGUI.image = EditorGUIUtility.FindTexture("FolderOpened Icon");
            newGUI.image = EditorGUIUtility.IconContent("Profiler.UIDetails").image;
            createGUI.image = EditorGUIUtility.FindTexture("CreateAddNew");
            toggleGridGUI.image = EditorGUIUtility.FindTexture("UnityEditor.SceneView");
            speakerColorGUI.image = EditorGUIUtility.IconContent("Custom").image;

            addSpeakerColorGUI.image = EditorGUIUtility.IconContent("CreateAddNew").image;
            removeSpeakerColorGUI.image = EditorGUIUtility.IconContent("winbtn_win_close").image;

            isActive = true;
            RefreshConversation();

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnFocus() {

            Object _selection = Selection.activeObject;

            if (_selection != conversation) {

                if (_selection is Conversation _conversation) {

                    // Update selection.
                    SetConversation(_conversation);
                }
                else if (conversation != null) {

                    // Select the editing conversation while focused to display various parameters in the inspector.
                    Selection.activeObject = conversation;
                }
            }
        }

        private void OnGUI() {
            Undo.RecordObject(this, UndoRecordTitle);

            // Toolbar.
            DrawToolbar();

            using (var _scope = new GUILayout.ScrollViewScope(scroll)) {
                scroll = _scope.scrollPosition;

                // No conversation message.
                if (conversation == null) {
                    GUILayout.Space(10f);
                    EditorGUILayout.HelpBox(NoConversationMessage, UnityEditor.MessageType.Info);
                    return;
                }

                Undo.RecordObject(conversation, UndoRecordTitle);

                serializedObject.UpdateIfRequiredOrScript();
                DrawGUI();
                serializedObject.ApplyModifiedProperties();
            }

            Repaint();
        }

        private void OnDisable() {
            // Save preferences.
            EditorPrefs.SetString(PreferencesKey, JsonUtility.ToJson(this));
            isActive = false;

            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        // ----------------------

        private void OnUndoRedoPerformed() {
            RefreshConversation(false);
            RefreshLinks();
        }
        #endregion

        #region Window Editor
        private const float OpenButtonWidth = 62f;
        private const float NewButtonWidth = 57f;
        private const float CreateButtonWidth = 100f;
        private const float GridToggleWidth = 25f;
        private const float ConfigurationButtonWidth = 28f;

        private const float ConfigurationSectionSpacing = 25f;
        private const float ConfigurationSectionWidth = 250f;
        private const float AddSpeakerColorButtonWidth = 30f;

        private const float IndentSpacing = 17f;
        private const float IconSpacing = 5f;
        private const float DotFoldoutSpacing = 6f;

        private const double DragMinTime = .2d;

        private const float MaxLabelWidth = 350f;
        private const string ElapsedLabelSymbol = " <b>[...]</b>";
        private const string DisplayedLabelSpeakerFormat = "<color=#{0}>{1}</color>";

        private const string OpenFileTitle = "Open Conversation";
        private const string FileExtension = "asset";

        private const string NewFileTitle = "Create New Conversation";
        private const string NewFileDefaultName = Conversation.FilePrefix + "NewConversation";
        private const string NewFileMessage = "Please select a valid path to create a new conversation";

        private readonly GUIContent speakerColorHeaderGUI = new GUIContent("Speaker Color", "Edit the colors used to display the line of each speaker.");
        private readonly GUIContent selectionSpeakerGUI = new GUIContent("Selected Node Speaker", "The name of the selected node speaker. Use this to assign a specific color for it.");

        private readonly GUIContent moveColorUpGUI = new GUIContent("\u25B2", "Move this color up in the list."); // ?
        private readonly GUIContent moveColorDownGUI = new GUIContent("\u25BC", "Move this color down in the list."); // ?

        private readonly Color moveButtonColor = new Color(.9f, .9f, .9f);

        private AnimBool configurationVisiblity = new AnimBool(false);
        private Rect dragPosition = Rect.zero;
        private double dragTimer = 0d;

        private float NodeEditorWindowWidth {
            get { return position.width - ConfigurationSectionWindowWidth; }
        }

        private float ConfigurationSectionWindowWidth {
            get { return ConfigurationSectionWidth * configurationVisiblity.faded; }
        }

        // -----------------------

        private void DrawToolbar() {
            using (var _scope = new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
                // Loading button.
                if (GUILayout.Button(openGUI, EditorStyles.toolbarButton, GUILayout.Width(OpenButtonWidth))) {
                    string _file = EditorUtility.OpenFilePanel(OpenFileTitle, GetFolderPath(), FileExtension);

                    if (!string.IsNullOrEmpty(_file)) {
                        Conversation _asset = AssetDatabase.LoadAssetAtPath<Conversation>(_file.Remove(0, EnhancedEditorUtility.GetProjectPath().Length));

                        if (_asset != null) {
                            SetConversation(_asset);
                        }
                    }
                }

                // Create button.
                if (GUILayout.Button(newGUI, EditorStyles.toolbarButton, GUILayout.Width(NewButtonWidth))) {
                    string _file = EditorUtility.SaveFilePanelInProject(NewFileTitle, NewFileDefaultName, FileExtension, NewFileMessage, GetFolderPath());

                    if (!string.IsNullOrEmpty(_file)) {
                        SetConversation(CreateInstance<Conversation>());

                        AssetDatabase.CreateAsset(conversation, _file);
                        AssetDatabase.Refresh();
                    }
                }

                GUILayout.FlexibleSpace();

                // Grid visiblity toggle.
                useGrid = GUILayout.Toggle(useGrid, toggleGridGUI, EditorStyles.toolbarButton, GUILayout.Width(GridToggleWidth));

                // Speaker displayed node colors.
                configurationVisiblity.target = GUILayout.Toggle(configurationVisiblity.target, speakerColorGUI, EditorStyles.toolbarButton, GUILayout.Width(ConfigurationButtonWidth));

                Conversation _conversation = conversation;
                if (_conversation == null) {
                    return;
                }

                // Button to create a new node.
                if (GUILayout.Button(createGUI, EditorStyles.toolbarButton, GUILayout.Width(CreateButtonWidth))) {
                    CreateDefaultNode();
                }
            }

            // ----- Local Methods ----- \\

            string GetFolderPath() {
                if (string.IsNullOrEmpty(conversationPath)) {
                    return EnhancedEditorUtility.GetProjectSelectedFolderPath();
                }

                string _path = Path.GetDirectoryName(conversationPath);
                return Directory.Exists(_path) ? _path : EnhancedEditorUtility.GetProjectSelectedFolderPath();
            }
        }

        private void DrawGUI() {
            float _configurationWidth = ConfigurationSectionWindowWidth;

            using (var _global = new EditorGUILayout.HorizontalScope(EditorStyles.inspectorFullWidthMargins)) {
                // Drag and drop visual.
                Event _event = Event.current;

                if ((_event.type == EventType.DragUpdated) && (_event.mousePosition.y > _global.rect.y)) {
                    DragAndDrop.visualMode = ((DragAndDrop.objectReferences.Length == 1) && (DragAndDrop.objectReferences[0] == conversation))
                                           ? DragAndDropVisualMode.Copy
                                           : DragAndDropVisualMode.None;

                    _event.Use();
                }

                if (_event.type == EventType.MouseDown) {
                    dragTimer = EditorApplication.timeSinceStartup + DragMinTime;
                }

                dragPosition = Rect.zero;

                // Node editor.
                using (var _vertical = new EditorGUILayout.VerticalScope(EditorStyles.inspectorFullWidthMargins, GUILayout.Width(position.width - _configurationWidth - ConfigurationSectionSpacing))) {
                    int _index = 0;
                    DrawConversationNode(conversation.Root, Rect.zero, 0, ref _index, out Rect _position);

                    GUILayout.FlexibleSpace();
                    GUILayout.Space(7f);
                }

                // Draw the drag to feedback after all nodes were drawn, to ensure that nothing is drawn above it.
                if ((DragAndDrop.visualMode == DragAndDropVisualMode.Copy) && (dragPosition.height != 0f)) {
                    Rect _temp = new Rect(dragPosition) {
                        x = dragPosition.x - 1f,
                        y = dragPosition.yMax - 2f,
                    };

                    EnhancedEditorGUI.DropHereLine(_temp);
                }

                // Draw the separation outside of the FadeGroupScope to ensure it gets properly drawn.
                if (_configurationWidth != 0f) {
                    Rect _temp = new Rect(_global.rect) {
                        x = position.width - _configurationWidth,
                    };

                    EditorGUI.DrawRect(_temp, EnhancedEditorGUIUtility.GUIPeerLineColor);

                    _temp.width = 1f;
                    EditorGUI.DrawRect(_temp, SuperColor.SmokyBlack.Get());

                    GUILayout.Space(ConfigurationSectionSpacing);
                }

                // Configuration section.
                using (var _fadeScope = new EditorGUILayout.FadeGroupScope(configurationVisiblity.faded)) {
                    if (_fadeScope.visible) {

                        float _width = _configurationWidth - ConfigurationSectionSpacing + 16f;
                        using (var _vertical = new EditorGUILayout.VerticalScope(EditorStyles.inspectorFullWidthMargins, GUILayout.Width(_width))) {
                            EnhancedEditorGUILayout.UnderlinedLabel(speakerColorHeaderGUI, EditorStyles.boldLabel);
                            GUILayout.Space(5f);

                            // Speaker color.
                            for (int i = 0; i < speakerColors.Length; i++) {
                                GUILayout.Space(2f);

                                using (var _horizontal = new GUILayout.HorizontalScope()) {
                                    speakerColors[i].First = EditorGUILayout.TextField(speakerColors[i].First);

                                    using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                                        speakerColors[i].Second = EditorGUILayout.ColorField(speakerColors[i].Second);

                                        if (_changeCheck.changed) {
                                            Repaint();
                                        }
                                    }
                                    GUILayout.Space(1f);

                                    using (var _colorScope = EnhancedGUI.GUIContentColor.Scope(moveButtonColor)) {
                                        if (GUILayout.Button(moveColorDownGUI, Styles.LeftButton, GUILayout.Width(15f))) {
                                            ArrayUtility.ShiftElement(speakerColors, i, i + 1);
                                        }
                                        if (GUILayout.Button(moveColorUpGUI, Styles.RightButton, GUILayout.Width(13f))) {
                                            ArrayUtility.ShiftElement(speakerColors, i, i - 1);
                                        }
                                    }

                                    using (var _colorScope = EnhancedGUI.GUIColor.Scope(SuperColor.Crimson.Get())) {
                                        if (EnhancedEditorGUILayout.IconButton(removeSpeakerColorGUI, 0f, GUILayout.Width(20f))) {
                                            ArrayUtility.RemoveAt(ref speakerColors, i--);
                                        }
                                    }
                                }
                            }

                            GUILayout.Space(10f);

                            // Default speaker.
                            using (var _horizontal = new GUILayout.HorizontalScope()) {

                                EditorGUILayout.PrefixLabel("Default Speaker");

                                using (var _changeCheck = new EditorGUI.ChangeCheckScope()) {
                                    defaultSpeakerColor = EditorGUILayout.ColorField(defaultSpeakerColor);

                                    if (_changeCheck.changed) {
                                        Repaint();
                                    }
                                }
                            }

                            GUILayout.Space(10f);

                            // Add speaker color button.
                            using (var _buttonScope = new EditorGUILayout.HorizontalScope()) {
                                GUILayout.FlexibleSpace();

                                if (GUILayout.Button(addSpeakerColorGUI, GUILayout.Width(AddSpeakerColorButtonWidth))) {
                                    ArrayUtility.Add(ref speakerColors, new Pair<string, Color>("New Speaker", Color.white));
                                }
                            }

                            // Selected node speaker.
                            if ((selectedNodeIndexes.Count == 1)) {
                                GUILayout.Space(20f);
                                EnhancedEditorGUILayout.UnderlinedLabel(selectionSpeakerGUI);

                                GUILayout.Space(5f);
                                EditorGUILayout.SelectableLabel(GetSelectedNode().GetEditorSpeakerName(conversation.Settings), EditorStyles.miniTextField);
                            }

                            GUILayout.FlexibleSpace();
                        }
                    }
                }

                // Unfocus on empty space click.
                if (EnhancedEditorGUIUtility.DeselectionClick(_global.rect)) {
                    UnselectAllNodes();
                }

                // Validation events.
                ValidateCommand _command = EnhancedEditorGUIUtility.ValidateCommand(out _event);
                switch (_command) {
                    case ValidateCommand.Copy:
                        CopyNode();
                        _event.Use();
                        break;

                    case ValidateCommand.Cut:
                        CopyNode();
                        _event.Use();
                        break;

                    case ValidateCommand.Paste:
                        PasteNodeAsNew();
                        _event.Use();
                        break;

                    case ValidateCommand.Delete:
                        DeleteSelectedNodes();
                        _event.Use();
                        break;

                    case ValidateCommand.SoftDelete:
                        DeleteSelectedNodes();
                        _event.Use();
                        break;

                    case ValidateCommand.Duplicate:
                        DuplicateNode();
                        _event.Use();
                        break;

                    case ValidateCommand.SelectAll:
                        for (int i = 0; i < nodes.Count; i++) {
                            SelectNode(i, true);
                        }
                        _event.Use();
                        break;

                    default:
                        break;
                }

                _event = Event.current;

                if (_event.isKey && _event.shift && (_event.keyCode == KeyCode.V) && !_event.control && !_event.alt) {
                    PasteNodeAsLink();
                    _event.Use();
                }

                // Multi-selection keys.
                EnhancedEditorGUIUtility.VerticalMultiSelectionKeys(nodes, IsNodeSelected, CanSelectNode, SelectNode, (selectedNodeIndexes.Count != 0) ? selectedNodeIndexes.Last() : -1);

                // Context click menu.
                if (EnhancedEditorGUIUtility.ContextClick(_global.rect)) {
                    ShowContextMenu();
                }
            }
        }

        private bool DrawConversationNode(ConversationNode _node, Rect _neighbour, int _indent, ref int _index, out Rect _nodePosition) {
            Rect _position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing);
            _position.yMin -= EditorGUIUtility.standardVerticalSpacing;

            Rect _full = new Rect(_position){
                x = 0f,
                width = NodeEditorWindowWidth
            };
            
            float _dotHSpacing = 0f;
            float _dotVpacing = 0f;

            _position.xMin += (IconSpacing + IndentSpacing) * _indent;

            bool _isSelected = _node.isSelected;
            int _nodeIndex = _index++;

            if (_isSelected || useGrid) {
                EnhancedEditorGUI.BackgroundLine(_full, _isSelected, _nodeIndex);
            }

            // Pre-draw callback.
            _node.OnEditorDraw(Conversation);

            // Foldout and inner nodes.
            if (_node.ShowNodes && (_node.nodes.Length > 0)) {

                _dotHSpacing = DotFoldoutSpacing;
                _dotVpacing = DotFoldoutSpacing + 1f;

                _node.foldout.target = EditorGUI.Foldout(_position, _node.foldout.target, GUIContent.none);

                using (var _scope = new EditorGUILayout.FadeGroupScope(_node.foldout.faded)) {
                    if (_scope.visible) {
                        _nodePosition = _position;

                        for (int i = 0; i < _node.nodes.Length; i++) {
                            if (!DrawConversationNode(_node.nodes[i], _nodePosition, _indent + 1, ref _index, out _nodePosition)) {
                                return false;
                            }
                        }
                    }
                }
            }

            float _spacing = EnhancedEditorGUIUtility.FoldoutWidth + IconSpacing;

            Rect _dotPosition = new Rect(_position) {
                x = _position.x + DotFoldoutSpacing + 2f,
                y = _position.y + (_position.height / 2f),
            };

            // Indent dots.
            _dotPosition.xMin += _dotHSpacing;
            _dotPosition.xMax = _position.x + _spacing + 1f;

            if (_neighbour.width != 0f || _dotHSpacing != 0f) {
                EnhancedEditorGUI.HorizontalDottedLine(_dotPosition, 1f, 1f, 1f);
            }

            if (_neighbour.height != 0f) {
                _dotPosition.xMin = _position.x + DotFoldoutSpacing;
                _dotPosition.yMin = _neighbour.yMax - 1f;
                _dotPosition.yMax = _position.yMax - (_position.height / 2f) + 1f - _dotVpacing;

                EnhancedEditorGUI.InvertedVerticalDottedLine(_dotPosition, 1f, 1f, 1f);
            }

            _position.xMin += _spacing;
            _dotPosition.yMax += _dotVpacing * 2f;

            Rect _thisPosition = new Rect(_position) {
                xMax = _full.xMax
            };

            // Icon(s).
            int _iconIndex = 0;
            while (_iconIndex < _node.GetEditorIcon(_iconIndex++, out string _iconName)) {
                _position = DrawIcon(_position, _iconName);
            }

            _position.xMin += 5f;

            // Label.
            GUIStyle _style = EnhancedEditorStyles.RichText;
            string _text = _node.Text;

            string _displayedText = EnhancedStringParserUtility.Parse(_node.GetEditorDisplayedText().Replace('\n', ' '));
            string _newText;

            // Configured speaker color.
            string _speaker = _node.GetEditorSpeakerName(conversation.Settings);
            int _speakerIndex = string.IsNullOrEmpty(_speaker) ? -1 : _speakerIndex = Array.FindIndex(speakerColors, (p) => p.First == _speaker);

            Color _color = (_speakerIndex == -1) ? defaultSpeakerColor : speakerColors[_speakerIndex].Second;

            if ((_speakerIndex == -1) || conversation.HasDuplicateName) {
                _color *= ((_node.SpeakerIndex % 2) != 0) ? Color.white : new Color(.75f, .75f, .75f);
            }

            _displayedText = string.Format(DisplayedLabelSpeakerFormat, ColorUtility.ToHtmlStringRGBA(_color), _displayedText);

            // Content.
            Vector2 _size = _style.CalcSize(EnhancedEditorGUIUtility.GetLabelGUI(_displayedText));
            float _maxWidth = Mathf.Min(MaxLabelWidth, _position.width);

            _newText = EnhancedEditorGUI.EditableLabel(_position, _displayedText, _text, _style, EditorStyles.textField);

            // If the displayed label is too long and not being edited, truncate it by drawing the GUI background color over it.
            if ((_size.x > _maxWidth) && (GUI.GetNameOfFocusedControl() != EnhancedEditorGUIUtility.GetLastControlID().ToString())) {
                float _symbolWidth = _style.CalcSize(EnhancedEditorGUIUtility.GetLabelGUI(ElapsedLabelSymbol)).x + 5f;

                Rect _temp = new Rect(_position){
                    xMin = _position.x + (_maxWidth - _symbolWidth)
                };

                EditorGUI.DrawRect(_temp, EnhancedEditorGUIUtility.GUIThemeBackgroundColor);
                if (_isSelected || useGrid) {
                    EnhancedEditorGUI.BackgroundLine(_temp, _isSelected, _nodeIndex);
                }

                EditorGUI.LabelField(_temp, ElapsedLabelSymbol, _style);
            }

            if (_text != _newText) {
                _node.Text = _newText;
                EditorUtility.SetDirty(conversation);
            }

            _nodePosition = _dotPosition;

            // Select on click.
            _nodeIndex = nodes.IndexOf(_node);
            if (_nodeIndex == -1) {
                Debug.LogError("Null Node");
                RefreshNodes();
                return true;
            }

            EnhancedEditorGUIUtility.MultiSelectionClick(_full, nodes, _nodeIndex, IsNodeSelected, CanSelectNode, SelectNode);
            if (OnDragAndDrop(_thisPosition, _nodeIndex)) {
                return false;
            }

            // Focus.
            if (doFocusSelection && _node.isSelected && (Event.current.type == EventType.Repaint)) {
                scroll = EnhancedEditorGUIUtility.FocusScrollOnPosition(scroll, _full, new Vector2(position.width, position.height - EditorStyles.toolbar.CalcHeight(GUIContent.none, 0f)));
                doFocusSelection = false;
            }

            return true;

            // ----- Local Method ----- \\

            Rect DrawIcon(Rect _position, string _iconName) {
                EditorGUI.LabelField(_position, EditorGUIUtility.IconContent(_iconName));

                _position.xMin += EnhancedEditorGUIUtility.MiniIconWidth - 2f;
                return _position;
            }
        }

        private bool OnDragAndDrop(Rect _position, int _index) {
            Event _event = Event.current;
            Rect _full = new Rect(_position);

            if (_index == nodes.Count - 1) {
                _full.yMax = position.yMax;
            }

            if (!_full.Contains(_event.mousePosition)) {
                return false;
            }

            dragPosition = _position;

            if (nodes[0].isSelected || (selectedNodeIndexes.Count == 0)) {
                return false;
            }

            // Special events.
            if ((_event.type == EventType.MouseDrag) && (_event.button == 0) && (dragTimer < EditorApplication.timeSinceStartup)) {
                GUI.FocusControl(string.Empty);

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { conversation };
                DragAndDrop.StartDrag("Conversation Node(s) Drag");

                _event.Use();
            } else if (_event.type == EventType.DragPerform) {
                // Get destination parent node.
                ConversationNode _selected = nodes[_index];
                int _insert = 0;

                if ((_selected.nodes.Length == 0) || !_selected.foldout.value || !_selected.ShowNodes) {
                    _insert = Array.IndexOf(_selected.parent.nodes, _selected) + 1;
                    _selected = _selected.parent;
                }

                if (_selected.isSelected) {
                    _selected.isSelected = false;
                }

                // Reorder all selected nodes.
                for (int i = 1; i < _index; i++) {
                    ConversationNode _node = nodes[i];

                    if (_node.isSelected) {
                        ConversationNode _parent = _selected.parent;

                        while (_parent != null) {
                            if (_parent == _node) {
                                ArrayUtility.Remove(ref _selected.parent.nodes, _selected);
                                ArrayUtility.Insert(ref _node.parent.nodes, Array.IndexOf(_node.parent.nodes, _node), _selected);

                                _selected.parent = _node.parent;
                                break;
                            }

                            _parent = _parent.parent;
                        }

                        if (_node.parent == _selected) {
                            int _indexOf = Array.IndexOf(_node.parent.nodes, _node);
                            if (_indexOf < _insert && _indexOf != -1) {
                                _insert--;
                            }
                        }

                        ArrayUtility.Remove(ref _node.parent.nodes, _node);
                        ArrayUtility.Insert(ref _selected.nodes, _insert, _node);

                        _node.parent = _selected;
                        _node.isSelected = false;
                    }
                }

                for (int i = nodes.Count; i-- > _index + 1;) {
                    ConversationNode _node = nodes[i];

                    if (_node.isSelected) {
                        ArrayUtility.Remove(ref _node.parent.nodes, _node);
                        ArrayUtility.Insert(ref _selected.nodes, _insert, _node);

                        _node.parent = _selected;
                        _node.isSelected = false;
                    }
                }

                DragAndDrop.AcceptDrag();
                RefreshNodes();

                _event.Use();
                return true;
            }

            return false;
        }
        #endregion

        #region Node Management
        private bool IsNodeSelected(int _index) {
            try {
                return nodes[_index].isSelected;
            } catch (ArgumentOutOfRangeException _e) {
                Debug.LogException(_e);
                Debug.LogError("Node Index => " + _index + " - Count => " + nodes.Count);
            }

            return false;
        }

        private bool CanSelectNode(int _index) {
            ConversationNode _node = nodes[_index];

            while (_node.parent != null) {
                _node = _node.parent;

                if (!_node.foldout.value) {
                    return false;
                }
            }

            return true;
        }

        private void SelectNode(int _index, bool _isSelected) {
            ConversationNode _node = nodes[_index];
            if (_node.isSelected == _isSelected) {
                return;
            }

            _node.isSelected = _isSelected;

            if (_isSelected) {
                selectedNodeIndexes.Add(_index);
                doFocusSelection = _isSelected;
            } else {
                selectedNodeIndexes.Remove(_index);
            }

            RefreshSelectedNode();
        }

        private void UnselectAllNodes() {
            foreach (int _index in selectedNodeIndexes) {
                nodes[_index].isSelected = false;
            }

            selectedNodeIndexes.Clear();
            doFocusSelection = false;

            RefreshSelectedNode();
        }

        // -----------------------

        private void CreateNode(Type _nodeType) {
            if (selectedNodeIndexes.Count == 0) {
                conversation.AddNode(conversation.Root, _nodeType).isSelected = true;
            } else {
                for (int i = 0; i < selectedNodeIndexes.Count; i++) {
                    ConversationNode _node = GetSelectedNodeAtIndex(i);
                    _node.isSelected = false;

                    conversation.AddNode(_node, _nodeType).isSelected = true;
                }
            }

            RefreshNodes(false);
            EditorUtility.SetDirty(conversation);
        }

        private void DeleteSelectedNodes() {
            if (selectedNodeIndexes.Count == 0) {
                return;
            }

            if (nodes[0].isSelected) {
                Array.Resize(ref conversation.Root.nodes, 0);
            } else {
                selectedNodeIndexes.Sort();

                for (int i = selectedNodeIndexes.Count; i-- > 0;) {
                    conversation.RemoveNode(GetSelectedNodeAtIndex(i));
                }
            }

            RefreshNodes();
            RefreshLinks();

            EditorUtility.SetDirty(conversation);
        }
        #endregion

        #region Node Menu
        private const string MenuCreateSpecificTextFormat = "Create Specific Node/{0}";
        private const string MenuChangeTypeTextFormat = "Change Node Type/{0}";

        private const string PasteNodeDialogTitle = "Paste Node Values";
        private const string PasteNodeDialogMessage = "Do you want the selected node(s) to preserve their respective type, or to be converted to the type of the one copied in the clipboard?";
        private const string PasteNodeDialogConfirm = "Preserve Type";
        private const string PasteNodeDialogAlternative = "Convert";
        private const string PasteNodeDialogCancel = "Cancel";

        private readonly GUIContent menuCreateGUI = new GUIContent("Create Node", "Create a new default sub-node for all selected one(s).");

        private readonly GUIContent menuCopyGUI = new GUIContent("Copy", "Copy the selected node into the clipboard.");
        private readonly GUIContent menuDuplicateGUI = new GUIContent("Duplicate", "Duplicate the selected node(s).");

        private readonly GUIContent menuPasteValuesGUI = new GUIContent("Paste Values", "Replace the selected node(s) values by the one copied in the clipboard.");
        private readonly GUIContent menuPasteAsNewGUI = new GUIContent("Paste as New", "Create a new sub-node based on the one copied in the clipboard.");
        private readonly GUIContent menuPasteAsLinkGUI = new GUIContent("Paste as Link #v", "Create a new link sub-node redirecting to the last one copied in the clipboard.");

        // -----------------------

        private void ShowContextMenu() {
            GenericMenu _menu = new GenericMenu();
            _menu.allowDuplicateNames = true;

            // Create node.
            _menu.AddItem(menuCreateGUI, false, CreateDefaultNode);

            var _derived = TypeCache.GetTypesDerivedFrom(typeof(ConversationNode));
            foreach (Type _nodeType in _derived) {
                if (_nodeType.IsAbstract || _nodeType.IsDefined(typeof(EtherealAttribute), false)) {
                    continue;
                }

                _menu.AddItem(GetNodeTypeGUI(_nodeType, MenuCreateSpecificTextFormat), false, CreateSpecificNode, _nodeType);
            }

            if (selectedNodeIndexes.Count != 0) {
                bool _hasBuffer = ConversationNodeUtility.copyBuffer != null;
                bool _isRoot = nodes[0].isSelected;

                // Copy and duplicate (not applicable to the root).

                _menu.AddSeparator(string.Empty);

                if (!_isRoot && (selectedNodeIndexes.Count == 1)) {
                    _menu.AddItem(menuCopyGUI, false, CopyNode);
                } else {
                    _menu.AddDisabledItem(menuCopyGUI);
                }

                if (!_isRoot) {
                    _menu.AddItem(menuDuplicateGUI, false, DuplicateNode);
                } else {
                    _menu.AddDisabledItem(menuDuplicateGUI);
                }

                // Paste options.
                _menu.AddSeparator(string.Empty);

                if (_hasBuffer) {
                    _menu.AddItem(menuPasteValuesGUI, false, PasteNodeValues);
                    _menu.AddItem(menuPasteAsNewGUI, false, PasteNodeAsNew);
                    _menu.AddItem(menuPasteAsLinkGUI, false, PasteNodeAsLink);
                } else {
                    _menu.AddDisabledItem(menuPasteValuesGUI);
                    _menu.AddDisabledItem(menuPasteAsNewGUI);
                    _menu.AddDisabledItem(menuPasteAsLinkGUI);
                }

                // Node specific options.
                if (selectedNodeIndexes.Count == 1) {
                    ConversationNode _selected = GetSelectedNodeAtIndex(0);

                    for (int i = 0; i < _selected.OnEditorContextMenu(i, out GUIContent _content, out Action _callback, out bool _enabled); i++) {
                        if (i == 0) {
                            _menu.AddSeparator(string.Empty);
                        }

                        if (_enabled) {
                            _menu.AddItem(_content, false, () => _callback?.Invoke());
                        } else {
                            _menu.AddDisabledItem(_content);
                        }
                    }
                }

                // Change type.
                Type _selectedType = (selectedNodeIndexes.Count == 1) ? GetSelectedNodeAtIndex(0).GetType() : null;
                _menu.AddSeparator(string.Empty);

                foreach (Type _nodeType in _derived) {
                    if (_nodeType.IsAbstract || _nodeType.IsDefined(typeof(EtherealAttribute), false)) {
                        continue;
                    }

                    GUIContent _content = GetNodeTypeGUI(_nodeType, MenuChangeTypeTextFormat);

                    if (_nodeType == _selectedType) {
                        _menu.AddDisabledItem(_content);
                    } else {
                        _menu.AddItem(_content, false, ChangeNodeType, _nodeType);
                    }
                }

                // Delete.
                _menu.AddSeparator(string.Empty);
                _menu.AddItem(new GUIContent("Delete", "Delete the selected node(s)."), false, DeleteSelectedNodes);
            }

            _menu.ShowAsContext();
            Repaint();

            // ----- Local Method ----- \\

            GUIContent GetNodeTypeGUI(Type _nodeType, string _format) {
                DisplayNameAttribute _attribute = _nodeType.GetCustomAttribute<DisplayNameAttribute>(false);
                string _namespace = _nodeType.Namespace.Split('.')[0];
                string _name = (_attribute != null) ? _attribute.Label.text : ObjectNames.NicifyVariableName(_nodeType.Name);

                return new GUIContent(string.Format(_format, string.IsNullOrEmpty(_namespace) ? _name : $"{_namespace}/{_name}"));
            }
        }

        // -----------------------

        private void CreateDefaultNode() {
            CreateNode(conversation.DefaultNodeType);
        }

        private void CreateSpecificNode(object _type) {
            CreateNode(_type as Type);
        }

        private void CopyNode() {
            if (selectedNodeIndexes.Count == 0) {
                return;
            }

            ConversationNodeUtility.copyBuffer = GetSelectedNodeAtIndex(0);
        }

        private void DuplicateNode() {
            DupplicateNode(conversation.Root);
            RefreshNodes(false);

            // ----- Local Method ----- \\

            void DupplicateNode(ConversationNode _node) {
                for (int i = _node.nodes.Length; i-- > 0;) {
                    ConversationNode _innerNode = _node.nodes[i];

                    if (_innerNode.isSelected) {
                        ConversationNode _duplicate = Activator.CreateInstance(_innerNode.GetType()) as ConversationNode;

                        _duplicate.CopyNode(_innerNode);
                        _node.AddNode(_duplicate);

                        _innerNode.isSelected = false;
                    }

                    DupplicateNode(_innerNode);
                }
            }
        }

        private void PasteNodeValues() {
            ConversationNode _source = ConversationNodeUtility.copyBuffer;
            if ((selectedNodeIndexes.Count == 0) || (_source == null)) {
                return;
            }

            if (_source.GetType() != GetSelectedNodeAtIndex(0).GetType()) {
                int _result = EditorUtility.DisplayDialogComplex(PasteNodeDialogTitle, PasteNodeDialogMessage, PasteNodeDialogConfirm, PasteNodeDialogCancel, PasteNodeDialogAlternative);

                switch (_result) {
                    // Preserve.
                    case 0:
                        break;

                    // Convert.
                    case 2:
                        ConvertNode(Conversation.Root);
                        return;

                    default:
                        return;
                }
            }

            for (int i = selectedNodeIndexes.Count; i-- > 0;) {
                ConversationNode _node = GetSelectedNodeAtIndex(i);
                _node.CopyNode(_source, false);
            }

            // ----- Local Method ----- \\

            void ConvertNode(ConversationNode _node) {
                for (int i = _node.nodes.Length; i-- > 0;) {
                    ConversationNode _innerNode = _node.nodes[i];

                    if (_innerNode.isSelected) {
                        _innerNode = _innerNode.Transmute(conversation, _source.GetType(), true, false);
                        _node.nodes[i] = _innerNode.CopyNode(_source, false);
                    }

                    ConvertNode(_innerNode);
                }
            }
        }

        private void PasteNodeAsNew() {
            ConversationNode _source = ConversationNodeUtility.copyBuffer;
            if ((selectedNodeIndexes.Count == 0) || (_source == null)) {
                return;
            }

            for (int i = selectedNodeIndexes.Count; i-- > 0;) {
                ConversationNode _node = GetSelectedNodeAtIndex(i);
                ConversationNode _new = Activator.CreateInstance(_source.GetType()) as ConversationNode;

                _new.CopyNode(_source);
                _node.AddNode(_new);

                _new.isSelected = true;
                _node.isSelected = false;
            }

            RefreshNodes(false);
        }

        private void PasteNodeAsLink() {
            ConversationNode _source = ConversationNodeUtility.copyBuffer;
            if ((selectedNodeIndexes.Count == 0) || (_source == null)) {
                return;
            }

            for (int i = selectedNodeIndexes.Count; i-- > 0;) {
                ConversationNode _node = GetSelectedNodeAtIndex(i);
                ConversationLink _link = Activator.CreateInstance(conversation.DefaultLinkType) as ConversationLink;

                _link.SetLink(_source);
                _node.AddNode(_link);

                _link.isSelected = true;
                _node.isSelected = false;
            }

            RefreshNodes(false);
        }

        private void ChangeNodeType(object _type) {
            Type _nodeType = _type as Type;

            for (int i = 0; i < selectedNodeIndexes.Count; i++) {
                ConversationNode _node = GetSelectedNodeAtIndex(i);
                int _index = Array.IndexOf(_node.parent.nodes, _node);

                if (_index != -1) {
                    _node.Transmute(conversation, _nodeType, true, false);
                }
            }

            RefreshNodes(false);
        }
        #endregion

        #region External Editor
        internal static bool DrawInsepector(Conversation _conversation) {
            if (!isActive || (_conversation != Conversation)) {
                return false;
            }

            // Simply draw the conversation inspector on multi node selection or when the root node is selected.
            if ((selectedNodeIndexes.Count != 1) || (selectedNode == Conversation.Root)) {
                return false;
            }

            // Selected node proeprty drawer.
            EditorGUILayout.PropertyField(selectedProperty, true);
            return true;
        }

        /// <summary>
        /// Selects a specific node in the window.
        /// </summary>
        public void SelectNode(ConversationNode _node, bool _unslectOthers = true) {
            for (int i = 0; i < nodes.Count; i++) {
                if (_node == nodes[i]) {
                    UnselectAllNodes();
                    SelectNode(i, true);
                    break;
                }
            }
        }

        /// <summary>
        /// Get the currently selected node in the window.
        /// </summary>
        public static ConversationNode GetSelectedNode() {
            return selectedNode;
        }
        #endregion

        #region Utility
        private static ConversationNode GetSelectedNodeAtIndex(int _index) {
            return nodes[selectedNodeIndexes[_index]];
        }

        /// <summary>
        /// Sets this window editing conversation.
        /// </summary>
        public void SetConversation(Conversation _conversation) {
            conversation = _conversation;
            scroll = Vector2.zero;

            conversationPath = AssetDatabase.GetAssetPath(_conversation);

            RefreshConversation();
        }

        // -----------------------

        private void RefreshConversation(bool _unselect = true) {
            Conversation = conversation;
            serializedObject = (conversation != null) ? new SerializedObject(conversation) : null;

            selectedNodeIndexes.Clear();
            doFocusSelection = false;
            selectedProperty = null;
            selectedNode = null;

            ConversationNodeUtility.copyBuffer = null;

            if ((conversation != null) && (conversation.Root != null)) {
                RefreshNodes(_unselect);
            }
        }

        private void RefreshNodes(bool _unselect = true) {
            nodes.Clear();
            selectedNodeIndexes.Clear();

            if (_unselect) {
                doFocusSelection = false;
            }

            RegisterNode(conversation.Root, null);
            RefreshSelectedNode();

            // ----- Local Method ----- \\

            void RegisterNode(ConversationNode _node, ConversationNode _parent) {
                nodes.Add(_node);
                _node.parent = _parent;

                if (_node.isSelected) {
                    if (_unselect) {
                        _node.isSelected = false;
                    } else {
                        selectedNodeIndexes.Add(nodes.Count - 1);
                    }
                }

                if (_node.ShowNodes) {
                    for (int i = 0; i < _node.nodes.Length; i++) {
                        RegisterNode(_node.nodes[i], _node);
                    }
                }
            }
        }

        private void RefreshLinks() {
            foreach (ConversationNode _node in nodes) {
                if ((_node is ConversationLink _link) && (_node.nodes.Length > 0)) {
                    int _guid = _node.nodes[0].guid;

                    int _index = nodes.FindIndex((n) => n.guid == _guid);
                    if (_index == -1) {
                        _link.RemoveLink();
                    } else {
                        _link.SetLink(nodes[_index]);
                    }
                }
            }
        }

        private void RefreshSelectedNode() {
            GUI.FocusControl(string.Empty);

            if (selectedNodeIndexes.Count != 1) {
                return;
            }

            serializedObject.Update();
            SerializedProperty _root = serializedObject.FindProperty("root");
            if (!RefreshSelectedNodeRecursive(conversation.Root, _root)) {
                selectedProperty = null;
                selectedNode = conversation.Root;
            }

            InternalEditorUtility.RepaintAllViews();

            // ----- Local Method ----- \\

            bool RefreshSelectedNodeRecursive(ConversationNode _node, SerializedProperty _property) {
                if (_node.isSelected) {
                    selectedProperty = _property;
                    selectedNode = _node;
                    return true;
                }

                if (_node.ShowNodes) {
                    SerializedProperty _nodes = _property.FindPropertyRelative("nodes");

                    for (int i = 0; i < _nodes.arraySize; i++) {
                        if (RefreshSelectedNodeRecursive(_node.nodes[i], _nodes.GetArrayElementAtIndex(i))) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
        #endregion
    }
}
