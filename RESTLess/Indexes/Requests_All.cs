using System.Linq;

using Raven.Client.Indexes;

using RESTLess.Models;

namespace RESTLess.Indexes
{
    public class Requests_All : AbstractIndexCreationTask<Request>
    {
        public Requests_All()
        {
            Map = requests => from request in requests
                            select new
                            {
                                request.Id,
                                request.Url,
                                request.When,
                                request.Method,
                                request.Path,
                                request.Body,
                                request.Headers
                            };
        }
    }
}
