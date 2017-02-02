using IDSign.Logging;
using IDSign.Logging.Core;
using Code.Scheduler.Common;
using Code.Scheduler.Service.Interfaces;
using log4net;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web;

namespace Code.Scheduler.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SchedulerService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SchedulerService.svc or SchedulerService.svc.cs at the Solution Explorer and start debugging.
    public class SchedulerService : ISchedulerService
    {
        //TODO: imposta sonde in giro
        protected ILog _logger;

        protected IJobIdProducer _idProducer;

        public SchedulerService()
        {
            _logger = LogManager.GetLogger(GetType());

            _idProducer = new DefaultJobIdProducer();
        }
        public void StopJob(string jobId, bool success)
        {
            using (ILoggingOperation log = _logger.NormalOperation()
                .AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    if (!Global.Scheduler.DeleteJob(JobKey.Create(jobId, ServiceConstants.JobGroupName)))
                        throw new Exception("Error executing job deletion.");
                    if(success)
                        log.Info($"job stopped with success");
                    else
                        log.Info($"job stopped with failure");
                });
            }
        }
        
        public string DelayedChannelMessage(string channelName, bool broadcast, string message, TimeSpan delay, int retryCount, bool fireAndForget)
        {
            if (delay == null)
                throw new ArgumentNullException("delay");
            if (delay < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("delay must be positive");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} ChannelsMessageJob with parameters channelName={channelName}, broadcast={broadcast}, message={message}, delay={delay}, retryCount={retryCount}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<ChannelsMessageJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("ChannelName", channelName)
                        .UsingJobData("Broadcast", broadcast)
                        .UsingJobData("Message", message)
                        .UsingJobData("RetryCount", retryCount.ToString())
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate((int)delay.TotalMilliseconds, IntervalUnit.Millisecond))
                        .WithSimpleSchedule(p => p.WithMisfireHandlingInstructionFireNow())
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} ChannelsMessageJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }

        public string DelayedHttpRequestMessage(string method, Uri destination, Dictionary<string, string> headers, Dictionary<string, string> parameters, TimeSpan delay, int retryCount, bool fireAndForget)
        {
            if (delay == null)
                throw new ArgumentNullException("delay");
            if (delay < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("delay must be positive");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} HttpRequestJob with parameters method={method}, destination={destination}, headaers={headers}, parameters={parameters}, delay={delay}, retryCount={retryCount}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<HttpRequestJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("Method", method)
                        .UsingJobData("Destination", destination.ToString())
                        .UsingJobData("HeadersJSON", JsonConvert.SerializeObject(headers))
                        .UsingJobData("ParametersJSON", JsonConvert.SerializeObject(parameters))
                        .UsingJobData("RetryCount", retryCount.ToString())
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate((int)delay.TotalMilliseconds, IntervalUnit.Millisecond))
                        .WithSimpleSchedule(p => p.WithMisfireHandlingInstructionFireNow())
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} HttpRequestJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }

        public string DelayedSOAPRequestMessage(string hostname, int port, byte[] payload, TimeSpan delay, int retryCount, bool fireAndForget)
        {
            if (delay == null)
                throw new ArgumentNullException("delay");
            if (delay < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("delay must be positive");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} TCPRequestJob with parameters hostname={hostname}, port={port}, payload={Convert.ToBase64String(payload)}, delay={delay}, retryCount={retryCount}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<SOAPRequestJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("Hostname", hostname)
                        .UsingJobData("Port", port.ToString())
                        .UsingJobData("PayloadBase64", Convert.ToBase64String(payload))
                        .UsingJobData("RetryCount", retryCount.ToString())
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate((int)delay.TotalMilliseconds, IntervalUnit.Millisecond))
                        .WithSimpleSchedule(p => p.WithMisfireHandlingInstructionFireNow())
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} TCPRequestJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }

        public string PeriodicChannelMessage(string channelName, bool broadcast, string message, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget)
        {
            if (delay == null)
                throw new ArgumentNullException("delay");
            if (delay < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("delay must be positive");

            if (period == null)
                throw new ArgumentNullException("period");
            if (period < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("period must be positive");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} ChannelsMessageJob with parameters channelName={channelName}, broadcast={broadcast}, message={message}, delay={delay}, period={period}, timeLimit={timeLimit}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<ChannelsMessageJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("ChannelName", channelName)
                        .UsingJobData("Broadcast", broadcast)
                        .UsingJobData("Message", message)
                        .UsingJobData("RetryCount", "0")
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate((int)delay.TotalMilliseconds, IntervalUnit.Millisecond))
                        .WithSimpleSchedule(p => p
                            .WithMisfireHandlingInstructionFireNow()
                            .WithInterval(period)
                            .RepeatForever()
                        )
                        .EndAt(DateBuilder.FutureDate((int)(timeLimit == null ? TimeSpan.MaxValue : timeLimit).TotalMilliseconds, IntervalUnit.Millisecond))
                        .Build();
                    
                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} ChannelsMessageJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }

        public string PeriodicHttpRequestMessage(string method, Uri destination, Dictionary<string, string> headers, Dictionary<string, string> parameters, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget)
        {
            if (delay == null)
                throw new ArgumentNullException("delay");
            if (delay < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("delay must be positive");

            if (period == null)
                throw new ArgumentNullException("period");
            if (period < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("period must be positive");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} HttpRequestJob with parameters method={method}, destination={destination}, headaers={headers}, parameters={parameters}, delay={delay}, period={period}, timeLimit={timeLimit}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<HttpRequestJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("Method", method)
                        .UsingJobData("Destination", destination.ToString())
                        .UsingJobData("HeadersJSON", JsonConvert.SerializeObject(headers))
                        .UsingJobData("ParametersJSON", JsonConvert.SerializeObject(parameters))
                        .UsingJobData("RetryCount", "0")
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate((int)delay.TotalMilliseconds, IntervalUnit.Millisecond))
                        .WithSimpleSchedule(p => p
                            .WithMisfireHandlingInstructionFireNow()
                            .WithInterval(period)
                            .RepeatForever()
                        )
                        .EndAt(DateBuilder.FutureDate((int)(timeLimit == null ? TimeSpan.MaxValue : timeLimit).TotalMilliseconds, IntervalUnit.Millisecond))
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} HttpRequestJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }

        public string PeriodicSOAPRequestMessage(string hostname, int port, byte[] payload, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget)
        {
            if (delay == null)
                throw new ArgumentNullException("delay");
            if (delay < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("delay must be positive");

            if (period == null)
                throw new ArgumentNullException("period");
            if (period < TimeSpan.Parse("00:00:00"))
                throw new ArithmeticException("period must be positive");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} TCPRequestJob with parameters hostname={hostname}, port={port}, payload={Convert.ToBase64String(payload)}, delay={delay}, period={period}, timeLimit={timeLimit}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<SOAPRequestJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("Hostname", hostname)
                        .UsingJobData("Port", port.ToString())
                        .UsingJobData("PayloadBase64", Convert.ToBase64String(payload))
                        .UsingJobData("RetryCount", "0")
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate((int)delay.TotalMilliseconds, IntervalUnit.Millisecond))
                        .WithSimpleSchedule(p => p
                            .WithMisfireHandlingInstructionFireNow()
                            .WithInterval(period)
                            .RepeatForever()
                        )
                        .EndAt(DateBuilder.FutureDate((int)(timeLimit == null ? TimeSpan.MaxValue : timeLimit).TotalMilliseconds,IntervalUnit.Millisecond))
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} TCPRequestJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }


        public string CRONChannelMessage(string channelName, bool broadcast, string message, string CRONString, TimeSpan timeLimit, bool fireAndForget)
        {
            if (CRONString == null)
                throw new ArgumentNullException("CRONString");
            if (!CronExpression.IsValidExpression(CRONString))
                throw new ArgumentException("CRONString is not a valid cron string");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} ChannelsMessageJob with parameters channelName={channelName}, broadcast={broadcast}, message={message}, CRONString={CRONString}, timeLimit={timeLimit}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<ChannelsMessageJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("ChannelName", channelName)
                        .UsingJobData("Broadcast", broadcast)
                        .UsingJobData("Message", message)
                        .UsingJobData("RetryCount", "0")
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate(0, IntervalUnit.Millisecond))
                        .WithCronSchedule(CRONString, p => p
                            .WithMisfireHandlingInstructionFireAndProceed()
                        )
                        .EndAt(DateBuilder.FutureDate((int)(timeLimit == null ? TimeSpan.MaxValue : timeLimit).TotalMilliseconds, IntervalUnit.Millisecond))
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} ChannelsMessageJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }

        public string CRONHttpRequestMessage(string method, Uri destination, Dictionary<string, string> headers, Dictionary<string, string> parameters, string CRONString, TimeSpan timeLimit, bool fireAndForget)
        {
            if (CRONString == null)
                throw new ArgumentNullException("CRONString");
            if (!CronExpression.IsValidExpression(CRONString))
                throw new ArgumentException("CRONString is not a valid cron string");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} HttpRequestJob with parameters method={method}, destination={destination}, headaers={headers}, parameters={parameters}, CRONString={CRONString}, timeLimit={timeLimit}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<HttpRequestJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("Method", method)
                        .UsingJobData("Destination", destination.ToString())
                        .UsingJobData("HeadersJSON", JsonConvert.SerializeObject(headers))
                        .UsingJobData("ParametersJSON", JsonConvert.SerializeObject(parameters))
                        .UsingJobData("RetryCount", "0")
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate(0, IntervalUnit.Millisecond))
                        .WithCronSchedule(CRONString, p => p
                            .WithMisfireHandlingInstructionFireAndProceed()
                        )
                        .EndAt(DateBuilder.FutureDate((int)(timeLimit == null ? TimeSpan.MaxValue : timeLimit).TotalMilliseconds, IntervalUnit.Millisecond))
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} HttpRequestJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }

        public string CRONSOAPRequestMessage(string hostname, int port, byte[] payload, string CRONString, TimeSpan timeLimit, bool fireAndForget)
        {
            if (CRONString == null)
                throw new ArgumentNullException("CRONString");
            if (!CronExpression.IsValidExpression(CRONString))
                throw new ArgumentException("CRONString is not a valid cron string");

            string jobId = _idProducer.GetId();
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    log.Info($"scheduling request {jobId} TCPRequestJob with parameters hostname={hostname}, port={port}, payload={Convert.ToBase64String(payload)}, CRONString={CRONString}, timeLimit={timeLimit}, fireAndForget={fireAndForget}");

                    IJobDetail job = JobBuilder.Create<SOAPRequestJob>()
                        .WithIdentity(jobId, ServiceConstants.JobGroupName)
                        .StoreDurably(false)
                        .UsingJobData("Hostname", hostname)
                        .UsingJobData("Port", port.ToString())
                        .UsingJobData("PayloadBase64", Convert.ToBase64String(payload))
                        .UsingJobData("RetryCount", "0")
                        .UsingJobData("FireAndForget", fireAndForget.ToString())
                        .Build();
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(Guid.NewGuid().ToString(), Service.ServiceConstants.TriggerGroupName)
                        .StartAt(DateBuilder.FutureDate(0, IntervalUnit.Millisecond))
                        .WithCronSchedule(CRONString, p => p
                            .WithMisfireHandlingInstructionFireAndProceed()
                        )
                        .EndAt(DateBuilder.FutureDate((int)(timeLimit == null ? TimeSpan.MaxValue : timeLimit).TotalMilliseconds, IntervalUnit.Millisecond))
                        .Build();

                    DateTimeOffset t = Global.Scheduler.ScheduleJob(job, trigger);

                    log.Info($"scheduled request {jobId} TCPRequestJob in {t.ToString("HH:mm:ss.fff")}");
                });

                return jobId;
            }
        }
    }
}
