using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Code.Scheduler.ClientUtils;
using Code.Scheduler.ClientUtils.HandlerUtils;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace TestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "UnitTestService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select UnitTestService.svc or UnitTestService.svc.cs at the Solution Explorer and start debugging.
    public class UnitTestService : SoapWcfHandlerBase, IUnitTestService
    {
        public UnitTestService()
            :base()
        {

        }

        public void BlockFor(TimeSpan blockTime)
        {
            Thread.Sleep((int)blockTime.TotalMilliseconds);
        }

        public void FailAt(DateTime datetime)
        {
            if (DateTime.Now > datetime)
                EndWithFailure();
        }

        public void SendDateTimeToUrl(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                Task<HttpResponseMessage> resp;

                resp = client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>() { { "dateTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") } }));

                resp.Result.EnsureSuccessStatusCode();
            }
        }

        public void SuccedAt(DateTime datetime)
        {
            if (DateTime.Now > datetime)
                EndWithSuccess();
        }
    }
}
