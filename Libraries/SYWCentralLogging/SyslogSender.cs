using SyslogNet.Client;
using SyslogNet.Client.Serialization;
using SyslogNet.Client.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SYWCentralLogging
{
    internal class SyslogSender
    {
        private const string syslogServerIp = "192.168.2.101";
        private const int syslogServerPort = 514;
        public SyslogProtocol Protocol { get; set; } = SyslogProtocol.UDP;

        internal void LogToSyslogServer(string appName, SyslogLogMessage message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);

            switch (Protocol)
            {
                case SyslogProtocol.SecureTCP:
                    SyslogEncryptedTcpSender _syslogSecureTcpSender = new SyslogEncryptedTcpSender(syslogServerIp, syslogServerPort);
                    _syslogSecureTcpSender.Send(
                        new SyslogMessage(
                            DateTime.Now,
                            Facility.SecurityOrAuthorizationMessages1,
                            Severity.Informational,
                            Environment.MachineName,
                            appName,
                            jsonMessage),
                        new SyslogRfc3164MessageSerializer());
                    break;

                case SyslogProtocol.TCP:
                    SyslogEncryptedTcpSender _syslogTcpSender = new SyslogEncryptedTcpSender(syslogServerIp, syslogServerPort);
                    _syslogTcpSender.Send(
                        new SyslogMessage(
                            DateTime.Now,
                            Facility.SecurityOrAuthorizationMessages1,
                            Severity.Informational,
                            Environment.MachineName,
                            appName,
                            jsonMessage),
                        new SyslogRfc3164MessageSerializer());
                    break;

                case SyslogProtocol.UDP:
                default:
                    SyslogUdpSender _syslogSender = new SyslogUdpSender(syslogServerIp, syslogServerPort);
                    _syslogSender.Send(
                        new SyslogMessage(
                            DateTime.Now,
                            Facility.SecurityOrAuthorizationMessages1,
                            Severity.Informational,
                            Environment.MachineName,
                            appName,
                            jsonMessage),
                        new SyslogRfc3164MessageSerializer());
                    break;
            }
        }
    }

    public class SyslogLogMessage
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string ApplicationName { get; set; }
        public SeverityLevel Severity { get; set; }
    }

    public enum SeverityLevel
    {
        Info,
        Low,
        Mediocre,
        High,
        Extreme,
    }

    public enum SyslogProtocol
    {
        SecureTCP,
        TCP,
        UDP,
    }
}
