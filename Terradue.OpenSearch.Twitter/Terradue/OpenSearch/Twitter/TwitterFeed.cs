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

    public class TwitterFeed : IOpenSearchable {

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
                feed.Title = tweet.User.Name;
                feed.Content = tweet.TextAsHtml;
                feed.Url = "http://twitter.com/" + tweet.Author + "/status/" + tweet.Id;
                feed.Time = tweet.CreatedDate;
                result.Add(feed);
            }
            return result;
        }

        /// <summary>
        /// Generates the atom feed.
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="parameters">Parameters of the query</param>
        void GenerateAtomFeed(Stream input, System.Collections.Specialized.NameValueCollection parameters) {

            if(this.Author == null && this.Tags == null) return;

            AtomFeed feed = new AtomFeed();
            List<AtomItem> items = new List<AtomItem>();

            var service = new TwitterService(Application.ConsumerKey, Application.ConsumerSecretKey);
            service.AuthenticateWith(Application.Token, Application.SecretToken);

            SearchOptions searchOptions = new SearchOptions();
            searchOptions.Count = Int32.Parse(parameters["count"] != null ? parameters["count"] : "20");
            searchOptions.Q = "";

            if (this.Author != null) searchOptions.Q += "from:" + this.Author;
            if (this.Tags != null) searchOptions.Q += " #" + string.Join(", #",this.Tags);

            TwitterSearchResult tweetResults = null;

            try{
                tweetResults= service.Search(searchOptions);
            }catch(Exception){
            }

            if (tweetResults != null) {
                foreach (TwitterStatus tweet in tweetResults.Statuses) {
                    
                    AtomItem item = new AtomItem(tweet.User.Name, new TextSyndicationContent(tweet.TextAsHtml, TextSyndicationContentKind.Html), new Uri("http://twitter.com/" + tweet.User.ScreenName + "/status/" + tweet.Id), tweet.Id.ToString(), tweet.CreatedDate);
                    item.Categories.Add(new SyndicationCategory("twitter"));
                    if (tweet.CreatedDate.Ticks > 0)
                        item.PublishDate = tweet.CreatedDate;
                    else
                        item.PublishDate = DateTime.Now;
                    item.Authors.Add(new SyndicationPerson(tweet.User.Name,tweet.User.ScreenName,tweet.User.ProfileImageUrl));
                    items.Add(item);
                }
            }

            feed.Items = items;

            var sw = XmlWriter.Create(input);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed.Feed);
            atomFormatter.WriteTo(sw);
            sw.Flush();
            sw.Close();

            return;

        }
         
        /// <summary>
        /// Gets the query settings.
        /// </summary>
        /// <returns>The query settings.</returns>
        /// <param name="ose">Ose.</param>
        public QuerySettings GetQuerySettings(OpenSearchEngine ose) {
            IOpenSearchEngineExtension osee = ose.GetExtensionByContentTypeAbility(this.DefaultMimeType);
            if (osee == null)
                return null;
            return new QuerySettings(this.DefaultMimeType, osee.ReadNative);
        }

        /// <summary>
        /// Gets the default MIME-type that the entity can be searched for
        /// </summary>
        /// <value>The default MIME-type.</value>
        public string DefaultMimeType {
            get {
                return "application/atom+xml";
            }
        }

        /// <summary>
        /// Create the OpenSearch Request for the requested mime-type the specified type and parameters.
        /// </summary>
        /// <param name="mimetype">Mime-Type requested to the OpenSearchable entity</param>
        /// <param name="parameters">Parameters of the request</param>
        public OpenSearchRequest Create(string mimetype, System.Collections.Specialized.NameValueCollection parameters) {
            UriBuilder url = new UriBuilder(this.BaseUrl);
            url.Path += "twitter/"+this.Identifier+"/search";
            var array = (from key in parameters.AllKeys
                         from value in parameters.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

            MemoryOpenSearchRequest request = new MemoryOpenSearchRequest(new OpenSearchUrl(url.ToString()), mimetype);

            Stream input = request.MemoryInputStream;

            GenerateAtomFeed(input, parameters);

            return request;
        }

        /// <summary>
        /// Get the entity's OpenSearchDescription.
        /// </summary>
        /// <returns>The OpenSearchDescription describing the IOpenSearchable.</returns>
        public Terradue.OpenSearch.Schema.OpenSearchDescription GetOpenSearchDescription() {
            OpenSearchDescription OSDD = new OpenSearchDescription();

            OSDD.ShortName = "Terradue Catalogue";
            OSDD.Attribution = "European Space Agency";
            OSDD.Contact = "info@esa.int";
            OSDD.Developer = "Terradue GeoSpatial Development Team";
            OSDD.SyndicationRight = "open";
            OSDD.AdultContent = "false";
            OSDD.Language = "en-us";
            OSDD.OutputEncoding = "UTF-8";
            OSDD.InputEncoding = "UTF-8";
            OSDD.Description = "This Search Service performs queries in the available services of Tep QuickWin. There are several URL templates that return the results in different formats (RDF, ATOM or KML). This search service is in accordance with the OGC 10-032r3 specification.";

            // The new URL template list 
            Hashtable newUrls = new Hashtable();
            UriBuilder urib;
            NameValueCollection query = new NameValueCollection();
            string[] queryString;

            urib = new UriBuilder(this.BaseUrl);
            urib.Path = String.Format("/{0}/search",this.Identifier);
            query.Add(this.GetOpenSearchParameters("application/atom+xml"));

            query.Set("format", "atom");
            queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urib.Query = string.Join("&", queryString);
            newUrls.Add("application/atom+xml", new OpenSearchDescriptionUrl("application/atom+xml", urib.ToString(), "search"));

            query.Set("format", "json");
            queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urib.Query = string.Join("&", queryString);
            newUrls.Add("application/json", new OpenSearchDescriptionUrl("application/json", urib.ToString(), "search"));

            query.Set("format", "html");
            queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urib.Query = string.Join("&", queryString);
            newUrls.Add("text/html", new OpenSearchDescriptionUrl("application/html", urib.ToString(), "search"));

            OSDD.Url = new OpenSearchDescriptionUrl[newUrls.Count];

            newUrls.Values.CopyTo(OSDD.Url, 0);

            return OSDD;

        }

        /// <summary>
        /// Gets the OpenSearch parameters for a given Mime-Type.
        /// </summary>
        /// <returns>OpenSearch parameters NameValueCollection.</returns>
        /// <param name="mimeType">MIME type for the requested parameters</param>
        public System.Collections.Specialized.NameValueCollection GetOpenSearchParameters(string mimeType) {

            NameValueCollection parameters = OpenSearchFactory.GetBaseOpenSearchParameter();
            parameters.Add("scn", "{twit:screename?}");
           
            return parameters;
        }

        /// <summary>
        /// Get the total of possible results for the OpenSearchable entity
        /// </summary>
        /// <returns>a unsigned long number representing the number of items searchable</returns>
        public long TotalResults {
            get {
                return 0;
            }
        }

        /// <summary>
        /// Gets the search base URL.
        /// </summary>
        /// <returns>The search base URL.</returns>
        /// <param name="mimeType">MIME type.</param>
        public OpenSearchUrl GetSearchBaseUrl(string mimeType) {
            return new OpenSearchUrl (string.Format("{0}/{1}/search", this.BaseUrl, "twitter"));
        }

        public bool CanCache {
            get {
                return true;
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType) {
        }
    }

}

