using System;
using System.Runtime.InteropServices;

namespace CTFAK.Utils;

public static class NativeLib
{
//#if WIN64
    private const string DllPath = "CTFAK-Native.dll";

    [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi,
        SetLastError = false)]
    public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr h, string m, string c, int type);
//#else
//        private const string _dllPath = "x86\\Decrypter-x86.dll";
//#endif


    [DllImport(DllPath, EntryPoint = "decompressOld", CharSet = CharSet.Auto)]
    public static extern int decompressOld(IntPtr source, int sourceSize, IntPtr output, int outputSize);


    [DllImport(DllPath, EntryPoint = "TranslateToRGBMasked", CharSet = CharSet.Auto)]
    public static extern void TranslateToRGBMasked(IntPtr result, int width, int height, int alpha, int size,
        IntPtr imageData, int tranparent, int colorMode);

    [DllImport(DllPath, EntryPoint = "TranslateToRGBA", CharSet = CharSet.Auto)]
    public static extern void TranslateToRGBA(IntPtr result, int width, int height, int alpha, int size,
        IntPtr imageData, int tranparent, int colorMode);

    [DllImport(DllPath, EntryPoint = "TranslateToBGRA", CharSet = CharSet.Auto)]
    public static extern void TranslateToBGRA(IntPtr result, int width, int height, int alpha, int size,
        IntPtr imageData, int tranparent, int colorMode);
}