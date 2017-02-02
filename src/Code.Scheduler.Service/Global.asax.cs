using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Channels.RabbitMQ;
using log4net;
using IDSign.Logging.Core;
using IDSign.Logging;
using CodeProbe.Sensing;
using CodeProbe.HealthChecks;
using CodeProbe.Reporting;
using CodeProbe.Reporting.Remote;

namespace Code.Scheduler.Service
{
    public class Global : System.Web.HttpApplication
    {
        protected ILog _logger;

        public static ISchedulerFactory Factory;
        public static IScheduler Scheduler;

        protected void Application_Start(object sender, EventArgs e)
        {
            _logger = LogManager.GetLogger(GetType());

            using (ILoggingOperation log = _logger.CriticalOperation())
            {
                log.Wrap(() =>
                {
                    ProbeManager.Init();
                    HealthCheckManager.Init();
                    ReportingManager.Init();
                    RemoteReportingManager.Init();
                });

                log.Wrap(()=> ChannelsRabbitMQManager.Init());

                log.Wrap(() => Factory = new StdSchedulerFactory());
                log.Wrap(() => Scheduler = Factory.GetScheduler());

                log.Wrap(() => Scheduler.Start());
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            using (ILoggingOperation log = _logger.CriticalOperation())
            {
                log.Wrap(() => Scheduler.Shutdown(true));
            }
        }
    }
}