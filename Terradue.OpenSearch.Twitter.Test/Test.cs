using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch.Twitter.Test {
    [TestFixture()]
    public class Test {

        private string TwitterConsumerKey = "KKZAdNUnhNWkJrFQ47dRHyfzG";
        private string TwitterConsumerSecret = "GOqrWTDElrESHDdAWbpg9NDUDyyvpKjVq0VHuRvdyBvN9VBKK9";
        private string TwitterToken = " 1196754241-WreYmYr3dO0EGtJ8tto3gx6oOuItloc0GjyuBZk";
        private string TwitterTokenSecret = "YQkWMaxfQB0ged6fe5uO2LF37Bli4iVdkea94ejX0OqFD";

        private List<string> TwitterAccounts = new List<string>{ "ESA_EO", "terradue" };
        private List<string> TwitterTags = new List<string> { "EGU17" };

        private TwitterApplication App;
        private OpenSearchEngine ose;

        [TestFixtureSetUp]
        public void FixtureSetup() {
            App = new TwitterApplication(TwitterConsumerKey, TwitterConsumerSecret, TwitterToken, TwitterTokenSecret); 

            if (ose == null) {
                ose = new OpenSearchEngine();
                var aosee = new Engine.Extensions.AtomOpenSearchEngineExtension();
                ose.RegisterExtension(new Engine.Extensions.AtomOpenSearchEngineExtension());
            }
        }

        [Test()]
        public void GetAllFeeds() {

            var collection = new TwitterCollection(App, "http://localhost");
            collection.Identifier = "tweet";
            collection.Accounts = new List<TwitterAccount>();
            var feeds = collection.GetFeeds(new NameValueCollection());
            Assert.IsNull(feeds);

            foreach (var account in TwitterAccounts) {
                collection.Accounts.Add(new TwitterAccount { Title = null, Author = account, Tags = TwitterTags });
            }
            feeds = collection.GetFeeds(new NameValueCollection());
            Assert.IsNotNull(feeds);
        }

        [Test()]
        public void SearchFeedsWithTags(){

            var parameters = new NameValueCollection();

            var collection = new TwitterCollection(App, "http://localhost");
            collection.Identifier = "tweet";
            collection.Accounts = new List<TwitterAccount>();

            foreach (var account in TwitterAccounts) {
                collection.Accounts.Add(new TwitterAccount { Title = null, Author = account, Tags = TwitterTags });
            }

            var osr = ose.Query(collection, parameters);
            Assert.AreNotEqual(0, osr.TotalResults);
        }

        [Test()]
        public void SearchFeeds() {
            
            var parameters = new NameValueCollection();

            var collection = new TwitterCollection(App, "http://localhost");
            collection.Identifier = "tweet";
            collection.Accounts = new List<TwitterAccount>();

            foreach (var account in TwitterAccounts) {
                collection.Accounts.Add(new TwitterAccount { Title = null, Author = account, Tags = null });
            }

            parameters.Set("count","5");
            var osr = ose.Query(collection, parameters);
            Assert.AreEqual(5, osr.TotalResults);
        }


        [Test()]
        public void SearchFeedsSpecial() {

            var twitterClient = new TwitterClient(App.ConsumerKey, App.ConsumerSecretKey);
            var parameters = new NameValueCollection();

            var collection = new TwitterCollection(App, "http://localhost");
            collection.Identifier = "tweet";
            collection.Accounts = new List<TwitterAccount>();

            collection.Accounts.Add(new TwitterAccount { Title = null, Author = "AschbacherJosef", Tags = null });

            parameters.Set("q", "Today we signed the 14th ESA @ESA_EO collaborative ground segment agreement, with Poland");
            var osr = ose.Query(collection, parameters);
            Assert.AreEqual(1, osr.TotalResults);
        }


    }
}

