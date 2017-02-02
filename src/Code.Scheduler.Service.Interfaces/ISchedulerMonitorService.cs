using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Code.Scheduler.Service.Interfaces
{
    [ServiceContract]
    public interface ISchedulerMonitorService
    {
        [OperationContract]
        List<JobDetail> ListJobs(string jobIdFilter=".*");
        [OperationContract]
        void ResumeJobs(IEnumerable<string> jobIdList);
        [OperationContract]
        void AbortJob(string jobId);
    }
}
