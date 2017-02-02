using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public interface IDelayedChannelsMessage : IDelayedJob<IDelayedChannelsMessage>
    {
        IDelayedChannelsMessage ChannelName(string channelName);
        IDelayedChannelsMessage Broadcast(bool broadcast = true);
        IDelayedChannelsMessage WithMessage(object message);
    }
}
