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
    internal class PeriodicChannelsMessage : IPeriodicChannelsMessage
    {
        protected TimeSpan _delay = TimeSpan.FromSeconds(0);
        protected TimeSpan _period = TimeSpan.FromSeconds(0);
        protected TimeSpan _timeLimit = TimeSpan.MaxValue;
        protected bool _fireAndForget = false;

        protected string _CRONString = null;
        protected bool _timeSpec = false;

        protected bool _broadcast = false;
        protected string _channelName;

        protected string _messageJson;

        public IPeriodicChannelsMessage WithCRONExpression(string CRONString)
        {
            if (_timeSpec)
                throw new InvalidOperationException("The timespec has already been set. Only one schedule specification is allowed");

            if (string.IsNullOrEmpty(CRONString))
                throw new ArgumentException("CRONString must be not null  and non empty");

            _CRONString = CRONString;

            return this;
        }

        public IPeriodicChannelsMessage After(TimeSpan delay)
        {
            if (!string.IsNullOrEmpty(_CRONString))
                throw new InvalidOperationException("The CRONString has already been set. Only one schedule specification is allowed");

            if (delay < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The delay must be 0 or positive.");
            _delay = delay;

            _timeSpec = true;

            return this;
        }

        public IPeriodicChannelsMessage Every(TimeSpan period)
        {
            if (!string.IsNullOrEmpty(_CRONString))
                throw new InvalidOperationException("The CRONString has already been set. Only one schedule specification is allowed");

            if (period < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The period must be 0 or positive.");
            _period = period;

            _timeSpec = true;

            return this;
        }

        public IPeriodicChannelsMessage WithAllottedTime(TimeSpan timeLimit)
        {
            if(timeLimit < TimeSpan.FromSeconds(0))
                throw new ArgumentException("The timeLimit must be 0 or positive.");

            _timeLimit = timeLimit;

            return this;
        }

        public IPeriodicChannelsMessage AsFireAndForget(bool fireAndForget = true)
        {
            _fireAndForget = fireAndForget;

            return this;
        }

        public IPeriodicChannelsMessage Broadcast(bool broadcast = true)
        {
            _broadcast = broadcast;

            return this;
        }

        public IPeriodicChannelsMessage ChannelName(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
                throw new ArgumentNullException("The channel name must be set and nonempty.");
            _channelName = channelName;

            return this;
        }

        public IPeriodicChannelsMessage WithMessage(object message)
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

                if(!string.IsNullOrEmpty(_CRONString))
                    return svc.CRONChannelMessage(_channelName, _broadcast, _messageJson, _CRONString, _timeLimit, _fireAndForget);
                else
                    return svc.PeriodicChannelMessage(_channelName, _broadcast, _messageJson, _delay, _period, _timeLimit, _fireAndForget);
            }
        }
    }
}
