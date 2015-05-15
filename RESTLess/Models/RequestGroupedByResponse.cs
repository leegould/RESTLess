using Caliburn.Micro;

namespace RESTLess.Models
{
    public class RequestGroupedByResponse
    {
        public string Id { get; set; }

        public string StatusCode { get; set; }

        public RequestGroupedByResponse()
        {
            Children = new BindableCollection<RequestGroupedByResponse>();
        }

        public IObservableCollection<RequestGroupedByResponse> Children { get; set; }
    }
}
