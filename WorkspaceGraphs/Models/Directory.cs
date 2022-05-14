using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using WorkspaceGraphs.Extensions;

namespace WorkspaceGraphs.Models
{
    public class Directory
	{
        public Directory(int count, string path)
        {
            FilesCount = count;
            Path = path;
        }

        public Directory(DirectoryInfo info)
        {
            FilesCount = info.CountFiles();
            Info = info;
            Path = info.FullName;
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("filesCount")]
        public int FilesCount { get; set; }

        public DirectoryInfo Info { get; }

        public Dictionary<string, object> AsDictionary()
        {
            return new Dictionary<string, object>
            {
                { "id", Id },
                { "path", Path },
                { "filesCount",  FilesCount }
            };
        }
    }
}

