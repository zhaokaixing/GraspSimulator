using System;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

namespace TriLib.Extras
{
    /// <summary>
    /// Represents the avatar loader sample scene.
    /// </summary>
    public class AvatarLoaderSample : MonoBehaviour
    {
        /// <summary>
        /// Use this field to specity the Standard Assets FreeLookCam Prefab.
        /// </summary>
        public GameObject FreeLookCamPrefab;

        /// <summary>
        /// Use this field to specity the Standard Assets ThirdPersonController Prefab.
        /// </summary>
        public GameObject ThirdPersonControllerPrefab;

        /// <summary>
        /// Active Camera Game Object reference.
        /// </summary>
        public GameObject ActiveCameraGameObject;

        /// <summary>
        /// Use this field to specify your models directory within the current Application directory.
        /// </summary>
        public string ModelsDirectory = "Models";

        /// <summary>
        /// Use this field to specify the <see cref="UnityEngine.UI.Text"/> component where the sample information is displayed.
        /// </summary>
        public Text InformationText;

        /// <summary>
        /// Available avatar files list.
        /// </summary>
        private string[] _files;

        /// <summary>
        /// UI Window area.
        /// </summary>
        private Rect _windowRect;

        /// <summary>
        /// UI scroll position.
        /// </summary>
        private Vector3 _scrollPosition;

        /// <summary>
        /// Avatar Loader script reference.
        /// </summary>
        private AvatarLoader _avatarLoader;

        /// <summary>
        /// Setups the Avatar Loader instance reference and fills the available files list.
        /// </summary>
        protected void Start()
        {
            _avatarLoader = FindObjectOfType<AvatarLoader>();
            if (_avatarLoader == null)
            {
                Debug.LogError("Could not find any Avatar Loader script instance.");
                return;
            }
#if UNITY_EDITOR
            var modelsPath = string.Format("{0}/Samples/{1}", TriLibProjectUtils.FindPathRelativeToProject("TriLibExtras", "t:DefaultAsset TriLibExtras"), ModelsDirectory);
#else
            var modelsPath = Path.Combine(Path.GetFullPath("."), ModelsDirectory); 
#endif
            var supportedExtensions = AssetLoaderBase.GetSupportedFileExtensions();
            _files = Directory.GetFiles(modelsPath, "*.*").Where(x => supportedExtensions.Contains("*" + FileUtils.GetFileExtension(x) + ";")).ToArray();
            _windowRect = new Rect(20, 20, 240, Screen.height - 40);
            InformationText.text = string.Format(InformationText.text, ModelsDirectory, modelsPath);
        }

        /// <summary>
        /// Shows available files and let user select them from the UI.
        /// </summary>
        protected void OnGUI()
        {
            if (_files == null || _avatarLoader == null || FreeLookCamPrefab == null || ThirdPersonControllerPrefab == null)
            {
                return;
            }
            _windowRect = GUI.Window(0, _windowRect, HandleWindowFunction, "Available Models");
        }

        /// <summary>
        /// Handles the available files UI Window.
        /// </summary>
        /// <param name="id">Window identifier.</param>
        private void HandleWindowFunction(int id)
        {
            GUILayout.BeginVertical();
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            foreach (var file in _files)
            {
                if (GUILayout.Button(Path.GetFileName(file)))
                {
                    LoadFile(file);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void LoadFile(string file)
        {
            var thirdPersonController = Instantiate(ThirdPersonControllerPrefab);
            thirdPersonController.transform.DestroyChildren(true);
            if (_avatarLoader.LoadAvatar(file, thirdPersonController))
            {
                if (ActiveCameraGameObject != null)
                {
                    Destroy(ActiveCameraGameObject.gameObject);
                }
                ActiveCameraGameObject = Instantiate(FreeLookCamPrefab);
            }
            else
            {
                if (ActiveCameraGameObject != null)
                {
                    Destroy(ActiveCameraGameObject.gameObject);
                }
                Destroy(thirdPersonController);
            }
        }
    }
}
