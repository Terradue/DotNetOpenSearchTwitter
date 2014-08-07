//
//  ServiceStackJsonSerializer.cs
//
//  Author:
//       Enguerran Boissier <enguerran.boissier@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using ServiceStack;
using ServiceStack.Text;
using TweetSharp;
using Hammock.Serialization;
using System.Globalization;

namespace Terradue.OpenSearch.Twitter
{
    public class ServiceStackJsonSerializer : ISerializer, IDeserializer
    {
        public ServiceStackJsonSerializer() {
            JsConfig<TwitterStatus>.RawDeserializeFn = text =>
            {
                TwitterStatus status = new TwitterStatus();

                ServiceStack.Text.JsonObject obj = ServiceStack.Text.JsonObject.Parse(text);
                string date  = obj.Get<string>("created_at");
                status.CreatedDate = DateTime.ParseExact(date,"ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);
                status.Entities = obj.Get<TwitterEntities>("entities");
                status.Id = obj.Get<long>("id");
                status.IdStr = obj.Get<string>("id_str");
                status.InReplyToScreenName = obj.Get<string>("in_reply_to_status_id");
                status.InReplyToStatusId = obj.Get<long?>("in_reply_to_status_id");
                status.InReplyToUserId = obj.Get<int?>("in_reply_to_user_id");
                status.IsFavorited = obj.Get<bool>("favorited");
                status.IsPossiblySensitive = obj.Get<bool?>("possibly_sensitive");
                status.IsTruncated = obj.Get<bool>("truncated");
                status.Language = obj.Get<string>("lang");
                status.Location = obj.Get<TwitterGeoLocation>("geo");
                status.Place = obj.Get<TwitterPlace>("place");
                status.RawSource = obj.Get<string>("raw_source");
                status.RetweetCount = obj.Get<int>("retweet_count");
                status.RetweetedStatus = obj.Get<TwitterStatus>("retweeted_status");
                status.Source = obj.Get<string>("source");
                status.Text = obj.Get<string>("text");
                status.User = obj.Get<TwitterUser>("user");

                return status;
            };

            JsConfig<TwitterUser>.RawDeserializeFn = text =>
            {
                TwitterUser user = new TwitterUser();

                ServiceStack.Text.JsonObject obj = ServiceStack.Text.JsonObject.Parse(text);
                user.Name = obj.Get<string>("name");
                user.ScreenName = obj.Get<string>("screen_name");
                user.ProfileImageUrl = obj.Get<string>("profile_image_url");

                return user;
            };
        }

        #region ISerializer implementation
        public string Serialize(object instance, Type type) {
            return JsonSerializer.SerializeToString(instance, type);
        }
        public string ContentType {
            get {
                return "application/json";
            }
        }
        public System.Text.Encoding ContentEncoding {
            get {
                return System.Text.Encoding.UTF8;
            }
        }
        #endregion

        #region IDeserializer implementation

        public object Deserialize(Hammock.RestResponseBase response, Type type) {
            Console.Write(response.Content);
            return JsonSerializer.DeserializeFromString(response.Content,type);
        }

        public T Deserialize<T>(Hammock.RestResponseBase response) {
            Console.Write(response.Content);
            return JsonSerializer.DeserializeFromString<T>(response.Content);
        }

        public dynamic DeserializeDynamic(Hammock.RestResponseBase response) {
            return JsonSerializer.DeserializeFromString<object>(response.Content);
        }

        #endregion
    }
}