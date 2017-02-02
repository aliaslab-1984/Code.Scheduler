# Server

The server component is based on [Quartz.NET](http://www.quartz-scheduler.net) and exposes two webservices:

- **SchedulerService**: allow to schedule a remote method call
- **SchedulerMonitorService**: allows to monitor the execution of the tasks on the server and to manage their cancellation or resumption in case of pause (the failed tasks are paused in order to allow the forced rescheduling if the failure cause is temporary)

## SchedulerService interface
<pre>
string DelayedChannelMessage(string channelName, bool broadcast, string message, TimeSpan delay, int retryCount, bool fireAndForget)
string DelayedHttpRequestMessage(string method, Uri destination, Dictionary<string,string> headers, Dictionary<string,string> parameters, TimeSpan delay, int retryCount, bool fireAndForget)
string DelayedSOAPRequestMessage(string hostname, int port, byte[] payload, TimeSpan delay, int retryCount, bool fireAndForget)

string PeriodicChannelMessage(string channelName, bool broadcast, string message, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget)
string PeriodicHttpRequestMessage(string method, Uri destination, Dictionary<string, string> headers, Dictionary<string, string> parameters, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget)
string PeriodicSOAPRequestMessage(string hostname, int port, byte[] payload, TimeSpan delay, TimeSpan period, TimeSpan timeLimit, bool fireAndForget)

string CRONChannelMessage(string channelName, bool broadcast, string message, string CRONString, TimeSpan timeLimit, bool fireAndForget)
string CRONHttpRequestMessage(string method, Uri destination, Dictionary<string, string> headers, Dictionary<string, string> parameters, string CRONString, TimeSpan timeLimit, bool fireAndForget)
string CRONSOAPRequestMessage(string hostname, int port, byte[] payload, string CRONString, TimeSpan timeLimit, bool fireAndForget)

void StopJob(string jobId, bool success)
</pre>

## SchedulerMonitorService interface
<pre>
List<JobDetail> ListJobs(string jobIdFilter=".*")
void ResumeJobs(IEnumerable<string> jobIdList)
void AbortJob(string jobId)
</pre>