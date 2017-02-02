using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.Service.Interfaces
{
    [DataContract]
    public class TriggerDetail
    {
        [DataMember]
        public string TriggerId { get; set; }

        [DataMember]
        public Dictionary<string, string> JobDataMap { get; set; }

        [DataMember]
        public DateTime? StartTime { get; set; }
        [DataMember]
        public DateTime? NextFireTime { get; set; }
    }
}
