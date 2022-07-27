using System;
using System.IO;
using CTFAK;
using CTFAK.Memory;
using CTFAK.MFA;
using Ionic;
using JFusion;

public class Program
{
    public static string[] asciiArt =
    {
        @"                                  ",
        @"   _ ______         _             ",
        @"  (_)  ____|       (_)            ",
        @"   _| |__ _   _ ___ _  ___  _ __  ",
        @"  | |  __| | | / __| |/ _ \| '_ \ ",
        @"  | | |  | |_| \__ \ | (_) | | | |",
        @"  | |_|   \__,_|___/_|\___/|_| |_|",
        @" _/ |                             ",
        @"|__/                              ",
    };
    public static void Main(string[] args)
    {
        Core.Init();
    SELECT_MODE:
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        foreach (var ascartLine in asciiArt)
        {
            Console.WriteLine(ascartLine);
        }

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("1. Convert MFA->Json");
        Console.WriteLine("2. Convert Json-MFA");
        Console.WriteLine("3. Read and write MFA (Debug)");
        var key = Console.ReadKey().Key;
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkMagenta;


        string mfapath = string.Empty;
        string jpath = string.Empty;


        foreach (var ascartLine in asciiArt)
        {
            Console.WriteLine(ascartLine);
        }
        try
        {
            switch (key)
            {
                case ConsoleKey.D1:
                    while (true)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("MFA path: ");
                            mfapath = Console.ReadLine().Trim('"');
                            string JOutN = string.Empty;
                            Console.WriteLine("Output Name: ");
                            JOutN = Console.ReadLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Converting MFA to Json");
                            Console.WriteLine("Reading MFA...");
                            var newMfa = new MFAData();
                            newMfa.Read(new ByteReader(mfapath, FileMode.Open));
                            Console.WriteLine("MFA reading finished.");
                            Console.WriteLine("Converting MFA...");
                            var mfaFProj = JMFAFile.FromMFA(newMfa);
                            Console.WriteLine("MFA conversion finished.");
                            Console.WriteLine("Writing Json...");
                            mfaFProj.Write($"output\\{JOutN}");
                            Console.WriteLine("Json writing finished");
                        }
                        catch when (!File.Exists(mfapath))
                        {

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR: File not found");
                            // goto MFAPATH;
                        }
                    }
                    break;


                case ConsoleKey.D2:
                    while (true)
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("Json path: ");
                            jpath = Console.ReadLine().Trim('"');
                            Console.ForegroundColor = ConsoleColor.Green;
                            string MOutN = string.Empty;
                            Console.WriteLine("Output Name: ");
                            MOutN = Console.ReadLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Converting Json to MFA");
                            Console.WriteLine("Reading Json...");
                            var ogFProj = JMFAFile.Open(jpath);
                            Console.WriteLine("Json reading finished.");
                            Console.WriteLine("Converting Json...");
                            var mfa = ogFProj.ToMFA();
                            Console.WriteLine("Json conversion finished.");
                            Console.WriteLine("Writing MFA...");
                            mfa.Write(new ByteWriter($"output\\{MOutN}.mfa", FileMode.Create));
                            Console.WriteLine("MFA writing finished");
                        }
                        catch when (!File.Exists(mfapath))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR: Project not found");
                        }
                    }
                    break;


                case ConsoleKey.D3:
                    var testMfa = new MFAData();
                    testMfa.Read(new ByteReader("test.mfa", FileMode.Open));
                    testMfa.Write(new ByteWriter("test_out.mfa", FileMode.Create));
                    break;
                default:
                    goto SELECT_MODE;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Enter) break;
            }
        }
        goto SELECT_MODE;









    }
}
