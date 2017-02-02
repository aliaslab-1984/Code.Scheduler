using Code.Scheduler.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils.Impl
{
    internal class DelayedHttpRequest : IDelayedHttpRequest
    {
        protected TimeSpan _delay = TimeSpan.FromSeconds(0);
        protected int _retryCount = 0;
        protected bool _fireAndForget = false;

        protected string _method = "GET";
        protected Dictionary<string, string> _headers = new Dictionary<string, string>();
        protected Dictionary<string, string> _parameters = new Dictionary<string, string>();

        protected Uri _to;
        
        public IDelayedHttpRequest After(TimeSpan delay)
        {
            if (delay < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The delay must be 0 or positive.");
            _delay = delay;

            return this;
        }

        public IDelayedHttpRequest WithRetry(int count = 0)
        {
            if (count < 0)
                throw new ArgumentException("The retryCount must be 0 or positive.");
            _retryCount = count;

            return this;
        }

        public IDelayedHttpRequest AsFireAndForget(bool fireAndForget = true)
        {
            _fireAndForget = fireAndForget;

            return this;
        }

        public IDelayedHttpRequest Method(string method = "GET")
        {
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Method must be set and nonempty.");
            _method = method;

            return this;
        }

        public IDelayedHttpRequest SetHeader(string name, string value)
        {
            if (!_headers.ContainsKey(name))
                _headers.Add(name, value);
            _headers[name] = value;

            return this;
        }

        public IDelayedHttpRequest SetHeaders(Dictionary<string, string> headers)
        {
            _headers.Clear();

            foreach (KeyValuePair<string,string> kv in headers)
            {
                _headers.Add(kv.Key, kv.Value);
            }

            return this;
        }

        public IDelayedHttpRequest SetParameter(string name, string value)
        {
            if (!_parameters.ContainsKey(name))
                _parameters.Add(name, value);
            _parameters[name] = value;

            return this;
        }

        public IDelayedHttpRequest SetParameters(Dictionary<string, string> parameters)
        {
            _parameters.Clear();

            foreach (KeyValuePair<string, string> kv in parameters)
            {
                _parameters.Add(kv.Key, kv.Value);
            }

            return this;
        }

        public IDelayedHttpRequest To(Uri endpoint)
        {
            _to = endpoint;

            return this;
        }
        public IDelayedHttpRequest To(string endpoint)
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

                return svc.DelayedHttpRequestMessage(_method,_to,_headers,_parameters,_delay,_retryCount, _fireAndForget);
            }
        }
    }
}
