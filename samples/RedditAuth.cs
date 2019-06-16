// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var redditAuth = RedditAuth.FromJson(jsonString);

namespace QuickTypeAuth
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class RedditAuth
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }
    }

    public partial class RedditAuth
    {
        public static RedditAuth FromJson(string json) => JsonConvert.DeserializeObject<RedditAuth>(json, QuickTypeAuth.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this RedditAuth self) => JsonConvert.SerializeObject(self, QuickTypeAuth.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
