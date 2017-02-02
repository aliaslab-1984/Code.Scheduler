using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.Common
{
    public class JobContextData
    {
        public const string JobDataHeaderName = "X-Scheduler-data";

        public string JobId { get; set; }
        public string JobGroup { get; set; }
    }
}
