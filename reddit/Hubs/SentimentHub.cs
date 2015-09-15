using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace reddit.Hubs
{
    public class SentimentHub : Hub
    {
        public void Results(List<string> values)
        {

        }
    }
}