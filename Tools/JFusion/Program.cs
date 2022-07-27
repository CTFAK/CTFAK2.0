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
        
        Console.WriteLine("1. Convert MFA to Json");
        Console.WriteLine("2. Convert Json to MFA");
        Console.WriteLine("3. Read and write MFA (Debug)");
        var key = Console.ReadKey().Key;
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        foreach (var ascartLine in asciiArt)
        {
            Console.WriteLine(ascartLine);
        }
        try
        {
            switch (key)
            {
                case ConsoleKey.D1:
                    MFAPATH:
                    Console.ForegroundColor = ConsoleColor.Green;
                    string mfapath = string.Empty;
                    Console.Write("MFA path: ");
                    mfapath = Console.ReadLine().Trim('"');
                    if (!File.Exists(mfapath))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: File not found");
                        goto MFAPATH;
                    }
                    string JOutN = string.Empty;
                    Console.WriteLine("Output Name: ");
                    JOutN = Console.ReadLine();
                    string JOutP = string.Empty;
                    Console.WriteLine("Output Path: ");
                    JOutP = Console.ReadLine().TrimEnd('\\');
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
                    mfaFProj.Write($"{JOutP}\\{JOutN}");
                    Console.WriteLine("Json writing finished");
                    break;
                

                case ConsoleKey.D2:
                    JSONPATH:
                    Console.ForegroundColor = ConsoleColor.Green;
                    string jpath = string.Empty;
                    Console.Write("Json path: ");
                    jpath = Console.ReadLine().Trim('"');
                    if (!File.Exists(jpath))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: Project not found");
                        goto JSONPATH;
                    }
                    MFAOUT:
                    Console.ForegroundColor = ConsoleColor.Green;
                    string MOutN = string.Empty;
                    string MOutP = string.Empty;
                    Console.WriteLine("Output Name: ");
                    MOutN = Console.ReadLine();
                    Console.WriteLine("Output Path: ");
                    MOutP = Console.ReadLine().TrimEnd('\\');
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Converting Json to MFA");
                    Console.WriteLine("Reading Json...");
                    var ogFProj = JMFAFile.Open(jpath);
                    Console.WriteLine("Json reading finished.");
                    Console.WriteLine("Converting Json...");
                    var mfa = ogFProj.ToMFA();
                    Console.WriteLine("Json conversion finished.");
                    Console.WriteLine("Writing MFA...");
                    mfa.Write(new ByteWriter($"{MOutP}\\{MOutN}.mfa", FileMode.Create));
                    Console.WriteLine("MFA writing finished");
                    break;
                

                case ConsoleKey.D3:
                    var testMfa = new MFAData();
                    testMfa.Read(new ByteReader("test.mfa",FileMode.Open));
                    testMfa.Write(new ByteWriter("test_out.mfa",FileMode.Create));
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
