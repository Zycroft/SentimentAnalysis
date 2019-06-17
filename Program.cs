using System;
using RestSharp;
using QuickTypeAuth;
using QuickTypeArticles;
using QuickTypeComments;
using QuickTypeEtitiesData;
using QuickTypeEtitiesResult;
using QuickTypeSemtimentData;
using QuickTypeSemtimentResult;

namespace RedditSentiment
{
    class Program
    {
        static void Main(string[] args)
        {
            string redditAccessToken = "11804495-klXSQhPxHH7LsDXDVNmqQMbTiPQ";
            string AzureAppKey = "0dc6ffa484c1449ab35f6b25ced49da5";
            //string redditAccessToken = AuthToken();
            RedditArticles myArt = GetArticles(redditAccessToken);
            foreach(QuickTypeArticles.Child articl in myArt.Data.Children)
            {
                if (articl.Kind == "t3" && articl.Data.Author != "AutoModerator")  // Ensure its an Article and not a bot post
                {

                    SemtimentResult ArtSentiment = GetSentiment(AzureAppKey, articl.Data.Title);
                    EtitiesResult ArtEntities = GetEntities(AzureAppKey, articl.Data.Title);
                    Console.WriteLine("A:" + articl.Data.Title);
                    Console.WriteLine("S:" + ArtSentiment.Documents[0].Score.ToString());
                    foreach(QuickTypeEtitiesResult.Entity artEnt in ArtEntities.Documents[0].Entities)
                    {
                        Console.WriteLine("e:" + artEnt.Name);
                    }
                    System.Threading.Thread.Sleep(1000);
                    ParseComments(redditAccessToken, articl.Data.Name.Substring(3));
                }

            }
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

        static SemtimentResult GetSentiment(string _Azuretoken, string _Text)
        {
            SemtimentData _AzData = new SemtimentData();
            QuickTypeSemtimentData.Document[] _Docs = new QuickTypeSemtimentData.Document[1];
            _Docs[0] = new QuickTypeSemtimentData.Document 
            {
                Id = 1,
                Language = "en",
                Text = _Text
            };
            _AzData.Documents = _Docs;
            String _JsonData = _AzData.ToJson();
            var client = new RestClient("https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment");
            var request = new RestRequest(Method.POST);            
            request.AddHeader("Ocp-Apim-Subscription-Key", _Azuretoken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", _JsonData, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            SemtimentResult _Sentement= SemtimentResult.FromJson(response.Content);
            return _Sentement;
        }

        static EtitiesResult GetEntities(string _Azuretoken, string _Text)
        {
            EtitiesData _AzData = new EtitiesData();
            QuickTypeEtitiesData.Document[] _Docs = new QuickTypeEtitiesData.Document[1];
            _Docs[0] = new QuickTypeEtitiesData.Document 
            {
                Id = 1,
                Language = "en",
                Text = _Text
            };
            _AzData.Documents = _Docs;
            String _JsonData = _AzData.ToJson();
            var client = new RestClient("https://westus2.api.cognitive.microsoft.com/text/analytics/v2.0/entities");
            var request = new RestRequest(Method.POST);            
            request.AddHeader("Ocp-Apim-Subscription-Key", _Azuretoken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", _JsonData, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            EtitiesResult _Entities = EtitiesResult.FromJson(response.Content);
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

        static string AuthToken()
        {
            var client = new RestClient("https://www.reddit.com/api/v1/access_token?grant_type=password&username=Zy&password=");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("content-length", "");
            request.AddHeader("accept-encoding", "gzip, deflate");
            request.AddHeader("Host", "www.reddit.com");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.15.0");
            request.AddHeader("Authorization", "Basic ");
            IRestResponse response = client.Execute(request);
            RedditAuth myReddit = RedditAuth.FromJson(response.Content);
            return myReddit.AccessToken;
        }

        
    }
}
