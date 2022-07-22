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

        Console.ForegroundColor = ConsoleColor.White;
        
        Console.WriteLine("1. Convert MFA->Json");
        Console.WriteLine("2. Convert Json-MFA");
        Console.WriteLine("3. Read and write MFA (Debug)");
        var key = Console.ReadKey().Key;
        //Console.Clear();
        try
        {
            switch (key)
            {
                case ConsoleKey.D1:
                    Console.WriteLine("Converting MFA to Json.Reading MFA...");
                    var newMfa = new MFAData();
                    newMfa.Read(new ByteReader("snakeinmyass.mfa", FileMode.Open));
                    Console.WriteLine("MFA reading finished. Converting MFA...");
                    var mfaFProj = JMFAFile.FromMFA(newMfa);
                    Console.WriteLine("MFA conversion finished. Writing Json...");
                    mfaFProj.Write("Tests\\TestFProject");
                    Console.WriteLine("Json writing finished");
                    break;
                case ConsoleKey.D2:
                    var ogFProj = JMFAFile.Open("Tests\\TestFProject\\Five Nights at Freddy's.json");
                    var mfa = ogFProj.ToMFA();
                    mfa.Write(new ByteWriter("my_ass.mfa", FileMode.Create));
                    break;
                case ConsoleKey.D3:
                    var testMfa = new MFAData();
                    testMfa.Read(new ByteReader("test.mfa",FileMode.Open));
                    testMfa.Write(new ByteWriter("test_out.mfa",FileMode.Create));
                    break;
                default:
                    goto SELECT_MODE;
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Enter) break;
            }
        }
        goto SELECT_MODE;









    }
}