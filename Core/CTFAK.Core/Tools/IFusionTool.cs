using CTFAK.FileReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.Tools
{
    public interface IFusionTool
    {
        string Name { get; }
        public int[] Progress { get; }
        void Execute(IFileReader reader);
    }
}
