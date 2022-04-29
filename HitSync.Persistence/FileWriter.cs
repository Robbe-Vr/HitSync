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
        internal static void WriteToLocation(byte[] content, string path)
        {
            File.WriteAllBytes(path, content);
        }

        internal static void WriteToLocation(string content, string path)
        {
            File.WriteAllText(path, content);
        }

        private static JsonSerializerOptions jsonOptions;

        internal static string ConvertToJson(object obj, Type type)
        {
            if (jsonOptions == null)
            {
                jsonOptions = new JsonSerializerOptions();

                jsonOptions.WriteIndented = true;
            }

            return JsonSerializer.Serialize(obj, type, jsonOptions);
        }

        internal static string ReadStringFromLocation(object preferredAudioCaptureDeviceNameFile)
        {
            throw new NotImplementedException();
        }

        internal static string ConvertToJson<T>(T obj)
        {
            if (jsonOptions == null)
            {
                jsonOptions = new JsonSerializerOptions();

                jsonOptions.WriteIndented = true;
            }

            return JsonSerializer.Serialize(obj, jsonOptions);
        }
    }
}
