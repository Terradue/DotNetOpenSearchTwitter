//
//  TwitterFeed.cs
//
//  Author:
//       Enguerran Boissier <enguerran.boissier@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Hammock.Serialization;
using Newtonsoft.Json;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;
using Terradue.ServiceModel.Syndication;
using TweetSharp;
using System.Net;

namespace Terradue.OpenSearch.Twitter {

    public class TwitterApplication {

        /// <summary>
        /// Gets or sets the Twitter consumer key.
        /// </summary>
        /// <value>The consumer key.</value>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// Gets or sets the consumer secret key.
        /// </summary>
        /// <value>The consumer secret key.</value>
        public string ConsumerSecretKey { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>The token.</value>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the Bearer.
        /// </summary>
        /// <value>The Bearer.</value>
        public string Bearer { get; set; }

        /// <summary>
        /// Gets or sets the secret token.
        /// </summary>
        /// <value>The secret token.</value>
        public string SecretToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Twitter.TwitterApplication"/> class.
        /// </summary>
        /// <param name="consumerKey">Twitter Application Consumer key.</param>
        /// <param name="consumerSecret">Twitter Application Consumer secret.</param>
        /// <param name="token">Twitter Application Token.</param>
        /// <param name="secretToken">Twitter Application Secret token.</param>
        public TwitterApplication(string consumerKey, string consumerSecret, string token, string secretToken){
            this.ConsumerKey = consumerKey;
            this.ConsumerSecretKey = consumerSecret;
            this.Token = token;
            this.SecretToken = secretToken;
        }

    }

    public class TwitterFeed {

        /// <summary>
        /// Get the local id.
        /// </summary>
        /// <value>The local id of the OpenSearchable entity.</value>
        public string Id { get; set; }

        /// <summary>
        /// Get the local identifier.
        /// </summary>
        /// <value>The local identifier of the OpenSearchable entity.</value>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>The time.</value>
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>The author.</value>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the author image URL.
        /// </summary>
        /// <value>The author image url.</value>
        public string AuthorImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>The application containing all keys.</value>
        protected TwitterApplication Application { get; set; }

        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        protected string BaseUrl { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Twitter.TwitterFeed"/> class.
        /// </summary>
        /// <param name="baseurl">Baseurl.</param>
        public TwitterFeed(string baseurl){
            this.BaseUrl = baseurl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Twitter.TwitterFeed"/> class.
        /// </summary>
        /// <param name="app">App.</param>
        public TwitterFeed(TwitterApplication app, string baseurl){
            this.Application = app;
            this.BaseUrl = baseurl;
        }

        /// <summary>
        /// Updates the with online feeds.
        /// </summary>
        /// <param name="context">Context.</param>
        public List<TwitterFeed> GetFeeds(){

            if(this.Author == null && this.Tags == null) return null;

            var service = new TwitterService(Application.ConsumerKey, Application.ConsumerSecretKey);
            service.AuthenticateWith(Application.Token, Application.SecretToken);

            SearchOptions searchOptions = new SearchOptions();
            searchOptions.Count = 20;
            searchOptions.Q = "";

            if (this.Author != null) searchOptions.Q = "from:" + this.Author;
            if (this.Tags != null) searchOptions.Q += " #" + string.Join(" OR #",this.Tags);

            System.Net.ServicePointManager.Expect100Continue = false;

            TwitterSearchResult tweetResults = null;
            try{
                tweetResults = service.Search(searchOptions);
            }catch(Exception){
            }
            if (tweetResults == null) return new List<TwitterFeed>();

            List<TwitterFeed> result = new List<TwitterFeed>();

            foreach (TwitterStatus tweet in tweetResults.Statuses){
                TwitterFeed feed = new TwitterFeed(this.BaseUrl);
                feed.Identifier = tweet.Id.ToString();
                feed.Author = tweet.User.ScreenName;
                feed.AuthorImageUrl = tweet.User.ProfileImageUrlHttps;
                feed.Title = tweet.User.Name;
                feed.Content = tweet.TextAsHtml;
                feed.Url = "http://twitter.com/" + tweet.Author + "/status/" + tweet.Id;
                feed.Time = tweet.CreatedDate;
                result.Add(feed);
            }
            return result;
        }
    }

}

