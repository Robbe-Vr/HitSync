using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SYWCentralLogging;

namespace SYWPipeNetworkManager
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Logger.AdditionalLogFolderLocation = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                Logger.FileOnlyLogging = false;
                Logger.SourceAppName = nameof(SYWPipeNetworkManager);

                PipeMessageControl.Init(nameof(SYWPipeNetworkManager));

                Task task = PipeMessageControl.StartClient(
                    delegate(string appName, string message)
                    {
                        Console.WriteLine("Message from: " + appName + "\n" + message + "\n");

                        return $"Recieved message from {appName}!";
                    });

                Console.WriteLine("Press Q to quit the process.");
                while (Console.ReadKey().Key != ConsoleKey.Q)
                {
                    task.Wait(250);
                }

                PipeMessageControl.StopClient();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}
