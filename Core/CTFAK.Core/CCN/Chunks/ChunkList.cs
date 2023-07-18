using System.Collections.Generic;

namespace CTFAK.CCN.Chunks
{
    public static class ChunkList
    {
        public static Dictionary<int, string> ChunkNames = new Dictionary<int, string>()
        {
            { 4386,  "Preview" },                        // 0x1122
            { 8738,  "Mini-Header" },                    // 0x2222
            { 8739,  "Header" },                         // 0x2223
            { 8740,  "Name" },                           // 0x2224
            { 8741,  "Author" },                         // 0x2225
            { 8742,  "Menu" },                           // 0x2226
            { 8743,  "Extension Path" },                 // 0x2227
            { 8744,  "Extensions" },                     // 0x2228
            { 8745,  "Frame Items" },                    // 0x2229
            { 8746,  "Global Events" },                  // 0x222A
            { 8747,  "Frame Handles" },                  // 0x222B
            { 8748,  "Extension Data" },                 // 0x222C
            { 8749,  "Additional Extension" },           // 0x222D
            { 8750,  "App Editor-Filename" },            // 0x222E
            { 8751,  "App Target-Filename" },            // 0x222F
            { 8752,  "App Docs" },                       // 0x2230
            { 8753,  "Other Extensions" },               // 0x2231
            { 8754,  "Global Values" },                  // 0x2232
            { 8755,  "Global Strings" },                 // 0x2233
            { 8756,  "Extension List" },                 // 0x2234
            { 8757,  "Icon 16x16" },                     // 0x2235
            { 8758,  "Is Demo" },                        // 0x2236
            { 8759,  "Serial Number" },                  // 0x2237
            { 8760,  "Binary Files" },                   // 0x2238
            { 8761,  "Menu Images" },                    // 0x2239
            { 8762,  "About Text" },                     // 0x223A
            { 8763,  "Copyright" },                      // 0x223B
            { 8764,  "Global Value Names" },             // 0x223C
            { 8765,  "Global String Names" },            // 0x223D
            { 8766,  "Movement Extensions" },            // 0x223E
            { 8767,  "Frame Items 2" },                  // 0x223F
            { 8768,  "Exe Only" },                       // 0x2240
            { 8770,  "Protection" },                     // 0x2242
            { 8771,  "Shaders" },                        // 0x2243
            { 8773,  "Extended Header" },                // 0x2245
            { 8774,  "Spacer" },                         // 0x2246
            { 8775,  "Frame Offset" },                   // 0x2247
            { 8776,  "Ad Mob ID" },                      // 0x2248
            { 8779,  "Android Menu" },                   // 0x224B
            { 8787,  "2.5+ Object Headers" },            // 0x2253
            { 8788,  "2.5+ Object Names" },              // 0x2254
            { 8789,  "2.5+ Object Shaders" },            // 0x2255
            { 8790,  "2.5+ Object Properties" },         // 0x2256
            { 8792,  "Font Info" },                      // 0x2258
            { 8793,  "Fonts" },                          // 0x2259
            { 8794,  "Shaders" },                        // 0x225A
            { 13107, "Frame" },                          // 0x3333
            { 13108, "Frame Header" },                   // 0x3334
            { 13109, "Frame Name" },                     // 0x3335
            { 13110, "Frame Password" },                 // 0x3336
            { 13111, "Frame Palette" },                  // 0x3337
            { 13112, "Frame Item Instances" },           // 0x3338
            { 13113, "Frame Fade In Frame" },            // 0x3339
            { 13114, "Frame Fade Out Frame" },           // 0x333A
            { 13115, "Frame Fade In" },                  // 0x333B
            { 13116, "Frame Fade Out" },                 // 0x333C
            { 13117, "Frame Events" },                   // 0x333D
            { 13118, "Frame Play Header" },              // 0x333E
            { 13119, "Additional Frame Item" },          // 0x333F
            { 13120, "Additional Frame Item Instance" }, // 0x3340
            { 13121, "Frame Layers" },                   // 0x3341
            { 13122, "Frame Virtual Rect" },             // 0x3342
            { 13123, "Demo File Path" },                 // 0x3343
            { 13124, "Random Seed" },                    // 0x3344
            { 13125, "Frame Layer Effects" },            // 0x3345
            { 13126, "Blu-Ray Frame Options" },          // 0x3346
            { 13127, "Mvt Timer Base" },                 // 0x3347
            { 13128, "Mosiac Image Table" },             // 0x3348
            { 13129, "Frame Effects" },                  // 0x3349
            { 13130, "Frame iPhone Options" },           // 0x334A
            { 17476, "Object Info Header" },             // 0x4444
            { 17477, "Object Info Name" },               // 0x4445
            { 17478, "Object Common" },                  // 0x4446
            { 17479, "Unknown Object Chunk" },           // 0x4447
            { 17480, "Object Effects" },                 // 0x4448
            { 17664, "Object Shapes" },                  // 0x4500
            { 21845, "Image Handles" },                  // 0x5555
            { 21846, "Font Handles" },                   // 0x5556
            { 21847, "Sound Handles" },                  // 0x5557
            { 21848, "Music Handles" },                  // 0x5558
            { 26213, "Bank Offsets" },                   // 0x6665
            { 26214, "Images" },                         // 0x6666
            { 26215, "Fonts" },                          // 0x6667
            { 26216, "Sounds" },                         // 0x6668
            { 26217, "Music" },                          // 0x6669
            { 32494, "Fusion 3 Seed" },                  // 0x7EEE
            { 32639, "Last Chunk" },                     // 0x7F7F
        };
    }
}