using System;
using System.IO;
using CTFAK;
using CTFAK.Memory;
using CTFAK.MFA;
using JFusion;

public class Program
{
    public static void Main(string[] args)
    {
        Core.Init();
        if (Console.ReadKey().Key == ConsoleKey.C)
        {
            var newMfa = new MFAData();
            newMfa.Read(new ByteReader("snakeinmyass.mfa",FileMode.Open));
            var fProj = JMFAFile.FromMFA(newMfa);
            fProj.Write("Tests\\TestFProject");
        }
        else
        {
            var fProj = JMFAFile.Open("Tests\\TestFProject\\Five Nights at Freddy's.json");
            var mfa = fProj.ToMFA();
            mfa.Write(new ByteWriter("my_ass.mfa",FileMode.Create));
        }
       
       



    }
}