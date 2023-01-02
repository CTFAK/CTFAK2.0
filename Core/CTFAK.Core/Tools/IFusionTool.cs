using CTFAK.FileReaders;

namespace CTFAK.Tools;

public interface IFusionTool
{
    string Name { get; }
    public int[] Progress { get; }
    void Execute(IFileReader reader);
}