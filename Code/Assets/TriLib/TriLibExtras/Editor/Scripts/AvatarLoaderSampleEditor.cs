using UnityEngine;
using UnityEditor;
using TriLib.Extras;
using UnityEngine.SceneManagement;

namespace TriLibEditor
{
    /// <summary>
    /// Represents the avatar loader sample scene editor.
    /// </summary>
    [CustomEditor(typeof(AvatarLoaderSample))]
    public class AvatarLoaderSampleEditor : Editor
    {
        /// <summary>
        /// Dialogs title.
        /// </summary>
        private const string DialogTitle = "TriLib - Configure Sample";

        /// <summary>
        /// Displays the Option to Configure the Avatar Sample.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (SceneManager.GetActiveScene().name == "AvatarLoader")
            {
                if (GUILayout.Button("Configure Avatar Loader Sample"))
                {
                    SetupAvatarLoaderSample();
                }
            }
            base.OnInspectorGUI();
        }

        /// <summary>
        /// Setups the Avatar Loader Sample Scene.
        /// </summary>
        private void SetupAvatarLoaderSample()
        {
            var avatarLoaderSample =  target as AvatarLoaderSample;
            if (avatarLoaderSample == null)
            {
                return;
            }
            var thirdPersonControllerResults = AssetDatabase.FindAssets("ThirdPersonController t:GameObject");
            foreach (var thirdPersonControllerResult in thirdPersonControllerResults)
            {
                var thirdPersonControllerPrefabPath = AssetDatabase.GUIDToAssetPath(thirdPersonControllerResult);
                if (thirdPersonControllerPrefabPath.IndexOf("/ThirdPersonController.prefab", System.StringComparison.Ordinal) > -1)
                {
                    avatarLoaderSample.ThirdPersonControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(thirdPersonControllerPrefabPath);
                    var freeLookCameraRigResults = AssetDatabase.FindAssets("FreeLookCameraRig t:GameObject");
                    foreach (var freeLookCameraRigResult in freeLookCameraRigResults)
                    {
                        var freeLookCameraRigPath = AssetDatabase.GUIDToAssetPath(freeLookCameraRigResult);
                        if (freeLookCameraRigPath.IndexOf("/FreeLookCameraRig.prefab", System.StringComparison.Ordinal) > -1)
                        {
                            avatarLoaderSample.FreeLookCamPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(freeLookCameraRigPath);
                            EditorUtility.DisplayDialog(DialogTitle, "Sample configured.", "Ok");
                            return;
                        }
                    }
                    break;
                }
            }
            EditorUtility.DisplayDialog(DialogTitle, "To configure the Avatar Loader Sample, please download and extract \"Standard Assets\" from Asset Store first.", "Ok");
        }
    }
}
