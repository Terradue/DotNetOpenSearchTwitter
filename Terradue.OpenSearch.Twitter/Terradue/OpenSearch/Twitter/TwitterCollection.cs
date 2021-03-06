﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using ServiceStack.Text;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;


namespace Terradue.OpenSearch.Twitter {

    public class TwitterCollection : IOpenSearchable {

        /// <summary>
        /// Gets or sets the accounts.
        /// </summary>
        /// <value>The accounts.</value>
        public List<TwitterAccount> Accounts { get; set; }

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
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Identifier { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Terradue.OpenSearch.Twitter.Terradue.OpenSearch.Twitter.TwitterCollection"/> class.
        /// </summary>
        /// <param name="baseurl">Baseurl.</param>
        public TwitterCollection(string baseurl) {
            this.BaseUrl = baseurl;
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Terradue.OpenSearch.Twitter.Terradue.OpenSearch.Twitter.TwitterCollection"/> class.
        /// </summary>
        /// <param name="app">App.</param>
        /// <param name="baseurl">Baseurl.</param>
        public TwitterCollection(TwitterApplication app, string baseurl) {
            this.Application = app;
            this.BaseUrl = baseurl;
        }

        public List<TwitterFeed> GetFeeds(NameValueCollection parameters){
            if (this.Accounts == null || this.Accounts.Count == 0) return null;

            List<TwitterFeed> result = new List<TwitterFeed>();

            var twitterClient = new TwitterClient(Application.ConsumerKey, Application.ConsumerSecretKey);

            //dealing with SEARCH type accounts
            if (parameters["searchtype"] == null || parameters["searchtype"]=="search") {
                var q = parameters["q"] != null ? "\"" + parameters["q"] + "\" AND " : "";
                var authors = parameters["author"] != null && parameters["author"] != "" ? parameters["author"].Split(',') : null;
                foreach (var account in Accounts) {
                    if (account.Author != null) {

                        //check if author is requested in search
                        var addAuthor = false;
                        if (authors != null) {
                            foreach (var author in authors)
                                if (author == account.Author) addAuthor = true;
                        } else addAuthor = true; //we add all if null

                        if (addAuthor) {
                            q += "from:" + account.Author;
                            if (account.Tags != null && account.Tags.Count > 0) {
                                foreach (var tag in account.Tags) {
                                    q += " AND #" + tag;
                                }
                            }
                            q += " OR ";
                        }
                    }
                }
                q = q.TrimEnd(" OR ".ToCharArray());
                q = q.TrimEnd(" AND ".ToCharArray());

                SearchOptions options = new SearchOptions();
                options.Q = q;
                options.Count = parameters["count"];

                TwitterSearchResult tweetResults = null;
                try {
                    tweetResults = twitterClient.Search(options);
                } catch (Exception e) {
                    var ie = e;
                }

                if (tweetResults != null) {
                    foreach (Status tweet in tweetResults.statuses) {
                        result.Add(new TwitterFeed(this.BaseUrl, tweet));
                    }
                }
            }

            //dealing with TIMELINE type accounts
            else if (parameters["searchtype"] != null && parameters["searchtype"] == "timeline") {
                var author = parameters["author"];
                if (!string.IsNullOrEmpty(author)) {
                    SearchOptions options = new SearchOptions();
                    options.Q = parameters["q"];
                    options.Count = parameters["count"] ?? "10";

                    try {
                        var timelineResults = twitterClient.GetUserTimeline(author, options);
                        foreach (Status tweet in timelineResults) {
                            result.Add(new TwitterFeed(this.BaseUrl, tweet));
                        }
                    } catch (Exception e) {
                        var ie = e;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generates the atom feed.
        /// </summary>
        /// <param name="parameters">Parameters of the query</param>
        AtomFeed GenerateAtomFeed(NameValueCollection parameters) {

            if (this.Accounts == null || this.Accounts.Count == 0) return null;

            AtomFeed feed = new AtomFeed();
            List<AtomItem> items = new List<AtomItem>();

            //parameters.Set("count","100");//to allow a bigger total result
            var feeds = GetFeeds(parameters);

            if (feeds != null) {
                foreach (TwitterFeed tweet in feeds) {
                    items.Add(tweet.ToAtomItem());
                }
            }

            int count = !string.IsNullOrEmpty(parameters["count"]) ? Int32.Parse(parameters["count"]) : 20;
            int startPage = !string.IsNullOrEmpty(parameters["startPage"]) ? Int32.Parse(parameters["startPage"]) : 1;
            int startIndex = !string.IsNullOrEmpty(parameters["startIndex"]) ? Int32.Parse(parameters["startIndex"]) : 1;

            try {
                feed.Items = items.Skip((startIndex - 1) + ((startPage - 1) * count)).Take(count);
            }catch(Exception){
                feed.Items = new List<AtomItem>();
            }
            feed.TotalResults = items.Count;

            return feed;
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
        /// Create the specified querySettings and parameters.
        /// </summary>
        /// <param name="querySettings">Query settings.</param>
        /// <param name="parameters">Parameters.</param>
        public OpenSearchRequest Create(QuerySettings querySettings, NameValueCollection parameters) {
            UriBuilder url = new UriBuilder(this.BaseUrl);
            url.Path += "twitter/" + this.Identifier + "/search";

            var array = (from key in parameters.AllKeys
                         from value in parameters.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

            AtomOpenSearchRequest request = new AtomOpenSearchRequest(new OpenSearchUrl(url.ToString()), GenerateAtomFeed);
            return request;
        }

        /// <summary>
        /// Get the entity's OpenSearchDescription.
        /// </summary>
        /// <returns>The OpenSearchDescription describing the IOpenSearchable.</returns>
        public OpenSearchDescription GetOpenSearchDescription() {
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

            OSDD.ExtraNamespace.Add("geo", "http://a9.com/-/opensearch/extensions/geo/1.0/");
            OSDD.ExtraNamespace.Add("time", "http://a9.com/-/opensearch/extensions/time/1.0/");
            OSDD.ExtraNamespace.Add("dct", "http://purl.org/dc/terms/");
            OSDD.ExtraNamespace.Add("t2", "http://www.terradue.com/opensearch");

            // The new URL template list 
            Hashtable newUrls = new Hashtable();
            UriBuilder urib;
            NameValueCollection query = new NameValueCollection();
            string[] queryString;

            urib = new UriBuilder(this.BaseUrl);
            urib.Path = String.Format("/{0}/search", this.Identifier);
            query.Add(this.GetOpenSearchParameters("application/atom+xml"));

            query.Set("format", "atom");
            queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urib.Query = string.Join("&", queryString);
            newUrls.Add("application/atom+xml", new OpenSearchDescriptionUrl("application/atom+xml", urib.ToString(), "search", OSDD.ExtraNamespace));

            query.Set("format", "json");
            queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urib.Query = string.Join("&", queryString);
            newUrls.Add("application/json", new OpenSearchDescriptionUrl("application/json", urib.ToString(), "search", OSDD.ExtraNamespace));

            query.Set("format", "html");
            queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urib.Query = string.Join("&", queryString);
            newUrls.Add("text/html", new OpenSearchDescriptionUrl("application/html", urib.ToString(), "search", OSDD.ExtraNamespace));

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
            parameters.Add("scn", "{t2:screename?}");
            parameters.Add("searchtype", "{t2:searchtype?}");
            parameters.Add("author", "{t2:author?}");

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
            return new OpenSearchUrl(string.Format("{0}/{1}/search", this.BaseUrl, "twitter"));
        }

        public bool CanCache {
            get {
                return true;
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType) {
        }
    }

    public class TwitterAccount {

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

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
    }
}
