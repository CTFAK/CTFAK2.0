using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static CTFAK.EXE.IconUtil;

namespace CTFAK.EXE
{
    public class IconExtractor
    {
        ////////////////////////////////////////////////////////////////////////
        // Constants

        // Flags for LoadLibraryEx().

        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        // Resource types for EnumResourceNames().

        private readonly static IntPtr RT_ICON = (IntPtr)3;
        private readonly static IntPtr RT_GROUP_ICON = (IntPtr)14;

        private const int MAX_PATH = 260;

        ////////////////////////////////////////////////////////////////////////
        // Fields

        private byte[][] iconData = null;   // Binary data of each icon.

        ////////////////////////////////////////////////////////////////////////
        // Public properties

        /// <summary>
        /// Gets the full path of the associated file.
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the count of the icons in the associated file.
        /// </summary>
        public int Count
        {
            get { return iconData.Length; }
        }

        /// <summary>
        /// Initializes a new instance of the IconExtractor class from the specified file name.
        /// </summary>
        /// <param name="fileName">The file to extract icons from.</param>
        public IconExtractor(string fileName)
        {
            Initialize(fileName);
        }

        /// <summary>
        /// Extracts an icon from the file.
        /// </summary>
        /// <param name="index">Zero based index of the icon to be extracted.</param>
        /// <returns>A System.Drawing.Icon object.</returns>
        /// <remarks>Always returns new copy of the Icon. It should be disposed by the user.</remarks>
        public Icon GetIcon(int index)
        {
            if (index < 0 || Count <= index)
                throw new ArgumentOutOfRangeException("index");

            // Create an Icon based on a .ico file in memory.

            using (var ms = new MemoryStream(iconData[index]))
            {
                return new Icon(ms);
            }
        }

        /// <summary>
        /// Extracts all the icons from the file.
        /// </summary>
        /// <returns>An array of System.Drawing.Icon objects.</returns>
        /// <remarks>Always returns new copies of the Icons. They should be disposed by the user.</remarks>
        public Icon[] GetAllIcons()
        {
            var icons = new List<Icon>();
            for (int i = 0; i < Count; ++i)
                icons.Add(GetIcon(i));

            return icons.ToArray();
        }

        private void Initialize(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            IntPtr hModule = IntPtr.Zero;
            try
            {
                hModule = NativeMethods.LoadLibraryEx(fileName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (hModule == IntPtr.Zero)
                    throw new Win32Exception();

                FileName = GetFileName(hModule);

                // Enumerate the icon resource and build .ico files in memory.

                var tmpData = new List<byte[]>();

                ENUMRESNAMEPROC callback = (h, t, name, l) =>
                {
                    // Refer the following URL for the data structures used here:
                    // http://msdn.microsoft.com/en-us/library/ms997538.aspx

                    // RT_GROUP_ICON resource consists of a GRPICONDIR and GRPICONDIRENTRY's.

                    var dir = GetDataFromResource(hModule, RT_GROUP_ICON, name);

                    // Calculate the size of an entire .icon file.

                    int count = BitConverter.ToUInt16(dir, 4);  // GRPICONDIR.idCount
                    int len = 6 + 16 * count;                   // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count
                    for (int i = 0; i < count; ++i)
                        len += BitConverter.ToInt32(dir, 6 + 14 * i + 8);   // GRPICONDIRENTRY.dwBytesInRes

                    using (var dst = new BinaryWriter(new MemoryStream(len)))
                    {
                        // Copy GRPICONDIR to ICONDIR.

                        dst.Write(dir, 0, 6);

                        int picOffset = 6 + 16 * count; // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count

                        for (int i = 0; i < count; ++i)
                        {
                            // Load the picture.

                            ushort id = BitConverter.ToUInt16(dir, 6 + 14 * i + 12);    // GRPICONDIRENTRY.nID
                            var pic = GetDataFromResource(hModule, RT_ICON, (IntPtr)id);

                            // Copy GRPICONDIRENTRY to ICONDIRENTRY.

                            dst.Seek(6 + 16 * i, SeekOrigin.Begin);

                            dst.Write(dir, 6 + 14 * i, 8);  // First 8bytes are identical.
                            dst.Write(pic.Length);          // ICONDIRENTRY.dwBytesInRes
                            dst.Write(picOffset);           // ICONDIRENTRY.dwImageOffset

                            // Copy a picture.

                            dst.Seek(picOffset, SeekOrigin.Begin);
                            dst.Write(pic, 0, pic.Length);

                            picOffset += pic.Length;
                        }

                        tmpData.Add(((MemoryStream)dst.BaseStream).ToArray());
                    }

                    return true;
                };
                NativeMethods.EnumResourceNames(hModule, RT_GROUP_ICON, callback, IntPtr.Zero);

                iconData = tmpData.ToArray();
            }
            finally
            {
                if (hModule != IntPtr.Zero)
                    NativeMethods.FreeLibrary(hModule);
            }
        }

        private byte[] GetDataFromResource(IntPtr hModule, IntPtr type, IntPtr name)
        {
            // Load the binary data from the specified resource.

            IntPtr hResInfo = NativeMethods.FindResource(hModule, name, type);
            if (hResInfo == IntPtr.Zero)
                throw new Win32Exception();

            IntPtr hResData = NativeMethods.LoadResource(hModule, hResInfo);
            if (hResData == IntPtr.Zero)
                throw new Win32Exception();

            IntPtr pResData = NativeMethods.LockResource(hResData);
            if (pResData == IntPtr.Zero)
                throw new Win32Exception();

            uint size = NativeMethods.SizeofResource(hModule, hResInfo);
            if (size == 0)
                throw new Win32Exception();

            byte[] buf = new byte[size];
            Marshal.Copy(pResData, buf, 0, buf.Length);

            return buf;
        }

        private string GetFileName(IntPtr hModule)
        {
            // Alternative to GetModuleFileName() for the module loaded with
            // LOAD_LIBRARY_AS_DATAFILE option.

            // Get the file name in the format like:
            // "\\Device\\HarddiskVolume2\\Windows\\System32\\shell32.dll"

            string fileName;
            {
                var buf = new StringBuilder(MAX_PATH);
                int len = NativeMethods.GetMappedFileName(
                    NativeMethods.GetCurrentProcess(), hModule, buf, buf.Capacity);
                if (len == 0)
                    throw new Win32Exception();

                fileName = buf.ToString();
            }

            // Convert the device name to drive name like:
            // "C:\\Windows\\System32\\shell32.dll"

            for (char c = 'A'; c <= 'Z'; ++c)
            {
                var drive = c + ":";
                var buf = new StringBuilder(MAX_PATH);
                int len = NativeMethods.QueryDosDevice(drive, buf, buf.Capacity);
                if (len == 0)
                    continue;

                var devPath = buf.ToString();
                if (fileName.StartsWith(devPath))
                    return (drive + fileName.Substring(devPath.Length));
            }

            return fileName;
        }
    }
    public static class IconUtil
    {
        private delegate byte[] GetIconDataDelegate(Icon icon);

        static GetIconDataDelegate getIconData;

        static IconUtil()
        {
            // Create a dynamic method to access Icon.iconData private field.

            var dm = new DynamicMethod(
                "GetIconData", typeof(byte[]), new Type[] { typeof(Icon) }, typeof(Icon));
            var fi = typeof(Icon).GetField(
                "iconData", BindingFlags.Instance | BindingFlags.NonPublic);
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, fi);
            gen.Emit(OpCodes.Ret);

            getIconData = (GetIconDataDelegate)dm.CreateDelegate(typeof(GetIconDataDelegate));
        }

        /// <summary>
        /// Split an Icon consists of multiple icons into an array of Icon each
        /// consists of single icons.
        /// </summary>
        /// <param name="icon">A System.Drawing.Icon to be split.</param>
        /// <returns>An array of System.Drawing.Icon.</returns>
        public static Icon[] Split(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            // Get an .ico file in memory, then split it into separate icons.

            var src = GetIconData(icon);

            var splitIcons = new List<Icon>();
            {
                int count = BitConverter.ToUInt16(src, 4);

                for (int i = 0; i < count; i++)
                {
                    int length = BitConverter.ToInt32(src, 6 + 16 * i + 8);    // ICONDIRENTRY.dwBytesInRes
                    int offset = BitConverter.ToInt32(src, 6 + 16 * i + 12);   // ICONDIRENTRY.dwImageOffset

                    using (var dst = new BinaryWriter(new MemoryStream(6 + 16 + length)))
                    {
                        // Copy ICONDIR and set idCount to 1.

                        dst.Write(src, 0, 4);
                        dst.Write((short)1);

                        // Copy ICONDIRENTRY and set dwImageOffset to 22.

                        dst.Write(src, 6 + 16 * i, 12); // ICONDIRENTRY except dwImageOffset
                        dst.Write(22);                   // ICONDIRENTRY.dwImageOffset

                        // Copy a picture.

                        dst.Write(src, offset, length);

                        // Create an icon from the in-memory file.

                        dst.BaseStream.Seek(0, SeekOrigin.Begin);
                        splitIcons.Add(new Icon(dst.BaseStream));
                    }
                }
            }

            return splitIcons.ToArray();
        }

        /// <summary>
        /// Converts an Icon to a GDI+ Bitmap preserving the transparent area.
        /// </summary>
        /// <param name="icon">An System.Drawing.Icon to be converted.</param>
        /// <returns>A System.Drawing.Bitmap Object.</returns>
        public static Bitmap ToBitmap(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            // Quick workaround: Create an .ico file in memory, then load it as a Bitmap.

            using (var ms = new MemoryStream())
            {
                icon.Save(ms);
                using (var bmp = (Bitmap)Image.FromStream(ms))
                {
                    return new Bitmap(bmp);
                }
            }
        }

        /// <summary>
        /// Gets the bit depth of an Icon.
        /// </summary>
        /// <param name="icon">An System.Drawing.Icon object.</param>
        /// <returns>Bit depth of the icon.</returns>
        /// <remarks>
        /// This method takes into account the PNG header.
        /// If the icon has multiple variations, this method returns the bit 
        /// depth of the first variation.
        /// </remarks>
        public static int GetBitCount(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            // Get an .ico file in memory, then read the header.

            var data = GetIconData(icon);
            if (data.Length >= 51
                && data[22] == 0x89 && data[23] == 0x50 && data[24] == 0x4e && data[25] == 0x47
                && data[26] == 0x0d && data[27] == 0x0a && data[28] == 0x1a && data[29] == 0x0a
                && data[30] == 0x00 && data[31] == 0x00 && data[32] == 0x00 && data[33] == 0x0d
                && data[34] == 0x49 && data[35] == 0x48 && data[36] == 0x44 && data[37] == 0x52)
            {
                // The picture is PNG. Read IHDR chunk.

                switch (data[47])
                {
                    case 0:
                        return data[46];
                    case 2:
                        return data[46] * 3;
                    case 3:
                        return data[46];
                    case 4:
                        return data[46] * 2;
                    case 6:
                        return data[46] * 4;
                    default:
                        // NOP
                        break;
                }
            }
            else if (data.Length >= 22)
            {
                // The picture is not PNG. Read ICONDIRENTRY structure.

                return BitConverter.ToUInt16(data, 12);
            }

            throw new ArgumentException("The icon is corrupt. Couldn't read the header.", "icon");
        }

        private static byte[] GetIconData(Icon icon)
        {
            var data = getIconData(icon);
            if (data != null)
            {
                return data;
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    icon.Save(ms);
                    return ms.ToArray();
                }
            }
        }
        internal static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

            [DllImport("kernel32.dll", SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, ENUMRESNAMEPROC lpEnumFunc, IntPtr lParam);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

            [DllImport("kernel32.dll", SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

            [DllImport("kernel32.dll", SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr LockResource(IntPtr hResData);

            [DllImport("kernel32.dll", SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

            [DllImport("kernel32.dll", SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr GetCurrentProcess();

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

            [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int GetMappedFileName(IntPtr hProcess, IntPtr lpv, StringBuilder lpFilename, int nSize);
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal delegate bool ENUMRESNAMEPROC(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
    }
}
