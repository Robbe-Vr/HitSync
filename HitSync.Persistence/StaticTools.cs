using HitSync.Persistence.DTOs;
using System;
using System.Collections.Generic;
using System.IO;

namespace HitSync.Persistence
{
    public static partial class StaticTools
    {
        private const string _emptyJson = "{}";

        public static void Initialize()
        {
            if (!File.Exists(FilesManager.HueClientsDataFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilesManager.HueClientsDataFile));
                File.WriteAllText(FilesManager.HueClientsDataFile, _emptyJson);
            }
        }
    }
}
