#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
using System;
namespace AOT {
    ///<summary>
    ///Attribute used to annotate functions that will be called back from the unmanaged world.
    ///</summary>
    public class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute (Type delegateType) { }
    }
}
#endif