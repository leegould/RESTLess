using System;
using System.Collections.Generic;
using RestSharp;

namespace RESTLess.Models
{
    public class Request
    {
        public Request()
        {
        }

        public Request(string baseUrl, RestRequest restRequest, string body)
        {
            When = DateTime.UtcNow;
            BaseUrl = baseUrl;
            Path = restRequest.Resource;
            Method = restRequest.Method.ToString();

            Headers = new Dictionary<string, string>();
            foreach (var param in restRequest.Parameters)
            {
                Headers.Add(param.Name, param.Value.ToString());
            }

            Body = body;
        }

        public int Id { get; set; }

        public DateTime When { get; set; }

        public string BaseUrl { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Path { get; set; }

        public string Method { get; set; }

        public string Body { get; set; }
    }
}
