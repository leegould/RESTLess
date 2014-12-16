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

        private string rawResultsTextBlock;
        private string url;
        private string body;

        private bool getChecked;
        private bool postChecked;
        private bool putChecked;
        private bool deleteChecked;

        private IObservableCollection<HttpHeader> headers;

        private string statusBarTextBlock;

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

        public string RawResultsTextBlock
        {
            get { return rawResultsTextBlock; }
            set
            {
                rawResultsTextBlock = value;
                NotifyOfPropertyChange(() => RawResultsTextBlock);
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
                return !string.IsNullOrWhiteSpace(UrlTextBox);
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

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            StatusBarTextBlock = "Sending " + request.Method + " " + client.BaseUrl.ToString().Substring(0, client.BaseUrl.ToString().Length - 1) + request.Resource;

            client.ExecuteAsync(request,
                r =>
                {
                    stopWatch.Stop();
                    StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

                    var json = Newtonsoft.Json.Linq.JObject.Parse(r.Content);
                    RawResultsTextBlock = json.ToString(Formatting.Indented);
                });
        }

        #endregion


        #region Private Methods

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
