# Environment variables

You can configure some of BirdsiteLIVE's settings via environment variables (those are optionnals):

## Blacklisting & Whitelisting

### Fediverse users and instances 

Here are the supported patterns to describe Fediverse users and/or instances:

* `@user@instance.ext` to describe a Fediverse user
* `instance.ext` to describe an instance under a domain name
* `*.instance.ext` to describe instances from all subdomains of a domain name (this doesn't include the instance.ext, if you want both you need to add both)

You can whitelist or blacklist fediverses users by settings the followings variables with the above patterns separated by `;`: 

* `Moderation:FollowersWhiteListing` Fediverse Whitelisting
* `Moderation:FollowersBlackListing` Fediverse Blacklisting

If the whitelisting is set, only given patterns can follow twitter accounts on the instance. 
If blacklisted, the given patterns can't follow twitter accounts on the instance.
If both whitelisting and blacklisting are set, only the whitelisting will be active.

### Twitter users

Here is the supported pattern to describe Twitter users:

* `twitter_handle` to describe a Twitter user

You can whitelist or blacklist twitter users by settings the followings variables with the above pattern separated by `;`: 

* `Moderation:TwitterAccountsWhiteListing` Twitter Whitelisting
* `Moderation:TwitterAccountsBlackListing` Twitter Blacklisting

If the whitelisting is set, only given patterns can be followed on the instance. 
If blacklisted, the given patterns can't be followed on the instance.
If both whitelisting and blacklisting are set, only the whitelisting will be active.

## Logging

* `Logging:Type` (default: none) set the type of the logging and monitoring system, currently the only type supported is `insights` for *Azure Application Insights* (PR welcome to support other types)
* `Logging:InstrumentationKey` the instrumentation key for Azure Application Insights

## Instance customization 

* `Instance:Name` (default: BirdsiteLIVE) the name of the instance
* `Instance:ResolveMentionsInProfiles` (default: true) to enable or disable mentions parsing in profile's description. Resolving it will consume more User's API calls since newly discovered account can also contain references to others accounts as well. On a big instance it is recommended to disable it.
* `Instance:PublishReplies` (default: false) to enable or disable replies publishing.
* `Instance:UnlistedTwitterAccounts` (default: null) to enable unlisted publication for selected twitter accounts, separated by `;` (please limit this to brands and other public profiles). 
* `Instance:SensitiveTwitterAccounts` (default: null) mark all media from given accounts as sensitive by default, separated by `;`. 
* `Instance:FailingTwitterUserCleanUpThreshold` (default: 700) set the max allowed errors (due to a banned/deleted/private account) from a Twitter Account retrieval before auto-removal. (by default an account is called every 15 mins)

# Docker Compose full example

In order to illustrate above variables, here is an example of an updated `docker-compose.yml` file:

```diff
version: "3"

networks:
    [...]

services:
    server:
        image: nicolasconstant/birdsitelive:latest
        [...]
        environment:
            - Instance:Domain=domain.name
            - Instance:AdminEmail=name@domain.ext
            - Db:Type=postgres
            - Db:Host=db
            - Db:Name=birdsitelive
            - Db:User=birdsitelive
            - Db:Password=birdsitelive
            - Twitter:ConsumerKey=twitter.api.key
            - Twitter:ConsumerSecret=twitter.api.key
+           - Moderation:FollowersWhiteListing=@me@my-instance.ca;friend-instance.com;*.friend-instance.com
+           - Moderation:TwitterAccountsBlackListing=douchebag;jerk_88;theRealIdiot
+           - Instance:Name=MyTwitterRelay
+           - Instance:ResolveMentionsInProfiles=false
+           - Instance:PublishReplies=true
+           - Instance:UnlistedTwitterAccounts=cocacola;twitter
+           - Instance:SensitiveTwitterAccounts=archillect
        networks:
        [...]

    db:
        image: postgres:9.6
        [...]
```

## Apply the modifications

After the modification of the `docker-compose.yml` file, you will need to run `docker-compose up -d` to apply the changes.
