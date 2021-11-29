using System;
using System.Runtime.InteropServices;

namespace CTFAK.Utils
{
    public static class NativeLib
    {
//#if WIN64
        private const string _dllPath = "x64\\Decrypter-x64.dll";
//#else
//        private const string _dllPath = "x86\\Decrypter-x86.dll";
//#endif

        [DllImport(_dllPath, EntryPoint = "decode_chunk", CharSet = CharSet.Auto)]
        public static extern IntPtr decode_chunk(IntPtr chunkData, int chunkSize, byte magicChar, IntPtr wrapperKey);

        [DllImport(_dllPath, EntryPoint = "make_key", CharSet = CharSet.Auto)]
        public static extern IntPtr make_key(IntPtr cTitle, IntPtr cCopyright, IntPtr cProject, byte magicChar);

        [DllImport(_dllPath, EntryPoint = "make_key_w", CharSet = CharSet.Unicode)]
        public static extern IntPtr make_key_w(IntPtr cTitle, IntPtr cCopyright, IntPtr cProject, byte magicChar);

        [DllImport(_dllPath, EntryPoint = "make_key_combined", CharSet = CharSet.Auto)]
        public static extern IntPtr make_key_combined(IntPtr data, byte magicChar);

        [DllImport(_dllPath, EntryPoint = "make_key_w_combined", CharSet = CharSet.Auto)]
        public static extern IntPtr make_key_w_combined(IntPtr data, byte magicChar);

        [DllImport(_dllPath, EntryPoint = "GenChecksum", CharSet = CharSet.Auto)]
        public static extern UInt32 GenChecksum(IntPtr name, IntPtr pass);

        [DllImport(_dllPath, EntryPoint = "decompressOld", CharSet = CharSet.Auto)]
        public static extern int decompressOld(IntPtr source, int source_size, IntPtr output, int output_size);

    }
}