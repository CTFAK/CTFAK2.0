using CTFAK.Memory;
using Joveler.Compression.ZLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FusionAndroidTool
{
    class Program
    {
        public static string parameters;
        static string AppName;
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string arch = null;
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    arch = "x86";
                    break;
                case Architecture.X64:
                    arch = "x64";
                    break;
                case Architecture.Arm:
                    arch = "armhf";
                    break;
                case Architecture.Arm64:
                    arch = "arm64";
                    break;
            }
            string libPath = Path.Combine(arch, "zlibwapi.dll");

            if (!File.Exists(libPath))
                throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

            ZLibInit.GlobalInit(libPath);
            Console.ForegroundColor = ConsoleColor.Magenta;
            for (int i = 0; i < ASCIIArt.art.Length; i++)
            {
                Console.WriteLine(ASCIIArt.art[i]);
            }
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("By 1987kostya");
            var path = "";
            Console.ForegroundColor = ConsoleColor.White;
            if (args.Length == 0)
            {
                Console.Write(".ccn file path: ");
                path = Console.ReadLine();
                path = path.Replace("\"", "");
            }
            else path = args[0];
            

            if (!File.Exists(path))
            {
                Console.WriteLine("File not found. Press any key to exit");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            AppName = Path.GetFileNameWithoutExtension(path);
                Directory.CreateDirectory(AppName);
            var reader = new ByteReader(path, System.IO.FileMode.Open);
            string magic = reader.ReadAscii(4); //Reading header
            //Checking for header
            //if (magic == Constants.UnicodeGameHeader) Settings.Unicode = true;//PAMU
            //else if (magic == Constants.GameHeader) Settings.Unicode = false;//PAME
            //else Logger.Log("Couldn't found any known headers", true, ConsoleColor.Red);//Header not found
            var runtimeVersion = (short)reader.ReadUInt16();
            var runtimeSubversion = (short)reader.ReadUInt16();
            var productVersion = reader.ReadInt32();
            var productBuild = reader.ReadInt32();

            while (true)
            {
                var Id = reader.ReadInt16();
                if (Id == 32639) break;
                var Flag = reader.ReadInt16();
                var Size = reader.ReadInt32();
                var rawData = reader.ReadBytes(Size);
                var dataReader = new ByteReader(rawData);
                if (Id == 26214)
                {
                    var faggot = dataReader.ReadInt16();
                    var count = dataReader.ReadInt16();
                    Console.WriteLine($"Found {count} images\n");
                    for (int i = 0; i < count; i++)
                    {
                        var Handle = dataReader.ReadInt16();
                        var unk = (uint)dataReader.ReadInt32();

                        var width = dataReader.ReadInt16(); //width
                        var height = dataReader.ReadInt16(); //height
                        var HotspotX = dataReader.ReadInt16();
                        var HotspotY = dataReader.ReadInt16();
                        var ActionX = dataReader.ReadInt16();
                        var ActionY = dataReader.ReadInt16();
                        var size = dataReader.ReadInt32();
                        var thefuk1 = dataReader.PeekByte();
                        ByteReader imageReader;
                        byte[] imageData = null;
                        
                        if (thefuk1 == 255)
                        {
                            imageData = dataReader.ReadBytes(size);
                            //return;
                        }
                        else
                        {
                            imageReader = new ByteReader(Decompressor.DecompressBlock(dataReader, size));
                            imageReader.Seek(0);
                            imageData = imageReader.ReadBytes();
                        }
                        //if (Handle != 155) continue;

                        Console.SetCursorPosition(0, Console.CursorTop-1 );
                        Console.WriteLine($"{Math.Ceiling(((double)i / count) * 100)}% done");
                        new Thread(() => {

                            //Console.WriteLine($"Image {Handle},size: {width}x{height}, colormode {unk}, pad: {imageData.Length - 3*(width * height)}");
                            //Console.WriteLine(
                            //$"{i} Loading image {width}x{height} with handle {Handle}, Size {size}. unk: {unk}. Bytes left: {dataReader.Size() - dataReader.Tell()}");
                            DumpImage(Handle, imageData, width, height, unk);
                        }).Start();
                        //DumpImage(Handle, imageData, width, height, unk);

                    }

                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished in {stopwatch.Elapsed.TotalSeconds}");
            Console.ReadKey();
        }
        public static void DumpImage(int Handle, byte[] imageData,int width, int height,uint unk)
        {
            byte[] colorArray = new byte[width * height * 4];
            int stride = width * 4;

            int position = 0;
            if (unk == 0)
            {
                int pad = GetPadding(width, 4);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {



                        colorArray[(y * stride) + (x * 4) + 2] = imageData[position];
                        colorArray[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                        colorArray[(y * stride) + (x * 4) + 0] = imageData[position + 2];
                        colorArray[(y * stride) + (x * 4) + 3] = imageData[position + 3];




                        position += 4;
                    }

                    position += pad * 3;
                }
            }
            else if (unk == 1)
            {
                int pad = GetPadding(width, 3);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        UInt16 newShort = (ushort)(imageData[position] | imageData[position + 1] << 8);

                        byte a = (byte)((newShort & 0xf) >> 0);
                        byte r = (byte)((newShort & 0xF000) >> 12);
                        byte g = (byte)((newShort & 0xF00) >> 8);
                        byte b = (byte)((newShort & 0xf0) >> 4);

                        r = (byte)(r << 4);
                        g = (byte)(g << 4);
                        b = (byte)(b << 4);
                        a = (byte)(a << 4);
                        //r done
                        //g partially done


                        colorArray[(y * stride) + (x * 4) + 2] = r;
                        colorArray[(y * stride) + (x * 4) + 1] = g;
                        colorArray[(y * stride) + (x * 4) + 0] = b;
                        colorArray[(y * stride) + (x * 4) + 3] = a;

                        position += 2;
                    }

                    position += pad;// * 2;
                }
            }
            else if (unk==2)
            {
                int pad = GetPadding(width, 3);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        UInt16 newShort = (ushort)(imageData[position] | imageData[position + 1] << 8);
                        
                        byte a = (byte)((newShort & 0xf) >> 0);
                        byte r = (byte)((newShort & 0xf800) >> 11);
                        byte g = (byte)((newShort & 0x7c0) >> 6);
                        byte b = (byte)((newShort & 0x3e) >> 1);

                        r = (byte)(r << 3);
                        g = (byte)(g << 3);
                        b = (byte)(b << 3);
                        a = (byte)(a << 4);
                        //r done
                        //g partially done


                        colorArray[(y * stride) + (x * 4) + 2] = r;
                        colorArray[(y * stride) + (x * 4) + 1] = g;
                        colorArray[(y * stride) + (x * 4) + 0] = b;
                        colorArray[(y * stride) + (x * 4) + 3] = 255;
                        if(newShort==0) colorArray[(y * stride) + (x * 4) + 3] = 0;

                        position += 2;
                    }

                    position += pad * 2;
                }
            }
            else if (unk == 3)
            {
                int pad = GetPadding(width, 3,4);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {



                        colorArray[(y * stride) + (x * 4) + 2] = imageData[position];
                        colorArray[(y * stride) + (x * 4) + 1] = imageData[position + 1];
                        colorArray[(y * stride) + (x * 4) + 0] = imageData[position + 2];
                        colorArray[(y * stride) + (x * 4) + 3] = 255;

                        position += 3;
                    }
                    position += pad;
                    
                    /*if (height * width * 3 != imageData.Length && height * width * 3 + height * 3 != imageData.Length && height * width * 3 + height != imageData.Length)
                    {
                        position += 2;
                    }
                    else if (height * width * 3 + height * 3 == imageData.Length)
                    {
                        position += 3;
                    }
                    else if (height * width * 3 + height == imageData.Length)
                    {
                        position++;
                    }*/


                }
                

            }
            else if (unk == 4)
            {
                int pad = GetPadding(width, 3);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        UInt16 newShort = (ushort)(imageData[position] | imageData[position + 1] << 8);

                        byte r = (byte)((newShort & 0xF800) >> 11);
                        byte g = (byte)((newShort & 0x7E0) >> 5);
                        byte b = (byte)((newShort & 0x1F));

                        r = (byte)(r << 3);
                        g = (byte)(g << 2);
                        b = (byte)(b << 3);
                        //r done
                        //g partially done


                        colorArray[(y * stride) + (x * 4) + 2] = r;
                        colorArray[(y * stride) + (x * 4) + 1] = g;
                        colorArray[(y * stride) + (x * 4) + 0] = b;
                        colorArray[(y * stride) + (x * 4) + 3] = 255;

                        position += 2;
                    }

                    position += pad * 2;
                }
            }
            else if (unk == 5)
            {
                File.WriteAllBytes($"{AppName}\\{Handle}-{unk}.jpg", imageData);
                return;
            }
            else Console.WriteLine("BROKEN COLOR MODE "+unk);


            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
                        bmp.Width,
                        bmp.Height),
                    ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                IntPtr pNative = bmpData.Scan0;
                Marshal.Copy(colorArray, 0, pNative, colorArray.Length);

                bmp.UnlockBits(bmpData);
                bmp.Save($"{AppName}\\{Handle}-{unk}.png");








            }
        }
        public static int GetPadding(int width, int pointSize, int bytes = 2)
        {
            return (bytes - ((width * pointSize) % bytes)) % bytes;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }

}
