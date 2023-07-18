using System;
using System.Runtime.InteropServices;

namespace CTFAK.Utils
{
    public static class NativeLib
    {
        private const string DllPath = "CTFAK-Native.dll";

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = false)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        [DllImport(DllPath, EntryPoint = "decompressOld", CharSet = CharSet.Auto)]
        public static extern int decompressOld(IntPtr source, int source_size, IntPtr output, int output_size);

        [DllImport(DllPath, EntryPoint = "TranslateToRGBMasked", CharSet = CharSet.Auto)]
        public static extern void TranslateToRGBMasked(IntPtr result, int width, int height, int alpha, int size, IntPtr imageData, int tranparent, int colorMode);

        [DllImport(DllPath, EntryPoint = "TranslateToRGBA", CharSet = CharSet.Auto)]
        public static extern void TranslateToRGBA(IntPtr result, int width, int height, int alpha, int size, IntPtr imageData, int tranparent, int colorMode);

        [DllImport(DllPath, EntryPoint = "TranslateToBGRA", CharSet = CharSet.Auto)]
        public static extern void TranslateToBGRA(IntPtr result, int width, int height, int alpha, int size, IntPtr imageData, int tranparent, int colorMode);
    }
}