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
using System.Text.RegularExpressions;
using System.Web;

namespace Code.Scheduler.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SchedulerMonitorService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SchedulerMonitorService.svc or SchedulerMonitorService.svc.cs at the Solution Explorer and start debugging.
    public class SchedulerMonitorService : ISchedulerMonitorService
    {
        //TODO: imposta sonde in giro
        protected ILog _logger;

        protected IJobIdProducer _idProducer;

        public SchedulerMonitorService()
        {
            _logger = LogManager.GetLogger(GetType());

            _idProducer = new DefaultJobIdProducer();
        }

        public void AbortJob(string jobId)
        {
            using (ILoggingOperation log = _logger.NormalOperation()
                .AddProperty("jobId", jobId))
            {
                log.Wrap(() =>
                {
                    if (!Global.Scheduler.DeleteJob(JobKey.Create(jobId, ServiceConstants.JobGroupName)))
                        throw new Exception("Error executing job deletion.");
                });
            }
        }

        public List<JobDetail> ListJobs(string jobIdFilter)
        {
            if (string.IsNullOrEmpty(jobIdFilter))
                jobIdFilter = ".*";

            using (ILoggingOperation log = _logger.NormalOperation())
            {
                return log.Wrap<List<JobDetail>>(() =>
                {
                    List<JobDetail> result = new List<JobDetail>();

                    foreach (JobKey item in Global.Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(ServiceConstants.JobGroupName))
                        .Where(p=>Regex.IsMatch(p.Name, jobIdFilter))
                    )
                    {
                        IJobDetail detail = Global.Scheduler.GetJobDetail(item);
                        IEnumerable<ITrigger> triggers = Global.Scheduler.GetTriggersOfJob(item);

                        JobDetail jdet = new JobDetail()
                        {
                            JobId = item.Name,
                            JobDataMap = detail.JobDataMap.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                            Triggers = new List<TriggerDetail>(),
                            Paused = triggers.Any(p=>p.Key.Group==ServiceConstants.TriggerGroupName && Global.Scheduler.GetTriggerState(p.Key)==TriggerState.Paused)
                        };

                        foreach (ITrigger trigger in triggers)
                        {
                            DateTimeOffset? nf = trigger.GetNextFireTimeUtc();
                            jdet.Triggers.Add(new TriggerDetail()
                            {
                                TriggerId = trigger.Key.ToString(),
                                JobDataMap = trigger.JobDataMap.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                                StartTime = trigger.StartTimeUtc.UtcDateTime,
                                NextFireTime = nf.HasValue ? nf.Value.UtcDateTime : (DateTime?)null
                            });
                        }

                        result.Add(jdet);
                    }

                    return result;
                });
            }
        }
        public void ResumeJobs(IEnumerable<string> jobIdList)
        {
            using (ILoggingOperation log = _logger.NormalOperation())
            {
                log.Wrap(() =>
                {
                    List<JobDetail> result = new List<JobDetail>();

                    foreach (JobKey item in Global.Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(ServiceConstants.JobGroupName))
                        .Where(p => jobIdList.Contains(p.Name))
                    )
                    {
                        using (ILoggingOperation tmpLog = log.NormalOperation()
                            .AddProperty("jobId", item.Name))
                        {
                            tmpLog.SinkWrap(() =>
                            {
                                ITrigger trigger = Global.Scheduler.GetTriggersOfJob(item).First(p => p.Key.Group == ServiceConstants.TriggerGroupName && Global.Scheduler.GetTriggerState(p.Key) == TriggerState.Paused);

                                Global.Scheduler.RescheduleJob(trigger.Key, (ITrigger)trigger.Clone());
                                //Global.Scheduler.ResumeJob(item);
                            });
                        }
                    }
                });
            }
        }
    }
}
