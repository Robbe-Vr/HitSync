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
        public static IEnumerable<HueClientData> GetHueClientsData()
        {
            return FilesManager.ConvertFromJson<List<HueClientData>>(FilesManager.ReadStringFromLocation(FilesManager.HueClientsDataFile));
        }

        public static string GetPreferredHueBridge()
        {
            return FilesManager.ReadStringFromLocation(FilesManager.HuePreferredBridgeFile);
        }

        public static string GetAudioCaptureDeviceName()
        {
            return FilesManager.ReadStringFromLocation(FilesManager.PreferredAudioCaptureDeviceNameFile);
        }
    }
}
