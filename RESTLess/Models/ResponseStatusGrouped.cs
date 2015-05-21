namespace RESTLess.Models
{
    public class ResponseStatusGrouped
    {
        public string Id { get; set; }

        public string RequestId { get; set; }

        public string Url { get; set; }

        public string Path { get; set; }

        public int StatusCode { get; set; }
    }
}
