﻿using System;
using System.Collections.Generic;
using RestSharp;

namespace RESTLess.Models
{
    public class Request
    {
        public Request()
        {
            
        }

        public Request(Uri baseUrl, RestRequest restRequest, string body, RequestSettings requestSettings)
        {
            When = DateTime.UtcNow;
            Url = baseUrl;
            Path = restRequest.Resource;
            Method = restRequest.Method.ToString();

            Headers = new Dictionary<string, string>();
            foreach (var param in restRequest.Parameters)
            {
                if (param.Type == ParameterType.HttpHeader && param.Value != null && !Headers.ContainsKey(param.Name))
                {
                    Headers.Add(param.Name, param.Value.ToString());
                }
            }

            Timeout = requestSettings.Timeout;

            Body = body;
        }

        public string Id { get; set; }

        public DateTime When { get; set; }

        public Uri Url { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Path { get; set; }

        public string Method { get; set; }

        public string Body { get; set; }

        public int Timeout { get; set; }

        public bool Favourite { get; set; }
    }
}
