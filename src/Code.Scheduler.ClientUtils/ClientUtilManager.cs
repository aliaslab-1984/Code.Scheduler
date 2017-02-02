using Code.Scheduler.ClientUtils.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public class ClientUtilManager
    {
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static ClientUtilManager _instance;

        protected ClientUtilManager(ClientUtilConfigurationSection conf)
        {
            try
            {
                EndpointConfigurationName = conf.ServiceEndpoint.Value;
            }
            catch (Exception e)
            {
                _logger.Debug("Error configuring SchedulerServicesClientUtil", e);

                throw e;
            }
        }

        public static void Init()
        {
            Init("schedulerServiceConfig");
        }

        public static void Init(string section)
        {
            _logger.Info("Initializing.");

            try
            {
                if (_instance != null)
                    throw new InvalidOperationException("Already initialized.");

                ClientUtilConfigurationSection conf = (ClientUtilConfigurationSection)ConfigurationManager.GetSection(section);
                _instance = new ClientUtilManager(conf);

                _logger.Info("Initialized.");
            }
            catch (Exception e)
            {
                _instance = null;
                _logger.Error("Error during intialization.", e);
                throw e;
            }
        }

        public static ClientUtilManager Ask()
        {
            if (_instance == null)
                throw new InvalidOperationException("Component not initialized.");
            return _instance;
        }

        public string EndpointConfigurationName { get; protected set; }
    }
}
