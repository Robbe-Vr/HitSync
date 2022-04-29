using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HitSync.Core.Hue;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.Models.Groups;
using Q42.HueApi.Streaming.Effects;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.Streaming.Models;

namespace HitSync.Core.Animation
{
    internal static class EntertainmentOperator
    {
        private static CancellationTokenSource autoUpdateCancellationTokenSource;

        private static EntertainmentLayer baseEntLayer;
        private static CancellationTokenSource baseEntLayerCancellationTokenSource;

        private static EntertainmentLayer effectLayer;
        private static CancellationTokenSource effectLayerCancellationTokenSource;

        private static string groupId;
        private static StreamingGroup stream;

        private static readonly List<byte> _verticalStripsIds = new List<byte>();

        private static List<EntertainmentLight> Mid { get; } = new List<EntertainmentLight>();
        private static List<EntertainmentLight> Left { get; } = new List<EntertainmentLight>();
        private static List<EntertainmentLight> Right { get; } = new List<EntertainmentLight>();

        private static List<List<EntertainmentLight>> VerticalStripsLeft { get; } = new List<List<EntertainmentLight>>();
        private static List<List<EntertainmentLight>> VerticalStripsMid { get; } = new List<List<EntertainmentLight>>();
        private static List<List<EntertainmentLight>> VerticalStripsRight { get; } = new List<List<EntertainmentLight>>();

        internal static async Task Init(string room)
        {
            if (ClientProvider.StreamingClient == null)
            {
                ClientProvider.RecreateStreamingClient();
            }

            bool useSimulator = false;

            IReadOnlyList<Group> all = await ClientProvider.Client.GetEntertainmentGroups();
            var group = all.FirstOrDefault(x => x.Name == room);

            if (group == null) return;

            stream = new StreamingGroup(group.Locations);

            stream.IsForSimulator = useSimulator;

            groupId = group.Id;
            await ClientProvider.StreamingClient.Connect(groupId, useSimulator);

            autoUpdateCancellationTokenSource = new CancellationTokenSource();
            ClientProvider.StreamingClient.AutoUpdate(stream, autoUpdateCancellationTokenSource.Token, 50, onlySendDirtyStates: false);

            ResetZones();

            if (stream == null) return;

            baseEntLayer = stream.GetNewLayer(isBaseLayer: true);
            effectLayer = stream.GetNewLayer();

            baseEntLayerCancellationTokenSource = new CancellationTokenSource();
            baseEntLayer.AutoCalculateEffectUpdate(baseEntLayerCancellationTokenSource.Token);

            effectLayerCancellationTokenSource = new CancellationTokenSource();
            effectLayer.AutoCalculateEffectUpdate(effectLayerCancellationTokenSource.Token);

            List<EntertainmentLight> mid = new List<EntertainmentLight>();

            mid.AddRange(baseEntLayer.GetFront().GetCenter());
            mid.AddRange(baseEntLayer.GetCenter());
            mid.AddRange(baseEntLayer.GetBack().GetCenter());

            Mid.AddRange(mid.Distinct());

            Left.AddRange(baseEntLayer.GetLeft());
            Right.AddRange(baseEntLayer.GetRight());

            List<EntertainmentLight> verticalStripsLeft = new List<EntertainmentLight>(Left.Where(x => _verticalStripsIds.Contains(x.Id)));
            foreach (EntertainmentLight verticalStrip in verticalStripsLeft) Left.RemoveAll(x => x.Id == verticalStrip.Id);

            IEnumerable<EntertainmentLight> verticalStripsMid = new List<EntertainmentLight>(Mid.Where(x => _verticalStripsIds.Contains(x.Id)));
            foreach (EntertainmentLight verticalStrip in verticalStripsMid) Mid.RemoveAll(x => x.Id == verticalStrip.Id);

            IEnumerable<EntertainmentLight> verticalStripsRight = new List<EntertainmentLight>(Right.Where(x => _verticalStripsIds.Contains(x.Id)));
            foreach (EntertainmentLight verticalStrip in verticalStripsRight) Right.RemoveAll(x => x.Id == verticalStrip.Id);

            for (int i = 6; i >= 0; i--)
            {
                VerticalStripsLeft.Add(verticalStripsLeft.ToList());
                VerticalStripsMid.Add(verticalStripsMid.ToList());
                VerticalStripsRight.Add(verticalStripsRight.ToList());
            }

            Trace.WriteLine($"Entertainment room '{room}' setup completed!");

            SetLeft(0.05, 1.0, 1.0, 1.0);
            SetMid(0.05, 1.0, 1.0, 1.0);
            SetRight(0.05, 1.0, 1.0, 1.0);
        }

        internal static void SetVerticalStrips(params byte[] ids)
        {
            foreach (byte id in ids)
            {
                if (!_verticalStripsIds.Contains(id))
                {
                    _verticalStripsIds.Add(id);
                }
            }
        }

        internal static bool RemoveVerticalStrip(byte id)
        {
            return _verticalStripsIds.Contains(id) && _verticalStripsIds.Remove(id);
        }

        private static void ResetZones()
        {
            Mid.Clear();
            Left.Clear();
            Right.Clear();

            foreach (List<EntertainmentLight> lights in VerticalStripsLeft) lights.Clear();
            VerticalStripsLeft.Clear();
            foreach (List<EntertainmentLight> lights in VerticalStripsMid) lights.Clear();
            VerticalStripsMid.Clear();
            foreach (List<EntertainmentLight> lights in VerticalStripsRight) lights.Clear();
            VerticalStripsRight.Clear();
        }

        internal static void Stop()
        {
            if (ClientProvider.StreamingClient != null)
            {
                ClientProvider.Client.SetStreamingAsync(groupId, false);
                groupId = string.Empty;

                ClientProvider.RecreateStreamingClient();
            }

            if (autoUpdateCancellationTokenSource != null && !autoUpdateCancellationTokenSource.IsCancellationRequested)
            {
                autoUpdateCancellationTokenSource.Cancel();
            }

            if (baseEntLayerCancellationTokenSource != null && !baseEntLayerCancellationTokenSource.IsCancellationRequested)
            {
                baseEntLayerCancellationTokenSource.Cancel();
            }

            if (effectLayerCancellationTokenSource != null && !effectLayerCancellationTokenSource.IsCancellationRequested)
            {
                effectLayerCancellationTokenSource.Cancel();
            }

            ResetZones();

            if (stream != null)
            {
                stream.Clear();
                stream = null;
            }

            if (baseEntLayer != null)
            {
                baseEntLayer.Clear();
                baseEntLayer = null;
            }

            if (effectLayer != null)
            {
                effectLayer.Clear();
                effectLayer = null;
            }
        }

        public static void SetLeft(double bri, double r, double g, double b, bool urgent = false)
        {
            Left.SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));
        }

        public static void SetMid(double bri, double r, double g, double b, bool urgent = false)
        {
            Mid.SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));
        }

        public static void SetRight(double bri, double r, double g, double b, bool urgent = false)
        {
            Right.SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));
        }

        public static void SetMasterPeakLeft(double bri, double r, double g, double b, double briBg, double rBg, double gBg, double bBg, int level, bool urgent = false)
        {


            int currentLvl = 6;
            bri = level >= currentLvl ? bri : briBg;
            r = level >= currentLvl ? r : rBg;
            g = level >= currentLvl ? g : gBg;
            b = level >= currentLvl ? b : bBg;
            VerticalStripsLeft[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsLeft[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsLeft[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsLeft[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsLeft[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsLeft[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsLeft[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri * AnimationValues.BRIGHTNESS_CEILING, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));
        }

        public static void SetMasterPeakMid(double bri, double r, double g, double b, double briBg, double rBg, double gBg, double bBg, int level, bool urgent = false)
        {


            int currentLvl = 6;
            bri = level >= currentLvl ? bri : briBg;
            r = level >= currentLvl ? r : rBg;
            g = level >= currentLvl ? g : gBg;
            b = level >= currentLvl ? b : bBg;
            VerticalStripsMid[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsMid[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsMid[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsMid[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsMid[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsMid[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsMid[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));
        }

        public static void SetMasterPeakRight(double bri, double r, double g, double b, double briBg, double rBg, double gBg, double bBg, int level, bool urgent = false)
        {


            int currentLvl = 6;
            bri = level >= currentLvl ? bri : briBg;
            r = level >= currentLvl ? r : rBg;
            g = level >= currentLvl ? g : gBg;
            b = level >= currentLvl ? b : bBg;
            VerticalStripsRight[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsRight[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsRight[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsRight[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsRight[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsRight[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));

            currentLvl--;
            bri = level > currentLvl ? bri : briBg;
            r = level > currentLvl ? r : rBg;
            g = level > currentLvl ? g : gBg;
            b = level > currentLvl ? b : bBg;
            VerticalStripsRight[currentLvl].SetState(CancellationToken.None, new RGBColor(r, g, b), bri, transitionTime: (urgent ? default : TimeSpan.FromMilliseconds(100)));
        }
    }
}
