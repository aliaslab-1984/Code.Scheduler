using Code.Scheduler.Service.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils.Impl
{
    internal class DelayedChannelsMessage : IDelayedChannelsMessage
    {
        protected TimeSpan _delay = TimeSpan.FromSeconds(0);
        protected int _retryCount = 0;
        protected bool _fireAndForget = false;

        protected bool _broadcast = false;
        protected string _channelName;

        protected string _messageJson;

        public IDelayedChannelsMessage After(TimeSpan delay)
        {
            if (delay < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The delay must be 0 or positive.");
            _delay = delay;

            return this;
        }
        
        public IDelayedChannelsMessage WithRetry(int count = 0)
        {
            if (count < 0)
                throw new ArgumentException("The retryCount must be 0 or positive.");
            _retryCount = count;

            return this;
        }

        public IDelayedChannelsMessage AsFireAndForget(bool fireAndForget = true)
        {
            _fireAndForget = fireAndForget;

            return this;
        }

        public IDelayedChannelsMessage Broadcast(bool broadcast = true)
        {
            _broadcast = broadcast;

            return this;
        }

        public IDelayedChannelsMessage ChannelName(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
                throw new ArgumentNullException("The channel name must be set and nonempty.");
            _channelName = channelName;

            return this;
        }

        public IDelayedChannelsMessage WithMessage(object message)
        {
            _messageJson = JsonConvert.SerializeObject(message);

            return this;
        }
        public string Schedule()
        {
            if (_messageJson == null)
                throw new ArgumentException("The message must be set");
            if (_channelName == null)
                throw new ArgumentException("The channelname must be set.");

            using (ChannelFactory<ISchedulerService> ch = new ChannelFactory<ISchedulerService>(ClientUtilManager.Ask().EndpointConfigurationName))
            {
                ISchedulerService svc = ch.CreateChannel();

                return svc.DelayedChannelMessage(_channelName, _broadcast, _messageJson, _delay, _retryCount, _fireAndForget);
            }
        }
    }
}
