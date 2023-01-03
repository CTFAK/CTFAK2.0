//#define RELEASE

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CTFAK.Attributes;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks;

public class ChunkList
{
    public class ChunkLoaderData
    {
        public MethodBase AfterHandler;
        public int ChunkId;

        public string ChunkName;

        //[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public Type LoaderType;
        public Dictionary<Settings.GameType, MethodBase> ReadingHandlers;
    }

    public static readonly Dictionary<int, ChunkLoaderData> knownLoaders = new();

#if RELEASE
        static void AddChunkLoader(int id, string chunkName,[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]Type loaderType)
        {
            var loader = new ChunkLoaderData();
            loader.ChunkId = id;
            loader.ChunkName = chunkName;
            loader.LoaderType = loaderType;
            knownLoaders.Add(id,loader);
        }
#endif
    [RequiresUnreferencedCode("ChunkList locates ChunkLoaders with reflection")]
    public static void Init()
    {
#if RELEASE
            Logger.Log("Using AOT-friendly mode");
            AddChunkLoader(0x2223,"AppHeader",typeof(AppHeader));
            AddChunkLoader(0x2224,"AppName",typeof(AppName));
            AddChunkLoader(26214,"ImageBank",typeof(ImageBank));
            AddChunkLoader(8767,"FrameItems",typeof(FrameItems));
            AddChunkLoader(8745,"FrameItems",typeof(FrameItems));
            AddChunkLoader(0x3333,"Frame",typeof(Frame.Frame));
            AddChunkLoader(0x222E,"EditorFilename",typeof(EditorFilename));
            AddChunkLoader(0x223B,"Copyright",typeof(Copyright));
#else
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
            try
            {
                foreach (var type in asm.GetTypes())
                    if (type.GetCustomAttributes().Any(a => a.GetType() == typeof(ChunkLoaderAttribute)))
                    {
                        var attribute =
                            type.GetCustomAttributes().First(a => a.GetType() == typeof(ChunkLoaderAttribute)) as
                                ChunkLoaderAttribute;
                        var newChunkLoaderData = new ChunkLoaderData();
                        newChunkLoaderData.LoaderType = type;
                        newChunkLoaderData.ChunkId = attribute.chunkId;
                        newChunkLoaderData.ChunkName = attribute.chunkName;
                        foreach (var method in type.GetMethods())
                        {
                            if (method.Name == "Handle")
                                newChunkLoaderData.AfterHandler = method;

                            if (method.GetCustomAttributes().Any(a => a.GetType() == typeof(LoaderHandleAttribute)))
                                newChunkLoaderData.AfterHandler = method;
                        }

                        Logger.Log(
                            $"Found chunk loader handler for chunk id {newChunkLoaderData.ChunkId} with name \"{newChunkLoaderData.ChunkName}\"");
                        if (!knownLoaders.ContainsKey(newChunkLoaderData.ChunkId))
                            knownLoaders.Add(newChunkLoaderData.ChunkId, newChunkLoaderData);
                        else
                            Logger.Log("Multiple loaders are getting registered for chunk: " +
                                       newChunkLoaderData.ChunkId);
                    }
            }
            catch
            {
            }
#endif
    }

    public Chunk CreateChunk(ChunkLoader loader, int flag = 0)
    {
        var newChk = new Chunk();
        return newChk;
    }

    public List<Chunk> chunks;

    public delegate void OnChunkLoadedEvent(int chunkId, ChunkLoader loader);

    public delegate void HandleChunkEvent(int chunkId, ChunkLoader loader);


    public void HandleChunk(int id, ChunkLoader chunk, object parent)
    {
        if (knownLoaders.ContainsKey(id))
            knownLoaders[id].AfterHandler?.Invoke(chunk, new[] { parent });
    }

    public event HandleChunkEvent OnHandleChunk;
    public event OnChunkLoadedEvent OnChunkLoaded;

    private int chunkIndex;

    public void Read(ByteReader reader)
    {
        var i = 0;
        while (true)
        {
            i++;
            Chunk newChunk = null;
            byte[] chunkData = null;
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
                if (newChunk.Id == 8787) Settings.gameType |= Settings.GameType.TWOFIVEPLUS;

                if (knownLoaders.TryGetValue(newChunk.Id, out var loaderData))
                {
                    var newInstance = Activator.CreateInstance(loaderData.LoaderType) as ChunkLoader;
                    Logger.Log($"Reading chunk {newChunk.Id} using {loaderData.ChunkName} loader");
                    try
                    {
                        newInstance.Read(new ByteReader(new MemoryStream(chunkData)));
                        //File.WriteAllBytes($"Chunks\\{loaderData.ChunkName}-{reader.Tell()-newChunk.Size}.bin",chunkData);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(
                            $"Error while reading chunk {loaderData.ChunkName}\n{ex.Message}\n{ex.StackTrace}");
                    }

                    try
                    {
                        OnChunkLoaded?.Invoke(newChunk.Id, newInstance);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(
                            $"Error while handling chunk loading {loaderData.ChunkName}\n{ex.Message}\n{ex.StackTrace}");
                    }

                    try
                    {
                        OnHandleChunk?.Invoke(newChunk.Id, newInstance);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(
                            $"Error while doing chunk handling {loaderData.ChunkName}\n{ex.Message}\n{ex.StackTrace}");
                    }
                }
                else
                {
                    var found = ChunkNames.TryGetValue(newChunk.Id, out var chunkName);
                    Logger.Log($"Loader not found for chunk {newChunk.Id} - {(found ? chunkName : string.Empty)}");
                }
            }

            chunkIndex++;

            if (Core.parameters.Contains("-trace_chunks"))
            {
                Directory.CreateDirectory("CHUNK_TRACE");
                string chunkName = "";
                if (!ChunkList.ChunkNames.TryGetValue(newChunk.Id, out chunkName))
                {
                    chunkName = $"UNKOWN-{newChunk.Id}";
                }

                Logger.Log(
                    $"Encountered chunk: {chunkName}, chunk flag: {newChunk.Flag}, exe size: {newChunk.Size}, decompressed size: {chunkData.Length}");
                File.WriteAllBytes($"CHUNK_TRACE\\{chunkName}-{chunkIndex}.bin",chunkData);
                Logger.Log($"Raw chunk data written to CHUNK_TRACE\\{chunkName}-{chunkIndex}.bin");
            }
        }
    }


    public static Dictionary<int, string> ChunkNames = new()
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
        { 8767, "FRAMEITEMS_2" },
        { 8768, "EXEONLY" },
        { 8770, "PROTECTION" },
        { 8771, "SHADERS" },
        { 8773, "APPHEADER2" },
        { 8792, "FONTS_2" },
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
        { 32639, "LAST" }
    };
}