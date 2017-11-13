using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;
using Terradue.ServiceModel.Syndication;
using TweetSharp;

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

        /// <summary>
        /// Generates the atom feed.
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="parameters">Parameters of the query</param>
        void GenerateAtomFeed(Stream input, System.Collections.Specialized.NameValueCollection parameters) {

            if (this.Accounts == null || this.Accounts.Count == 0) return;

            AtomFeed feed = new AtomFeed();
            List<AtomItem> items = new List<AtomItem>();

            var service = new TwitterService(Application.ConsumerKey, Application.ConsumerSecretKey);
            service.AuthenticateWith(Application.Token, Application.SecretToken);

            SearchOptions searchOptions = new SearchOptions();
            //searchOptions.Count = 20;

            var q = "";
            foreach(var account in Accounts){
                if (account.Author != null) {
                    q += "(from: " + account.Author;
                    if (account.Tags != null && account.Tags.Count > 0){
                        foreach(var tag in account.Tags){
                            q += " AND #" + tag;
                        }
                    }
                    q += ")";
                }
                q += " OR ";
            }
            q = q.TrimEnd(" OR ".ToCharArray());
            searchOptions.Q = q;

            TwitterSearchResult tweetResults = null;
            try{
                tweetResults = service.Search(searchOptions);
            }catch(Exception e){
                var ie = e;
            }

            if (tweetResults != null) {
                foreach (TwitterStatus tweet in tweetResults.Statuses) {

                    AtomItem item = new AtomItem(tweet.User.Name, new TextSyndicationContent(tweet.TextAsHtml, TextSyndicationContentKind.Html), new Uri("http://twitter.com/" + tweet.User.ScreenName + "/status/" + tweet.Id), tweet.Id.ToString(), tweet.CreatedDate);
                    item.Categories.Add(new SyndicationCategory("twitter"));
                    if (tweet.CreatedDate.Ticks > 0)
                        item.PublishDate = tweet.CreatedDate;
                    else
                        item.PublishDate = DateTime.Now;
                    item.Authors.Add(new SyndicationPerson(tweet.User.Name, tweet.User.ScreenName, tweet.User.ProfileImageUrlHttps));
                    items.Add(item);
                }
            }

            feed.Items = items;
            feed.TotalResults = items.Count;

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

            MemoryOpenSearchRequest request = new MemoryOpenSearchRequest(new OpenSearchUrl(url.ToString()), querySettings.PreferredContentType);

            Stream input = request.MemoryInputStream;

            GenerateAtomFeed(input, parameters);

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
