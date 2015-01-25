using Caliburn.Micro;

namespace RESTLess.Models
{
    public class RequestGrouped
    {
        public string Id { get; set; }

        public string Part { get; set; }

        public RequestGrouped()
        {
            Children = new BindableCollection<RequestGrouped>();
        }

        public IObservableCollection<RequestGrouped> Children { get; set; }
    }
}
