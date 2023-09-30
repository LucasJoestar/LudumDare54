// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

namespace EnhancedFramework.Conversations.Editor {
    /// <summary>
    /// Editor class manipulating and updating the data contained in the <see cref="Conversation"/>.
    /// </summary>
    [InitializeOnLoad]
    public class ConversationDatabaseManager : IPreprocessBuildWithReport {
        #region Global Members
        private static readonly AutoManagedResource<ConversationDatabase> resource = new AutoManagedResource<ConversationDatabase>("ConversationDatabase", false);

        int IOrderedCallback.callbackOrder => 999;

        /// <summary>
        /// Database containing informations about all flags included in build.
        /// </summary>
        public static ConversationDatabase Database => resource.GetResource();

        // -----------------------

        static ConversationDatabaseManager() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region Management
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport _report) {
            // Called just before a build is started.
            UpdateDatabase();
            AssetDatabase.SaveAssets();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state) {
            if (_state == PlayModeStateChange.EnteredPlayMode) {
                UpdateDatabase();
            }
        }

        private static void UpdateDatabase() {
            // Register all conversations in the database.
            Database.SetDatabase(EnhancedEditorUtility.LoadAssets<Conversation>());
            EditorUtility.SetDirty(Database);
        }
        #endregion
    }
}
