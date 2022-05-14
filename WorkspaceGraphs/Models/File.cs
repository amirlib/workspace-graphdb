using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace WorkspaceGraphs.Models
{
    public class File
	{
        public File(FileInfo info)
        {
            Extension = info.Extension;
            Path = info.FullName;
            Size = info.Length;
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("extension")]
        public string Extension { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }


        public Dictionary<string, object> AsDictionary()
        {
            return new Dictionary<string, object>
            {
                { "id", Id },
                { "path", Path },
                { "extension",  Extension },
                { "size",  Size }
            };
        }
    }
}

