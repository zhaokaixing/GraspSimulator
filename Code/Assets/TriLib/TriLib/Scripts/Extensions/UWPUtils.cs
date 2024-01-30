#if !UNITY_EDITOR && UNITY_WINRT && (NET_4_6 || NETFX_CORE || NET_STANDARD_2_0)
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace TriLib {
    /// <summary>
    /// Represents a series of Universal Windows Platform functions.
    /// </summary>
    public static class UWPUtils {
        /// <summary>
        /// Reads a StorageFile into a Byte array.
        /// </summary>
        /// <param name="storageFile">StorageFile to read.</param>
        /// <returns>The storage file bytes.</returns>
        public static async Task<byte[]> ReadStorageFileIntoBuffer(Windows.Storage.StorageFile storageFile)
        {
            byte[] result;
            using (var stream = await storageFile.OpenStreamForReadAsync())
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    result = memoryStream.ToArray();
                }
            }
            return result;
        }
    }
}
#endif
