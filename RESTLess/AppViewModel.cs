using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

using Caliburn.Micro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client;
using RestSharp;

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


        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager, IDocumentStore documentStore)
        {
            this.windowManager = windowManager;
            this.documentStore = documentStore;
            HeadersDataGrid = new BindableCollection<HttpHeader>();
            MethodViewModel = new MethodViewModel();
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
                    req = new Request {When = DateTime.UtcNow, RestClient = client, RestRequest = request};
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

            client.ExecuteAsync(request,
                r =>
                {
                    if (isWaiting)
                    {
                        stopWatch.Stop();
                        StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

                        var json = JObject.Parse(r.Content);
                        RawResultsTextBox = json.ToString(Formatting.Indented);
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
