using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
#endif
using TriLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_2018_2_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
namespace TriLibEditor
{
    [InitializeOnLoad]
#if UNITY_2018_1_OR_NEWER
    public class TriLibCheckPlugins : IPreprocessBuildWithReport, IPostprocessBuildWithReport
#else
    public class TriLibCheckPlugins : IPreprocessBuild, IPostprocessBuild
#endif
    {
#if UNITY_EDITOR_OSX
	    public const string XCodeProjectPath = "Libraries/TriLib/TriLib/Plugins/iOS";
#endif
        public static bool PluginsLoaded { get; private set; }

        public int callbackOrder
        {
            get { return 1000; }
        }

	    public static bool IOSFileSharingEnabled;

        static TriLibCheckPlugins()
        {
            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
#if !TRILIB_DISABLE_OLD_VER_CHECK
                CheckForOldVersions();
#endif
#if !TRILIB_DISABLE_PLUGINS_CHECK
                CheckPlugins();
#endif
            }
        }

#if !TRILIB_DISABLE_PLUGINS_CHECK
        private static void CheckPlugins()
        {
            try
            {
                AssimpInterop.ai_IsExtensionSupported(".3ds");
                PluginsLoaded = true;
            }
            catch (Exception exception)
            {
                if (exception is DllNotFoundException)
                {
                    if (EditorUtility.DisplayDialog("TriLib plugins not found", "TriLib was unable to find the native plugins.\n\nIf you just imported the package, you will have to restart Unity editor.\n\nIf you click \"Ask to save changes and restart\", you will be prompted to save your changes (if there is any) then Unity editor will restart.\n\nOtherwise, you will have to save your changes and restart Unity editor manually.", "Ask to save changes and restart", "I will do it manually"))
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        var projectPath = Directory.GetParent(Application.dataPath);
                        EditorApplication.OpenProject(projectPath.FullName);
                    }
                }
            }
        }
#endif

#if !TRILIB_DISABLE_OLD_VER_CHECK
        private static void CheckForOldVersions()
        {
            var assimpInteropAssetGuids = AssetDatabase.FindAssets("AssimpInterop t:Script");
            if (assimpInteropAssetGuids.Length > 0)
            {
                try
                {
                    var assimpInteropAssetPath = AssetDatabase.GUIDToAssetPath(assimpInteropAssetGuids[0]);
                    if (assimpInteropAssetPath.EndsWith("AssimpInterop.cs"))
                    {
                        var assimpInteropDirectory = FileUtils.GetFileDirectory(assimpInteropAssetPath);
                        var scriptsDirectory = Directory.GetParent(assimpInteropDirectory);
                        if (scriptsDirectory != null && scriptsDirectory.Parent != null)
                        {
                            var pluginsDirectories = scriptsDirectory.Parent.GetDirectories("Plugins", SearchOption.TopDirectoryOnly);
                            if (pluginsDirectories.Length > 0)
                            {
                                var hasDeprecatedFolder = false;
                                var pluginsDirectory = pluginsDirectories[0];
                                if (pluginsDirectory.GetDirectories("Windows").Length == 0 && pluginsDirectory.GetDirectories("OSX").Length == 0 && pluginsDirectory.GetDirectories("Linux").Length == 0)
                                {
                                    hasDeprecatedFolder = true;
                                }
                                else
                                {
                                    var webGLDirectory = pluginsDirectory.GetDirectories("WebGL");
                                    if (webGLDirectory.Length > 0 && webGLDirectory[0].GetDirectories("Emscripten1.37.3").Length > 0 || webGLDirectory[0].GetDirectories("Emscripten1.37.33").Length > 0 || webGLDirectory[0].GetDirectories("Emscripten1.37.40").Length > 0 || webGLDirectory[0].GetDirectories("Emscripten1.38.11").Length > 0)
                                    {
                                        hasDeprecatedFolder = true;
                                    }
                                }
                                if (hasDeprecatedFolder)
                                {
                                    EditorUtility.DisplayDialog("TriLib", "Looks like you have an old TriLib install mixed with the newest update.\nPlease make a clean TriLib install (remove all TriLib files and install TriLib again).", "Ok");
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }
#endif

#if UNITY_2018_1_OR_NEWER
        public void OnPreprocessBuild(BuildReport report)
        {
            var pluginsBasePath = TriLibProjectUtils.FindPathRelativeToProject("Plugins");
            var buildTarget = report.summary.platform;
#else
        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            TriLibSettings.UpdateBatchSettings();
            var pluginsBasePath = TriLibProjectUtils.FindPathRelativeToProject("Plugins");
            var buildTarget = target;
#endif
            if (buildTarget == BuildTarget.iOS)
            {

#if TRILIB_USE_IOS_SIMULATOR
                var pluginsSuffix = ".debug.a";
#else
                var pluginsSuffix = "release.a";
#endif
#if TRILIB_ENABLE_IOS_FILE_SHARING
                IOSFileSharingEnabled = true;
#else
                IOSFileSharingEnabled = false;
#endif
                var iOSImporters = PluginImporter.GetImporters(BuildTarget.iOS);
                foreach (var importer in iOSImporters)
                {
                    if (importer.isNativePlugin)
                    {
                        if (importer.assetPath.StartsWith(string.Format((string)"{0}/iOS/", (object)pluginsBasePath)))
                        {
                            importer.SetIncludeInBuildDelegate(assetPath => importer.assetPath.EndsWith(pluginsSuffix));
                        }
                    }
                }
                if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
                {
#if UNITY_EDITOR_OSX
                    EditorUtility.DisplayDialog("TriLib", "Warning: Bitcode is not supported by TriLib and will be disabled.", "Ok");
#else
                    EditorUtility.DisplayDialog("TriLib", "Warning:\nBitcode is not supported. You should disable it on your project settings.", "Ok");
#endif
                }
            }
            var allImporters = PluginImporter.GetImporters(buildTarget);
            foreach (var importer in allImporters)
            {
                if (!importer.isNativePlugin)
                {
                    if (importer.assetPath == string.Format((string)"{0}/ICSharpCode.SharpZipLib.dll", (object)pluginsBasePath))
                    {
#if TRILIB_USE_ZIP
                        importer.SetIncludeInBuildDelegate(assetPath => PlayerSettings.GetScriptingBackend(BuildPipeline.GetBuildTargetGroup(buildTarget)) != ScriptingImplementation.WinRTDotNET);
#else
                        importer.SetIncludeInBuildDelegate(assetPath => false);
#endif
                    }
                }
            }
        }

#if UNITY_2018_1_OR_NEWER
        public void OnPostprocessBuild(BuildReport report)
        {
#if UNITY_EDITOR_OSX
            var buildTarget = report.summary.platform;
            var buildPath = report.summary.outputPath;
#endif
#else
        public void OnPostprocessBuild(BuildTarget target, string path)
        {
#if UNITY_EDITOR_OSX
            var buildTarget = target;
            var buildPath = path;
#endif
#endif
#if UNITY_EDITOR_OSX
		    if (buildTarget == BuildTarget.iOS) {
		    	var pbxProject = new PBXProject ();
		    	var pbxProjectPath = PBXProject.GetPBXProjectPath (buildPath);
		    	pbxProject.ReadFromFile (pbxProjectPath);
		    	var targetGuid = pbxProject.TargetGuidByName (PBXProject.GetUnityTargetName ());
		    	pbxProject.SetBuildProperty (targetGuid, "ENABLE_BITCODE", "NO");
		    	pbxProject.WriteToFile (pbxProjectPath);
		    	if (IOSFileSharingEnabled) {
		    		var plistPath = buildPath + "/info.plist";
		    		var plist = new PlistDocument ();
		    		plist.ReadFromFile (plistPath);
		    		var dict = plist.root.AsDict ();
		    		dict.SetBoolean ("UIFileSharingEnabled", true);
		    		plist.WriteToFile (plistPath);
		    	}
		    }
#endif
        }
    }
}