using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TriLibEditor
{
#if UNITY_2018_3_OR_NEWER
    public class TriLibSettingsProvider : SettingsProvider
    {
        private class Styles
        {
            public static readonly GUIContent DisableNativePluginsChecking = new GUIContent("Disable Native Plugins Checking");
            public static readonly GUIContent DisableOldVersionsChecking = new GUIContent("Disable Deprecated Versions Checking");
            public static readonly GUIContent DisableEditorAutomaticImporting = new GUIContent("Disable Automatic Models Importing on Editor");
            public static readonly GUIContent EnableZipLoading = new GUIContent("Enable ZIP Files Loading");
            public static readonly GUIContent EnableOutputMessages = new GUIContent("Enable Importing Messages");
            public static readonly GUIContent UseIOSSimulator = new GUIContent("Link IOS Libraries for Simulator instead of Device");
            public static readonly GUIContent UseIOSFileSharing = new GUIContent("Enable IOS File Sharing");
            public static readonly GUIStyle Group = new GUIStyle { padding = new RectOffset(10, 10, 5, 5) };
        }

        public TriLibSettingsProvider(string path, SettingsScope scopes = SettingsScope.User, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {

        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            var contentWidth = GUILayoutUtility.GetLastRect().width * 0.5f;
            EditorGUIUtility.labelWidth = contentWidth;
            EditorGUIUtility.fieldWidth = contentWidth;
            EditorGUILayout.BeginVertical(Styles.Group);
            ShowToggle(TriLibSettings.DisableNativePluginsChecking, Styles.DisableNativePluginsChecking, delegate (bool value) { TriLibSettings.DisableNativePluginsChecking = value; });
            ShowToggle(TriLibSettings.DisableOldVersionsChecking, Styles.DisableOldVersionsChecking, delegate (bool value) { TriLibSettings.DisableOldVersionsChecking = value; });
            ShowToggle(TriLibSettings.DisableEditorAutomaticImporting, Styles.DisableEditorAutomaticImporting, delegate (bool value) { TriLibSettings.DisableEditorAutomaticImporting = value; });
            ShowToggle(TriLibSettings.EnableZipLoading, Styles.EnableZipLoading, delegate (bool value) { TriLibSettings.EnableZipLoading = value; });
            ShowToggle(TriLibSettings.EnableOutputMessages, Styles.EnableOutputMessages, delegate (bool value) { TriLibSettings.EnableOutputMessages = value; });
            ShowToggle(TriLibSettings.UseIOSSimulator, Styles.UseIOSSimulator, delegate (bool value) { TriLibSettings.UseIOSSimulator = value; });
            ShowToggle(TriLibSettings.EnableIOSFileSharing, Styles.UseIOSFileSharing, delegate (bool value) { TriLibSettings.EnableIOSFileSharing = value; });
            if (GUILayout.Button("Configure Native Plugins"))
            {
                TriLibConfigurePlugins.ConfigurePlugins();
                EditorUtility.DisplayDialog("TriLib", "Done configuring Native Plugins", "Ok");
            }
            EditorGUILayout.EndVertical();
            base.OnGUI(searchContext);
        }

        private static void ShowToggle(bool value, GUIContent guiContent, Action<bool> onValueChanged)
        {
            var newValue = EditorGUILayout.Toggle(guiContent, value);
            if (newValue != value)
            {
                onValueChanged(newValue);
            }
        }

        [SettingsProvider]
        public static SettingsProvider Register()
        {
            var provider = new TriLibSettingsProvider("Project/TriLib", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;
        }
    }
#else
    public class TriLibSettingsProvider : EditorWindow
    {
        private class Styles
        {
            public static readonly GUIContent DisableNativePluginsChecking = new GUIContent("Disable Native Plugins Checking");
            public static readonly GUIContent DisableOldVersionsChecking = new GUIContent("Disable Deprecated Versions Checking");
            public static readonly GUIContent DisableEditorAutomaticImporting = new GUIContent("Disable Automatic Models Importing on Editor");
            public static readonly GUIContent EnableZipLoading = new GUIContent("Enable ZIP Files Loading");
            public static readonly GUIContent EnableOutputMessages = new GUIContent("Enable Importing Messages");
            public static readonly GUIContent UseIOSSimulator = new GUIContent("Link IOS Libraries for Simulator instead of Device");
            public static readonly GUIContent UseIOSFileSharing = new GUIContent("Enable IOS File Sharing");
            public static readonly GUIStyle Group = new GUIStyle { padding = new RectOffset(10, 10, 5, 5) };
        }

        [MenuItem("Edit/Project Settings/TriLib")]
        public static void Settings()
        {
            var window = (TriLibSettingsProvider)GetWindow(typeof(TriLibSettingsProvider));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            var contentWidth = GUILayoutUtility.GetLastRect().width * 0.5f;
            EditorGUIUtility.labelWidth = contentWidth;
            EditorGUIUtility.fieldWidth = contentWidth;
            EditorGUILayout.BeginVertical(Styles.Group);
            ShowToggle(TriLibSettings.DisableNativePluginsChecking, Styles.DisableNativePluginsChecking, delegate (bool value) { TriLibSettings.DisableNativePluginsChecking = value; });
            ShowToggle(TriLibSettings.DisableOldVersionsChecking, Styles.DisableOldVersionsChecking, delegate (bool value) { TriLibSettings.DisableOldVersionsChecking = value; });
            ShowToggle(TriLibSettings.DisableEditorAutomaticImporting, Styles.DisableEditorAutomaticImporting, delegate (bool value) { TriLibSettings.DisableEditorAutomaticImporting = value; });
            ShowToggle(TriLibSettings.EnableZipLoading, Styles.EnableZipLoading, delegate (bool value) { TriLibSettings.EnableZipLoading = value; });
            ShowToggle(TriLibSettings.EnableOutputMessages, Styles.EnableOutputMessages, delegate (bool value) { TriLibSettings.EnableOutputMessages = value; });
            ShowToggle(TriLibSettings.UseIOSSimulator, Styles.UseIOSSimulator, delegate (bool value) { TriLibSettings.UseIOSSimulator = value; });
            ShowToggle(TriLibSettings.EnableIOSFileSharing, Styles.UseIOSFileSharing, delegate (bool value) { TriLibSettings.EnableIOSFileSharing = value; });
            if (GUILayout.Button("Configure Native Plugins"))
            {
                TriLibConfigurePlugins.ConfigurePlugins();
                EditorUtility.DisplayDialog("TriLib", "Done configuring Native Plugins", "Ok");
            }
            EditorGUILayout.EndVertical();
        }

        private static void ShowToggle(bool value, GUIContent guiContent, Action<bool> onValueChanged)
        {
            var newValue = EditorGUILayout.Toggle(guiContent, value);
            if (newValue != value)
            {
                onValueChanged(newValue);
            }
        }
    }
#endif
}
