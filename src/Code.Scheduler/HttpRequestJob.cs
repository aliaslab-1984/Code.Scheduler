using CodeProbe.Sensing;
using Code.Configuration;
using IDSign.Logging;
using IDSign.Logging.Core;
using Code.Scheduler.Common;
using log4net;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Code.Scheduler
{
    public class HttpRequestJob : RemoteCallJobBase
    {
        public HttpRequestJob()
            :base()
        {}
        public string Method { protected get; set; }
        public string Destination { protected get; set; }
        public string HeadersJSON { protected get; set; }
        public string ParametersJSON { protected get; set; }
        protected override void RemoteCall(IJobExecutionContext context)
        {
            using (ILoggingOperation log = _logger.NormalOperation())
            using (HttpClient client = new HttpClient())
            {
                log.Wrap(() =>
                {
                    Dictionary<string, string> content = JsonConvert.DeserializeObject<Dictionary<string, string>>(ParametersJSON);
                    Dictionary<string, string> headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(HeadersJSON);

                    headers.Add(JobContextData.JobDataHeaderName, JsonConvert.SerializeObject(new JobContextData() { JobGroup = context.JobDetail.Key.Group, JobId = context.JobDetail.Key.Name }));
                    log.Debug("added jobContextData");

                    client.Timeout = TimeSpan.FromMilliseconds((int)_readTimeoutMillis + (int)_writeTimeoutMillis);

                    foreach (string hd in headers.Keys)
                    {
                        if (client.DefaultRequestHeaders.Contains(hd))
                            client.DefaultRequestHeaders.Remove(hd);
                        client.DefaultRequestHeaders.Add(hd, headers[hd]);
                    }

                    log.Debug("calling service");

                    Task<HttpResponseMessage> resp;
                    switch (Method.ToUpper())
                    {
                        case "GET":
                            resp = client.GetAsync(Destination);
                            break;
                        case "POST":
                            resp = client.PostAsync(Destination, new FormUrlEncodedContent(content));
                            break;
                        case "PUT":
                            resp = client.PutAsync(Destination, new FormUrlEncodedContent(content));
                            break;
                        default:
                            throw new ArgumentException($"Method {Method} not allowed");
                    }

                    log.Debug("request sent");

                    if (Convert.ToBoolean(FireAndForget)==false)
                    {
                        log.Debug("waiting response");
                        resp.Wait();
                        log.Debug("response received");
                        resp.Result.EnsureSuccessStatusCode();
                    }
                    else
                        log.Debug("FireAndForget set.");
                });
            }
        }
    }
}
