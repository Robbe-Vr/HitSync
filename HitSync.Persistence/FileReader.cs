using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HitSync.Persistence
{
    internal static partial class FilesManager
    {
        internal static byte[] ReadBytesFromLocation(string path)
        {
            return File.ReadAllBytes(path);
        }

        internal static string ReadStringFromLocation(string path)
        {
            return File.ReadAllText(path);
        }

        internal static T ConvertFromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, jsonOptions);
        }
    }
}
