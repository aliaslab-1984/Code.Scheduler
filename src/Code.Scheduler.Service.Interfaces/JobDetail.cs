using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.Service.Interfaces
{
    [DataContract]
    public class JobDetail
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public Dictionary<string, string> JobDataMap { get; set; }
        [DataMember]
        public List<TriggerDetail> Triggers { get; set; }
        [DataMember]
        public bool Paused { get; set; }
    }
}
