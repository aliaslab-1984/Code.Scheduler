using CodeProbe.Sensing;
using Code.Configuration;
using IDSign.Logging;
using IDSign.Logging.Core;
using log4net;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler
{
    [DisallowConcurrentExecution]
    public abstract class RemoteCallJobBase : IJob
    {
        protected AbstractConfigValue _readTimeoutMillis = new AppSettingsConfigValue("readTimeoutMillis", "300000");
        protected AbstractConfigValue _writeTimeoutMillis = new AppSettingsConfigValue("writeTimeoutMillis", "300000");

        protected ILog _logger;
        public RemoteCallJobBase()
        {
            _logger = LogManager.GetLogger(GetType());
            ProbeManager.Ask().AppCounter($"{GetType().Name}.created").Increment();
        }

        public virtual string RetryCount { protected get; set; }
        public virtual string FireAndForget { protected get; set; }

        public virtual void Execute(IJobExecutionContext context)
        {
            using (ILoggingOperation log = _logger.NormalOperation().AddProperty("jobId", context.JobDetail.Key.Name))
            {
                try
                {
                    RemoteCall(context);
                    ProbeManager.Ask().AppCounter($"{GetType().Name}.completed").Increment();
                    _logger.Info($"job={context.JobDetail.Key.Name} executed");
                }
                catch (Exception e)
                {
                    if (context.RefireCount < Convert.ToInt32(RetryCount))
                    {
                        ProbeManager.Ask().AppCounter($"{GetType().Name}.error-refire").Increment();
                        _logger.Warn($"job={context.JobDetail.Key.Name}, refireCount={context.RefireCount}");
                        throw new JobExecutionException(e, true);
                    }
                    else
                    {
                        ProbeManager.Ask().AppCounter($"{GetType().Name}.error-definitive").Increment();
                        _logger.Error($"job={context.JobDetail.Key.Name}, refireCount={context.RefireCount}, params={JsonConvert.SerializeObject(context.MergedJobDataMap)}");

                        context.Scheduler.RescheduleJob(context.Trigger.Key, (ITrigger)context.Trigger.Clone());

                        context.Scheduler.PauseJob(context.JobDetail.Key);
                    }
                }
            }
        }

        protected abstract void RemoteCall(IJobExecutionContext context);
    }
}
