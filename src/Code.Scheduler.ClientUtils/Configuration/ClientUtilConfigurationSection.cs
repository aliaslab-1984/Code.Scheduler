using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils.Configuration
{
    public class ClientUtilConfigurationSection : ConfigurationSection
    {
        #region nested types

        public class ServiceEndpointElement : ConfigurationElement
        {
            [ConfigurationProperty("value", IsRequired = true)]
            public string Value
            {
                get { return this["value"].ToString(); }
            }
        }

        #endregion

        [ConfigurationProperty("serviceEndpoint", IsRequired = true)]
        public ServiceEndpointElement ServiceEndpoint
        {
            get { return (ServiceEndpointElement)this["serviceEndpoint"]; }
        }
    }
}
