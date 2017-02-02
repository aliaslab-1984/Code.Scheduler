using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public interface IPeriodicSOAPRequest : IPeriodicJob<IPeriodicSOAPRequest>
    {
        IPeriodicSOAPRequest To(Uri endpoint);
        IPeriodicSOAPRequest To(string endpoint);
        IPeriodicSOAPRequest ToConfigEndpoint(string endpointName);
        IPeriodicSOAPRequest WithBinding(Binding binding);
        IPeriodicSOAPRequest Request<T>(Expression<Action<T>> call);
    }
}
