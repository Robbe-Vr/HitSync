using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SYWCentralLogging;

namespace SYWPipeNetworkManager
{
    public static class PipeMessageControl
    {
        private static string currentAppName;

        private static CancellationTokenSource tokenSource;

        public static void Init(string currentAppName)
        {
            PipeMessageControl.currentAppName = currentAppName;
        }

        /// <summary>
        /// Sends a message to another process / app over an anonymous pipeline.
        /// </summary>
        /// <param name="targetName">The process / app you want to send your message to, the target.</param>
        /// <param name="msg">The message you want to send to the other end of the anonymous pipeline, the target process / app</param>
        /// <returns>The response from the client of the pipeline as a list of strings.</returns>
        public static List<string> SendToApp(string targetName, string msg)
        {
            List<string> msgs = new List<string>();

            try
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream($"{targetName}pipe", PipeDirection.InOut))
                {
                    pipeServer.WaitForConnection();

                    if (pipeServer.IsConnected)
                    {
                        using (StreamWriter sw = new StreamWriter(pipeServer))
                        using (StreamReader sr = new StreamReader(pipeServer))
                        {
                            sw.AutoFlush = true;

                            PipeDataPacket packet = new PipeDataPacket()
                            {
                                SourceName = currentAppName,
                                Data = msg,
                            };

                            sw.WriteLine(System.Text.Json.JsonSerializer.Serialize(packet));

                            pipeServer.WaitForPipeDrain();

                            Task.Delay(100);

                            while (sr.Peek() > 0)
                            {
                                msgs.Add(sr.ReadLine());
                            }

                            if (pipeServer.IsConnected)
                                pipeServer.Disconnect();
                        }
                    }
                }
            }
            catch (ObjectDisposedException e) { }
            catch (IOException e)
            {
                if (e.Message == "All pipe instances are busy.")
                {
                    Task.Delay(1000);
                    return SendToApp(targetName, msg);
                }
                else
                {
                    Logger.Log($"Error sending message to app: '{targetName}'.\nError: {e.Message}\nStackTrace:\n{e.StackTrace}", SeverityLevel.Mediocre, nameof(SYWPipeNetworkManager));
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Error sending message to app: '{targetName}'.\nError: {e.Message}\nStackTrace:\n{e.StackTrace}", SeverityLevel.Mediocre, nameof(SYWPipeNetworkManager));
            }

            return msgs;
        }

        public static Task StartClient(Func<string, string, string> messageResponseCallback)
        {
            tokenSource = new CancellationTokenSource();
            return Task.Run(delegate()
            {
                AwaitMessages(messageResponseCallback, client: true);
            }, tokenSource.Token);
        }

        public static void StopClient()
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = null;
            }
        }

        /// <summary>
        /// Will wait for a server pipeline for the source app to connect and process the send messages using the given callback function.
        /// </summary>
        /// <param name="sourceName">The source process / app, the server of the pipeline, which will send messages for the current process / app to recieve.</param>
        /// <param name="messageResponseCallback">A callback to process the messages send from the server pipeline. first string contains the source app name, the second string array contains the messages send from the server pipeline.</param>
        public static void AwaitMessages(Func<string, string, string> messageResponseCallback, bool client = false)
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", $"{currentAppName}pipe", PipeDirection.InOut))
                {
                    pipeClient.Connect();

                    if (pipeClient.IsConnected)
                    {
                        using (StreamReader sr = new StreamReader(pipeClient))
                        using (StreamWriter sw = new StreamWriter(pipeClient))
                        {
                            sw.AutoFlush = true;

                            while (sr.Peek() > 0)
                            {
                                string msg = sr.ReadLine();

                                PipeDataPacket recPacket;
                                try
                                {
                                    recPacket = System.Text.Json.JsonSerializer.Deserialize<PipeDataPacket>(msg);
                                }
                                catch (Exception e)
                                {
                                    Logger.Log($"Unable to parse message for: '{currentAppName}' to json object.\nMessage: {msg}\nError: {e.Message}\nStackTrace:\n{e.StackTrace}", SeverityLevel.High, nameof(SYWPipeNetworkManager));
                                    continue;
                                }

                                string response = ProcessMessages(messageResponseCallback, recPacket.Data, recPacket.SourceName);

                                PipeDataPacket packet = new PipeDataPacket()
                                {
                                    SourceName = currentAppName,
                                    Data = response,
                                };

                                sw.WriteLine(System.Text.Json.JsonSerializer.Serialize(packet));

                                Task.Delay(100);
                            }

                            if (pipeClient.IsConnected)
                                pipeClient.Close();
                        }
                    }
                }
            }
            catch (ObjectDisposedException e) { }
            catch (Exception e)
            {
                Logger.Log($"Error retrieving messages for app: '{currentAppName}'.\nError: {e.Message}\nStackTrace:\n{e.StackTrace}", SeverityLevel.Mediocre, nameof(SYWPipeNetworkManager));
            }

            if (client)
                AwaitMessages(messageResponseCallback, client);
        }

        private static string ProcessMessages(Func<string, string, string> messageResponseCallback, string message, string source)
        {
            return messageResponseCallback != null ? messageResponseCallback(source, message) : "NO RESPONSE";
        }

        private class PipeDataPacket
        { 
            public string SourceName { get; set; }
            public string Data { get; set; }
        }
    }
}
