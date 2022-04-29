using HitSync.Persistence.DTOs;
using HitSync.Persistence;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Q42.HueApi.Streaming;

namespace HitSync.Core.Hue
{
    internal class ClientProvider
    {
        internal static ILocalHueClient Client { get { return StreamingClient?.LocalHueClient; } }

        internal static StreamingHueClient StreamingClient { get; private set; }

        private static string bridgeIp;
        private static string appKey;
        private static string clientKey;

        internal static async Task<bool> IsStreaming()
        {
            Bridge bridge = await Client.GetBridgeAsync();

            return bridge.IsStreamingActive;
        }

        internal static void RecreateStreamingClient()
        {
            if (StreamingClient != null)
            {
                try
                {
                    StreamingClient.Close();
                    StreamingClient.Dispose();
                    StreamingClient = null;
                }
                catch
                {
                    StreamingClient = null;
                }
            }

            StreamingClient = new StreamingHueClient(bridgeIp, appKey, clientKey);
        }

        internal static async Task<IEnumerable<LocatedBridge>> FindBridges(string id = null)
        {
            List<LocatedBridge> bridges = new List<LocatedBridge>();

            bridges.AddRange(await new HttpBridgeLocator().LocateBridgesAsync(TimeSpan.FromSeconds(5)));

            if (bridges.Any(x => x.BridgeId == id)) return bridges;

            bridges.AddRange(
                (await new LocalNetworkScanBridgeLocator().LocateBridgesAsync(TimeSpan.FromSeconds(5)))
                    .Where(x => !bridges.Any(e => e.IpAddress == x.IpAddress))
            );

            if (bridges.Any(x => x.BridgeId == id)) return bridges;

            bridges.AddRange(
                (await new MdnsBridgeLocator().LocateBridgesAsync(TimeSpan.FromSeconds(5)))
                    .Where(x => !bridges.Any(e => e.IpAddress == x.IpAddress))
            );

            return bridges.Distinct();
        }

        internal static async Task<bool> ConnectToBridge(LocatedBridge bridge)
        {
            try
            {
                List<HueClientData> knownClients = StaticTools.GetHueClientsData().ToList();

                HueClientData rememberedData = knownClients.FirstOrDefault(x => x.BridgeId == bridge.BridgeId);
                if (rememberedData != null && rememberedData.AppKey != null && rememberedData.ClientKey != null)
                {
                    StreamingClient = new StreamingHueClient(bridge.IpAddress, rememberedData.AppKey, rememberedData.ClientKey);
                }
                else
                {
                    LocalHueClient client = new LocalHueClient(bridge.IpAddress);

                    RegisterEntertainmentResult registerEntertainment = await client.RegisterAsync("HitSync", Environment.MachineName, generateClientKey: true);

                    rememberedData = new HueClientData()
                    {
                        BridgeIP = bridge.IpAddress,
                        BridgeId = bridge.BridgeId.ToUpper(),
                        AppKey = registerEntertainment.Username,
                        ClientKey = registerEntertainment.StreamingClientKey,
                    };

                    knownClients.Add(rememberedData);

                    StaticTools.Update(knownClients);

                    StreamingClient = new StreamingHueClient(bridge.IpAddress, rememberedData.AppKey, rememberedData.ClientKey);
                }

                bridgeIp = bridge.IpAddress;
                appKey = rememberedData.AppKey;
                clientKey = rememberedData.ClientKey;

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error connecting to bridge: {bridge.IpAddress}! Error: {e.Message}");

                return false;
            }
        }

        internal static async Task<bool> ConnectToBridge(string ip, bool reconnect = true)
        {
            try
            {
                HueClientData newClient = new HueClientData();
                LocalHueClient client = new LocalHueClient(ip);

                HueClientData rememberedData = null;
                List<HueClientData> knownClients = new List<HueClientData>();

                if (reconnect)
                {
                    knownClients = StaticTools.GetHueClientsData().ToList();
                    rememberedData = knownClients.FirstOrDefault(x => x.BridgeIP == ip);
                }
                
                if (rememberedData != null && rememberedData.AppKey != null && rememberedData.ClientKey != null)
                {
                    StreamingClient = new StreamingHueClient(ip, rememberedData.AppKey, rememberedData.ClientKey);
                }
                else
                {
                    RegisterEntertainmentResult registerEntertainment = await client.RegisterAsync("HitSync", Environment.MachineName, generateClientKey: true);

                    newClient.AppKey = registerEntertainment.Username;
                    newClient.ClientKey = registerEntertainment.StreamingClientKey;

                    StreamingClient = new StreamingHueClient(ip, newClient.AppKey, newClient.ClientKey);

                    Bridge bridge = await Client.GetBridgeAsync();

                    newClient.BridgeId = bridge.Config.BridgeId.ToUpper();
                    newClient.BridgeIP = ip;

                    StaticTools.Update(new List<HueClientData>() { newClient });
                }

                bridgeIp = ip;
                appKey = newClient.AppKey;
                clientKey = newClient.ClientKey;

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error connecting to bridge: {ip}! Error: {e.Message}");

                return false;
            }
        }

        internal static async Task<bool> ConnectToPreferredBridge()
        {
            try
            {
                string[] data = StaticTools.GetPreferredHueBridge().Split("::");

                string preferredBridge = data[0];
                string preferredBridgeAppKey = data[1];
                string preferredBridgeClientKey = data[2];

                LocatedBridge bridge;
                if (!String.IsNullOrWhiteSpace(preferredBridge) && (bridge = (await FindBridges(preferredBridge)).FirstOrDefault(x => x.BridgeId.ToUpper() == preferredBridge)) != null)
                {
                    StreamingClient = new StreamingHueClient(bridge.IpAddress, preferredBridgeAppKey, preferredBridgeClientKey);

                    bridgeIp = bridge.IpAddress;
                    appKey = preferredBridgeAppKey;
                    clientKey = preferredBridgeClientKey;

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error connecting to preferred bridge! Error: {e.Message}");

                return false;
            }
}
    }
}
