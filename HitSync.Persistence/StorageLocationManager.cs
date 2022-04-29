using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitSync.Persistence
{
    internal static partial class FilesManager
    {
        private static string _storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HitSync");

        internal static string HueClientsDataFile { get; } = Path.Combine(_storageFolder, "Hue", "hue_clients_data.json");
        internal static string HuePreferredBridgeFile { get; } = Path.Combine(_storageFolder, "Hue", "hue_preferred_bridge.txt");
        internal static string PreferredAudioCaptureDeviceNameFile { get; } = Path.Combine(_storageFolder, "Audio", "audio_capture_device.txt");
    }
}
