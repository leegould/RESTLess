using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;

using Raven.Client;
using Raven.Client.Document;
using RestSharp;
using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderViewModel : PropertyChangedBase, IHandle<MethodSelectedMessage>
    {
        #region Private members

        private readonly AppSettings appSettings;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private IObservableCollection<HttpHeader> headers;

        private string url;
        private string body;

        private Method selectedMethod;

        private Stopwatch stopWatch;

        private bool bodyIsVisible;

        private bool isWaiting;

        #endregion

        static RequestBuilderViewModel()
        {
            Mapper.CreateMap<Request, RequestBuilderViewModel>()
                .ForMember(d => d.HeadersDataGrid, o => o.MapFrom(s => CreateHeadersFromDict(s.Headers)))
                .ForMember(d => d.UrlTextBox, o => o.MapFrom(s => s.Url + s.Path.Substring(1)))
                .ForMember(d => d.BodyTextBox, o => o.MapFrom(s => s.Body));
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

            using (var conn = documentStore.OpenSession())
            {
                appSettings = conn.Query<AppSettings>().FirstOrDefault();
                if (appSettings == null)
                {
                    appSettings = AppSettings.CreateDefault();
                    conn.Store(appSettings);
                    conn.SaveChanges();
                }
            }
        }

        #region Properties

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

        public bool CanSendButton
        {
            get
            {
                //return !string.IsNullOrWhiteSpace(UrlTextBox) && !isWaiting;
                return true;
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
            //BodyIsVisible = UseBody(message.Method);
        }

        #endregion

        #region Buttons

        public void SendButton()
        {
            var uri = new Uri(UrlTextBox);
            var client = GetRestClient(uri);
            var request = GetRestRequest(uri);

            stopWatch = new Stopwatch();
            stopWatch.Start();

            isWaiting = true;
            NotifyOfPropertyChange(() => CanStopButton);

            Request req = null;
            using (var conn = documentStore.OpenSession())
            {
                try
                {
                    req = new Request(client.BaseUrl, request, body, appSettings.RequestSettings);
                    conn.Store(req);
                    conn.SaveChanges();
                }
                catch (Exception ex)
                {
                    // TODO : Create message
                    StopSending();
                }
            }
            eventAggregator.PublishOnUIThread(new RequestSavedMessage { Request = req });

            client.ExecuteAsync(request,
                r =>
                {
                    if (isWaiting)
                    {
                        stopWatch.Stop();
                        //StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

                        Response response = new Response(req != null ? req.Id : 0, r, stopWatch.ElapsedMilliseconds);

                        //DisplayResponse(response);

                        StopSending();

                        using (var conn = documentStore.OpenSession())
                        {
                            try
                            {
                                conn.Store(response);
                                conn.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                // TODO : Create message
                                //StatusBarTextBlock = "Response Error";
                                //RawResultsTextBox = ex.ToString();
                            }
                        }
                    }
                });
        }

        public void StopButton()
        {
            StopSending();
        }

        #endregion

        private void LoadSelected(Request request)
        {
            Mapper.Map(request, this);

            //if (appSettings.LoadResponses)
            //{
            //    using (var docstore = documentStore.OpenSession())
            //    {
            //        Response response = null;
            //        if (request != null)
            //        {
            //            response = docstore.Query<Response>().FirstOrDefault(x => x.RequestId == request.Id);
            //        }
            //    }
            //}
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

            //if (UseBody(method) && !string.IsNullOrWhiteSpace(BodyTextBox))
            //{
            //    request.AddJsonBody(BodyTextBox);
            //}
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
