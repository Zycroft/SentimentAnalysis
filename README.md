# RedditSentiment

Reddit Post Sentiment Analysis demonstrating the use of the Reddit API, Microsft Azure Cognitive Services Text Analytics,
and Microsoft Azure Cosmos DB Query graph and Gremlin API. The goal of the application is to query the Reddit /r/Soccer 
Subreddit and build a graph of two verticies Articles and entities of the articles with the edges representing the 
sentiment of the Article to the entity.

![ArticleEntity](https://github.com/Zycroft/SentimentAnalysis/Resources/ArticleEntity.png?raw=true)
## Setup

1. Create Reddit API account <https://www.reddit.com/wiki/api#wiki_reddit_api_access>
    * you need to read the terms and register
    * Then you should be at <https://www.reddit.com/prefs/apps>
    * Create a new application:
        - Name:anything you want
        - Select script app
        - about url:you can leave blank
        - redirect uri:<http://localhost:8080>
    * Record the following:
        - RedditLogin: Your reddit account
        - RedditPWD: Your reddit password
        - RedditAppID: This is in the top left corner of your application that you just created under your name and the label "personal use script"
        - RedditAppSecret: This is below the AppID with the label "secret"
2. Create Azure account (you are responsible for all account charges. Make sure you understand your Azure costs before proceeding)
3. Create **"Text Analytics"** resource
    * Name the resource
    * select subscription
    * select location
    * select pricing tier (F0)
    * select/create resource group
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
9. Create graph database
    * Goto you Cosmo DB resource
    * Goto Overview
    * Copy the Gremlin Endpoint e.g. [YourCosmoDB].gremlin.cosmos.azure.com
    * Goto Data Explorer
    * Select "New Graph"
        - Database id: Type and record the database id
        - Graph id: Type and record the graph id
        - Storage Capacity: Select Fixed (10GB) Storage Capacity
        - Patition key: you should not see this if you select Fixed (10GB)
        - Throughput: leave at 400
10. I used VS Code to develop this project with C# Extension
11. Open program.cs and update your recorded values for Reddit, Azure Text Analytics, and Azure Cosmo DB
![CodeUserVaulues](https://github.com/Zycroft/SentimentAnalysis/Resources/Code1.png?raw=true)
12. The project should query /r/soccer/hot and populate the cosmos db you created.
13. Run some queries  (the queries below will not be the same for you)
    * First install **Azure Cosmos DB** Extension into VS Code
    * Goto data explorer select your Graph and "load graph"
    * Find an entity e.g. Neymar g.V().hasLabel('Entity').has('Name','Neymar')
    * Find all negative sentiment articles about Neymar g.V().hasLabel('Entity').has('Name','Neymar').in('Negative')

<br />
    ![Running a query](images/running_a_query.png)

    1. Enter your gremlin query and execute.
    2. If the output is a graph, it will show on the network graph surface.
    3. All output are shown on the console area.  This is especially important if your query returns a string or an array of strings instead of a graph.

