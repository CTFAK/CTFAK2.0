using System.Collections.Generic;

namespace EventPreprocessor
{
    public static class ExpressionNames
    {
        public static Dictionary<int, Dictionary<int, string>> ExpressionSystemDict =
            new Dictionary<int, Dictionary<int, string>>()
            {
                {
                    -7, new Dictionary<int, string>
                    {
                        { 0, "PlayerScore" },
                        { 1, "PlayerLives" },
                        { 2, "PlayerInputDevice" },
                        { 3, "PlayerKeyName" },
                        { 4, "PlayerName" }
                    }
                },

                {
                    -6, new Dictionary<int, string>()
                    {
                        { 0, "XMouse" },
                        { 1, "YMouse" },
                        { 2, "MouseWheelValue" }
                    }
                },

                {
                    -5, new Dictionary<int, string>()
                    {
                        { 0, "TotalObjectCount" }
                    }

                },

                {
                    -4, new Dictionary<int, string>()
                    {

                        { 0, "TimerValue" },
                        { 1, "TimerHundreds" },
                        { 2, "TimerSeconds" },
                        { 3, "TimerHours" },
                        { 4, "TimerMinutes" },
                        { 5, "TimerEventIndex" }
                    }
                },

                {
                    -3, new Dictionary<int, string>()
                    {
                        { 0, "CurrentFrameOld" },
                        { 1, "PlayerCount" },
                        { 2, "XLeftFrame" },
                        { 3, "XRightFrame" },
                        { 4, "YTopFrame" },
                        { 5, "YBottomFrame" },
                        { 6, "FrameWidth" },
                        { 7, "FrameHeight" },
                        { 8, "CurrentFrame" },
                        { 9, "GetCollisionMask" },
                        { 10, "FrameRate" },
                        { 11, "GetVirtualWidth" },
                        { 12, "GetVirtualHeight" },
                        { 13, "FrameBackgroundColor" },
                        { 14, "DisplayMode" },
                        { 15, "PixelShaderVersion" },
                        { 16, "FrameAlphaCoefficient" },
                        { 17, "FrameRGBCoefficient" },
                        { 18, "FrameEffectParameter" }
                    }
                },

                {
                    -2, new Dictionary<int, string>()
                    {
                        { 0, "GetMainVolume" },
                        { 1, "GetSampleVolume" },
                        { 2, "GetChannelVolume" },
                        { 3, "GetMainPan" },
                        { 4, "GetSamplePan" },
                        { 5, "GetChannelPan" },
                        { 6, "GetSamplePosition" },
                        { 7, "GetChannelPosition" },
                        { 8, "GetSampleDuration" },
                        { 9, "GetChannelDuration" },
                        { 10, "GetSampleFrequency" },
                        { 11, "GetChannelFrequency" },
                    }
                },

                {
                    -1, new Dictionary<int, string>()
                    {
                        { -3, "," },
                        { -2, ")" },
                        { -1, "(" },
                        { 0, "Long" },
                        { 1, "Random" },
                        { 2, "GlobalValueExpression" },
                        { 3, "String" },
                        { 4, "ToString" },
                        { 5, "ToNumber" },
                        { 6, "ApplicationDrive" },
                        { 7, "ApplicationDirectory" },
                        { 8, "ApplicationPath" },
                        { 9, "ApplicationFilename" },
                        { 10, "Sin" },
                        { 11, "Cos" },
                        { 12, "Tan" },
                        { 13, "SquareRoot" },
                        { 14, "Log" },
                        { 15, "Ln" },
                        { 16, "Hex" },
                        { 17, "Bin" },
                        { 18, "Exp" },
                        { 19, "LeftString" },
                        { 20, "RightString" },
                        { 21, "MidString" },
                        { 22, "StringLength" },
                        { 23, "Double" },
                        { 24, "GlobalValue" },
                        { 28, "ToInt" },
                        { 29, "Abs" },
                        { 30, "Ceil" },
                        { 31, "Floor" },
                        { 32, "Acos" },
                        { 33, "Asin" },
                        { 34, "Atan" },
                        { 35, "Not" },
                        { 36, "DroppedFileCount" },
                        { 37, "DroppedFilename" },
                        { 38, "GetCommandLine" },
                        { 39, "GetCommandItem" },
                        { 40, "Min" },
                        { 41, "Max" },
                        { 42, "GetRGB" },
                        { 43, "GetRed" },
                        { 44, "GetGreen" },
                        { 45, "GetBlue" },
                        { 46, "LoopIndex" },
                        { 47, "NewLine" },
                        { 48, "Round" },
                        { 49, "GlobalStringExpression" },
                        { 50, "GlobalString" },
                        { 51, "LowerString" },
                        { 52, "UpperString" },
                        { 53, "Find" },
                        { 54, "ReverseFind" },
                        { 55, "GetClipboard" },
                        { 56, "TemporaryPath" },
                        { 57, "TemporaryBinaryFilePath" },
                        { 58, "FloatToString" },
                        { 59, "Atan2" },
                        { 60, "Zero" },
                        { 61, "Empty" },
                        { 62, "DistanceBetween" },
                        { 63, "AngleBetween" },
                        { 64, "ClampValue" },
                        { 65, "RandomRange" }
                    }
                },

                {
                    0, new Dictionary<int, string>()
                    {
                        { 0, ";" },
                        { 2, "+" },
                        { 4, "-" },
                        { 6, "*" },
                        { 8, "/" },
                        { 10, "%" },
                        { 12, "**" },
                        { 14, "&" },
                        { 16, "|" },
                        { 18, "^" },
                    }
                },

                {
                    2, new Dictionary<int, string>()
                    {
                        { 16, "AlterableValue" },
                        { 27, "GetAlphaCoefficient" },
                        { 80, "GetColorAt" },
                        { 81, "GetXScale" },
                        { 82, "GetYScale" },
                        { 83, "GetAngle" },
                    }
                },

                {
                    3, new Dictionary<int, string>()
                    {
                        { 80, "CurrentParagraphIndex" },
                        { 81, "CurrentText" },
                        { 82, "GetParagraph" },
                        { 83, "TextAsNumber" },
                        { 84, "ParagraphCount" },
                    }
                },

                {
                    7, new Dictionary<int, string>()
                    {
                        { 80, "CounterValue" },
                        { 81, "CounterMinimumValue" },
                        { 82, "CounterMaximumValue" },
                        { 83, "CounterColor1" },
                        { 84, "CounterColor2" },
                    }
                },

                {
                    8, new Dictionary<int, string>()
                    {
                        { 80, "RTFXPOS" },
                        { 81, "RTFYPOS" },
                        { 82, "RTFSXPAGE" },
                        { 83, "RTFSYPAGE" },
                        { 84, "RTFZOOM" },
                        { 85, "RTFWORDMOUSE" },
                        { 86, "RTFWORDXY" },
                        { 87, "RTFWORD" },
                        { 88, "RTFXWORD" },
                        { 89, "RTFYWORD" },
                        { 90, "RTFSXWORD" },
                        { 91, "RTFSYWORD" },
                        { 92, "RTFLINEMOUSE" },
                        { 93, "RTFLINEXY" },
                        { 94, "RTFXLINE" },
                        { 95, "RTFYLINE" },
                        { 96, "RTFSXLINE" },
                        { 97, "RTFSYLINE" },
                        { 98, "RTFPARAMOUSE" },
                        { 99, "RTFPARAXY" },
                        { 100, "RTFXPARA" },
                        { 101, "RTFYPARA" },
                        { 102, "RTFSXPARA" },
                        { 103, "RTFSYPARA" },
                        { 104, "RTFXWORDTEXT" },
                        { 105, "RTFYWORDTEXT" },
                        { 106, "RTFXLINETEXT" },
                        { 107, "RTFYLINETEXT" },
                        { 108, "RTFXPARATEXT" },
                        { 109, "RTFYPARATEXT" },
                        { 110, "RTFMEMSIZE" },
                        { 111, "RTFGETFOCUSWORD" },
                        { 112, "RTFGETHYPERLINK" },
                    }
                },

                {
                    9, new Dictionary<int, string>()
                    {
                        { 80, "SubApplicationFrameNumber" },
                        { 81, "SubApplicationGlobalValue" },
                        { 82, "SubApplicationGlobalString" },
                    }
                },

                {
                    32, new Dictionary<int, string>()
                    {
                        { 82, "GetValue" },
                        { 83, "GroupItemValue" }
                    }
                },

                {
                    35, new Dictionary<int, string>()
                    {
                        { 22, "GetTextColor" },
                        { 80, "GetText" }
                    }
                }
            };
    }
}
    


    
    
    
