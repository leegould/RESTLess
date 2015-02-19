namespace RESTLess.Models
{
    public class RequestSettings
    {
        public static RequestSettings CreateDefault()
        {
            return new RequestSettings
                   {
                       Timeout = 60000
                   };
        }

        public int Timeout { get; set; }
    }
}
