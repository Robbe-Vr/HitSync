using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYWPipeNetworkManager
{
    public static class HandleSupplier
    {
        private static string _handleFolder = "D:\\PipeNetwork\\Handles";

        /// <summary>
        /// Gets the HandlesContainer; a class containing all available handles in a Dictionary<string, string>.
        /// </summary>
        public static HandlesContainer Handles { get { return GetHandles(); } }

        private static HandlesContainer GetHandles()
        {
            HandlesContainer handles = new HandlesContainer();

            handles.Handles = GetHandlesFromFolder();

            return handles;
        }

        private static Dictionary<string, string> GetHandlesFromFolder()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (string dir in Directory.GetDirectories(_handleFolder))
            {
                string target = Path.GetFileName(dir);
                foreach (string file in Directory.GetFiles(dir))
                {
                    string source = Path.GetFileNameWithoutExtension(file);

                    string handle = File.ReadAllText(file).Trim();

                    dic.Add(target, $"{source}_{handle}");
                }
            }

            return dic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">The end of the pipeline, the reciever of the messages.</param>
        /// <param name="source">The initiator of the pipeline, the sender of the messages.</param>
        /// <param name="handle">The handle needed for the target to open the pipeline and recieve the messages send by the source process / app.</param>
        internal static void SaveHandle(string target, string source, string handle)
        {
            string targetFolder = Path.Combine(_handleFolder, target);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            string file = Path.Combine(targetFolder, $"{source}.txt");
            using (StreamWriter sw = File.CreateText(file))
            {
                sw.WriteLine(handle);
            }
        }
    }

    public class HandlesContainer
    {
        /// <summary>
        /// Contains all available handles to create anonymous pipelines.
        /// The key represents the target process / app.
        /// The value represents the source process / app with addition of the required handle for setting up an anonymous pipeline.
        /// The value string is formatted as: "{source}_{handle}". Split the value string with .Split('_') to get both the source and handle separated.
        /// </summary>
        public Dictionary<string, string> Handles { get; set; }

        /// <summary>
        /// Get the handle needed for the target process / app to create an anonymous pipeline to recieve messages from the source app / process.
        /// </summary>
        /// <param name="target">The end of the pipeline, the reciever of the messages.</param>
        /// <param name="source">The initiator of the pipeline, the sender of the messages.</param>
        /// <returns>The required handle as a string.</returns>
        public string this[string source, string target]
        {
            get
            {
                var pair = Handles.FirstOrDefault(x => x.Key == target && x.Value.StartsWith(source));
                return pair.Equals(default(KeyValuePair<string, string>)) ? null :
                    pair.Value.Split('_')[1];
            }
        }
    }
}
