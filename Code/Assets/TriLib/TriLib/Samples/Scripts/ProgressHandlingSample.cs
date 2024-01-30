#pragma warning disable 649
using System;
using UnityEngine;

namespace TriLib
{
    namespace Samples
    {
        /// <summary>
        /// This sample shows how to use the ProgressCallback to notify asset loading progress changes.
        /// </summary>
        public class ProgressHandlingSample : MonoBehaviour
        {
#if UNITY_EDITOR || !UNITY_WINRT
            //GUI text centered style
            private GUIStyle _centeredStyle;

            //Asset loading progress value
            private float _assetLoadingProgress;

            //Flag to indicate whether asset loading has been finished
            private bool _assetLoaded;

            //String used to hold an error message if it's thrown by the asset loader
            private string _error;

            //Empty progress bar texture used to draw progress bar on GUI
            private Texture2D _progressBarEmptyTexture;

            //Filled progress bar texture used to draw progress bar on GUI
            private Texture2D _progressBarFilledTexture;

            //GUI window width
            private const float WindowWidth = 400f;

            //GUI window height
            private const float WindowHeight = 100f;

            //GUI window content horizontal margin
            private const float HorizontalMargin = 20f;

            //Generates the progress bar textures and starts to load the asset
            private void Start()
            {
                //Generates progress bar textures
                GenerateProgressBarTextures();
                //Creates our AssetLoaderAsync instance
                using (var assetLoaderAsync = new AssetLoaderAsync())
                {
                    try
                    {
                        //Loads the given asset and set the loading progress and loading completed callback
                        assetLoaderAsync.LoadFromFile(string.Format("{0}/BigModel.obj", TriLibProjectUtils.FindPathRelativeToProject("Models", "t:Model BigModel")), null, gameObject, OnAssetLoaded, null, OnAssetLoadingProgress);
                    }
                    catch (Exception e)
                    {
                        //Stores the error message to show on GUI
                        _error = e.ToString();
                    }
                }
            }

            ///Called when the asset has been loaded
            private void OnAssetLoaded(GameObject loadedGameObject)
            {
                _assetLoaded = true;
            }

            //Called when the asset loading progress has changed
            private void OnAssetLoadingProgress(float progress)
            {
                _assetLoadingProgress = progress;
            }

            //Draws asset loading information on GUI
            private void OnGUI()
            {
                //Creates the centered text style, if it's hasn't been created yet
                if (_centeredStyle == null)
                {
                    _centeredStyle = GUI.skin.GetStyle("Label");
                    _centeredStyle.alignment = TextAnchor.UpperCenter;
                }

                //Draws the progress handling window
                var windowRect = new Rect(Screen.width / 2f - WindowWidth / 2f, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
                GUI.Window(0, windowRect, ProgressWindow, "Progress Handling Sample");
            }

            //Draws the progress message, progress bar or error message on GUI
            private void ProgressWindow(int windowID)
            {
                var labelRect = new Rect(HorizontalMargin, 30f, WindowWidth - HorizontalMargin * 2f, 30f);
                if (_error != null)
                {
                    //If there is any error, draws the error message to the GUI
                    GUI.Label(labelRect, string.Format("There was an error loading your asset: '{0}'", _error), _centeredStyle);
                }
                else if (!_assetLoaded)
                {
                    //If the asset hasn't been loaded yet, draws the loading progress message
                    GUI.Label(labelRect, string.Format("Asset loading progress: {0:P2}", _assetLoadingProgress), _centeredStyle);
                }
                else
                {
                    //If the asset has been loaded, shows the success message
                    GUI.Label(labelRect, "Asset loading completed", _centeredStyle);
                }

                //Draws the progress bar
                var progressBarWidth = WindowWidth - HorizontalMargin * 2f;
                var progressBarEmptyRect = new Rect(HorizontalMargin, 60f, progressBarWidth, 20f);
                GUI.DrawTexture(progressBarEmptyRect, _progressBarEmptyTexture);
                var progressBarFilledRect = new Rect(HorizontalMargin, 60f, progressBarWidth * _assetLoadingProgress, 20f);
                GUI.DrawTexture(progressBarFilledRect, _progressBarFilledTexture);
            }

            //Generates progress bar textures
            private void GenerateProgressBarTextures()
            {
                _progressBarEmptyTexture = new Texture2D(1, 1);
                _progressBarEmptyTexture.SetPixels(new[] {Color.black});
                _progressBarEmptyTexture.Apply();
                _progressBarFilledTexture = new Texture2D(1, 1);
                _progressBarFilledTexture.SetPixels(new[] {Color.green});
                _progressBarFilledTexture.Apply();
            }
#endif
        }
    }
}
#pragma warning restore 649
