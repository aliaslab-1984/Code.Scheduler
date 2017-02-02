using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.ClientUtils
{
    public interface IPeriodicJob<T>
    {
        T After(TimeSpan delay);
        T Every(TimeSpan period);
        T WithCRONExpression(string CRONString);
        T WithAllottedTime(TimeSpan timeLimit);
        T AsFireAndForget(bool fireAndForget = true);
        string Schedule();
    }
}
