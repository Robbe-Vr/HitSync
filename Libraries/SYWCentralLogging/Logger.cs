using System;
using System.Diagnostics;

namespace SYWCentralLogging
{
    public static class Logger
    {
        /// <summary>
        /// Set to true if you want to log to files only, is false by default. Will log to console using Trace.WriteLine and Console.WriteLine if false.
        /// </summary>
        public static bool FileOnlyLogging { get { return LoggingInfo.FileOnlyLogging; } set { LoggingInfo.FileOnlyLogging = value; } }

        /// <summary>
        /// If you want the Logger to save logfiles to another folder besides the central loggin location aswell, provide a valid folder location here.
        /// </summary>
        public static string AdditionalLogFolderLocation { get { return LoggingInfo.AdditionalLogFolderLocation; } set { LoggingInfo.AdditionalLogFolderLocation = value; } }

        private static readonly SyslogSender _fileLogger = new SyslogSender();

        public static string SourceAppName { get; set; }

        public static void LogObj(object obj, string includedMessage = null, string severity = "Info", string logUnderOtherName = null)
        {
            Log(String.IsNullOrEmpty(includedMessage) ?
                System.Text.Json.JsonSerializer.Serialize(obj) :
                $"{includedMessage}.\r\nIncluded Object:\r\n{System.Text.Json.JsonSerializer.Serialize(obj)}\r\n",
                logUnderOtherName: logUnderOtherName);
        }

        public static void LogNewLine(string logUnderOtherName = null)
        {
            if (String.IsNullOrEmpty(logUnderOtherName ?? SourceAppName))
            {
                throw new MissingFieldException("Please provide a source application name for the Logger to separate the application logs on the central logging location.\nEnter this name in the public string property named: 'SourceAppName'.");
            }

            string logMsg = "\r\n";

            Log(logMsg, logUnderOtherName: logUnderOtherName);
        }

        public static void Log(string message, SeverityLevel severity = SeverityLevel.Info, string logUnderOtherName = null)
        {
            if (String.IsNullOrEmpty(logUnderOtherName ?? SourceAppName))
            {
                throw new MissingFieldException("Please provide a source application name for the Logger to separate the application logs on the central logging location.\nEnter this name in the public string property named: 'SourceAppName'.");
            }

            SyslogLogMessage msg = new SyslogLogMessage()
            {
                Timestamp = DateTime.Now,
                ApplicationName = logUnderOtherName ?? SourceAppName,
                Message = message,
                Severity = severity,
            };

            _fileLogger.LogToSyslogServer(logUnderOtherName ?? SourceAppName, msg);

            if (!FileOnlyLogging)
            {
                Trace.WriteLine(message);
                Console.WriteLine(message);
            }
        }
    }
}
