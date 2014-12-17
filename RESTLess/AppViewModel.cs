using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Caliburn.Micro;

using RestSharp;
using Newtonsoft.Json;

namespace RESTLess
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : PropertyChangedBase
    {
        #region Private members

        private readonly IWindowManager windowManager;

        private string rawResultsTextBox;
        private string url;
        private string body;

        private bool getChecked;
        private bool postChecked;
        private bool putChecked;
        private bool deleteChecked;

        private IObservableCollection<HttpHeader> headers;

        private string statusBarTextBlock;

        private bool isWaiting;

        private Stopwatch stopWatch;

        #endregion


        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            HeadersDataGrid = new BindableCollection<HttpHeader>();
        }

        #region Properties

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

        public bool GetChecked
        {
            get { return getChecked; }
            set
            {
                if (value.Equals(getChecked)) return;
                getChecked = value;
                NotifyOfPropertyChange(() => GetChecked);
            }
        }

        public bool PostChecked
        {
            get { return postChecked; }
            set
            {
                if (value.Equals(postChecked)) return;
                postChecked = value;
                NotifyOfPropertyChange(() => PostChecked);
            }
        }

        public bool PutChecked
        {
            get { return putChecked; }
            set
            {
                if (value.Equals(putChecked)) return;
                putChecked = value;
                NotifyOfPropertyChange(() => PutChecked);
            }
        }

        public bool DeleteChecked
        {
            get { return deleteChecked; }
            set
            {
                if (value.Equals(deleteChecked)) return;
                deleteChecked = value;
                NotifyOfPropertyChange(() => DeleteChecked);
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

            var method = GetMethod();

            var request = new RestRequest(uri, method);

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
            client.ExecuteAsync(request,
                r =>
                {
                    if (isWaiting)
                    {
                        stopWatch.Stop();
                        StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

                        var json = Newtonsoft.Json.Linq.JObject.Parse(r.Content);
                        RawResultsTextBox = json.ToString(Formatting.Indented);
                        StopSending();
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

        private Method GetMethod()
        {
            if (GetChecked)
            {
                return Method.GET;
            }
            if (PostChecked)
            {
                return Method.POST;
            }
            if (PutChecked)
            {
                return Method.PUT;
            }
            if (DeleteChecked)
            {
                return Method.DELETE;
            }
            return Method.GET;
        }

        #endregion
    }
}
