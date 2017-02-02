# Code.Scheduler
Client+Server suite to enable the remote scheduling of remote tasks dividing the responsibility of the temporal scheduling from the tasks logic.

The service (which uses [Quartz.NET](http://www.quartz-scheduler.net)) allows to schedule a remote method call by the use of:

- SOAP message
- HTTP/REST request
- Message through a [Channels](https://github.com/aliaslab-1984/Channels) channel

Specifing as schedule mode:

1. single delayed invocation
2. repeated delayed and periodic invocation
3. repeated periodic invocation with the period based on a [CRON expression](https://docs.oracle.com/cd/E12058_01/doc/doc.1014/e12030/cron_expressions.htm)

For the single invocation mode is possible to specify a number of not delayed retries, before stop the execution of the task.
For the other repeated modes is possible to specify a maximum allotted time for the execution and is possible to block the repetition from the call handler, deeming it as concluded with success or failure.

For every mode is possible to specify if the remote call should have a fire&forget semantic or has to be synchronous with respect to the temporal scheduling.