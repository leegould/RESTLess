using System;
using System.ComponentModel;
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

namespace RESTLess
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : PropertyChangedBase, IApp
    {
        #region Private members

        private readonly IWindowManager windowManager;

        private readonly IDocumentStore documentStore;

        private string rawResultsTextBox;
        private string url;
        private string body;

        private IObservableCollection<HttpHeader> headers;

        private string statusBarTextBlock;

        private bool isWaiting;
        
        private Stopwatch stopWatch;

        #endregion

        static AppViewModel()
        {
            Mapper.CreateMap<Request, AppViewModel>()
                //.ForMember(d => d.HeadersDataGrid, o => o.MapFrom(s => s.Headers))
                .ForMember(d => d.UrlTextBox, o => o.MapFrom(s => s.BaseUrl))
                ;
        }
        
        public AppViewModel(IWindowManager windowManager, IDocumentStore documentStore)
        {
            this.windowManager = windowManager;
            this.documentStore = documentStore;
            HeadersDataGrid = new BindableCollection<HttpHeader>();
            MethodViewModel = new MethodViewModel();
            HistoryViewModel = new HistoryViewModel(documentStore);
            HistoryViewModel.PropertyChanged += LoadRequestFromHistory;
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

        public bool CanResetButton
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

        #endregion


        #region Public Methods

        private void LoadRequestFromHistory(object sender, PropertyChangedEventArgs e)
        {
            Mapper.Map(HistoryViewModel.SelectedItem, this);
        }

        public void SendButton()
        {
            var uri = new Uri(UrlTextBox);
            RestClient client = new RestClient(uri.GetLeftPart(UriPartial.Authority));
            var method = MethodViewModel.GetMethod();

            var request = new RestRequest(uri, method);
            
            //request.AddHeader("nocache", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            foreach (var header in HeadersDataGrid)
            {
                request.AddHeader(header.Name, header.Value);
            }

            if (method == Method.POST || method == Method.PUT)
            {
                request.AddJsonBody(BodyTextBox);
            }

            stopWatch = new Stopwatch();
            stopWatch.Start();

            StatusBarTextBlock = "Sending " + request.Method + " " + client.BaseUrl.ToString().Substring(0, client.BaseUrl.ToString().Length - 1) + request.Resource;

            isWaiting = true;
            NotifyOfPropertyChange(() => CanResetButton);

            Request req = null;
            using (var conn = documentStore.OpenSession())
            {
                try
                {
                    req = new Request(client.BaseUrl.ToString(), request);
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
            HistoryViewModel.HistoryRequests.Add(req);

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

        public void ResetButton()
        {
            StopSending();
            StatusBarTextBlock = "Cancelled request";
            RawResultsTextBox = string.Empty;
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
            NotifyOfPropertyChange(() => CanResetButton);
            NotifyOfPropertyChange(() => CanSendButton);
        }

        #endregion
    }
}
