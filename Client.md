# Client

The client component exposes the entrypoint **ClientUtilManager** and a base class for the simplification of WCF webservices handler.

The class must be initialized:
<pre>
ClientUtilManager.Init();
</pre>

Some examples of the fluent interface usage:

<pre>
ClientUtilManager.Ask().DelayedHttpRequest()
    .SetHeader("x-test", "ciao")
    .SetParameter("val", "3")
    .Method("POST")
    .To(new Uri("http://requestb.in/142s9c81"))
    .After(TimeSpan.FromSeconds(5))
    .WithRetry(1)
    .Schedule();
</pre>
<pre>
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
</pre>

The base class is **Code.Scheduler.ClientUtils.HandlerUtils.SoapWcfHandlerBase**, and exposes the methods:

<pre>
JobContextData GetContextData()
</pre>
which allows the gatering of the infromation of the scheduled call context.

and
<pre>
void StopJob(bool success)
void EndWithSuccess()
void EndWithFailure()
</pre>
which are used for the conclusion of the remote task from the invoked handler.

## Client Configuration

The necessary client application .config modifications are:

	<configSections>
  		<section name="schedulerServiceConfig" type="IDSign.Scheduler.ClientUtils.Configuration.ClientUtilConfigurationSection, IDSign.Scheduler.ClientUtils" />
	</configSections>

and the relative section:

	<schedulerServiceConfig>
		<serviceEndpoint value="BasicHttpBinding_ISchedulerService" />
	</schedulerServiceConfig>
