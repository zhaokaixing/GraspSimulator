using System;

namespace STB
{
    /// <summary>
    /// Represents a texture data loader that uses STBImage.
    /// </summary>
    public static class STBImageLoader
    {
        public static IntPtr LoadTextureDataFromByteArray(byte[] bytes, out int width, out int height, out int channelsInFile, out int dataLength)
        {
            return STBImage.STBImageInterop.LoadFromMemory(bytes, out width, out height, out channelsInFile, 4, out dataLength);
        }

        public static IntPtr LoadTextureFromDataPointer(IntPtr inDataPointer, int inDataLength, out int width, out int height, out int channelsInFile, out int dataLength)
        {
            return STBImage.STBImageInterop.LoadFromMemoryPointer(inDataPointer, inDataLength, out width, out height, out channelsInFile, 4, out dataLength);
        }

        public static void UnloadTextureData(IntPtr dataPointer)
        {
            STBImage.STBImageInterop.ImageFree(dataPointer);
        }
    }
}