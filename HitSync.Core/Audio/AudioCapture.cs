using HitSync.Persistence;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Linq;

namespace HitSync.Core.Audio
{
    internal static class AudioCapture
    {
        private static WasapiLoopbackCapture capture;

        private static readonly int bufferSize = (int)(Math.Pow(2, 15) / 4);

        internal static double[][] CapturedFFTData { get; private set; }
        internal static double MasterPeak { get; private set; }

        internal static void Init()
        {
            string deviceName = StaticTools.GetAudioCaptureDeviceName();

            var enumerator = new MMDeviceEnumerator();

            MMDevice device = String.IsNullOrWhiteSpace(deviceName) ? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia) : enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).FirstOrDefault(d => d.DeviceFriendlyName == deviceName);
            if (device == null)
            {
                Trace.WriteLine($"Could not find an output device by the name '{deviceName}'!\nSwitching to default device!");
                device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }

            capture = new WasapiLoopbackCapture(device);

            capture.DataAvailable += OnDataAvailable;
        }

        internal static bool Start()
        {
            if (capture == null) return false;

            capture.StartRecording();

            return true;
        }

        internal static void Stop()
        {
            capture?.StopRecording();
        }

        private static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (capture.CaptureState == CaptureState.Capturing)
            {
                float[] buffer = new float[bufferSize];

                float peak = 0.0f;
                for (int i = 0; i < bufferSize; i++)
                {
                    buffer[i] = BitConverter.ToSingle(e.Buffer, i * 4);

                    if (buffer[i] > peak)
                        peak = buffer[i];
                }

                CapturedFFTData = peak > 0.001f ? FFT.FFTHandler.PerformFFT(buffer) : new double[2][] { new double[bufferSize / 4], new double[bufferSize / 4] };
                MasterPeak = buffer.Max();
            }
        }
    }
}
