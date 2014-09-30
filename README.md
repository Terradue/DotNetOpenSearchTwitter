# DotNetOpenSearchTwitter - .Net Library to Access Twitter API via OpenSearch

DotNetOpenSearchTwitter is a library targeting .NET 4.0 and above providing an easy way to perform opensearch requests on the Twitter API

## Usage examples

```c#
using Terradue.OpenSearch; //from DotNetOpenSearch
using Terradue.OpenSearch.Engine; //from DotNetOpenSearch
using Terradue.OpenSearch.Result; //from DotNetOpenSearch
using Terradue.OpenSearch.Twitter; //from DotNetOpenSearchTwitter

HttpRequest httpRequest = HttpContext.Current.Request;
OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;
Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);

//Create Twitter Application
string twitterConsumerKey = "<YOUR_CONSUMER_KEY>";
string twitterConsumerSecret = "<YOUR_CONSUMER_SECRET>";
string twitterToken = "<YOUR_TOKEN>";
string twitterTokenSecret = "<YOUR_TOKEN_SECRET>";
TwitterApplication twitterApp = new TwitterApplication(twitterConsumerKey, 
                                                       twitterConsumerSecret, 
                                                       twitterToken, 
                                                       twitterTokenSecret);

//Create Twitter Feed content
string myBaseUrl = "<MY_APP_BASEURL>"; //will be use in the opensearch response
TwitterFeed twitter1 = new TwitterFeed(twitterApp, myBaseUrl);
twitter1.Author = "<AUTHOR_TO_SEARCH>";
twitter1.Tags = new List<string>{ "<TAG_1>", "<TAG_2>" };

List<TwitterFeed> twitters = new List<TwitterFeed>();
twitters.Add(twitter1);

MultiGenericOpenSearchable multiOSE = new MultiGenericOpenSearchable(twitters.Cast<IOpenSearchable>().ToList(), ose, false);
result = ose.Query(multiOSE, httpRequest.QueryString, type);
```

## Supported Platforms

* .NET 4.0 (Desktop / Server)
* Xamarin.iOS / Xamarin.Android / Xamarin.Mac
* Mono 2.10+

## Build

DotNetOpenSearchTwitter is a single assembly designed to be easily deployed anywhere. 

To compile it yourself, you’ll need:

* Visual Studio 2012 or later, or Xamarin Studio

To clone it locally click the "Clone in Desktop" button above or run the 
following git commands.

```
git clone git@github.com:Terradue/Terradue.OpenSearch.Twitter.git Terradue.OpenSearch.Twitter
```

## Copyright and License

Copyright (c) 2014 Terradue

Licensed under the [GPL v3 License](https://github.com/Terradue/DotNetOpenSearchTwitter/blob/master/LICENSE)

## Questions, bugs, and suggestions

Please file any bugs or questions as [issues](https://github.com/Terradue/DotNetOpenSearchTwitter/issues/new) 

## Want to contribute?

Fork the repository [here](https://github.com/Terradue/DotNetOpenSearchTwitter/fork) and send us pull requests.
