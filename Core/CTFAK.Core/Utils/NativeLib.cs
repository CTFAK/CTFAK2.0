using System;
using System.Runtime.InteropServices;

namespace CTFAK.Utils
{
    public static class NativeLib
    {
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = false)] 
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);
//#if WIN64
        private const string _dllPath = "CTFAK-Native.dll";
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




        [DllImport(_dllPath, EntryPoint = "decompressOld", CharSet = CharSet.Auto)]
        public static extern int decompressOld(IntPtr source, int source_size, IntPtr output, int output_size);
        

        [DllImport(_dllPath, EntryPoint = "TranslateToRGBMasked", CharSet = CharSet.Auto)]
        public static extern void TranslateToRGBMasked(IntPtr result, int width, int height,int alpha,int size, IntPtr imageData,int tranparent, int colorMode);
        [DllImport(_dllPath, EntryPoint = "TranslateToRGBA", CharSet = CharSet.Auto)]
        public static extern void TranslateToRGBA(IntPtr result, int width, int height,int alpha,int size, IntPtr imageData,int tranparent, int colorMode);
        
        


    }
}