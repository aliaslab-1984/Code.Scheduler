using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace Code.Scheduler.ClientUtils
{
    public interface IDelayedSOAPRequest : IDelayedJob<IDelayedSOAPRequest>
    {
        IDelayedSOAPRequest To(Uri endpoint);
        IDelayedSOAPRequest To(string endpoint);
        IDelayedSOAPRequest ToConfigEndpoint(string endpointName);
        IDelayedSOAPRequest WithBinding(Binding binding);
        IDelayedSOAPRequest Request<T>(Expression<Action<T>> call);
    }
}
