# Environment variables

You can configure some of BirdsiteLIVE's settings via environment variables (those are optionnals):

## Logging

* `Logging:Type` (default: none) set the type of the logging and monitoring system, currently the only type supported is `insights` for *Azure Application Insights* (PR welcome to support other types)
* `Logging:InstrumentationKey` the instrumentation key for Azure Application Insights

## Instance customization 

* `Instance:Name` (default: BirdsiteLIVE) the name of the instance
* `Instance:ResolveMentionsInProfiles` (default: true) to enable or disable mentions parsing in profile's description. Resolving it will consume more User's API calls since newly discovered account can also contain references to others accounts as well. On a big instance it is recommended to disable it.