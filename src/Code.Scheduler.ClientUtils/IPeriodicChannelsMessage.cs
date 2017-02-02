using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public interface IPeriodicChannelsMessage : IPeriodicJob<IPeriodicChannelsMessage>
    {
        IPeriodicChannelsMessage ChannelName(string channelName);
        IPeriodicChannelsMessage Broadcast(bool broadcast = true);
        IPeriodicChannelsMessage WithMessage(object message);
    }
}
