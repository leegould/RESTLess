using System;
using System.Collections.Generic;
using RestSharp;

namespace RESTLess.Models
{
    public class Response
    {
        public Response()
        {
        }

        public Response(int requestId, IRestResponse restResponse, long elapsed)
        {
            When = DateTime.UtcNow;
            Elapsed = elapsed;
            RequestId = requestId;

            Headers = new Dictionary<string, string>();
            foreach (var param in restResponse.Headers)
            {
                if (param.Value != null && !Headers.ContainsKey(param.Name))
                {
                    Headers.Add(param.Name, param.Value.ToString());
                }
            }

            ResponseStatus = restResponse.ResponseStatus.ToString();
            Server = restResponse.Server;
            Content = restResponse.Content;
            StatusCode = (int)restResponse.StatusCode;
            StatusCodeDescription = restResponse.StatusCode.ToString();
            ContentType = restResponse.ContentType;
            ContentLength = restResponse.ContentLength;
            RawBytes = restResponse.RawBytes;
            ResponseUri = restResponse.ResponseUri;
        }

        public int Id { get; set; }

        public DateTime When { get; set; }

        public long Elapsed { get; set; }

        public int RequestId { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string ResponseStatus { get; set; }

        public string Server { get; set; }

        public string Content { get; set; }

        public int StatusCode { get; set; }

        public string StatusCodeDescription { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public byte[] RawBytes { get; set; }

        public Uri ResponseUri { get; set; }
    }
}
