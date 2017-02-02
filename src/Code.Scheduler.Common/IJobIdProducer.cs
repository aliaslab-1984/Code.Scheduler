using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.Common
{
    public interface IJobIdProducer
    {
        string GetId();
    }
}
