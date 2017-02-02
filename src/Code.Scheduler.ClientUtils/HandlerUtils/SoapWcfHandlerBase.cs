using IDSign.Logging;
using IDSign.Logging.Core;
using Code.Scheduler.Common;
using Code.Scheduler.Service.Interfaces;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils.HandlerUtils
{
    public abstract class SoapWcfHandlerBase
    {
        protected ILog _logger;

        public SoapWcfHandlerBase()
        {
            _logger = LogManager.GetLogger(GetType().FullName);
        }

        protected virtual JobContextData GetContextData()
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;

            if (!request.Headers.AllKeys.Contains(JobContextData.JobDataHeaderName))
                return null;
            else
                return JsonConvert.DeserializeObject<JobContextData>(request.Headers[JobContextData.JobDataHeaderName]);
        }

        protected virtual void StopJob(bool success)
        {
            using (ILoggingOperation log = _logger.NormalOperation())
            using (ChannelFactory<ISchedulerService> ch = new ChannelFactory<ISchedulerService>(ClientUtilManager.Ask().EndpointConfigurationName))
            {
                log.Wrap(() =>
                {
                    ISchedulerService svc = ch.CreateChannel();

                    JobContextData data = GetContextData();
                    if (data == null)
                        log.Debug("No JobContextData found.");
                    else
                        svc.StopJob(data.JobId, success);
                });
            }
        }

        protected void EndWithSuccess()
        {
            StopJob(true);
        }

        protected void EndWithFailure()
        {
            StopJob(false);
        }
    }
}
