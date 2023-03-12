using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK
{
    public static class ASCIIArt
    {
        public static string version;
        static ASCIIArt()
        {
            var description = Assembly
                        .GetAssembly(typeof(CTFAKCore))
                        .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)
                        .OfType<AssemblyFileVersionAttribute>()
                        .FirstOrDefault()?
                        .Version ?? "";
            version = $"CTFAK 2.2";
            

        }

        static string[] art = {@" ____  _____  _____ ____  _  __   ____    ____ ",
                        @"/   _\/__ __\/    //  _ \/ |/ /  /_   \  /  _ \",
                        @"|  /    / \  |  __\| / \||   /    /   /  | / \|",
                        @"|  \__  | |  | |   | |-|||   \   /   /___| \_/|",
                        @"\____/  \_/  \_/   \_/ \|\_|\_\  \____/\/\____/",
                        @"                                               " };
        static string[] art2 =
        {
             @"                                                  ",
             @"                   /////////////                  ",
             @"             //                    &//            ",
             @"          /&     //(                   (/         ",
             @"       #/       ///////////               /&      ",
             @"      /         ///////////////             /     ",
             @"    /           /////          ///           #/   ",
             @"   /          (/(  //             /////////    /  ",
             @"   /         //      /&        ////////////    /  ",
             @"  /        ///%        /////         /////      / ",
             @"  /       ////        ///////        ////       / ",
             @"  /      /////         (///#        #///        / ",
             @"   /    ////////////        (/     %//         /  ",
             @"   /    ////////             ///  //          %/  ",
             @"    /&           ///&         ////           (/   ",
             @"      /             ///////////////         /     ",
             @"       &/               ///////////       /       ",
             @"          /(                   (//     //         ",
             @"             (/#                   (/%            ",
             @"                   (///////////(                  "
        };
        public static void SetStatus(string msg)
        {
            Console.Title = $"{version}. Status: {msg}";
        }
        public static void DrawArt()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (var item in art)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(version);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void DrawArt2()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            var coeff = Console.WindowWidth / Console.WindowHeight;
            foreach (var item in art2)
            {
                //foreach (var c in item)
                for(int i=0; i<item.Length;i++)
                {
                    if ((i % 4)==0) Console.Write(" ");
                    Console.Write(item[i]);
                }
                Console.Write("\n");
                
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
