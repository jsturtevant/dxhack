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
        private static string HUB_URL = "";
        private static string HUBPROXY_URL = "";
        private string HUB_METHOD = "";
        
        public static HubConnection _conn = new HubConnection(HUB_URL);
        public static IHubProxy connProxy = _conn.CreateHubProxy(HUBPROXY_URL);

        void Start()
        {
            // Start Signalr Connection
            //_conn.Start();

            // Setup reddit connection
            var reddit = new Reddit();
            var user = reddit.LogIn("dxhack", "P@ssw0rd");

            // Call Reddit/r/All Hot and get top 25
            // filter all NSFW
            // Send to the GETCOMMENT Method
            reddit.RSlashAll.Hot.Take(25).ToList().ForEach(p =>
            {
                if (p.NSFW != true)
                {
                    p.Comments.ToList().ForEach(c =>
                    {
                        getComment(c, p.Title);
                    });

                    Console.WriteLine(p.Title);
                }
            });
        }

        // Send to the SentimentService
        // Take results and metadata and send to Signalr hub
        void getComment(Comment c, string title)
        {
            var com = InvokeRequestResponseService(c).Result;

            RedditSentiment rs = new RedditSentiment
            {
                Value = com.Results.output1.value.Values[0][1],
                Title = title,
                Url = c.Parent.Shortlink,
                Body = c.Body
            };

            callSentimentSite(JsonConvert.SerializeObject(rs));

            if (c.Comments.Count > 0)
                c.Comments.ToList().ForEach(a =>
                {
                    getComment(a, title);
                });
        }

        static async Task<RootObject> InvokeRequestResponseService(Comment comment)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"tweet_text"},
                                Values = new string[,] {  { comment.Body }  }
                            }
                        },
                                        },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "XsYHUgA0Fncg0ZMr5cjnTb6T5//Fl88PuT1alIpECeYMBkVMbAoNgJSIMLdhieE+T5m7Oy91gQLrunHcyPGE/Q=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/b50809a072c24a698739941e65abe7aa/services/c6bb5cc807024febaefba173622a12a7/execute?api-version=2.0&details=true");

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    
                    Console.WriteLine("Result: {0}", result);
                }

                return JsonConvert.DeserializeObject<RootObject>(result);
            }
        }

        void callSentimentSite(string data)
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
