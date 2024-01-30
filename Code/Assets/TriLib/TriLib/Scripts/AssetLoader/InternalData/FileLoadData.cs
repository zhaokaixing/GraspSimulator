using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if TRILIB_USE_ZIP
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
using System.IO.Compression;
#else
using ICSharpCode.SharpZipLib.Zip;
#endif
#endif
namespace TriLib
{
    /// <summary>
    /// Represetns a ZIP file loading data.
    /// </summary>
#if TRILIB_USE_ZIP
    public class ZipGCFileLoadData : GCFileLoadData
    {
        /// <summary>
        /// Zip File from where the asset is being loaded.
        /// </summary>
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
        public ZipArchive ZipFile;
#else
        public ZipFile ZipFile;
#endif
    }
#endif

    /// <summary>
    /// Represents a browser file loading data.
    /// </summary>
    public class BrowserLoadData : FileLoadData
    {
        /// <summary>
        /// Browser files count.
        /// </summary>
        public int FilesCount;
    }

    /// <summary>
    /// Represents a model path information, with GC buffers to store resources data.
    /// </summary>
    public class GCFileLoadData : FileLoadData
    {
        private readonly List<GCHandle> _lockedBuffers = new List<GCHandle>();

        ///<inheritdoc/>
        public override void Dispose()
        {
            foreach (var lockedBuffer in _lockedBuffers)
            {
                lockedBuffer.Free();
            }
        }

        /// <inheritdoc />
        public override void AddBuffer(GCHandle bufferHandle)
        {
            _lockedBuffers.Add(bufferHandle);
        }
    }

    /// <summary>
    /// Represents a model path information.
    /// This class is used to retrieve models path information when calling <see cref="AssimpInterop.DataCallback"/> and <see cref="AssimpInterop.ExistsCallback"/> callbacks.
    /// </summary>
    public class FileLoadData : IDisposable
    {
        /// <summary>
        /// Model filename.
        /// </summary>
        public string Filename;

        /// <summary>
        /// Model base path.
        /// </summary>
        public string BasePath;

        ///<inheritdoc/>
        public virtual void Dispose()
        {

        }

        /// <summary>
        /// Virtual method used to add locked GC buffers to locked buffers list.
        /// </summary>
        /// <param name="bufferHandle">Locked GC buffer handle.</param>
        public virtual void AddBuffer(GCHandle bufferHandle)
        {

        }
    }
}
