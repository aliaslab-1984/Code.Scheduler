using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.Common
{
    public class DefaultJobIdProducer : IJobIdProducer
    {
        public string GetId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
