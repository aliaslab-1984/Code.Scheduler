using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Code.Scheduler.ClientUtils;
using System.Collections.Generic;
using Code.Scheduler.Service.Interfaces;
using System.ServiceModel;
using TestService;

namespace Test.ClientUtils
{
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            ClientUtilManager.Init();
        }
        [TestMethod]
        public void TestMethod1()
        {
            ClientUtilManager.Ask().DelayedHttpRequest()
                .SetHeader("x-test", "ciao")
                .SetParameter("val", "3")
                .Method("POST")
                .To(new Uri("http://requestb.in/142s9c81"))
                .After(TimeSpan.FromSeconds(5))
                .WithRetry(1)
                .Schedule();
        }

        [TestMethod]
        public void TestMethod2()
        {
            ClientUtilManager.Ask().DelayedSOAPRequest()
                .After(TimeSpan.FromSeconds(5))
                .WithBinding(new BasicHttpBinding())
                .Request<ISchedulerService>( p=>
                p.DelayedHttpRequestMessage("post",new Uri("http://requestb.in/142s9c81"),new Dictionary<string, string>() { { "x-cli","yesN2" } }, new Dictionary<string, string>() { { "par", "yesN2" } }, TimeSpan.FromSeconds(5),0,false))
                .To(new Uri("http://Code.test.aliaslab.net/SchedulerService/SchedulerService.svczzz"))
                //.To(new Uri("http://localhost:2213/SchedulerService.svc"))
                .Schedule();
        }

        [TestMethod]
        public void TestMethod3()
        {
            ClientUtilManager.Ask().DelayedSOAPRequest()
                .After(TimeSpan.FromSeconds(500))
                .ToConfigEndpoint("BasicHttpBinding_ISchedulerService")
                .Request<ISchedulerService>(p =>
               p.DelayedHttpRequestMessage("post", new Uri("http://requestb.in/142s9c81"), new Dictionary<string, string>() { { "x-cli", "yesN2" } }, new Dictionary<string, string>() { { "par", "yesN2" } }, TimeSpan.FromSeconds(5), 0, false))
                .Schedule();
        }


        [TestMethod]
        public void TestMethod4()
        {
            ClientUtilManager.Ask().PeriodicSOAPRequest()
                .After(TimeSpan.FromSeconds(10))
                .Every(TimeSpan.FromSeconds(5))
                .WithAllottedTime(TimeSpan.FromMinutes(2))
                .AsFireAndForget()
                .ToConfigEndpoint("BasicHttpBinding_UnitTestService")
                .Request<IUnitTestService>(p =>
                    p.SendDateTimeToUrl("http://requestb.in/12wvclh1")
                )
                .Schedule();
        }


        [TestMethod]
        public void TestMethod5()
        {
            ClientUtilManager.Ask().DelayedSOAPRequest()
                .After(TimeSpan.FromSeconds(10))
                .ToConfigEndpoint("BasicHttpBinding_UnitTestService")
                .Request<IUnitTestService>(p =>
                    p.BlockFor(TimeSpan.FromSeconds(15))
                )
                .Schedule();
        }


        [TestMethod]
        public void TestMethod6()
        {
            ClientUtilManager.Ask().DelayedSOAPRequest()
                .After(TimeSpan.FromSeconds(10))
                .AsFireAndForget()
                .ToConfigEndpoint("BasicHttpBinding_UnitTestService")
                .Request<IUnitTestService>(p =>
                    p.BlockFor(TimeSpan.FromSeconds(15))
                )
                .Schedule();
        }


        [TestMethod]
        public void TestMethod7()
        {
            string jobId=ClientUtilManager.Ask().PeriodicSOAPRequest()
                .After(TimeSpan.FromSeconds(10))
                .Every(TimeSpan.FromSeconds(5))
                .WithAllottedTime(TimeSpan.FromMinutes(2))
                .AsFireAndForget()
                .ToConfigEndpoint("BasicHttpBinding_UnitTestService")
                .Request<IUnitTestService>(p =>
                    p.SuccedAt(DateTime.Now+TimeSpan.FromSeconds(30))
                )
                .Schedule();
        }
    }
}
