using System;
using System.Text;
using System.Collections.Generic;
using RestSharp;
using QuickTypeAuth;
using QuickTypeArticles;
using QuickTypeComments;
using QuickTypeEntitiesData;
using QuickTypeEntitiesResult;
using QuickTypeSemtimentData;
using QuickTypeSemtimentResult;
using QuickTypeKeyPhrasesData;
using QuickTypeKeyPhrasesResult;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;

namespace RedditSentiment
{
    class Program
    {
        static GremlinServer gremlinServer;
        static HashSet<DBEntity> dbEnt = new HashSet<DBEntity>(new DBEntityComp());
        static int entityId = 0;
        static void Main(string[] args)
        {
            string RedditLogin = ""; // Reddit user login
            string RedditPWD = ""; // Reddit user password
            string RedditAppID = ""; // Reddit App ID
            string RedditAppSecret = ""; // Reddit App secret
            string redditAccessToken = "";  // Store key here if you going to build DB a few times it is good for 120min
            string AzureSentAppKey = "[YourSetimentAppKey]";
            gremlinServer = new GremlinServer("[YourCosmoDB].gremlin.cosmos.azure.com", 443, enableSsl: true,
            username: $"/dbs/[database id]/colls/[Graph id]",
            password: "[YourGraphDBPrimaryKey]");
            // Make sure the graph DB is empty to start
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                    var result = AzureAsync(gremlinClient, "g.V().drop()");  // Clear DB
                    Console.WriteLine("\n{{\"Returned\": \"{0}\"}}", result.Result.Count);
            }

            if (redditAccessToken=="")  
                redditAccessToken = AuthToken(RedditLogin, RedditPWD, RedditAppID, RedditAppSecret); // Get Reddit access token
            RedditArticles myArt = GetArticles(redditAccessToken);  // Get Reddit articles
            EntitiesData EntitiesData = CreateEntitiesData(myArt);  // Create sentiment entity data structure for entities
            EntitiesResult ArtEntities = GetEntities(AzureSentAppKey, EntitiesData); // Get entities from Azure API
            SemtimentData SentimentData = CreateSentimentData(myArt);  // Create sentiment data structure
            SemtimentResult ArtSentiment = GetSentiment(AzureSentAppKey, SentimentData); // Get sentiment for article
            SaveArtResults(SentimentData,ArtSentiment,ArtEntities);  // Save results in Cosmos DB

/* 
            foreach(QuickTypeEntitiesResult.Entity artEnt in ArtEntities.Documents[0].Entities)
            {
                Console.WriteLine("e:" + artEnt.Name);
            }
            System.Threading.Thread.Sleep(1000);
            ParseComments(redditAccessToken, articl.Data.Name.Substring(3));
*/
        }

        static void ParseComments(string _token, string _ArtID)
        {
            RedditComments[] myComments = GetComments(_token, _ArtID);
            foreach(RedditComments comm in myComments)
            {
                foreach(PurpleChild subcomm in comm.Data.Children)
                if (subcomm.Kind == "t1")
                {
                    Console.WriteLine("+++" + subcomm.Data.Body);
                }
            }
        }

        static void SaveArtResults(SemtimentData _ArtData, SemtimentResult _ArtSent,EntitiesResult _ArtEnt)
        {
            List<QuickTypeSemtimentData.Document> list = new List<QuickTypeSemtimentData.Document>();
            SemtimentData _AzData = new SemtimentData();
            int i = 0;
            foreach(QuickTypeSemtimentResult.Document doc in _ArtSent.Documents)
            {
                if(doc.Score < .3)
                    saveToDB(doc.Id.ToString(), _ArtData.Documents[i].Text, "Negative",_ArtEnt.Documents[i]);
                else
                {
                    if(doc.Score > .7)
                        saveToDB(doc.Id.ToString(), _ArtData.Documents[i].Text, "Positive",_ArtEnt.Documents[i]);
                    else
                        saveToDB(doc.Id.ToString(), _ArtData.Documents[i].Text, "Nuetral",_ArtEnt.Documents[i]);
                }
                i++;
            }
        }

        static void saveToDB(string Id, string _ArtText,string _sent ,QuickTypeEntitiesResult.Document _ArtEnt)
        {
            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                string EntID = "";
                string ArtID = "A" + Id;
                string cmd1 = $"g.addV('Article').property('id', '{ArtID}').property('Text', '{_ArtText.Replace("'","")}')";
                var result1 = AzureAsync(gremlinClient, cmd1);
                Console.WriteLine("\n{{\"Art Returned\": \"{0}\"}}", result1.Result.Count);

                foreach(QuickTypeEntitiesResult.Entity Ent in _ArtEnt.Entities)
                {
                    DBEntity myEsixtingEntity;
                    DBEntity myEntity = new DBEntity() { Id = "", Name = Ent.Name};
                    if(dbEnt.Contains(myEntity))
                    {
                        dbEnt.TryGetValue(myEntity,out myEsixtingEntity);
                        EntID = myEsixtingEntity.Id;
                    }
                    else{
                        entityId++;
                        myEntity.Id = "E" + entityId.ToString();
                        EntID = "E" + entityId.ToString();
                        dbEnt.Add(myEntity);
                        var result2 = AzureAsync(gremlinClient, $"g.addV('Entity').property('id', '{EntID}').property('Name', '{Ent.Name}')");
                        Console.WriteLine("\n{{\"Ent Returned\": \"{0}\"}}", result2.Result.Count);
                    }

                    var result3 = AzureAsync(gremlinClient, $"g.V('{ArtID}').addE('{_sent}').to(g.V('{EntID}'))");
                    Console.WriteLine("\n{{\"Sent Vert Returned\": \"{0}\"}}", result3.Result.Count);
                }
            }

        }

        public class DBEntity
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }
        public class DBEntityComp : IEqualityComparer<DBEntity>
        {
            public bool Equals(DBEntity x, DBEntity y)
            {
                return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
            }
        
            public int GetHashCode(DBEntity obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        static SemtimentData CreateSentimentData(RedditArticles _myArt)
        {
            List<QuickTypeSemtimentData.Document> list = new List<QuickTypeSemtimentData.Document>();
            SemtimentData _AzData = new SemtimentData();
            int i = 0;
            foreach(QuickTypeArticles.Child articl in _myArt.Data.Children)
            {
                if (articl.Kind == "t3" && articl.Data.Author != "AutoModerator")  // Ensure its an Article and not a bot post
                {
                    i++;
                    list.Add(new QuickTypeSemtimentData.Document
                    {
                        Id = i,
                        Language = "en",
                        Text = articl.Data.Title
                    });

                }
            }
            _AzData.Documents = list.ToArray();
            return _AzData;
        }

        static EntitiesData CreateEntitiesData(RedditArticles _myArt)
        {
            List<QuickTypeEntitiesData.Document> list = new List<QuickTypeEntitiesData.Document>();
            EntitiesData _AzData = new EntitiesData();
            int i = 0;
            foreach(QuickTypeArticles.Child articl in _myArt.Data.Children)
            {
                if (articl.Kind == "t3" && articl.Data.Author != "AutoModerator")  // Ensure its an Article and not a bot post
                {
                    i++;
                    list.Add(new QuickTypeEntitiesData.Document
                    {
                        Id = i,
                        Language = "en",
                        Text = articl.Data.Title
                    });
                }
            }
            _AzData.Documents = list.ToArray();
            return _AzData;
        }
        static KeyPhrasesData CreatePhraseData(RedditArticles _myArt)
        {
            List<QuickTypeKeyPhrasesData.Document> list = new List<QuickTypeKeyPhrasesData.Document>();
            KeyPhrasesData _AzData = new KeyPhrasesData();
            QuickTypeKeyPhrasesData.Document[] _Docs = new QuickTypeKeyPhrasesData.Document[1];
            int i = 0;
            foreach(QuickTypeArticles.Child articl in _myArt.Data.Children)
            {
                if (articl.Kind == "t3" && articl.Data.Author != "AutoModerator")  // Ensure its an Article and not a bot post
                {
                    i++;
                    list.Add(new QuickTypeKeyPhrasesData.Document 
                    {
                        Id = i,
                        Language = "en",
                        Text = articl.Data.Title
                    });
                }
            }
            _AzData.Documents = list.ToArray();
            return _AzData;
        }
        static SemtimentResult GetSentiment(string _Azuretoken, SemtimentData _Data)
        {
            String _JsonData = _Data.ToJson();
            var client = new RestClient("https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment");
            var request = new RestRequest(Method.POST);            
            request.AddHeader("Ocp-Apim-Subscription-Key", _Azuretoken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", _JsonData, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            SemtimentResult _Sentement= SemtimentResult.FromJson(response.Content);
            return _Sentement;
        }

        static EntitiesResult GetEntities(string _Azuretoken, EntitiesData _Data)
        {
            String _JsonData = _Data.ToJson();
            var client = new RestClient("https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/entities");
            var request = new RestRequest(Method.POST);            
            request.AddHeader("Ocp-Apim-Subscription-Key", _Azuretoken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", _JsonData, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            EntitiesResult _Entities = EntitiesResult.FromJson(response.Content);
            return _Entities;
        }

        static KeyPhrasesResult GetPhrases(string _Azuretoken, KeyPhrasesData _Data)
        {
            String _JsonData = _Data.ToJson();
            var client = new RestClient("https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases");
            var request = new RestRequest(Method.POST);            
            request.AddHeader("Ocp-Apim-Subscription-Key", _Azuretoken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", _JsonData, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            KeyPhrasesResult _Entities = KeyPhrasesResult.FromJson(response.Content);
            return _Entities;
        }
        static RedditComments[] GetComments(string _token, string _ArtID)
        {
            var client = new RestClient("https://oauth.reddit.com/r/soccer/comments/" + _ArtID);
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", "Bearer " + _token);
            request.AddParameter("undefined", "Sentiment Analysis/0.1 by Zycroft", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            RedditComments[] myReddit = RedditComments.FromJson(response.Content);
            return myReddit;
        }
        static RedditArticles GetArticles(string _token)
        {
            var client = new RestClient("https://oauth.reddit.com/r/soccer/hot");
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", "Bearer " + _token);
            request.AddParameter("undefined", "Sentiment Analysis/0.1 by Zycroft", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            RedditArticles myReddit = RedditArticles.FromJson(response.Content);
            return myReddit;
        }

        private static IRestResponse NewMethod(RestClient client, RestRequest request)
        {
            return client.Execute(request);
        }

        static string AuthToken(string _RedditLogin, string _RedditPWD, string _RedditAppID, string _RedditAppSecret)
        {
            // This call is good for 120 minutes
            string credentials = $"{_RedditAppID}:{_RedditAppSecret}";
            byte[] bytes = Encoding.ASCII.GetBytes(credentials);
            string base64 = Convert.ToBase64String(bytes);
            var client = new RestClient($"https://www.reddit.com/api/v1/access_token?grant_type=password&username={_RedditLogin}&password={_RedditPWD}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("content-length", "");
            request.AddHeader("accept-encoding", "gzip, deflate");
            request.AddHeader("Host", "www.reddit.com");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.15.0");
            request.AddHeader($"Authorization", "Basic {base64}");
            IRestResponse response = client.Execute(request);
            RedditAuth myReddit = RedditAuth.FromJson(response.Content);
            return myReddit.AccessToken;
        }

        private static Task<ResultSet<dynamic>> AzureAsync(GremlinClient gremlinClient, string query)
        {
            try
            {
                return gremlinClient.SubmitAsync<dynamic>(query);
            }
            catch (ResponseException ex)
            {
                Console.WriteLine("EXCEPTION: {0}", ex.StatusCode);
                throw;
            }
        }

        
    }
}
