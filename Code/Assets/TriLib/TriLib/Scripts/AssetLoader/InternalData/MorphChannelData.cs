using System.Collections.Generic;

namespace TriLib
{
    /// <summary>
    /// Represents an internal blend-shape channel.
    /// </summary>
    public class MorphChannelData
    {
        public string NodeName;
        public Dictionary<float, MorphChannelKey> MorphChannelKeys;
    }
}
