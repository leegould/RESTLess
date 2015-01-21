using System.Linq;

using Raven.Client.Indexes;

using RESTLess.Models;

namespace RESTLess.Indexes
{
    public class Responses_All: AbstractIndexCreationTask<Response>
    {
        public Responses_All()
        {
            Map = responses => from response in responses
                            select new
                            {
                                response.Id,
                                response.When,
                                response.ResponseUri,
                                response.StatusCode,
                                response.StatusCodeDescription,
                                response.Content,
                                response.Headers
                            };
        }
    }
}
