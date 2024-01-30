using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TriLib
{
    /// <summary>
    /// Represents a browser file loading event.
    /// This event passes the number of files that was loaded by the browser,
    /// which could be used when calling the GetBrowserFileName and GetBrowerFileData methods.
    /// </summary>
    [Serializable]
    public class BrowserFilesLoadedEvent : UnityEvent<int>
    {

    }

    /// <summary>
    /// Represents a series of Javascript helper functions.
    /// </summary>
    public class JSHelper : MonoBehaviour
    {
        private static JSHelper _instance;

        /// <summary>
        /// Gets the "TriLibJSHelper" instance.
        /// </summary>
        public static JSHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject().AddComponent<JSHelper>();
                }
                return _instance;
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr TriLibGetBrowserFileName(int index);

        [DllImport("__Internal")]
        private static extern IntPtr TriLibGetBrowserFileData(int index);
        
        [DllImport("__Internal")]
        private static extern int TriLibGetBrowserFileLength(int index);
        
        [DllImport("__Internal")]
        private static extern void TriLibFreeMemory(IntPtr pointer);
#endif
        /// <summary>
        /// Use this event when you want to load or treat files coming from the browser.
        /// </summary>
        public BrowserFilesLoadedEvent OnBrowserFilesLoaded;

        /// <summary>
        /// Gets the registered browser file name by index.
        /// </summary>
        /// <param name="index">Browser file index.</param>
        /// <returns>Browser file name.</returns>
        public string GetBrowserFileName(int index)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var pointer = TriLibGetBrowserFileName(index);
            var fileName = Marshal.PtrToStringAuto(pointer);
            TriLibFreeMemory(pointer);
            return fileName;
#else
			return null;
#endif
        }

        /// <summary>
        /// Gets the registered browser file byte data by index.
        /// </summary>
        /// <param name="index">Browser file index.</param>
        /// <returns>Browser file byte data.</returns>
        public byte[] GetBrowserFileData(int index)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var pointer = TriLibGetBrowserFileData(index);
            if (pointer == IntPtr.Zero)
            {
                return null;
            }
            var length = TriLibGetBrowserFileLength(index);
            var data = new byte[length];
            Marshal.Copy(pointer, data, 0, length);
            TriLibFreeMemory(pointer);
            return data;
#else
			return null;
#endif
        }

        /// <summary>
        /// Assures there is only one "TriLibJSHelper" instance.
        /// </summary>
        private void Start()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError("Only one TriLibJSHelper instance allowed. Destroying new instance.");
                Destroy(gameObject);
                return;
            }
            name = "TriLibJSHelper";
            _instance = this;
        }

        /// <summary>
        /// Removes the instance reference when destroying the main object.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Handles paste events coming from browser, since Unity can't access native clipboard on WebGL.
        /// </summary>
        /// <param name="value">Pasted text value.</param>
        private void OnPaste(string value)
        {
            var activeGameObject = EventSystem.current.currentSelectedGameObject;
            if (activeGameObject == null)
            {
                return;
            }
            var inputField = activeGameObject.GetComponentInParent<InputField>();
            if (inputField == null)
            {
                return;
            }
            inputField.text = string.Format("{0}{1}{2}", inputField.text.Substring(0, inputField.selectionAnchorPosition), value, inputField.text.Substring(inputField.selectionFocusPosition));
        }

        /// <summary>
        /// Triggers the OnBrowserFilesLoaded event when the browser has finished loading files.
        /// This method is called by the WebGLTemplate that comes with TriLib when user drags and drops a file in to the browser,
        /// but it can be adjusted to load files data from any other source.
        /// </summary>
        /// <param name="filesCount">Total number of loaded files (which may contains models, ZIP files, textures and extra data).</param>
        private void FilesLoaded(int filesCount)
        {
            if (OnBrowserFilesLoaded != null)
            {
                OnBrowserFilesLoaded.Invoke(filesCount);
            }
        }
    }
}