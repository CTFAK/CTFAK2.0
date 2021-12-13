using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN
{
    public class Constants
    {
        public enum Products
        {
            MMF1 = 1,
            STD = 2,
            DEV = 3,
            CNC1 = 0

        }


        public enum ValueType
        {
            Long = 0,
            Int = 0,
            String = 1,
            Float = 2,
            Double = 2
        }
        public enum ObjectType
        {
            Player = -7,
            Keyboard = -6,
            Create = -5,
            Timer = -4,
            Game = -3,
            Speaker = -2,
            System = -1,
            QuickBackdrop = 0,
            Backdrop = 1,
            Active = 2,
            Text = 3,
            Question = 4,
            Score = 5,
            Lives = 6,
            Counter = 7,
            Rtf = 8,
            SubApplication = 9,
            Extension = 32,

        }
    }
}
