using System;

namespace TriLib
{
    /// <summary>
    /// Delegate used to perform texture data pointer contents cleanup.
    /// </summary>
    /// <param name="dataPointer">Data pointer to perform the cleanup</param>
    public delegate void DataDisposalCallback(IntPtr dataPointer);

    /// <summary>
    /// Represents a texture.
    /// </summary>
    public class EmbeddedTextureData : IDisposable
    {
        /// <summary>
        /// Texture pixel data.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Pointer to the texture data.
        /// </summary>
        public IntPtr DataPointer;
        
        /// <summary>
        /// Texture data byte length.
        /// </summary>
        public int DataLength;

        /// <summary>
        /// Callback to run when texture data pointer contents can be disposed.
        /// </summary>
        public DataDisposalCallback OnDataDisposal;

        /// <summary>
        /// Texture width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Texture height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Texture bits-per-pixel.
        /// </summary>
        public int NumChannels = 4;

        /// <summary>
        /// <c>true</c> when the texture contains raw pixel data, <c>false</c> when it contains the file data.
        /// </summary>
        [Obsolete("Now all EmbeddedTextureData instances must contain raw data")]
        public bool IsRawData;

        /// <inheritdoc />
        public void Dispose()
        {
            if (DataPointer != IntPtr.Zero)
            {
                if (OnDataDisposal != null)
                {
                    OnDataDisposal(DataPointer);
                }
                DataPointer = IntPtr.Zero;
            }
        }
    }
}
