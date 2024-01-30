using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;

namespace TriLib
{
    public static class TriLibProjectUtils
    {
        public static string FindPathRelativeToProject(string baseDirectory, string searchFilter = null)
        {
#if UNITY_EDITOR
            var assetGUIDs = AssetDatabase.FindAssets(searchFilter ?? "t:DefaultAsset assimp");
            if (assetGUIDs.Length == 0)
            {
                EditorUtility.DisplayDialog("TriLib", searchFilter == null ? "Could not find any Standalone Assimp Plugin (assimp.dll, libassimp.bundle, libassimp.so).\n\nTriLib is unable to configure the Native Plugins without any of these files.\n\nPlease re-install TriLib Package." : "Could not find a TriLib Dependency.", "Ok");
                throw new Exception("Can't find TriLib Native Plugins path.");
            }
            foreach (var assetGUID in assetGUIDs)
            {
                var fullName = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(assetGUID));
                var directoryInfo = new DirectoryInfo(fullName);
                while (directoryInfo != null)
                {
                    fullName = directoryInfo.FullName;
                    if (directoryInfo.Name == baseDirectory)
                    {
                        return string.Format("Assets/{0}", FileUtils.GetRelativePath(Application.dataPath, fullName));
                    }
                    directoryInfo = Directory.GetParent(fullName);
                }
            }
            return null;
#else
            throw new Exception("You should not be calling this method outside Unity Editor.\nMaybe you're trying to run a TriLib sample designed only for Unity Editor.");
#endif
        }
    }
}