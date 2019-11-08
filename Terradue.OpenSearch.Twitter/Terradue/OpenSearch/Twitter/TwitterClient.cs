using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using ServiceStack.Text;
using System.Runtime.Serialization;

namespace Terradue.OpenSearch.Twitter {
    public class TwitterClient {

        private string ConsumerKey;
        private string ConsumerKeySecret;
        private string ApiBseUrl;

        private string bearer;
        private string Bearer {
            get{
                if (bearer == null) bearer = GetBearerToken();
                return bearer;
            }
        }

        public TwitterClient(string oauth_consumer_key, string oauth_consumer_secret, string apibaseurl = null) {
            ConsumerKey = oauth_consumer_key;
            ConsumerKeySecret = oauth_consumer_secret;
            ApiBseUrl = apibaseurl ?? "https://api.twitter.com";
        }

        public string GetBearerToken() {

            //Token URL
            var oauth_url = ApiBseUrl + "/oauth2/token?grant_type=client_credentials";
            var authHeader = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(this.ConsumerKey + ":" + this.ConsumerKeySecret));
            var postBody = "grant_type=client_credentials";

            byte[] data = System.Text.Encoding.ASCII.GetBytes(postBody);

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(oauth_url);
            request.Headers.Add(HttpRequestHeader.Authorization, authHeader);
            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            // Add POST data
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            using (var httpResponse = (System.Net.HttpWebResponse)request.GetResponse()) {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    string result = streamReader.ReadToEnd();
                    try {
                        BearerResponse response = JsonSerializer.DeserializeFromString<BearerResponse>(result);
                        return response.access_token;
                    } catch (Exception e) {
                        throw e;
                    }
                }
            }
        }

        public TwitterSearchResult Search(SearchOptions options = null) {
            var url = ApiBseUrl + "/1.1/search/tweets.json";
            if (options != null) {
                var separator = "?";
                if (options.Q != null) {
                    url += separator + "q=" + options.Q.Replace(":","%3A").Replace(" ","%20");
                    separator = "&";
                }
                if (options.Count != null) {
                    url += separator + "count=" + options.Count;
                    separator = "&";
                }
            }
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            var authHeader = "Bearer " + Bearer;
            request.Headers.Add(HttpRequestHeader.Authorization, authHeader);
            request.Method = "GET";

            using (var httpResponse = (HttpWebResponse)request.GetResponse()) {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    string result = streamReader.ReadToEnd();
                    try {
                        TwitterSearchResult response = JsonSerializer.DeserializeFromString<TwitterSearchResult>(result);
                        return response;
                    } catch (Exception e) {
                        throw e;
                    }
                }
            }
        }

        public List<Status> GetUserTimeline(string screen_name, SearchOptions options = null) {
            var url = ApiBseUrl + "/1.1/statuses/user_timeline.json?screen_name=" + screen_name;
            if (options != null) {
                var separator = "&";
                if (options.Q != null) {
                    url += separator + "q=" + options.Q.Replace(":", "%3A").Replace(" ", "%20");
                }
                if (options.Count != null) {
                    url += separator + "count=" + options.Count;
                }
            }
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            var authHeader = "Bearer " + Bearer;
            request.Headers.Add(HttpRequestHeader.Authorization, authHeader);
            request.Method = "GET";

            using (var httpResponse = (HttpWebResponse)request.GetResponse()) {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    string result = streamReader.ReadToEnd();
                    try {
                        List<Status> response = JsonSerializer.DeserializeFromString<List<Status>>(result);
                        return response;
                    } catch (Exception e) {
                        throw e;
                    }
                }
            }
        }
    }

    public class SearchOptions {
        public string Q { get; set; }
        public string Count { get; set; }
    }

    [DataContract]
    public class Entities {
        [DataMember]
        public List<object> hashtags { get; set; }
        [DataMember]
        public List<object> symbols { get; set; }
        [DataMember]
        public List<object> user_mentions { get; set; }
        [DataMember]
        public List<object> urls { get; set; }
    }

    [DataContract]
    public class Metadata {
        [DataMember]
        public string iso_language_code { get; set; }
        [DataMember]
        public string result_type { get; set; }
    }

    [DataContract]
    public class Url2 {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string expanded_url { get; set; }
        [DataMember]
        public string display_url { get; set; }
        [DataMember]
        public List<int> indices { get; set; }
    }

    [DataContract]
    public class Url {
        [DataMember]
        public List<Url2> urls { get; set; }
    }

    [DataContract]
    public class Description {
        [DataMember]
        public List<object> urls { get; set; }
    }

    [DataContract]
    public class Entities2 {
        [DataMember]
        public Url url { get; set; }
        [DataMember]
        public Description description { get; set; }
    }

    [DataContract]
    public class User {
        [DataMember]
        public object id { get; set; }
        [DataMember]
        public string id_str { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string screen_name { get; set; }
        [DataMember]
        public string location { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public Entities2 entities { get; set; }
        [DataMember]
        public bool @protected { get; set; }
        [DataMember]
        public int followers_count { get; set; }
        [DataMember]
        public int friends_count { get; set; }
        [DataMember]
        public int listed_count { get; set; }
        [DataMember]
        public string created_at { get; set; }
        [DataMember]
        public int favourites_count { get; set; }
        [DataMember]
        public int utc_offset { get; set; }
        [DataMember]
        public string time_zone { get; set; }
        [DataMember]
        public bool geo_enabled { get; set; }
        [DataMember]
        public bool verified { get; set; }
        [DataMember]
        public int statuses_count { get; set; }
        [DataMember]
        public string lang { get; set; }
        [DataMember]
        public bool contributors_enabled { get; set; }
        [DataMember]
        public bool is_translator { get; set; }
        [DataMember]
        public bool is_translation_enabled { get; set; }
        [DataMember]
        public string profile_background_color { get; set; }
        [DataMember]
        public string profile_background_image_url { get; set; }
        [DataMember]
        public string profile_background_image_url_https { get; set; }
        [DataMember]
        public bool profile_background_tile { get; set; }
        [DataMember]
        public string profile_image_url { get; set; }
        [DataMember]
        public string profile_image_url_https { get; set; }
        [DataMember]
        public string profile_banner_url { get; set; }
        [DataMember]
        public string profile_link_color { get; set; }
        [DataMember]
        public string profile_sidebar_border_color { get; set; }
        [DataMember]
        public string profile_sidebar_fill_color { get; set; }
        [DataMember]
        public string profile_text_color { get; set; }
        [DataMember]
        public bool profile_use_background_image { get; set; }
        [DataMember]
        public bool has_extended_profile { get; set; }
        [DataMember]
        public bool default_profile { get; set; }
        [DataMember]
        public bool default_profile_image { get; set; }
        [DataMember]
        public object following { get; set; }
        [DataMember]
        public object follow_request_sent { get; set; }
        [DataMember]
        public object notifications { get; set; }
        [DataMember]
        public string translator_type { get; set; }
    }

    [DataContract]
    public class Url3 {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string expanded_url { get; set; }
        [DataMember]
        public string display_url { get; set; }
        [DataMember]
        public List<int> indices { get; set; }
    }

    [DataContract]
    public class Entities3 {
        [DataMember]
        public List<object> hashtags { get; set; }
        [DataMember]
        public List<object> symbols { get; set; }
        [DataMember]
        public List<object> user_mentions { get; set; }
        [DataMember]
        public List<Url3> urls { get; set; }
    }

    [DataContract]
    public class Metadata2 {
        [DataMember]
        public string iso_language_code { get; set; }
        [DataMember]
        public string result_type { get; set; }
    }

    [DataContract]
    public class Description2 {
        [DataMember]
        public List<object> urls { get; set; }
    }

    [DataContract]
    public class Url5 {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string expanded_url { get; set; }
        [DataMember]
        public string display_url { get; set; }
        [DataMember]
        public List<int> indices { get; set; }
    }

    [DataContract]
    public class Url4 {
        [DataMember]
        public List<Url5> urls { get; set; }
    }

    [DataContract]
    public class Entities4 {
        [DataMember]
        public Description2 description { get; set; }
        [DataMember]
        public Url4 url { get; set; }
    }

    [DataContract]
    public class User2 {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string id_str { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string screen_name { get; set; }
        [DataMember]
        public string location { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public Entities4 entities { get; set; }
        [DataMember]
        public bool @protected { get; set; }
        [DataMember]
        public int followers_count { get; set; }
        [DataMember]
        public int friends_count { get; set; }
        [DataMember]
        public int listed_count { get; set; }
        [DataMember]
        public string created_at { get; set; }
        [DataMember]
        public int favourites_count { get; set; }
        [DataMember]
        public int? utc_offset { get; set; }
        [DataMember]
        public string time_zone { get; set; }
        [DataMember]
        public bool geo_enabled { get; set; }
        [DataMember]
        public bool verified { get; set; }
        [DataMember]
        public int statuses_count { get; set; }
        [DataMember]
        public string lang { get; set; }
        [DataMember]
        public bool contributors_enabled { get; set; }
        [DataMember]
        public bool is_translator { get; set; }
        [DataMember]
        public bool is_translation_enabled { get; set; }
        [DataMember]
        public string profile_background_color { get; set; }
        [DataMember]
        public string profile_background_image_url { get; set; }
        [DataMember]
        public string profile_background_image_url_https { get; set; }
        [DataMember]
        public bool profile_background_tile { get; set; }
        [DataMember]
        public string profile_image_url { get; set; }
        [DataMember]
        public string profile_image_url_https { get; set; }
        [DataMember]
        public string profile_link_color { get; set; }
        [DataMember]
        public string profile_sidebar_border_color { get; set; }
        [DataMember]
        public string profile_sidebar_fill_color { get; set; }
        [DataMember]
        public string profile_text_color { get; set; }
        [DataMember]
        public bool profile_use_background_image { get; set; }
        [DataMember]
        public bool has_extended_profile { get; set; }
        [DataMember]
        public bool default_profile { get; set; }
        [DataMember]
        public bool default_profile_image { get; set; }
        [DataMember]
        public object following { get; set; }
        [DataMember]
        public object follow_request_sent { get; set; }
        [DataMember]
        public object notifications { get; set; }
        [DataMember]
        public string translator_type { get; set; }
        [DataMember]
        public string profile_banner_url { get; set; }
    }

    [DataContract]
    public class RetweetedStatus {
        [DataMember]
        public string created_at { get; set; }
        [DataMember]
        public object id { get; set; }
        [DataMember]
        public string id_str { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public bool truncated { get; set; }
        [DataMember]
        public Entities3 entities { get; set; }
        [DataMember]
        public Metadata2 metadata { get; set; }
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public object in_reply_to_status_id { get; set; }
        [DataMember]
        public object in_reply_to_status_id_str { get; set; }
        [DataMember]
        public object in_reply_to_user_id { get; set; }
        [DataMember]
        public object in_reply_to_user_id_str { get; set; }
        [DataMember]
        public object in_reply_to_screen_name { get; set; }
        [DataMember]
        public User2 user { get; set; }
        [DataMember]
        public object geo { get; set; }
        [DataMember]
        public object coordinates { get; set; }
        [DataMember]
        public object place { get; set; }
        [DataMember]
        public object contributors { get; set; }
        [DataMember]
        public bool is_quote_status { get; set; }
        [DataMember]
        public int retweet_count { get; set; }
        [DataMember]
        public int favorite_count { get; set; }
        [DataMember]
        public bool favorited { get; set; }
        [DataMember]
        public bool retweeted { get; set; }
        [DataMember]
        public bool possibly_sensitive { get; set; }
        [DataMember]
        public string lang { get; set; }
    }

    [DataContract]
    public class Status {
        [DataMember]
        public string created_at { get; set; }
        [DataMember]
        public long id { get; set; }
        [DataMember]
        public string id_str { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public bool truncated { get; set; }
        [DataMember]
        public Entities entities { get; set; }
        [DataMember]
        public Metadata metadata { get; set; }
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public object in_reply_to_status_id { get; set; }
        [DataMember]
        public object in_reply_to_status_id_str { get; set; }
        [DataMember]
        public object in_reply_to_user_id { get; set; }
        [DataMember]
        public object in_reply_to_user_id_str { get; set; }
        [DataMember]
        public object in_reply_to_screen_name { get; set; }
        [DataMember]
        public User user { get; set; }
        [DataMember]
        public object geo { get; set; }
        [DataMember]
        public object coordinates { get; set; }
        [DataMember]
        public object place { get; set; }
        [DataMember]
        public object contributors { get; set; }
        [DataMember]
        public RetweetedStatus retweeted_status { get; set; }
        [DataMember]
        public bool is_quote_status { get; set; }
        [DataMember]
        public int retweet_count { get; set; }
        [DataMember]
        public int favorite_count { get; set; }
        [DataMember]
        public bool favorited { get; set; }
        [DataMember]
        public bool retweeted { get; set; }
        [DataMember]
        public string lang { get; set; }
        [DataMember]
        public bool? possibly_sensitive { get; set; }
    }

    [DataContract]
    public class SearchMetadata {
        [DataMember]
        public double completed_in { get; set; }
        [DataMember]
        public long max_id { get; set; }
        [DataMember]
        public string max_id_str { get; set; }
        [DataMember]
        public string next_results { get; set; }
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public string refresh_url { get; set; }
        [DataMember]
        public int count { get; set; }
        [DataMember]
        public int since_id { get; set; }
        [DataMember]
        public string since_id_str { get; set; }
    }

    [DataContract]
    public class TwitterSearchResult {
        [DataMember]
        public List<Status> statuses { get; set; }
        [DataMember]
        public SearchMetadata search_metadata { get; set; }
    }

    [DataContract]
    public class BearerResponse {
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string access_token { get; set; }
    }
}
