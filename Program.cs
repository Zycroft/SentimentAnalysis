using System;
using RestSharp;
using QuickTypeAuth;
using QuickTypeArticles;
using QuickTypeComments;

namespace RedditSentiment
{
    class Program
    {
        static void Main(string[] args)
        {
            string redditAccessToken = "11804495-3JQOnNM7QfNDlH-R69n5OlIuP4o";
            //string redditAccessToken = AuthToken();
            RedditArticles myArt = GetArticles(redditAccessToken);
            foreach(QuickTypeArticles.Child articl in myArt.Data.Children)
            {
                Console.WriteLine("+" + articl.Data.Title);
                System.Threading.Thread.Sleep(1000);
                RedditComments[] myComments = GetComments(redditAccessToken, articl.Data.Name.Substring(3));
                foreach(RedditComments comm in myComments)
                {
                    foreach(PurpleChild subcomm in comm.Data.Children)
                    if (subcomm.Kind == "t1")
                    {
                        Console.WriteLine("+++" + subcomm.Data.Body);
                    }
                }
            }



            /* 
            quicktype samples -o reddit.cs 

            curl -H "Authorization: bearer 11804495" -A "Sentiment Analysis/0.1 by Zycroft" https://oauth.reddit.com/api/v1/me

            GET [/r/subreddit]/new
            curl -H "Authorization: bearer 11804495" -A "Sentiment Analysis/0.1 by Zycroft" https://oauth.reddit.com/r/politics/new
            [/r/subreddit]/comments/article
            curl -H "Authorization: bearer 11804495" -A "Sentiment Analysis/0.1 by Zycroft" https://oauth.reddit.com/comments/c157lj
            */
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
            var client = new RestClient("https://oauth.reddit.com/r/soccer/new?limit=2");
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
