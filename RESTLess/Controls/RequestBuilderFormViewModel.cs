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
    public sealed class RequestBuilderFormViewModel : Screen, ITabItem, IHandle<MethodSelectedMessage>, IHandle<HistorySelectedMessage>, IHandle<AppSettingsChangedMessage>, IHandle<FavouriteSelectedMessage>, IHandle<GroupedSelectedMessage>, IHandle<SearchSelectedMessage>, IHandle<AddHeaderMessage>, IHandle<DeleteAllHistoryMessage>, IHandle<CreateRequestMessage>
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
                .ForMember(d => d.Headers, o => o.MapFrom(s => CreateHeadersFromDict(s.Headers)))
                .ForMember(d => d.Url, o => o.MapFrom(s => s.Url + s.Path.Substring(1)))
                .ForMember(d => d.Body, o => o.MapFrom(s => s.Body));
        }

        public RequestBuilderFormViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, IWindowManager windowManager, AppSettings appsettings)
        {
            DisplayName = "Request Builder";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            this.windowManager = windowManager;
            appSettings = appsettings;
            MethodViewModel = new MethodViewModel(eventAggregator);
            Headers = new BindableCollection<HttpHeader>();

            SelectedMethod = Method.GET;
            BodyIsVisible = false;
        }

        protected override void OnDeactivate(bool close)
        {
            var uri = new Uri(Url);
            var restRequest = RequestBuilderViewModel.GetRestRequest(uri, SelectedMethod, Body, Headers);
            var request = new Request(new Uri(uri.Scheme + Uri.SchemeDelimiter + uri.Authority), restRequest, Body, appSettings.RequestSettings);
            
            
            eventAggregator.BeginPublishOnUIThread(new CreateRequestMessage { Request = request });

            base.OnDeactivate(close);
        }

        #region properties

        public MethodViewModel MethodViewModel { get; set; }

        public Method SelectedMethod
        {
            get {  return selectedMethod; }
            set
            {
                selectedMethod = value;
                NotifyOfPropertyChange(() => SelectedMethod);
            }
        }

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                NotifyOfPropertyChange(() => Url);
                NotifyOfPropertyChange(() => CanSendButton);
            }
        }

        public string Body
        {
            get { return body; }
            set
            {
                body = value;
                NotifyOfPropertyChange(Body);
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
                    Body = string.Empty;
                }
                NotifyOfPropertyChange(() => BodyIsVisible);
            }
        }

        public IObservableCollection<HttpHeader> Headers
        {
            get { return headers; }
            set
            {
                headers = value;
                NotifyOfPropertyChange(() => Headers);
            }
        }

        public bool CanSendButton
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Url) && !isWaiting;
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
            SelectedMethod = message.Method;
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
            Headers.Add(new HttpHeader { Name = message.Header, Value = message.Value });
        }

        public void Handle(DeleteAllHistoryMessage message)
        {
            Url = string.Empty;
            Body = string.Empty;
            Headers.Clear();
        }

        public void Handle(CreateRequestMessage message)
        {
            LoadSelected(message.Request);
        }

        #endregion

        private void LoadSelected(Request request)
        {
            Mapper.Map(request, this);
        }

        //private void StopSending()
        //{
        //    if (stopWatch.IsRunning)
        //    {
        //        stopWatch.Stop();
        //        stopWatch.Reset();
        //    }

        //    isWaiting = false;
        //    NotifyOfPropertyChange(() => CanStopButton);
        //    NotifyOfPropertyChange(() => CanSendButton);
        //}
        
        #region Static Methods

        private static IObservableCollection<HttpHeader> CreateHeadersFromDict(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            var items = new BindableCollection<HttpHeader>();
            foreach (var item in dictionary)
            {
                items.Add(new HttpHeader { Name = item.Key, Value = item.Value });
            }
            return items;
        }

        public static bool UseBody(Method method)
        {
            return method != Method.GET && method != Method.HEAD && method != Method.OPTIONS;
        }

        #endregion
    }
}
