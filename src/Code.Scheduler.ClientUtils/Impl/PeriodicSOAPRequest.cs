using Code.Scheduler.Service.Interfaces;
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
    public class PeriodicSOAPRequest : IPeriodicSOAPRequest
    {
        protected TimeSpan _delay = TimeSpan.FromSeconds(0);
        protected TimeSpan _period = TimeSpan.FromSeconds(0);
        protected TimeSpan _timeLimit = TimeSpan.MaxValue;
        protected bool _fireAndForget = false;
        protected Uri _to;
        protected Binding _binding;
        protected string _soapMessage;

        protected string _CRONString = null;
        protected bool _timeSpec = false;

        protected bool _requestDone = false;
        protected bool _dirty = false;

        public IPeriodicSOAPRequest WithCRONExpression(string CRONString)
        {
            if (_timeSpec)
                throw new InvalidOperationException("The timespec has already been set. Only one schedule specification is allowed");

            if (string.IsNullOrEmpty(CRONString))
                throw new ArgumentException("CRONString must be not null  and non empty");

            _CRONString = CRONString;

            return this;
        }

        public IPeriodicSOAPRequest After(TimeSpan delay)
        {
            if (!string.IsNullOrEmpty(_CRONString))
                throw new InvalidOperationException("The CRONString has already been set. Only one schedule specification is allowed");

            if (delay < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The delay must be 0 or positive.");
            _delay = delay;

            _timeSpec = true;

            return this;
        }

        public IPeriodicSOAPRequest Every(TimeSpan period)
        {
            if (!string.IsNullOrEmpty(_CRONString))
                throw new InvalidOperationException("The CRONString has already been set. Only one schedule specification is allowed");

            if (period < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The period must be 0 or positive.");
            _period = period;

            _timeSpec = true;

            return this;
        }

        public IPeriodicSOAPRequest WithAllottedTime(TimeSpan timeLimit)
        {
            if (timeLimit < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The timeLimit must be 0 or positive.");

            _timeLimit = timeLimit;

            return this;
        }

        public IPeriodicSOAPRequest AsFireAndForget(bool fireAndForget = true)
        {
            _fireAndForget = fireAndForget;

            return this;
        }
        public IPeriodicSOAPRequest To(Uri endpoint)
        {
            _to = endpoint;

            return this;
        }
        public IPeriodicSOAPRequest To(string endpoint)
        {
            return To(new Uri(endpoint));
        }
        public IPeriodicSOAPRequest ToConfigEndpoint(string endpointName)
        {
            Tuple<Uri, Binding> result = SOAPRequestHelper.GetConfigurationBinding(endpointName);

            _to = result.Item1;
            _binding = result.Item2;

            if (_requestDone)
                _dirty = true;

            return this;
        }
        public IPeriodicSOAPRequest WithBinding(Binding binding)
        {
            _binding = binding;

            if (_requestDone)
                _dirty = true;

            return this;
        }

        public IPeriodicSOAPRequest Request<T>(Expression<Action<T>> call)
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

                if (!string.IsNullOrEmpty(_CRONString))
                    return svc.CRONSOAPRequestMessage(_to.Host, _to.Port, Encoding.UTF8.GetBytes(SOAPRequestHelper.GetMangeldRequest(_soapMessage, _to)), _CRONString, _timeLimit, _fireAndForget);
                else
                    return svc.PeriodicSOAPRequestMessage(_to.Host, _to.Port, Encoding.UTF8.GetBytes(SOAPRequestHelper.GetMangeldRequest(_soapMessage, _to)), _delay, _period, _timeLimit, _fireAndForget);
            }
        }
    }
}
