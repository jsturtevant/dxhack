using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



namespace RedditDataGetter
{
    using RedditSharp;
    using RedditSharp.Things;
    using System.Net;
    using System.IO;
    using Microsoft.AspNet.SignalR.Client;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        private static string HUB_URL = "http://localhost:54294/";
        private static string HUBPROXY_URL = "SentimentHub";
        private string HUB_METHOD = "UpdateSentiment";
        
        public static HubConnection _conn = new HubConnection(HUB_URL);
        public static IHubProxy connProxy = _conn.CreateHubProxy(HUBPROXY_URL);

        void Start()
        {
            _conn.Start().Wait();

            // Start Signalr Connection
            //_conn.Start();
           var value = new RedditSentiment()
            {
                Body = "Test",
                Title = "test",
                Value = ".8",
                Url ="http://jamessturtevant.com"
            };
            callSentimentSite(value);
        }

   


        void callSentimentSite(RedditSentiment data)
        {
            connProxy.Invoke(HUB_METHOD, data);
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
