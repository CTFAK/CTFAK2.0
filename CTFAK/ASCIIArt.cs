using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK
{
    public static class ASCIIArt
    {
        static string[] art = {@" ____  _____  _____ ____  _  __   ____    ____ ",
                        @"/   _\/__ __\/    //  _ \/ |/ /  /_   \  /  _ \",
                        @"|  /    / \  |  __\| / \||   /    /   /  | / \|",
                        @"|  \__  | |  | |   | |-|||   \   /   /___| \_/|",
                        @"\____/  \_/  \_/   \_/ \|\_|\_\  \____/\/\____/",
                        @"                                               " };
        public static void DrawArt()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (var item in art)
            {
                Console.WriteLine(item);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
