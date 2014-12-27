using System;
using RestSharp;

namespace RESTLess.Models
{
    public class Response
    {
        public int Id { get; set; }

        public DateTime When { get; set; }

        public int RequestId { get; set; }

        public IRestResponse RestResponse { get; set; }
    }
}
