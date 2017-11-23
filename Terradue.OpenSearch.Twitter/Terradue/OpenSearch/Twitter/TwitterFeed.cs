//
//  TwitterFeed.cs
//
//  Author:
//       Enguerran Boissier <enguerran.boissier@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Generic;
using System.Linq;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;

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
        public TwitterFeed(TwitterApplication app, string baseurl) : this(baseurl){
            this.Application = app;
        }

        public TwitterFeed(string baseurl, Status tweet) : this(baseurl){
            string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            var date = DateTime.ParseExact(tweet.created_at, format, System.Globalization.CultureInfo.InvariantCulture);

            this.Identifier = tweet.id_str;
            this.Author = tweet.user.screen_name;
            this.AuthorImageUrl = tweet.user.profile_image_url_https;
            this.Title = tweet.user.name;
            this.Content = tweet.text;
            this.Url = "http://twitter.com/" + tweet.user.screen_name + "/status/" + tweet.id_str;
            this.Time = date;
        }

        /// <summary>
        /// To atom item.
        /// </summary>
        /// <returns>The atom item.</returns>
        public AtomItem ToAtomItem(){
            AtomItem item = new AtomItem(this.Title, new TextSyndicationContent(this.Content), new Uri("http://twitter.com/" + this.Author + "/status/" + this.Identifier), this.Identifier, this.Time);
            item.Categories.Add(new SyndicationCategory("twitter"));
            if (this.Time.Ticks > 0)
                item.PublishDate = this.Time;
            else
                item.PublishDate = DateTime.Now;
            item.Authors.Add(new SyndicationPerson(this.Title, this.Author, this.AuthorImageUrl));
            item.LastUpdatedTime = this.Time;
            return item;
        }

    }

}

