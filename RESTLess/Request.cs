using System;
using RestSharp;

namespace RESTLess
{
    public class Request
    {
        public int Id { get; set; }

        public DateTime When { get; set; }

        public IRestClient RestClient { get; set; }

        public IRestRequest RestRequest { get; set; }
    }
}
