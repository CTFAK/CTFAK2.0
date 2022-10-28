using System.Collections.Generic;

namespace EventPreprocessor
{
    public class ConditionNames
    {
        public static Dictionary<int, Dictionary<int, string>> ConditionSystemDict =
               new Dictionary<int, Dictionary<int, string>>()
               {
                    {
                         -7, new Dictionary<int, string>()
                         {
                              {-6, "PlayerKeyDown"},
                              {-5, "PlayerDied"},
                              {-4, "PlayerKeyPressed"},
                              {-3, "NumberOfLives"},
                              {-2, "CompareScore"},
                              {-1, "PLAYERPLAYING"}
                         }
                    },

                    {
                         -6, new Dictionary<int, string>()
                         {
                              {-12, "MouseWheelDown"},
                              {-11, "MouseWheelUp"},
                              {-10, "MouseVisible"},
                              {-9, "AnyKeyPressed"},
                              {-8, "WhileMousePressed"},
                              {-7, "ObjectClicked"},
                              {-6, "MouseClickedInZone"},
                              {-5, "MouseClicked"},
                              {-4, "MouseOnObject"},
                              {-3, "MouseInZone"},
                              {-2, "KeyDown"},
                              {-1, "KeyPressed"}
                         }
                    },

                    {
                         -5, new Dictionary<int, string>()
                         {
                              {-23, "PickObjectsInLine"},
                              {-22, "PickFlagOff"},
                              {-21, "PickFlagOn"},
                              {-20, "PickAlterableValue"},
                              {-19, "PickFromFixed"},
                              {-18, "PickObjectsInZone"},
                              {-17, "PickRandomObject"},
                              {-16, "PickRandomObjectInZone"},
                              {-15, "CompareObjectCount"},
                              {-14, "AllObjectsInZone"},
                              {-13, "NoAllObjectsInZone"},
                              {-12, "PickFlagOff"},
                              {-11, "PickFlagOn"},
                              {-8, "PickAlterableValue"},
                              {-7, "PickFromFixed"},
                              {-6, "PickObjectsInZone"},
                              {-5, "PickRandomObject"},
                              {-4, "PickRandomObjectInZoneOld"},
                              {-3, "CompareObjectCount"},
                              {-2, "AllObjectsInZone"},
                              {-1, "NoAllObjectsInZone"}
                         }
                    },

                    {
                         -4, new Dictionary<int, string>()
                         {
                              {-8, "Every"},
                              {-7, "TimerEquals"},
                              {-6, "OnTimerEvent"},
                              {-5, "CompareAwayTime"},
                              {-4, "Every"},
                              {-3, "TimerEquals"},
                              {-2, "TimerLess"},
                              {-1, "TimerGreater"}
                         }
                    },

                    {
                         -3, new Dictionary<int, string>()
                         {
                              {-10, "FrameSaved"},
                              {-9, "FrameLoaded"},
                              {-8, "ApplicationResumed"},
                              {-7, "VsyncEnabled"},
                              {-6, "IsLadder"},
                              {-5, "IsObstacle"},
                              {-4, "EndOfApplication"},
                              {-3, "LEVEL"},
                              {-2, "EndOfFrame"},
                              {-1, "StartOfFrame"}
                         }
                    },

                    {
                         -2, new Dictionary<int, string>()
                         {
                              {-9, "ChannelPaused"},
                              {-8, "ChannelNotPlaying"},
                              {-7, "MusicPaused"},
                              {-6, "SamplePaused"},
                              {-5, "MusicFinished"},
                              {-4, "NoMusicPlaying"},
                              {-3, "NoSamplesPlaying"},
                              {-2, "SpecificMusicNotPlaying"},
                              {-1, "SampleNotPlaying"}
                         }
                    },

                    {
                         -1, new Dictionary<int, string>()
                         {
                              {-40, "RunningAs"},
                              {-39, "CompareGlobalValueDoubleGreater"},
                              {-38, "CompareGlobalValueDoubleGreaterEqual"},
                              {-37, "CompareGlobalValueDoubleLess"},
                              {-36, "CompareGlobalValueDoubleLessEqual"},
                              {-35, "CompareGlobalValueDoubleNotEqual"},
                              {-34, "CompareGlobalValueDoubleEqual"},
                              {-33, "CompareGlobalValueIntGreater"},
                              {-32, "CompareGlobalValueIntGreaterEqual"},
                              {-31, "CompareGlobalValueIntLess"},
                              {-30, "CompareGlobalValueIntLessEqual"},
                              {-29, "CompareGlobalValueIntNotEqual"},
                              {-28, "CompareGlobalValueIntEqual"},
                              {-27, "ElseIf"},
                              {-26, "Chance"},
                              {-25, "OrLogical"},
                              {-24, "OrFiltered"},
                              {-23, "OnGroupActivation"},
                              {-22, "ClipboardDataAvailable"},
                              {-21, "CloseSelected"},
                              {-20, "CompareGlobalString"},
                              {-19, "MenuVisible"},
                              {-18, "MenuEnabled"},
                              {-17, "MenuChecked"},
                              {-16, "OnLoop"},
                              {-15, "FilesDropped"},
                              {-14, "MenuSelected"},
                              {-13, "RECORDKEY"},
                              {-12, "GroupActivated"},
                              {-11, "GroupEnd"},
                              {-10, "NewGroup"},
                              {-9, "Remark"},
                              {-8, "CompareGlobalValue"},
                              {-7, "NotAlways"},
                              {-6, "Once"},
                              {-5, "Repeat"},
                              {-4, "RestrictFor"},
                              {-3, "Compare"},
                              {-2, "Never"},
                              {-1, "Always"}
                         }
                    },

                    {
                         2, new Dictionary<int, string>()
                         {
                              {-81, "ObjectClicked"},
                              {-25, "FlagOn"},
                              {-4, "Overlapping"},
                              {-24, "FlagOff"},
                              {-14, "UNK3"},
                              {-13, "UNK2"},
                              {-12, "UNK"},
                              {-42, "AlterableValue"},
                              {-2, "AnimationOver"}
                         }
                    },

                    {
                         4, new Dictionary<int, string>()
                         {
                              {-83, "AnswerMatches"},
                              {-82, "AnswerFalse"},
                              {-81, "AnswerTrue"}
                         }
                    },

                    {
                         7, new Dictionary<int, string>()
                         {
                              {-81, "CompareCounter"}
                         }
                    },

                    {
                         9, new Dictionary<int, string>()
                         {
                              {-84, "SubApplicationPaused"},
                              {-83, "SubApplicationVisible"},
                              {-82, "SubApplicationFinished"},
                              {-81, "SubApplicationFrameChanged"}
                         }
                    },

                    {
                         35, new Dictionary<int, string>()
                         {
                              {-85, "IsFocused"},
                              {22, "GetTextColor"},
                              {80, "GetText"}

                         }
                    }
               };
    }
}