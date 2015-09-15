using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace reddit.Hubs
{
    public class SentimentHub : Hub
    {
        public void UpdateSentiment(RedditSentiment sentiment)
        {
            Clients.All.updateSentiments(sentiment);
        }
    }

    public class RedditSentiment
    {
        public string Value { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
    }
}