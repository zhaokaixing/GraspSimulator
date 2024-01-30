using System.IO;
using UnityEngine;
using UnityEditor;
using TriLib;
using UnityEditor.Animations;

namespace TriLibEditor
{
    public static class TriLibAssetImporter
    {
        public static void Import(string assetPath)
        {
            var assimpLoaderOptions = AssetLoaderOptions.CreateInstance();
            var assetImporter = AssetImporter.GetAtPath(assetPath);
            var userData = assetImporter.userData;
            if (!string.IsNullOrEmpty(userData))
            {
                assimpLoaderOptions.Deserialize(userData);
            }
            var folderPath = Path.GetDirectoryName(assetPath);
            var filename = Path.GetFileName(assetPath);
            var filePath = folderPath + "/" + filename;
            var prefabPath = filePath + ".prefab";
            using (var assetLoader = new AssetLoader())
            {
                assetLoader.OnMeshCreated += (meshIndex, mesh) => AddSubAsset(mesh, prefabPath);
                assetLoader.OnMaterialCreated += (materialIndex, isOverriden, material) => AddSubAsset(material, prefabPath);
                assetLoader.OnTextureLoaded += (sourcePath, material, propertyName, texture) => AddSubAsset(texture, prefabPath);
                assetLoader.AnimatorControllerCreation += () => AnimatorController.CreateAnimatorControllerAtPath(assetPath + ".controller");
                assetLoader.OnAnimationClipCreated += (animationClipIndex, animationClip) => AddSubAsset(animationClip, prefabPath);
                assetLoader.OnAvatarCreated += (avatar, animator) => AddSubAsset(avatar, prefabPath);
                assetLoader.OnObjectLoaded += delegate (GameObject loadedGameObject)
                {
                    var existingPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                    if (existingPrefab == null)
                    {
#if UNITY_2018_3_OR_NEWER
                        existingPrefab = PrefabUtility.SaveAsPrefabAsset(loadedGameObject, prefabPath);
#else
                        existingPrefab = PrefabUtility.CreatePrefab(prefabPath, loadedGameObject);
#endif
                    }
                    else
                    {
#if UNITY_2018_3_OR_NEWER
                        existingPrefab = PrefabUtility.SaveAsPrefabAsset(loadedGameObject, prefabPath); 

#else
                        PrefabUtility.ReplacePrefab(loadedGameObject, existingPrefab, ReplacePrefabOptions.ReplaceNameBased);
#endif
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Object.DestroyImmediate(loadedGameObject);
                    var activeEditor = TriLibAssetEditor.Active;
                    if (activeEditor != null && activeEditor.AssetPath == assetPath)
                    {
                        activeEditor.OnPrefabCreated((GameObject)existingPrefab);
                    }
                };
                DeleteAllAssets(prefabPath);
                assetLoader.LoadFromFile(assetPath, assimpLoaderOptions);
            }
        }

        private static void DeleteAllAssets(string prefabPath)
        {
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(prefabPath);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(prefabPath);
            foreach (var subAsset in subAssets)
            {
                var assetType = subAsset.GetType();
                if (subAsset != mainAsset && (assetType == typeof(GameObject) || assetType == typeof(Mesh) || assetType == typeof(Texture2D) || assetType == typeof(Material) || assetType == typeof(AnimationClip)) || assetType == typeof(AnimatorController) || assetType == typeof(Avatar))
                {
                    Object.DestroyImmediate(subAsset, true);
                }
            }
        }

        private static void AddSubAsset(Object asset, string prefabPath)
        {
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(prefabPath);
            foreach (var subAsset in subAssets)
            {
                if (subAsset.name == asset.name && asset.GetType() == subAsset.GetType())
                {
#if TRILIB_OUTPUT_MESSAGES
                    Debug.LogWarningFormat("[{0}]{1}({2}) already exists in {3}", asset.GetType().Name, asset.name, asset.GetInstanceID(), prefabPath);
#endif
					if (subAsset == asset)
                    {
                        return;
                    }
                    asset.name += asset.GetInstanceID();
                }
            }
            AssetDatabase.AddObjectToAsset(asset, prefabPath);
        }
    }
}