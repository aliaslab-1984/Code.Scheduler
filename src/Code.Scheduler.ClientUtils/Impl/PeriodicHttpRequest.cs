using Code.Scheduler.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils.Impl
{
    public class PeriodicHttpRequest : IPeriodicHttpRequest
    {
        protected TimeSpan _delay = TimeSpan.FromSeconds(0);
        protected TimeSpan _period = TimeSpan.FromSeconds(0);
        protected TimeSpan _timeLimit = TimeSpan.MaxValue;
        protected bool _fireAndForget = false;

        protected string _CRONString = null;
        protected bool _timeSpec = false;

        protected string _method = "GET";
        protected Dictionary<string, string> _headers = new Dictionary<string, string>();
        protected Dictionary<string, string> _parameters = new Dictionary<string, string>();

        protected Uri _to;

        public IPeriodicHttpRequest WithCRONExpression(string CRONString)
        {
            if (_timeSpec)
                throw new InvalidOperationException("The timespec has already been set. Only one schedule specification is allowed");

            if (string.IsNullOrEmpty(CRONString))
                throw new ArgumentException("CRONString must be not null  and non empty");

            _CRONString = CRONString;

            return this;
        }

        public IPeriodicHttpRequest After(TimeSpan delay)
        {
            if (!string.IsNullOrEmpty(_CRONString))
                throw new InvalidOperationException("The CRONString has already been set. Only one schedule specification is allowed");

            if (delay < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The delay must be 0 or positive.");
            _delay = delay;

            _timeSpec = true;

            return this;
        }

        public IPeriodicHttpRequest Every(TimeSpan period)
        {
            if (!string.IsNullOrEmpty(_CRONString))
                throw new InvalidOperationException("The CRONString has already been set. Only one schedule specification is allowed");

            if (period < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The period must be 0 or positive.");
            _period = period;

            _timeSpec = true;

            return this;
        }

        public IPeriodicHttpRequest WithAllottedTime(TimeSpan timeLimit)
        {
            if (timeLimit < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The timeLimit must be 0 or positive.");

            _timeLimit = timeLimit;

            return this;
        }

        public IPeriodicHttpRequest AsFireAndForget(bool fireAndForget = true)
        {
            _fireAndForget = fireAndForget;

            return this;
        }

        public IPeriodicHttpRequest Method(string method = "GET")
        {
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Method must be set and nonempty.");
            _method = method;

            return this;
        }

        public IPeriodicHttpRequest SetHeader(string name, string value)
        {
            if (!_headers.ContainsKey(name))
                _headers.Add(name, value);
            _headers[name] = value;

            return this;
        }

        public IPeriodicHttpRequest SetHeaders(Dictionary<string, string> headers)
        {
            _headers.Clear();

            foreach (KeyValuePair<string, string> kv in headers)
            {
                _headers.Add(kv.Key, kv.Value);
            }

            return this;
        }

        public IPeriodicHttpRequest SetParameter(string name, string value)
        {
            if (!_parameters.ContainsKey(name))
                _parameters.Add(name, value);
            _parameters[name] = value;

            return this;
        }

        public IPeriodicHttpRequest SetParameters(Dictionary<string, string> parameters)
        {
            _parameters.Clear();

            foreach (KeyValuePair<string, string> kv in parameters)
            {
                _parameters.Add(kv.Key, kv.Value);
            }

            return this;
        }

        public IPeriodicHttpRequest To(Uri endpoint)
        {
            _to = endpoint;

            return this;
        }
        public IPeriodicHttpRequest To(string endpoint)
        {
            return To(new Uri(endpoint));
        }
        public string Schedule()
        {
            if (_to == null)
                throw new ArgumentException("The To must be set");

            using (ChannelFactory<ISchedulerService> ch = new ChannelFactory<ISchedulerService>(ClientUtilManager.Ask().EndpointConfigurationName))
            {
                ISchedulerService svc = ch.CreateChannel();

                if (!string.IsNullOrEmpty(_CRONString))
                    return svc.CRONHttpRequestMessage(_method, _to, _headers, _parameters, _CRONString, _timeLimit, _fireAndForget);
                else
                    return svc.PeriodicHttpRequestMessage(_method, _to, _headers, _parameters, _delay, _period, _timeLimit, _fireAndForget);
            }
        }
    }
}
