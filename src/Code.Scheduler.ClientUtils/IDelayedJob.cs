using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public interface IDelayedJob<T>
    {
        T After(TimeSpan delay);
        T WithRetry(int count = 0);
        T AsFireAndForget(bool fireAndForget = true);
        string Schedule();
    }
}
