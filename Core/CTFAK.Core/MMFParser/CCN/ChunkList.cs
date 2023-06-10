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

namespace CTFAK.MMFParser.CCN;

public class ChunkList
{
    public class ChunkLoaderData
    {
        public MethodBase AfterHandler;
        public short ChunkId;

        public string ChunkName;

        //[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public Type LoaderType;
        public Dictionary<Settings.GameType, MethodBase> ReadingHandlers;
    }

    public static readonly Dictionary<int, ChunkLoaderData> KnownLoaders = new();

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
                        newChunkLoaderData.ChunkId = attribute.ChunkId;
                        newChunkLoaderData.ChunkName = attribute.ChunkName;


                        Logger.Log(
                            $"Found chunk loader handler for chunk id {newChunkLoaderData.ChunkId} with name \"{newChunkLoaderData.ChunkName}\"");
                        if (!KnownLoaders.ContainsKey(newChunkLoaderData.ChunkId))
                            KnownLoaders.Add(newChunkLoaderData.ChunkId, newChunkLoaderData);
                        else
                            Logger.Log("Multiple loaders are getting registered for chunk: " +
                                       newChunkLoaderData.ChunkId);
                    }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error white loading chunk loaders: "+ex);
            }
#endif
    }

    private short GetChunkId(ChunkLoader loader)
    {
        foreach (var loaderData in KnownLoaders)
            if (loaderData.Value.LoaderType == loader.GetType())
                return loaderData.Value.ChunkId;

        return -1;
    }

    public Chunk CreateChunk(short id, ChunkFlags flag = ChunkFlags.NotCompressed)
    {
        var newChk = new Chunk();

        newChk.Id = id;

        newChk.Flag = flag;
        return newChk;
    }

    public Chunk CreateChunk(ChunkLoader loader, ChunkFlags flag = ChunkFlags.NotCompressed, short id = -1)
    {
        var newChk = new Chunk();
        if (id != -1)
            newChk.Id = GetChunkId(loader);
        else newChk.Id = id;

        newChk.Flag = flag;
        return newChk;
    }

    public List<Chunk> Items = new();

    public delegate void OnChunkLoadedEvent(int chunkId, ChunkLoader loader);

    public delegate void HandleChunkEvent(int chunkId, ChunkLoader loader);


    
    public event OnChunkLoadedEvent OnChunkLoaded;

    private int _chunkIndex;

    public void Write(ByteWriter writer)
    {
        foreach (var chk in Items)
        {
            chk.Write(writer);
        }
    }

    public void Read(ByteReader reader)
    {
        while (true)
        {
            Chunk newChunk = null;
            byte[] chunkData = null;
            if (reader.Tell() >= reader.Size()) break;
            try
            {
                newChunk = new Chunk();
                chunkData = newChunk.Read(reader);
                if (newChunk.Id == 32639) break;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while reading chunk {newChunk.Id} from file.");
                Logger.LogError(ex);
            }
            finally
            {
                if (newChunk.Id == 8787) Settings.gameType |= Settings.GameType.TWOFIVEPLUS;
                if (newChunk.Id == 8740 && newChunk.Flag == ChunkFlags.NotCompressed && !Settings.Old)
                    Settings.gameType = Settings.GameType.ANDROID; // kinda dumb but kinda smart at the same time

                if (KnownLoaders.TryGetValue(newChunk.Id, out var loaderData))
                {
                    var newInstance = Activator.CreateInstance(loaderData.LoaderType) as ChunkLoader;
                    Logger.Log($"Reading chunk {newChunk.Id} using {loaderData.ChunkName} loader");
                    try
                    {
                        var chunkReader = new ByteReader(new MemoryStream(chunkData));
                        newInstance.Read(chunkReader);
                        chunkReader.Close();
                        chunkReader.Dispose();
                        //File.WriteAllBytes($"Chunks\\{loaderData.ChunkName}-{reader.Tell()-newChunk.Size}.bin",chunkData);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(
                            $"Error while reading chunk {loaderData.ChunkName}\n{ex.Message}\n{ex.StackTrace}");
                        Logger.LogError(
                            $"Chunk data. Id: {newChunk.Id}. Flag: {newChunk.Flag}. Data size: {chunkData.Length}");
                        Console.WriteLine("Press enter to continue...");
                        Console.ReadLine();
                    }

                    try
                    {
                        OnChunkLoaded?.Invoke(newChunk.Id, newInstance);
                        newChunk.Loader = newInstance;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(
                            $"Error while handling chunk loading {loaderData.ChunkName}\n{ex.Message}\n{ex.StackTrace}");
                    }
                }
                else
                {
                    var found = ChunkNames.TryGetValue(newChunk.Id, out var chunkName);
                    Logger.Log($"Loader not found for chunk {newChunk.Id} - {(found ? chunkName : string.Empty)}");
                }
            }

            _chunkIndex++;
            Items.Add(newChunk);
            if (CTFAKCore.Parameters.Contains("-trace_chunks"))
            {
                Directory.CreateDirectory("CHUNK_TRACE");
                if (!ChunkNames.TryGetValue(newChunk.Id, out var chunkName)) chunkName = $"UNKOWN-{newChunk.Id}";

                Logger.Log(
                    $"Encountered chunk: {chunkName}, chunk flag: {newChunk.Flag}, decompressed size: {chunkData.Length}");
                File.WriteAllBytes($"CHUNK_TRACE\\{chunkName}-{_chunkIndex}.bin", chunkData);
                Logger.Log($"Raw chunk data written to CHUNK_TRACE\\{chunkName}-{_chunkIndex}.bin");
            }
        }
    }


    public static readonly Dictionary<int, string> ChunkNames = new()
    {
        { 4386, "Preview" },
        { 8738, "AppMiniHeader" },
        { 8739, "AppHeader" },
        { 8740, "AppName" },
        { 8741, "AppAuthor" },
        { 8742, "AppMenu" },
        { 8743, "ExtPath" },
        { 8744, "Extensions" },
        { 8745, "FrameItems" },
        { 8746, "GlobalEvents" },
        { 8747, "FrameHandles" },
        { 8748, "ExtData" },
        { 8749, "AdditionalExtension" },
        { 8750, "AppEditorFilename" },
        { 8751, "AppTargetFilename" },
        { 8752, "AppDoc" },
        { 8753, "OtherExts" },
        { 8754, "GlobalValues" },
        { 8755, "GlobalStrings" },
        { 8756, "Extensions2" },
        { 8757, "AppIcon" },
        { 8758, "DemoVersion" },
        { 8759, "SecNum" },
        { 8760, "BinaryFiles" },
        { 8761, "AppMenuImages" },
        { 8762, "AboutText" },
        { 8763, "Copyright" },
        { 8764, "GlobalValueNames" },
        { 8765, "GlobalStringNames" },
        { 8766, "MvtExts" },
        { 8767, "FrameItems2" },
        { 8768, "ExeOnly" },
        { 8770, "Protection" },
        { 8771, "Shaders" },
        { 8773, "AppHeader2" },
        { 8792, "TTFFonts" },
        { 13107, "Frame" },
        { 13108, "FrameHeader" },
        { 13109, "FrameName" },
        { 13110, "FramePassword" },
        { 13111, "FramePalette" },
        { 13112, "FrameItemInstances" },
        { 13113, "FrameFadeInFrame" },
        { 13114, "FrameFadeOutFrame" },
        { 13115, "FrameFadeIn" },
        { 13116, "FrameFadeOut" },
        { 13117, "FrameEvents" },
        { 13118, "FramePlayHeader" },
        { 13119, "Additional_FrameItem" },
        { 13120, "Additional_FrameItemInstance" },
        { 13121, "FrameLayers" },
        { 13122, "FrameVirtualLayers" },
        { 13123, "DemoFilePath" },
        { 13124, "RandomSeed" },
        { 13125, "FrameLayerEffects" },
        { 13126, "BlurayFrameOptions" },
        { 13127, "MVTTimerBase" },
        { 13128, "MosaicImageTable" },
        { 13129, "FrameEffects" },
        { 13130, "FrameIPhoneOptions" },
        { 17476, "OIHeader" },
        { 17477, "OIName" },
        { 17478, "OIProperties" },
        { 17479, "OIUnknown" },
        { 17480, "OIEffects" },
        { 21845, "ImageOffsets" },
        { 21846, "FontOffsets" },
        { 21847, "SoundOffsets" },
        { 21848, "MusicOffsets" },
        { 26214, "Images" },
        { 26215, "Fonts" },
        { 26216, "Sounds" },
        { 26217, "Musics" },
        { 32639, "Last" }
    };
}