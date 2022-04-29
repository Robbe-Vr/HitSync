using HitSync.Core.Animation;
using HitSync.Core.Hue;
using Q42.HueApi;
using Q42.HueApi.Models.Bridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using SYWPipeNetworkManager;
using Timer = System.Timers.Timer;

namespace HitSync.Core
{
    public static class HitSyncManager
    {
        private static Timer SyncTimer = new Timer(25);

        private static bool _running = false;
        public static bool Running { get { return _running; } }

        public static double[][] FFTData { get { return Audio.AudioCapture.CapturedFFTData; } }
        public static double[] Corrections { get { return AnimationManager.Corrections; } }
        public static AnimationMode AnimationMode { get { return AnimationManager.Mode; } }

        public static event EventHandler OnInitialized;
        public static event EventHandler OnError;

        public static event EventHandler OnStarted;
        public static event EventHandler OnStopped;

        public static event EventHandler OnUpdated;

        private static int[][] DefaultIntensityColorRange = new int[][]
        {
            new int[] { 0, 152, 204 },      // background color
            new int[] { 255, 0, 0 }         // intensity color
        };

        private static int[][] DefaultColorfulColorRange = new int[][]
        {
            new int[] { 255, 0, 0 },        // bass
            new int[] { 255, 165, 0 },      // deepMid
            new int[] { 0, 140, 255 },      // standardMid
            new int[] { 0, 0, 255 },        // higherMid
            new int[] { 80, 255, 135 },     // highestMid
            new int[] { 0, 255, 0 },        // treble
            new int[] { 255, 255, 0 },      // highTreble
            new int[] { 255, 255, 255 }     // finalColor (22 000 Hz)
        };

        public static int[][] GetDefaultColorRangeForAnimationMode(AnimationMode mode)
        {
            return mode switch
            {
                AnimationMode.Intensity => DefaultIntensityColorRange,
                AnimationMode.Colorful => DefaultColorfulColorRange,
                _ => Array.Empty<int[]>(),
            };
        }

        public static void Init()
        {
            Persistence.StaticTools.Initialize();

            Task.Run(async () =>
            {
                if (!await ClientProvider.ConnectToPreferredBridge())
                {
                    IEnumerable<LocatedBridge> bridges = await ClientProvider.FindBridges();

                    LocatedBridge bridge = bridges.FirstOrDefault(x => x.IpAddress == "192.168.2.112");
                    if (bridge != null)
                        await ClientProvider.ConnectToBridge(bridge);
                    else await ClientProvider.ConnectToBridge("192.168.2.112");
                }

                Audio.AudioCapture.Init();

                EntertainmentOperator.SetVerticalStrips(30, 31, 33, 34);

                PipeMessageControl.Init("HitSync");
                PipeMessageControl.StartClient((sourceName, message) =>
                {
                    if (sourceName == "Hue")
                    {
                        return $"{message} -> " + ProcessMessage(message);
                    }
                    else return $"{message} -> NO";
                });

                OnInitialized?.Invoke(new object(), new EventArgs());
            });
        }

        public static void ResetAudioDevice()
        {
            Audio.AudioCapture.Stop();
            Audio.AudioCapture.Init();
        }

        private static string ProcessMessage(string message)
        {
            string[] data = message.Split('=');

            switch (data[0].ToUpper())
            {
                case "START":
                    if (Start(data[1]))
                    {
                        break;
                    }
                    else return "FAIL";

                case "STOP":
                    if (Stop())
                    {
                        break;
                    }
                    else return "FAIL";

                case "MODE":
                    AnimationMode mode;
                    int modeNr;

                    if (Enum.TryParse(data[1], out mode))
                    {
                        SetAnimationMode(mode);
                        break;
                    }
                    else if (int.TryParse(data[1], out modeNr))
                    {
                        SetAnimationMode((AnimationMode)modeNr);
                        break;
                    }
                    else return "FAIL";

                case "COLORS":
                    string[] colorData = data[2].Split(new string[] { "[", "],[", "]" }, StringSplitOptions.RemoveEmptyEntries);

                    List<int[]> colors = new List<int[]>();
                    foreach (string colorStr in colorData)
                    {
                        string[] values = colorStr.Split(',');

                        if (int.TryParse(values[0], out int r) && int.TryParse(values[1], out int g) && int.TryParse(values[2], out int b))
                            colors.Add(new int[3] { r, g, b });
                    }

                    ColorRangeMode rangeMode;
                    int rangeModeNr;

                    if (Enum.TryParse(data[1], out rangeMode))
                    {
                        SetColors(rangeMode, colors.ToArray());
                        break;
                    }
                    else if (int.TryParse(data[1], out rangeModeNr))
                    {
                        SetColors((ColorRangeMode)rangeModeNr, colors.ToArray());
                        break;
                    }
                    else return "FAIL";

                case "SET":
                    SetAnimationValue(data.Skip(1));
                    break;

                default:
                    return "UNKNOWN COMMAND";
            }

            return "EXECUTED";
        }

        private static void SetAnimationValue(IEnumerable<string> data)
        {
            switch (data.FirstOrDefault())
            {
                case "BRIGHTNESS_CEILING":
                    AnimationValues.BRIGHTNESS_CEILING = int.Parse(data.ElementAtOrDefault(1)) / 100.00f;
                    break;

                case "COLOR_CHANGE":
                    AnimationValues.COLOR_CHANGE_AGGRESSION = int.Parse(data.ElementAtOrDefault(1)) / 100.00f;
                    break;

                case "BASS_INTENSITY_COLOR":
                    AnimationValues.BASS_INTENSITY_COLOR = data.ElementAtOrDefault(1)?.Split(',').Select(x => int.Parse(x)).ToArray();
                    break;

                case "BASS_AGGRESSION":
                    AnimationValues.BASS_AGGRESSION = int.Parse(data.ElementAtOrDefault(1)) / 100.00f;
                    break;

                case "BASS_INTENSITY_FIELD":
                    AnimationValues.BASS_INTENSITY_FIELD = int.Parse(data.ElementAtOrDefault(1));
                    break;

                case "BASS_INTENSITY_THRESHOLD":
                    AnimationValues.BASS_INTENSITY_THRESHOLD = int.Parse(data.ElementAtOrDefault(1)) / 100.00f;
                    break;

                case "BRIGHTNESS_AGGRESSION":
                    AnimationValues.BRIGHTNESS_AGGRESSION = int.Parse(data.ElementAtOrDefault(1)) / 100.00f;
                    break;

                case "BRIGHTNESS_FIELD":
                    AnimationValues.BRIGHTNESS_FIELD = int.Parse(data.ElementAtOrDefault(1));
                    break;

                case "BRIGHTNESS_THRESHOLD":
                    AnimationValues.BRIGHTNESS_THRESHOLD = int.Parse(data.ElementAtOrDefault(1)) / 100.00f;
                    break;

                case "UPPER_INTENSITY_THRESHOLD":
                    AnimationValues.UPPER_INTENSITY_THRESHOLD = int.Parse(data.ElementAtOrDefault(1)) / 100.00f;
                    break;
            }

            OnUpdated?.Invoke(new object(), new EventArgs());
        }

        public static void SetAnimationMode(AnimationMode mode)
        {
            AnimationManager.SetAnimationMode(mode);

            OnUpdated?.Invoke(new object(), new EventArgs());
        }

        public static int[][] GetCurrentColorRange()
        {
            return AnimationMode switch
            {
                AnimationMode.Intensity => AnimationManager.GetIntensityColorRange(),
                AnimationMode.Colorful => AnimationManager.GetColorfulColorRange(),
                _ => Array.Empty<int[]>(),
            };
        }

        public static int[][] SetColors(ColorRangeMode rangeMode, params int[][] colors)
        {
            switch (rangeMode)
            {
                case ColorRangeMode.Simple:
                    return AnimationManager.CreateSimpleColorRange(colors);

                case ColorRangeMode.FrequencyDependant:
                    if (colors.Length > 6)
                        return AnimationManager.CreateFrequencyDependantColorRange(colors[0], colors[1], colors[2], colors[3], colors[4], colors[5], colors[6], colors.Length > 7 ? colors[7] : null);
                    break;
            }

            return Array.Empty<int[]>();
        }

        public static bool Start(string room)
        {
            Task<bool> isStreaming = ClientProvider.IsStreaming();
            isStreaming.Wait();
            if (isStreaming.Result)
            {
                Stop();
            }

            Task task = Task.Run(async () =>
            {
                await EntertainmentOperator.Init(room);

                if (Audio.AudioCapture.Start())
                {
                    SyncTimer.Elapsed += OnSync;

                    SyncTimer.Start();
                }
                else
                {
                    EntertainmentOperator.Stop();
                }

                _running = await ClientProvider.IsStreaming();
            });

            while (!task.IsCompleted) { Thread.Sleep(10); }

            OnStarted?.Invoke(new object(), new EventArgs());

            return task.IsCompletedSuccessfully;
        }

        private static void OnSync(object sender, ElapsedEventArgs e)
        {
            AnimationManager.CalculateAnimation(
                Audio.AudioCapture.CapturedFFTData,
                Audio.AudioCapture.MasterPeak
            );
        }

        public static bool Stop()
        {
            SyncTimer.Elapsed -= OnSync;

            SyncTimer.Stop();

            Audio.AudioCapture.Stop();

            EntertainmentOperator.Stop();

            Task.Run(async() => _running = await ClientProvider.IsStreaming());

            OnStopped?.Invoke(new object(), new EventArgs());

            return true;
        }
    }
}
