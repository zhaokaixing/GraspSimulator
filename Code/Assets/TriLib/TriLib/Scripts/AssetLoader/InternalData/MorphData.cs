using UnityEngine;

namespace TriLib
{
    /// <summary>
    /// Internally represents a Unity Blend Shape.
    /// </summary>
    public class MorphData
    {
        public string Name;
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public float Weight;
    }
}
