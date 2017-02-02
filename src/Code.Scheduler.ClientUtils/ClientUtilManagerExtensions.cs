using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public static class ClientUtilManagerExtensions
    {
        public static IDelayedChannelsMessage DelayedChannelsMessage(this ClientUtilManager ext)
        {
            return new Impl.DelayedChannelsMessage();
        }
        public static IDelayedHttpRequest DelayedHttpRequest(this ClientUtilManager ext)
        {
            return new Impl.DelayedHttpRequest();
        }
        public static IDelayedSOAPRequest DelayedSOAPRequest(this ClientUtilManager ext)
        {
            return new Impl.DelayedSOAPRequest();
        }

        public static IPeriodicChannelsMessage PeriodicChannelsMessage(this ClientUtilManager ext)
        {
            return new Impl.PeriodicChannelsMessage();
        }
        public static IPeriodicHttpRequest PeriodicHttpRequest(this ClientUtilManager ext)
        {
            return new Impl.PeriodicHttpRequest();
        }
        public static IPeriodicSOAPRequest PeriodicSOAPRequest(this ClientUtilManager ext)
        {
            return new Impl.PeriodicSOAPRequest();
        }
    }
}
