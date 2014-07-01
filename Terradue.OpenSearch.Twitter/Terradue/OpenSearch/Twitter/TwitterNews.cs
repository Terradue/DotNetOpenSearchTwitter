using System;
using System.Collections.Generic;
using TweetSharp;
using Terradue.Portal;
using Terradue.OpenSearch;
using System.Web;
using System.Linq;
using Terradue.OpenSearch.Request;
using System.IO;
using Terradue.OpenSearch.Schema;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using Hammock.Serialization;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Engine;
using Terradue.ServiceModel.Syndication;
using Newtonsoft.Json;

namespace Terradue.OpenSearch.Twitter {
    public class TwitterNews : Article, IOpenSearchable {

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.TepQW.Controller.TwitterNews"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public TwitterNews(IfyContext context) : base(context){}

        /// <summary>
        /// Froms the identifier.
        /// </summary>
        /// <returns>The identifier.</returns>
        /// <param name="context">Context.</param>
        /// <param name="id">Identifier.</param>
        public new static TwitterNews FromId(IfyContext context, int id){
            return (TwitterNews)Article.FromId(context, id);
        }

        /// <summary>
        /// Updates the with online feeds.
        /// </summary>
        /// <param name="context">Context.</param>
        public List<TwitterNews> GetFeeds(){

            string consumerKey = context.GetConfigValue("Twitter-consumerKey");
            string consumerSecret = context.GetConfigValue("Twitter-consumerSecret");
            string token = context.GetConfigValue("Twitter-token");
            string tokenSecret = context.GetConfigValue("Twitter-tokenSecret");

            var service = new TwitterService(consumerKey, consumerSecret);
            service.Serializer = new ServiceStackJsonSerializer();
            service.Deserializer = new ServiceStackJsonSerializer();
            service.AuthenticateWith(token, tokenSecret);

            SearchOptions searchOptions = new SearchOptions();
            searchOptions.Q = " from:" + this.Author;
            searchOptions.Count = 20;

            var tweetResults = service.Search(searchOptions);

            List<TwitterNews> result = new List<TwitterNews>();

            foreach (TwitterStatus tweet in tweetResults.Statuses){
                TwitterNews feed = new TwitterNews(context);
                feed.Identifier = tweet.Id.ToString();
                feed.Author = tweet.User.ScreenName;
                feed.Title = tweet.User.Name;
                feed.Content = tweet.TextAsHtml;
                feed.Url = "http://twitter.com/"+tweet.Author+"/status/"+tweet.Id;
                feed.Time = tweet.CreatedDate;
                result.Add(feed);
            }
            return result;
        }

        void GenerateAtomFeed(Stream input, System.Collections.Specialized.NameValueCollection parameters) {

            string consumerKey = context.GetConfigValue("Twitter-consumerKey");
            string consumerSecret = context.GetConfigValue("Twitter-consumerSecret");
            string token = context.GetConfigValue("Twitter-token");
            string tokenSecret = context.GetConfigValue("Twitter-tokenSecret");

            AtomFeed feed = new AtomFeed();
            List<AtomItem> items = new List<AtomItem>();

            var service = new TwitterService(consumerKey, consumerSecret);
            //service.Serializer = new ServiceStackJsonSerializer();
            //service.Deserializer = new ServiceStackJsonSerializer();
            service.AuthenticateWith(token, tokenSecret);

            SearchOptions searchOptions = new SearchOptions();
            searchOptions.Count = Int32.Parse(parameters["count"] != null ? parameters["count"] : "20");
            searchOptions.Q = parameters["q"] + "from:" + this.Author + (this.Tags != null && this.Tags != string.Empty ? "," + this.Tags : "");

            TwitterSearchResult tweetResults;

            try{
                tweetResults= service.Search(searchOptions);
            }catch(Exception e){
                throw e;
            }

            if (tweetResults != null) {
                foreach (TwitterStatus tweet in tweetResults.Statuses) {
                    DateTimeOffset time;
                    if (tweet.CreatedDate.Ticks > 0)
                        time = new DateTimeOffset(tweet.CreatedDate);
                    else
                        time = new DateTimeOffset(DateTime.Now);
                    AtomItem item = new AtomItem(tweet.User.Name, new TextSyndicationContent(tweet.TextAsHtml, TextSyndicationContentKind.Html), new Uri("http://twitter.com/" + tweet.User.ScreenName + "/status/" + tweet.Id), tweet.Id.ToString(), time);
                    item.Categories.Add(new SyndicationCategory("twitter"));
                    item.PublishDate = time;
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
            
        public QuerySettings GetQuerySettings(OpenSearchEngine ose) {
            IOpenSearchEngineExtension osee = ose.GetExtensionByDiscoveryContentType(this.DefaultMimeType);
            if (osee == null)
                return null;
            return new QuerySettings(this.DefaultMimeType, osee.ReadNative);
        }


        public string DefaultMimeType {
            get {
                return "application/atom+xml";
            }
        }

        public OpenSearchRequest Create(string mimetype, System.Collections.Specialized.NameValueCollection parameters) {
            UriBuilder url = new UriBuilder(context.BaseUrl);
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

            urib = new UriBuilder(context.BaseUrl);
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
        public System.Collections.Specialized.NameValueCollection GetOpenSearchParameters(string mimeType) {

            NameValueCollection parameters = OpenSearchFactory.GetBaseOpenSearchParameter();
            parameters.Add("scn", "{twit:screename?}");
           
            return parameters;
        }

        public ulong TotalResults() {
            return 0;
        }

        public void ApplyResultFilters(ref IOpenSearchResult osr) {}

        public OpenSearchUrl GetSearchBaseUrl(string mimeType) {
            return new OpenSearchUrl (string.Format("{0}/{1}/search", context.BaseUrl, "twitter"));
        }
    }
}

