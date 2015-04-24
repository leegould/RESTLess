using System;
using System.Collections.Generic;
using System.Diagnostics;

using AutoMapper;

using Caliburn.Micro;

using Raven.Client;

using RestSharp;

using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderFormViewModel : Screen, ITabItem, IHandle<MethodSelectedMessage>, IHandle<HistorySelectedMessage>, IHandle<AppSettingsChangedMessage>, IHandle<FavouriteSelectedMessage>, IHandle<GroupedSelectedMessage>, IHandle<SearchSelectedMessage>, IHandle<AddHeaderMessage>, IHandle<DeleteAllHistoryMessage>
    {
        #region Private members

        private AppSettings appSettings;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private readonly IWindowManager windowManager;

        private IObservableCollection<HttpHeader> headers;

        private string url;
        private string body;

        private Method selectedMethod;

        private Stopwatch stopWatch;

        private bool bodyIsVisible;

        private bool isWaiting;

        #endregion

        static RequestBuilderFormViewModel()
        {
            Mapper.CreateMap<Request, RequestBuilderFormViewModel>()
                .ForMember(d => d.HeadersDataGrid, o => o.MapFrom(s => CreateHeadersFromDict(s.Headers)))
                .ForMember(d => d.UrlTextBox, o => o.MapFrom(s => s.Url + s.Path.Substring(1)))
                .ForMember(d => d.BodyTextBox, o => o.MapFrom(s => s.Body));
        }

        public RequestBuilderFormViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, IWindowManager windowManager, AppSettings appsettings)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            this.windowManager = windowManager;
            appSettings = appsettings;
            MethodViewModel = new MethodViewModel(eventAggregator);
            HeadersDataGrid = new BindableCollection<HttpHeader>();

            selectedMethod = Method.GET;
            BodyIsVisible = false;
        }

        #region properties

        public MethodViewModel MethodViewModel { get; set; }

        public string UrlTextBox
        {
            get { return url; }
            set
            {
                url = value;
                NotifyOfPropertyChange(() => UrlTextBox);
                NotifyOfPropertyChange(() => CanSendButton);
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

        public bool CanSendButton
        {
            get
            {
                return !string.IsNullOrWhiteSpace(UrlTextBox) && !isWaiting;
            }
        }

        public bool CanStopButton
        {
            get
            {
                return isWaiting;
            }
        }
        
        #endregion

        #region Handlers

        public void Handle(MethodSelectedMessage message)
        {
            selectedMethod = message.Method;
            BodyIsVisible = UseBody(message.Method);
        }

        public void Handle(HistorySelectedMessage historyRequest)
        {
            LoadSelected(historyRequest.Request);
        }

        public void Handle(AppSettingsChangedMessage message)
        {
            appSettings = message.AppSettings;
        }

        public void Handle(FavouriteSelectedMessage message)
        {
            LoadSelected(message.Request);
        }

        public void Handle(GroupedSelectedMessage message)
        {
            using (var docstore = documentStore.OpenSession())
            {
                var item = docstore.Load<Request>(message.Request.Id);
                if (item != null)
                {
                    Mapper.Map(item, this);
                }
            }
        }

        public void Handle(SearchSelectedMessage message)
        {
            LoadSelected(message.Request);
        }


        public void Handle(AddHeaderMessage message)
        {
            HeadersDataGrid.Add(new HttpHeader { Name = message.Header, Value = message.Value });
        }

        public void Handle(DeleteAllHistoryMessage message)
        {
            UrlTextBox = string.Empty;
            BodyTextBox = string.Empty;
            HeadersDataGrid.Clear();
        }

        #endregion

        private void LoadSelected(Request request)
        {
            Mapper.Map(request, this);
        }

        private void StopSending()
        {
            if (stopWatch.IsRunning)
            {
                stopWatch.Stop();
                stopWatch.Reset();
            }

            isWaiting = false;
            NotifyOfPropertyChange(() => CanStopButton);
            NotifyOfPropertyChange(() => CanSendButton);
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

            if (UseBody(method) && !string.IsNullOrWhiteSpace(BodyTextBox))
            {
                request.AddJsonBody(BodyTextBox);
            }

            return request;
        }

        private RestClient GetRestClient(Uri uri)
        {
            RestClient client = new RestClient(uri.GetLeftPart(UriPartial.Authority))
            {
                Timeout = appSettings.RequestSettings.Timeout
            };
            return client;
        }

        #region Static Methods

        private static IObservableCollection<HttpHeader> CreateHeadersFromDict(Dictionary<string, string> dictionary)
        {
            var items = new BindableCollection<HttpHeader>();
            foreach (var item in dictionary)
            {
                items.Add(new HttpHeader { Name = item.Key, Value = item.Value });
            }
            return items;
        }

        private static bool UseBody(Method method)
        {
            return method != Method.GET && method != Method.HEAD && method != Method.OPTIONS;
        }

        #endregion
    }
}
