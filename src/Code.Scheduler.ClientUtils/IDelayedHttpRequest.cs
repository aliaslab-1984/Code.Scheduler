using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public interface IDelayedHttpRequest : IDelayedJob<IDelayedHttpRequest>
    {
        IDelayedHttpRequest Method(string method = "GET");
        IDelayedHttpRequest To(Uri endpoint);
        IDelayedHttpRequest To(string endpoint);
        IDelayedHttpRequest SetHeader(string name, string value);
        IDelayedHttpRequest SetHeaders(Dictionary<string,string> headers);
        IDelayedHttpRequest SetParameter(string name, string value);
        IDelayedHttpRequest SetParameters(Dictionary<string, string> parameters);
    }
}
