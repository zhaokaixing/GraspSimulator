using System;
using System.Collections.Generic;
using TriLib;
using UnityEditor;
using UnityEngine;

namespace TriLibEditor
{
    public static class TriLibConfigurePlugins
    {
        public static void ConfigurePlugins()
        {
            var pluginsBasePath = TriLibProjectUtils.FindPathRelativeToProject("Plugins");
            var pluginImporters = PluginImporter.GetAllImporters();
            foreach (var pluginImporter in pluginImporters)
            {
                var buildTargets = new List<BuildTarget>();
                var addToEditor = true;
                var addToAnyPlatform = false;
                string editorCPU = null;
                string targetCPU = null;
                string targetOS = null;
                var assetDirectory = FileUtils.GetFileDirectory(pluginImporter.assetPath);
                if (assetDirectory.StartsWith(string.Format("{0}/Android", pluginsBasePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    if (assetDirectory.EndsWith("/x86"))
                    {
                        targetCPU = "x86";
                        buildTargets.Add(BuildTarget.Android);
                    }
                    else if (assetDirectory.EndsWith("/armeabi-v7a"))
                    {
                        targetCPU = "ARMV7";
                        buildTargets.Add(BuildTarget.Android);
                    }
                    else if (assetDirectory.EndsWith("/arm64"))
                    {
#if UNITY_2018_1_OR_NEWER
                        targetCPU = "ARM64";
                        buildTargets.Add(BuildTarget.Lumin);
                        buildTargets.Add(BuildTarget.Android);
#endif
                    }
                    addToEditor = false;
                }
                else if (assetDirectory.StartsWith(string.Format("{0}/iOS", pluginsBasePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    targetCPU = "AnyCPU";
                    buildTargets.Add(BuildTarget.iOS);
                    addToEditor = false;
                }
                else if (assetDirectory.StartsWith(string.Format("{0}/UWP", pluginsBasePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    if (assetDirectory.EndsWith("/x86"))
                    {
                        targetCPU = "X86";
                        buildTargets.Add(BuildTarget.WSAPlayer);
                    }
                    else if (assetDirectory.EndsWith("/x86_64"))
                    {
                        targetCPU = "X64";
                        buildTargets.Add(BuildTarget.WSAPlayer);
                    }
                    else if (assetDirectory.EndsWith("/arm64"))
                    {
#if UNITY_2018_1_OR_NEWER
                        targetCPU = "ARM64";
                        buildTargets.Add(BuildTarget.WSAPlayer);
#endif
                    }
                    addToEditor = false;
                }
                else if (assetDirectory.StartsWith(string.Format("{0}/WebGL", pluginsBasePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    targetCPU = "AnyCPU";
                    buildTargets.Add(BuildTarget.WebGL);
                    addToEditor = false;
                }
                else if (assetDirectory.StartsWith(string.Format("{0}/Windows", pluginsBasePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    if (assetDirectory.EndsWith("/x86"))
                    {
                        buildTargets.Add(BuildTarget.StandaloneWindows);
                        editorCPU = "x86";
                        targetCPU = "AnyCPU";
                    }
                    else if (assetDirectory.EndsWith("/x86_64"))
                    {
                        buildTargets.Add(BuildTarget.StandaloneWindows64);
                        editorCPU = "x86_64";
                        targetCPU = "AnyCPU";
                    }
                    targetOS = "Windows";
                }
                else if (assetDirectory.StartsWith(string.Format("{0}/Linux", pluginsBasePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    if (assetDirectory.EndsWith("/x86"))
                    {
                        editorCPU = "x86";
                        targetCPU = "x86";
                        buildTargets.Add(BuildTarget.StandaloneLinux);
                    }
                    else if (assetDirectory.EndsWith("/x86_64"))
                    {
                        editorCPU = "x86_64";
                        targetCPU = "x86_64";
                        buildTargets.Add(BuildTarget.StandaloneLinux64);
                    }
                    targetOS = "Linux";
                }
                else if (assetDirectory.StartsWith(string.Format("{0}/OSX", pluginsBasePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    editorCPU = "AnyCPU";
                    targetCPU = "AnyCPU";
                    buildTargets.Add(BuildTarget.StandaloneOSX);
                    targetOS = "OSX";
                }
                else if (assetDirectory.Equals(pluginsBasePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    targetCPU = "AnyCPU";
                    targetOS = "AnyOS";
                    AddAllBuildTargets(buildTargets);
                    addToAnyPlatform = true;
                }
                else
                {
                    continue;
                }
                pluginImporter.ClearSettings();
                if (targetCPU == null)
                {
                    RemoveFromAllPlatforms(pluginImporter, buildTargets);
                    Debug.LogWarningFormat("TriLib: Could not find suitable CPU for Plugin '{0}'", pluginImporter.assetPath);
                    continue;
                }
                AddToAnyPlatform(pluginImporter, addToAnyPlatform);
                AddToStandaloneAndEditorPlatforms(pluginImporter, buildTargets, addToEditor);
                SetTargetCPU(pluginImporter, buildTargets, editorCPU, targetCPU, targetOS, addToEditor);
                pluginImporter.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }

        private static void RemoveFromAllPlatforms(PluginImporter pluginImporter, List<BuildTarget> buildTargets)
        {
            pluginImporter.SetEditorData("CPU", null);
            AddToAnyPlatform(pluginImporter, false);
            AddAllBuildTargets(buildTargets);
            foreach (var buildTarget in buildTargets)
            {
                pluginImporter.SetCompatibleWithPlatform(buildTarget, false);
            }
        }

        private static void AddToAnyPlatform(PluginImporter pluginImporter, bool addToAnyPlatform)
        {
            Debug.LogFormat("TriLib: Removing '{0}' from other Platforms.", pluginImporter.assetPath);
            pluginImporter.SetCompatibleWithAnyPlatform(addToAnyPlatform);
        }

        private static void AddAllBuildTargets(List<BuildTarget> buildTargets)
        {
            buildTargets.Add(BuildTarget.Android);
#if UNITY_2018_1_OR_NEWER
                    buildTargets.Add(BuildTarget.Lumin);
#endif
            buildTargets.Add(BuildTarget.iOS);
            buildTargets.Add(BuildTarget.WSAPlayer);
            buildTargets.Add(BuildTarget.WebGL);
            buildTargets.Add(BuildTarget.StandaloneWindows);
            buildTargets.Add(BuildTarget.StandaloneWindows64);
            buildTargets.Add(BuildTarget.StandaloneLinux);
            buildTargets.Add(BuildTarget.StandaloneLinux64);
            buildTargets.Add(BuildTarget.StandaloneOSX);
        }

        private static void SetTargetCPU(PluginImporter pluginImporter, List<BuildTarget> buildTargets, string editorCPU, string targetCPU, string targetOS, bool addToEditor)
        {
            foreach (var buildTarget in buildTargets)
            {
                Debug.LogFormat("TriLib: Changing Target CPU of '{0}' to '{1}'", pluginImporter.assetPath, targetCPU);
                pluginImporter.SetPlatformData(buildTarget, "CPU", targetCPU);
            }
            if (addToEditor)
            {
                Debug.LogFormat("TriLib: Changing Editor Target CPU of '{0}' to '{1}'", pluginImporter.assetPath, editorCPU);
                pluginImporter.SetEditorData("CPU", editorCPU);
                Debug.LogFormat("TriLib: Changing Editor Target OS of '{0}' to '{1}'", pluginImporter.assetPath, targetOS);
                pluginImporter.SetEditorData("OS", targetOS);
            }
        }

        private static void AddToStandaloneAndEditorPlatforms(PluginImporter pluginImporter, List<BuildTarget> buildTargets, bool addToEditor)
        {
            foreach (var buildTarget in buildTargets)
            {
                Debug.LogFormat("TriLib: Adding '{0}' to '{1}' Platform.", pluginImporter.assetPath, buildTarget);
                pluginImporter.SetCompatibleWithPlatform(buildTarget, true);
            }
            if (addToEditor)
            {
                Debug.LogFormat("TriLib: Adding '{0}' to Editor.", pluginImporter.assetPath);
                pluginImporter.SetCompatibleWithEditor(true);
            }
        }
    }
}