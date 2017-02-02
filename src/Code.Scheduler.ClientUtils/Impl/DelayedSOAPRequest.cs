using Code.Scheduler.Service.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Code.Scheduler.ClientUtils.Impl
{
    public class DelayedSOAPRequest : IDelayedSOAPRequest
    {
        protected TimeSpan _delay = TimeSpan.FromSeconds(0);
        protected int _retryCount = 0;
        protected bool _fireAndForget = false;
        protected Uri _to;
        protected Binding _binding;
        protected string _soapMessage;

        protected bool _requestDone = false;
        protected bool _dirty = false;

        public IDelayedSOAPRequest After(TimeSpan delay)
        {
            if (delay < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The delay must be 0 or positive.");
            _delay = delay;

            return this;
        }

        public IDelayedSOAPRequest WithRetry(int count = 0)
        {
            if (count < 0)
                throw new ArgumentException("The retryCount must be 0 or positive.");
            _retryCount = count;

            return this;
        }

        public IDelayedSOAPRequest AsFireAndForget(bool fireAndForget = true)
        {
            _fireAndForget = fireAndForget;

            return this;
        }
        public IDelayedSOAPRequest To(Uri endpoint)
        {
            _to = endpoint;

            return this;
        }
        public IDelayedSOAPRequest To(string endpoint)
        {
            return To(new Uri(endpoint));
        }
        public IDelayedSOAPRequest ToConfigEndpoint(string endpointName)
        {
            Tuple<Uri, Binding>  result = SOAPRequestHelper.GetConfigurationBinding(endpointName);

            _to = result.Item1;
            _binding = result.Item2;

            if (_requestDone)
                _dirty = true;

            return this;
        }
        public IDelayedSOAPRequest WithBinding(Binding binding)
        {
            _binding = binding;

            if (_requestDone)
                _dirty = true;

            return this;
        }

        public IDelayedSOAPRequest Request<T>(Expression<Action<T>> call)
        {
            _soapMessage = SOAPRequestHelper.GetSOAPMessage<T>(call, _binding);

            if (string.IsNullOrEmpty(_soapMessage))
                throw new Exception("Unable to get soap envelope");

            _requestDone = true;

            if (_dirty)
                _dirty = false;

            return this;
        }

        public string Schedule()
        {
            if (_dirty)
                throw new InvalidOperationException("The data that produced the request has changed. Produce another request.");
            if (_to == null)
                throw new ArgumentException("The To must be set");

            using (ChannelFactory<ISchedulerService> ch = new ChannelFactory<ISchedulerService>(ClientUtilManager.Ask().EndpointConfigurationName))
            {
                ISchedulerService svc = ch.CreateChannel();

                return svc.DelayedSOAPRequestMessage(_to.Host, _to.Port, Encoding.UTF8.GetBytes(SOAPRequestHelper.GetMangeldRequest(_soapMessage,_to)), _delay, _retryCount, _fireAndForget);
            }
        }
    }
}
