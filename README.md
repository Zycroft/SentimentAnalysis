# RedditSentiment

Reddit Post Sentiment Analysis demonstrating the use of the Reddit API, Microsft Azure Cognitive Services Text Analytics,
and Microsoft Azure Cosmos DB Query graph and Gremlin API. The goal of the application is to query the Reddit /r/Soccer 
Subreddit and build a graph of two verticies Articles and entities of the articles with the edges representing the 
sentiment of the Article to the entity.

![ArticleEntity](https://github.com/Zycroft/SentimentAnalysis/blob/master/Resources/ArticleEntity.png?raw=true)
## Setup

1. Create Reddit API account <https://www.reddit.com/wiki/api#wiki_reddit_api_access>
    * you need to read the terms and register
    * Then you should be at <https://www.reddit.com/prefs/apps>
    * Create a new application:
        - Name:anything you want
        - Select script app
        - about url:you can leave blank
        - redirect uri:<http://localhost:8080>

![Reddit](https://github.com/Zycroft/SentimentAnalysis/blob/master/Resources/Reddit1.png?raw=true)

2. Record the following:
    * RedditLogin: Your reddit account (A)
    * RedditPWD: Your reddit password
    * RedditAppID: This is in the top left corner of your application that you just created under your name and the label "personal use script" (B)
    * RedditAppSecret: This is below the AppID with the label "secret" (C)
2. Create Azure account (you are responsible for all account charges. Make sure you understand your Azure costs before proceeding)
3. Create **"Text Analytics"** resource
    * Name the resource
    * select subscription
    * select location
    * select pricing tier (F0)
    * select/create resource group

![Azure1](https://github.com/Zycroft/SentimentAnalysis/blob/master/Resources/Azure1.png?raw=true)

4. Go to your resource and copy the access key
5. Test landing page <https://[location].dev.cognitive.microsoft.com/docs/services/TextAnalytics.V2.0>
6. Open API testing console for your location
7. Input your key and test sentiment and entity REST calls
8. Create a **"Cosmo DB"** resource
    * select the subscription
    * select/creat resource group
    * Enter an account name
    * API - Select Gremlin **(graph)**
    * Leave Apache Spark disabled
    * Location select on near you
    * Geo-Redundancy leave disabled
    * Multi-regioin writes leave disabled

![Azure2](https://github.com/Zycroft/SentimentAnalysis/blob/master/Resources/Azure2.png?raw=true)

9. Create graph database
    * Goto you Cosmo DB resource
    * Goto Overview
    * Copy the Gremlin Endpoint e.g. [YourCosmoDB].gremlin.cosmos.azure.com
    * Goto Data Explorer
    * Select "New Graph"
        - Database id: Type and record the database id
        - Graph id: Type and record the graph id
        - Storage Capacity: Select Fixed (10GB) Storage Capacity
        - Partition key: you should not see this if you select Fixed (10GB)
        - Throughput: leave at 400

![Azure3](https://github.com/Zycroft/SentimentAnalysis/blob/master/Resources/Azure3.png?raw=true)

10. I used VS Code to develop this project with C# Extension
11. Open program.cs and update your recorded values for Reddit, Azure Text Analytics, and Azure Cosmo DB.  This is an example project and we are storing the API key in the code which is **not safe**.  For production applications store keys in a secure store like Azure key Vault.

![CodeUserValues](https://github.com/Zycroft/SentimentAnalysis/blob/master/Resources/Code1.png?raw=true)

12. The project should query /r/soccer/hot and populate the cosmos db you created.
13. Run some queries  (the queries below will not be the same for you as you will have different entities and articles)
    * First install **Azure Cosmos DB** Extension into VS Code
    * Execute g.V() you should see something like below

![Graph1](https://github.com/Zycroft/SentimentAnalysis/blob/master/Resources/Graph1.png?raw=true)
    
14. Find an entity e.g. 'Neymar' g.V().hasLabel('Entity').has('Name','Neymar')
15. Find all negative sentiment articles about 'Neymar' g.V().hasLabel('Entity').has('Name','Neymar').in('Negative')
