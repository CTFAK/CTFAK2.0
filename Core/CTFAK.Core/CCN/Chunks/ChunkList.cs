using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
    public class ChunkList
    {
        public class ChunkLoaderData
        {
            public Type loaderType;
            public int chunkID;
            public string chunkName;
            public Dictionary<Settings.GameType, MethodBase> readingHandlers;
        }

        public static Dictionary<int,ChunkLoaderData> knownLoaders=new Dictionary<int, ChunkLoaderData>();
        public static void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.GetCustomAttributes().Any((a) => a.GetType()==typeof(ChunkLoaderAttribute)))
                    {
                        var attribute = type.GetCustomAttributes().First((a) => a.GetType() == typeof(ChunkLoaderAttribute)) as ChunkLoaderAttribute;
                        var newChunkLoaderData = new ChunkLoaderData();
                        newChunkLoaderData.loaderType = type;
                        newChunkLoaderData.chunkID = attribute.chunkId;
                        newChunkLoaderData.chunkName = attribute.chunkName;
                        Logger.Log($"Found chunk loader handler for chunk id {newChunkLoaderData.chunkID} with name \"{newChunkLoaderData.chunkName}\"");
                        if (!knownLoaders.ContainsKey(newChunkLoaderData.chunkID))
                        {
                            knownLoaders.Add(newChunkLoaderData.chunkID,newChunkLoaderData);
                        }
                        else
                        {
                            Logger.Log("Multiple loaders are getting registered for chunk: "+newChunkLoaderData.chunkID);
                        }
                    }
                }
            }
            
        }
        
        
        public List<Chunk> chunks;
        public delegate void OnChunkLoadedEvent(int chunkId, ChunkLoader loader);

        public event OnChunkLoadedEvent OnChunkLoaded;
        
        int chunkIndex = 0;
        public void Read(ByteReader reader)
        {
            while (true)
            {

                Chunk newChunk=null;
                byte[] chunkData=null;
                if (reader.Tell() >= reader.Size()) break;
                try
                {
                    newChunk = new Chunk();
                    chunkData = newChunk.Read(reader);
                    if (newChunk.Id == 32639) break;

                }
                catch
                {
                    continue;
                }
                finally
                {
                    
                    if (newChunk.Id == 8787) Settings.gameType = Settings.GameType.TWOFIVEPLUS;

                    if (knownLoaders.TryGetValue(newChunk.Id, out var loaderData))
                    {
                        var newInstance = Activator.CreateInstance(loaderData.loaderType) as ChunkLoader;
                        Logger.Log($"Reading chunk {newChunk.Id} using {loaderData.chunkName} loader");
                        try
                        {
                            newInstance.Read(new ByteReader(new MemoryStream(chunkData)));
                        }
                        catch(Exception ex)
                        {
                            Logger.LogWarning($"Error while reading chunk {loaderData.chunkName}\n{ex.Message}\nP{ex.StackTrace}");
                            Console.ReadKey();
                        }
                        try
                        {
                            OnChunkLoaded?.Invoke(newChunk.Id,newInstance);
                        }
                        catch(Exception ex)
                        {
                            Logger.LogWarning($"Error while handling chunk loading {loaderData.chunkName}\n{ex.Message}\nP{ex.StackTrace}");
                            Console.ReadKey();
                        }
                    
                    }
                    else Logger.Log($"Loader not found for chunk {newChunk.Id}");
                }
                
                
                
                chunkIndex++;
                
                
                
                /*if (Core.parameters.Contains("-trace_chunks"))
                {
                    string chunkName = "";
                    if (!ChunkList.ChunkNames.TryGetValue(newChunk.Id, out chunkName))
                    {
                        chunkName = $"UNKOWN-{newChunk.Id}";
                    }

                    Logger.Log(
                        $"Encountered chunk: {chunkName}, chunk flag: {newChunk.Flag}, exe size: {newChunk.Size}, decompressed size: {chunkData.Length}");
                    File.WriteAllBytes($"CHUNK_TRACE\\{gameExeName}\\{chunkName}-{chunkIndex}.bin",chunkData);
                    Logger.Log($"Raw chunk data written to CHUNK_TRACE\\{gameExeName}\\{chunkName}-{chunkIndex}.bin");
                } */
            }
        }
        
        
        public static Dictionary<int, string> ChunkNames = new Dictionary<int, string>()
        {
            { 4386, "PREVIEW" },
            { 8738, "APPMINIHEADER" },
            { 8739, "APPHEADER" },
            { 8740, "APPNAME" },
            { 8741, "APPAUTHOR" },
            { 8742, "APPMENU" },
            { 8743, "EXTPATH" },
            { 8744, "EXTENSIONS" },
            { 8745, "FRAMEITEMS" },
            { 8746, "GLOBALEVENTS" },
            { 8747, "FRAMEHANDLES" },
            { 8748, "EXTDATA" },
            { 8749, "ADDITIONAL_EXTENSION" },
            { 8750, "APPEDITORFILENAME" },
            { 8751, "APPTARGETFILENAME" },
            { 8752, "APPDOC" },
            { 8753, "OTHEREXTS" },
            { 8754, "GLOBALVALUES" },
            { 8755, "GLOBALSTRINGS" },
            { 8756, "EXTENSIONS2" },
            { 8757, "APPICON_16x16x8" },
            { 8758, "DEMOVERSION" },
            { 8759, "SECNUM" },
            { 8760, "BINARYFILES" },
            { 8761, "APPMENUIMAGES" },
            { 8762, "ABOUTTEXT" },
            { 8763, "COPYRIGHT" },
            { 8764, "GLOBALVALUENAMES" },
            { 8765, "GLOBALSTRINGNAMES" },
            { 8766, "MVTEXTS" },
            { 8767, "PRAMEITEMS_2" },
            { 8768, "EXEONLY" },
            { 8770, "PROTECTION" },
            { 8771, "SHADERS" },
            { 8773, "APPHEADER2" },
            { 13107, "FRAME" },
            { 13108, "FRAMEHEADER" },
            { 13109, "FRAMENAME" },
            { 13110, "FRAMEPASSWORD" },
            { 13111, "FRAMEPALETTE" },
            { 13112, "FRAMEITEMINSTANCES" },
            { 13113, "FRAMEFADEINFRAME" },
            { 13114, "FRAMEFADEOUTFRAME" },
            { 13115, "FRAMEFADEIN" },
            { 13116, "FRAMEFADEOUT" },
            { 13117, "FRAMEEVENTS" },
            { 13118, "FRAMEPLAYHEADER" },
            { 13119, "ADDITIONAL_FRAMEITEM" },
            { 13120, "ADDITIONAL_FRAMEITEMINSTANCE" },
            { 13121, "FRAMELAYERS" },
            { 13122, "FRAMEVIRTUALRECT" },
            { 13123, "DEMOFILEPATH" },
            { 13124, "RANDOMSEED" },
            { 13125, "FRAMELAYEREFFECTS" },
            { 13126, "BLURAYFRAMEOPTIONS" },
            { 13127, "MVTTIMERBASE" },
            { 13128, "MOSAICIMAGETABLE" },
            { 13129, "FRAMEEFFECTS" },
            { 13130, "FRAME_IPHONE_OPTIONS" },
            { 17476, "OBJINFOHEADER" },
            { 17477, "OBJINFONAME" },
            { 17478, "OBJECTSCOMMON" },
            { 17479, "OBJECTUNKNOWN" },
            { 17480, "OBJECTEFFECTS" },
            { 21845, "IMAGESOFFSETS" },
            { 21846, "FONTSOFFSETS" },
            { 21847, "SOUNDSOFFSETS" },
            { 21848, "MUSICSOFFSETS" },
            { 26214, "IMAGES" },
            { 26215, "FONTS" },
            { 26216, "SOUNDS" },
            { 26217, "MUSICS" },
            { 32639, "LAST" },

        };
    }
}