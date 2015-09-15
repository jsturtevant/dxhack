using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;


namespace reddit.Controllers
{
    public class ThumperController : ApiController
    {
        // GET api/values
        public async Task<bool> Get(string message)
        {

            var value =   await AnalysisClass.InvokeRequestResponseService(message);

            switch (value)
            {
                case "positive":
                    // drop through
                case "neutral":
                    return false;
                case "negative":
                    return true;
                default:
                    return false;
            }
        }

    }


    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    public class AnalysisClass
    {

        public static async Task<string> InvokeRequestResponseService(string text)
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
                                Values = new string[,] {  { text },  }
                            }
                        },
                                        },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };


                
                string apiKey = System.Configuration.ConfigurationManager.AppSettings["mlapi"]; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/b50809a072c24a698739941e65abe7aa/services/c6bb5cc807024febaefba173622a12a7/execute?api-version=2.0&details=true");

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    var obj = JsonConvert.DeserializeObject<RootObject>(result);

                    string value =  obj.Results.output1.value.Values.First().First();

                    return value;
                }
                else
                {


                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);

                    throw new Exception("response.StatusCode");
                }
            }
        }
    }


    public class Value
    {
        public List<string> ColumnNames { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<List<string>> Values { get; set; }
    }

    public class Output1
    {
        public string type { get; set; }
        public Value value { get; set; }
    }

    public class Results
    {
        public Output1 output1 { get; set; }
    }

    public class RootObject
    {
        public Results Results { get; set; }
    }
}
