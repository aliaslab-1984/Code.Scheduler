using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Code.Scheduler.Service.Interfaces
{
    [ServiceContract]
    public interface ISchedulerService
    {
        [OperationContract]
        string DelayedChannelMessage(string channelName, bool broadcast, string message, TimeSpan delay, int retryCount, bool fireAndForget);
        [OperationContract]
        string DelayedHttpRequestMessage(string method, Uri destination, Dictionary<string,string> headers, Dictionary<string,string> parameters, TimeSpan delay, int retryCount, bool fireAndForget);
        [OperationContract]
        string DelayedSOAPRequestMessage(string hostname, int port, byte[] payload, TimeSpan delay, int retryCount, bool fireAndForget);
        
        [OperationContract]
        string PeriodicChannelMessage(string channelName, bool broadcast, string message, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget);
        [OperationContract]
        string PeriodicHttpRequestMessage(string method, Uri destination, Dictionary<string, string> headers, Dictionary<string, string> parameters, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget);
        [OperationContract]
        string PeriodicSOAPRequestMessage(string hostname, int port, byte[] payload, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget);

        [OperationContract]
        string CRONChannelMessage(string channelName, bool broadcast, string message, string CRONString, TimeSpan timeLimit, bool fireAndForget);
        [OperationContract]
        string CRONHttpRequestMessage(string method, Uri destination, Dictionary<string, string> headers, Dictionary<string, string> parameters, string CRONString, TimeSpan timeLimit, bool fireAndForget);
        [OperationContract]
        string CRONSOAPRequestMessage(string hostname, int port, byte[] payload, string CRONString, TimeSpan timeLimit, bool fireAndForget);

        [OperationContract]
        void StopJob(string jobId, bool success);
    }
}
