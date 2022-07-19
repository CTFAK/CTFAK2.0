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
        var mfaReader = new ByteReader("snakeinmyass.mfa", FileMode.Open);
        var newMfa = new MFAData();
        newMfa.Read(mfaReader);

        var fProj = JMFAFile.FromMFA(newMfa);
        fProj.Write("Tests\\TestFProject");



    }
}