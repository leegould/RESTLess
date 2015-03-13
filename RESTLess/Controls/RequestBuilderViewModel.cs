using System;

using Caliburn.Micro;

using Raven.Client;

using RestSharp;

using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderViewModel : PropertyChangedBase, IHandle<MethodSelectedMessage>
    {
        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private IObservableCollection<HttpHeader> headers;

        private string url;
        private string body;

        private Method selectedMethod;

        private bool bodyIsVisible;

        static RequestBuilderViewModel()
        {
            //Mapper.CreateMap<Request, AppViewModel>()
            //    .ForMember(d => d.HeadersDataGrid, o => o.MapFrom(s => CreateHeadersFromDict(s.Headers)))
            //    .ForMember(d => d.UrlTextBox, o => o.MapFrom(s => s.Url + s.Path.Substring(1)))
            //    .ForMember(d => d.BodyTextBox, o => o.MapFrom(s => s.Body));
        }

        public RequestBuilderViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            MethodViewModel = new MethodViewModel(eventAggregator);
            HeadersDataGrid = new BindableCollection<HttpHeader>();

            selectedMethod = Method.GET;
            BodyIsVisible = false;
        }

        public MethodViewModel MethodViewModel { get; set; }

        public string UrlTextBox
        {
            get { return url; }
            set
            {
                url = value;
                NotifyOfPropertyChange(() => UrlTextBox);
                //NotifyOfPropertyChange(() => CanSendButton);
            }
        }

        public string BodyTextBox
        {
            get { return body; }
            set
            {
                body = value;
                NotifyOfPropertyChange(BodyTextBox);
            }
        }

        public bool BodyIsVisible
        {
            get { return bodyIsVisible; }
            set
            {
                bodyIsVisible = value;
                if (!bodyIsVisible)
                {
                    BodyTextBox = string.Empty;
                }
                NotifyOfPropertyChange(() => BodyIsVisible);
            }
        }

        public IObservableCollection<HttpHeader> HeadersDataGrid
        {
            get { return headers; }
            set
            {
                headers = value;
                NotifyOfPropertyChange(() => HeadersDataGrid);
            }
        }

        public void Handle(MethodSelectedMessage message)
        {
            selectedMethod = message.Method;
            //BodyIsVisible = UseBody(message.Method);
        }

        private RestRequest GetRestRequest(Uri uri)
        {
            var method = selectedMethod;

            var request = new RestRequest(uri, method);

            //request.AddHeader("nocache", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            foreach (var header in HeadersDataGrid)
            {
                request.AddHeader(header.Name, header.Value);
            }

            //if (UseBody(method) && !string.IsNullOrWhiteSpace(BodyTextBox))
            //{
            //    request.AddJsonBody(BodyTextBox);
            //}
            return request;
        }
    }
}
