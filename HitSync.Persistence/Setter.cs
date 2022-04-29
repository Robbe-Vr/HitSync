using HitSync.Persistence.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitSync.Persistence
{
    public static partial class StaticTools
    {
        public static void Update(IEnumerable<HueClientData> hueClientsData)
        {
            FilesManager.WriteToLocation(FilesManager.ConvertToJson(hueClientsData), FilesManager.HueClientsDataFile);
        }

        public static void UpdatePreferredBridge(string hueBridgeId)
        {
            FilesManager.WriteToLocation(hueBridgeId, FilesManager.HuePreferredBridgeFile);
        }

        public static void UpdatePreferredAudioCaptureDevice(string audioDeviceName)
        {
            FilesManager.WriteToLocation(audioDeviceName, FilesManager.PreferredAudioCaptureDeviceNameFile);
        }
    }
}
