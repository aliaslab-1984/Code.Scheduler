using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IUnitTestService" in both code and config file together.
    [ServiceContract]
    public interface IUnitTestService
    {
        [OperationContract]
        void SendDateTimeToUrl(string url);

        [OperationContract]
        void BlockFor(TimeSpan blockTime);

        [OperationContract]
        void FailAt(DateTime datetime);

        [OperationContract]
        void SuccedAt(DateTime datetime);
    }
}
