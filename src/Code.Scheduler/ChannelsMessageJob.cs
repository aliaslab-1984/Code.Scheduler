using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Channels;
using Channels.RabbitMQ;
using log4net;
using CodeProbe.Sensing;
using Newtonsoft.Json;
using IDSign.Logging.Core;
using IDSign.Logging;

namespace Code.Scheduler
{
    public class ChannelsMessageJob : RemoteCallJobBase
    {

        public ChannelsMessageJob()
            :base()
        {}

        public string ChannelName { protected get; set; }
        public bool Broadcast { protected get; set; }
        public string Message { protected get; set; }
        protected override void RemoteCall(IJobExecutionContext context)
        {
            using (ILoggingOperation log = _logger.NormalOperation())
            {
                log.Wrap(() =>
                {
                    RabbitMQChannelsFactory factory = new RabbitMQChannelsFactory();

                    IChannelWriter<object> ch = null;
                    if (Broadcast)
                        ch = factory.GetSubscribableChannel<object>(ChannelName);
                    else
                        ch = factory.GetChannel<object>(ChannelName);

                    using (ch)
                    {
                        ch.Write(Message);
                    }
                });
            }
        }
    }
}
