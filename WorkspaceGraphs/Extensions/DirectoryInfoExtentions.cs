using System.IO;
using System.Linq;

namespace WorkspaceGraphs.Extensions
{
    public static class DirectoryInfoExtentions
	{
        public static int CountFiles(this DirectoryInfo info)
        {
            return info
                .EnumerateFiles()
                .Count();
        }
    }
}

