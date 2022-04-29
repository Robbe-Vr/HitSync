using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitSync.Persistence.DTOs
{
    public class HueClientData
    {
        public string BridgeIP { get; set; }
        public string BridgeId { get; set; }
        public string AppKey { get; set; }
        public string ClientKey { get; set; }
    }
}
