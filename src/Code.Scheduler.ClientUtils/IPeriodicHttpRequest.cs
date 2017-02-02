using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public interface IPeriodicHttpRequest : IPeriodicJob<IPeriodicHttpRequest>
    {
        IPeriodicHttpRequest Method(string method = "GET");
        IPeriodicHttpRequest To(Uri endpoint);
        IPeriodicHttpRequest To(string endpoint);
        IPeriodicHttpRequest SetHeader(string name, string value);
        IPeriodicHttpRequest SetHeaders(Dictionary<string, string> headers);
        IPeriodicHttpRequest SetParameter(string name, string value);
        IPeriodicHttpRequest SetParameters(Dictionary<string, string> parameters);
    }
}
