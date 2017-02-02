using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using Code.Configuration;
using IDSign.Logging.Core;
using IDSign.Logging;
using Code.Scheduler.Common;
using Newtonsoft.Json;

namespace Code.Scheduler
{
    public class SOAPRequestJob : RemoteCallJobBase
    {
        public SOAPRequestJob()
            :base()
        { }
        public string Hostname { protected get; set; }
        public string Port { protected get; set; }
        public string PayloadBase64 { protected get; set; }
        protected override void RemoteCall(IJobExecutionContext context)
        {
            using (ILoggingOperation log = _logger.NormalOperation())
            using (TcpClient client = new TcpClient(Hostname, Convert.ToInt32(Port)))
            using (NetworkStream stream = client.GetStream())
            {
                log.Wrap(() =>
                {
                    stream.ReadTimeout = _readTimeoutMillis;
                    stream.WriteTimeout = _writeTimeoutMillis;

                    byte[] tmp = Convert.FromBase64String(PayloadBase64);

                    StringBuilder result = new StringBuilder(tmp.Length + 200);
                    using (MemoryStream ms = new MemoryStream(tmp))
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        string line = null;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (Regex.IsMatch(line, "Content-Length", RegexOptions.IgnoreCase))
                            {
                                result.AppendLine($"{JobContextData.JobDataHeaderName}:{JsonConvert.SerializeObject(new JobContextData() { JobGroup = context.JobDetail.Key.Group, JobId = context.JobDetail.Key.Name })}");
                                log.Debug("added jobContextData");
                            }
                            result.AppendLine(line);
                        }
                        tmp = sr.CurrentEncoding.GetBytes(result.ToString());
                    }

                    log.Debug("calling service");
                    stream.Write(tmp, 0, tmp.Length);
                    log.Debug("request sent");

                    if (Convert.ToBoolean(FireAndForget) == false)
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            log.Debug("waiting response");

                            StringBuilder b = new StringBuilder();
                            string x = null;
                            int cntLength = 0;
                            while (x != "")
                            {
                                x = sr.ReadLine();
                                Match m = Regex.Match(x, "Content-Length:\\s*(\\d+)");
                                if (m.Success)
                                    cntLength = Convert.ToInt32(m.Groups[1].ToString());
                                b.AppendLine(x);
                            }

                            byte[] msg = new byte[cntLength];
                            for (int i = 0; i < cntLength; i++)
                            {
                                msg[i] = (byte)sr.Read();
                            }
                            string res = b.ToString();

                            log.Debug("response received");

                            if (!Regex.IsMatch(res.Split('\n')[0], "20\\d"))
                                throw new Exception($"Error invoking service. {res}");
                        }
                    }
                    else
                    {
                        log.Debug("FireAndForget set.");
                    }
                });
            }
        }
    }
}
