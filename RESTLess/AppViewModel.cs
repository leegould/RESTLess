using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using AutoMapper;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client;
using RestSharp;
using RESTLess.Controls;
using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : PropertyChangedBase, IApp, IHandle<HistorySelectedMessage>, IHandle<MethodSelectedMessage>
    {
        #region Private members

        private readonly IEventAggregator eventAggregator;

        private readonly IWindowManager windowManager;

        private readonly IDocumentStore documentStore;

        private string rawResultsTextBox;
        private string url;
        private string body;

        private string responseStatusTextBlock;
        private string responseWhenTextBlock;
        private string responseElapsedTextBlock;

        private IObservableCollection<HttpHeader> headers;

        private string statusBarTextBlock;

        private bool isWaiting;
        
        private Stopwatch stopWatch;

        private Method selectedMethod;

        private bool bodyIsVisible;

        #endregion

        static AppViewModel()
        {
            Mapper.CreateMap<Request, AppViewModel>()
                .ForMember(d => d.HeadersDataGrid, o => o.MapFrom(s => CreateHeadersFromDict(s.Headers)))
                .ForMember(d => d.UrlTextBox, o => o.MapFrom(s => s.Url + s.Path.Substring(1)))
                .ForMember(d => d.BodyTextBox, o => o.MapFrom(s => s.Body));
        }

        private static IObservableCollection<HttpHeader> CreateHeadersFromDict(Dictionary<string, string> dictionary)
        {
            var items = new BindableCollection<HttpHeader>();
            foreach (var item in dictionary)
            {
                items.Add(new HttpHeader {Name = item.Key, Value = item.Value});
            }
            return items;
        }

        public AppViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.windowManager = windowManager;
            this.documentStore = documentStore;
            HeadersDataGrid = new BindableCollection<HttpHeader>();
            MethodViewModel = new MethodViewModel(eventAggregator);
            HistoryViewModel = new HistoryViewModel(eventAggregator, documentStore);
            selectedMethod = Method.GET;
            BodyIsVisible = false;
        }

        #region Properties

        public MethodViewModel MethodViewModel { get; set; }

        public HistoryViewModel HistoryViewModel { get; set; }

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

        public string RawResultsTextBox
        {
            get { return rawResultsTextBox; }
            set
            {
                rawResultsTextBox = value;
                NotifyOfPropertyChange(() => RawResultsTextBox);
            }
        }

        public string ResponseStatusTextBlock
        {
            get
            {
                return responseStatusTextBlock;
            }
            set
            {
                responseStatusTextBlock = value;
                NotifyOfPropertyChange(() => ResponseStatusTextBlock);
            }
        }

        public string ResponseWhenTextBlock
        {
            get
            {
                return responseWhenTextBlock;
            }
            set
            {
                responseWhenTextBlock = value;
                NotifyOfPropertyChange(() => ResponseWhenTextBlock);
            }
        }

        public string ResponseElapsedTextBlock
        {
            get
            {
                return responseElapsedTextBlock;
            }
            set
            {
                responseElapsedTextBlock = value;
                NotifyOfPropertyChange(() => ResponseElapsedTextBlock);
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

        public string StatusBarTextBlock
        {
            get { return statusBarTextBlock; }
            set
            {
                statusBarTextBlock = value;
                NotifyOfPropertyChange(() => StatusBarTextBlock);
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

        #endregion


        #region Public Methods

        public void SendButton()
        {
            var uri = new Uri(UrlTextBox);
            RestClient client = new RestClient(uri.GetLeftPart(UriPartial.Authority));
            var method = selectedMethod;

            var request = new RestRequest(uri, method);
            
            //request.AddHeader("nocache", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            foreach (var header in HeadersDataGrid)
            {
                request.AddHeader(header.Name, header.Value);
            }

            if (method == Method.POST || method == Method.PUT && !string.IsNullOrWhiteSpace(BodyTextBox))
            {
                request.AddJsonBody(BodyTextBox);
            }

            stopWatch = new Stopwatch();
            stopWatch.Start();

            StatusBarTextBlock = "Sending " + request.Method + " " + client.BaseUrl.ToString().Substring(0, client.BaseUrl.ToString().Length - 1) + request.Resource;

            isWaiting = true;
            NotifyOfPropertyChange(() => CanStopButton);

            Request req = null;
            using (var conn = documentStore.OpenSession())
            {
                try
                {
                    req = new Request(client.BaseUrl, request, body);
                    conn.Store(req);
                    conn.SaveChanges();
                }
                catch (Exception ex)
                {
                    StatusBarTextBlock = "Cancelled request";
                    RawResultsTextBox = ex.ToString();
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
                        StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

                        try
                        {
                            var json = JObject.Parse(r.Content);
                            RawResultsTextBox = json.ToString(Formatting.Indented);
                            ResponseElapsedTextBlock = stopWatch.ElapsedMilliseconds.ToString() + " ms.";
                            ResponseStatusTextBlock = r.ResponseStatus.ToString();
                            ResponseWhenTextBlock = DateTime.UtcNow.ToString();
                        }
                        catch (Exception ex)
                        {
                            RawResultsTextBox = ex.ToString();
                        }

                        StopSending();

                        using (var conn = documentStore.OpenSession())
                        {
                            try
                            {
                                conn.Store(new Response
                                {
                                    When = DateTime.UtcNow,
                                    RequestId =  req != null ? req.Id : 0,
                                    RestResponse = r
                                });
                                conn.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                StatusBarTextBlock = "Response Error";
                                RawResultsTextBox = ex.ToString();
                            }
                        }
                    }
                });
        }

        public void StopButton()
        {
            StopSending();
            StatusBarTextBlock = "Cancelled request";
            RawResultsTextBox = string.Empty;
            ResponseElapsedTextBlock = string.Empty;
            ResponseStatusTextBlock = string.Empty;
            ResponseWhenTextBlock = string.Empty;
        }

        public void ClearButton()
        {
            UrlTextBox = string.Empty;
            BodyTextBox = string.Empty;
            HeadersDataGrid.Clear();
        }

        public void Handle(HistorySelectedMessage historyRequest)
        {
            Mapper.Map(historyRequest.Request, this);
            //MethodViewModel.Method = (Method)Enum.Parse(typeof(Method), historyRequest.Request.Method);
        }

        public void Handle(MethodSelectedMessage message)
        {
            selectedMethod = message.Method;
            BodyIsVisible = message.Method == Method.POST || message.Method == Method.PUT;
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}
