using System;
using System.Runtime.InteropServices;
namespace STBImage
{
    /// <summary>
    ///     Represents the internal STBImage library functions and helpers.
    ///     @warning Do not modify!
    /// </summary>
    public static class STBImageInterop
    {
        #region DllPath
        /// <summary>
        /// Specifies the stb_image native library used in the bindings
        /// </summary>
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WINRT)
        private const string DllPath = "stb_image";
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
		private const string DllPath = "libstb_image";
#elif (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)
		private const string DllPath = "stb_image";
#elif (UNITY_IOS && !UNITY_EDITOR)
		private const string DllPath = "__Internal";
#elif (UNITY_WEBGL && !UNITY_EDITOR)
		private const string DllPath = "__Internal";
#else
		private const string DllPath = "libstb_image";
#endif
        #endregion DllPath

        [DllImport(DllPath, EntryPoint = "loadFromMemory")]
        private static extern IntPtr _loadFromMemory(IntPtr buffer, int len, ref int x, ref int y, ref int channelsInFile, int desiredChannels);

        [DllImport(DllPath, EntryPoint = "imageFree")]
        private static extern void _imageFree(IntPtr buffer);

        public static IntPtr LoadFromMemoryPointer(IntPtr inDataPointer, int inDataLength, out int outX, out int outY, out int outChannelsInFile, int desiredChannels, out int dataLength)
        {
            outX = 0;
            outY = 0;
            outChannelsInFile = 0;
            var dataPtr = _loadFromMemory(inDataPointer, inDataLength, ref outX, ref outY, ref outChannelsInFile, desiredChannels);
            dataLength = outX * outY * desiredChannels;
            return dataPtr;
        }

        public static IntPtr LoadFromMemory(byte[] data, out int outX, out int outY, out int outChannelsInFile, int desiredChannels, out int dataLength)
        {
            var dataHandle = LockGc(data);
            outX = 0;
            outY = 0;
            outChannelsInFile = 0;
            var dataPtr = _loadFromMemory(dataHandle.AddrOfPinnedObject(), data.Length, ref outX, ref outY, ref outChannelsInFile, desiredChannels);
            dataHandle.Free();
            dataLength = outX * outY * desiredChannels;
            return dataPtr;
        }
        public static void ImageFree(IntPtr dataPtr)
        {
            _imageFree(dataPtr);
        }
        private static GCHandle LockGc(object value)
        {
            return GCHandle.Alloc(value, GCHandleType.Pinned);
        }
    }
}