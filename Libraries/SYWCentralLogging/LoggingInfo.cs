using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYWCentralLogging
{
    internal static class LoggingInfo
    {
        public static bool InDockerContainer { get; set; } = false;
        public static bool FileOnlyLogging { get; set; } = false;

        private static string _folderLocation;
        public static string AdditionalLogFolderLocation { get { return _folderLocation; } set { if (Directory.Exists(value)) _folderLocation = value; else Directory.CreateDirectory(value); } }
        
    }
}
