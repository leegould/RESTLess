using System;
using System.Diagnostics;

using Caliburn.Micro;

using RestSharp;

namespace RESTLess
{
    public class AppViewModel : PropertyChangedBase
    {
        private string rawResultsTextBlock;
        private string url;

        public string UrlTextBox
        {
            get { return url; }
            set
            {
                url = value;
                NotifyOfPropertyChange(() => UrlTextBox);
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

        public void SendButton()
        {
            var uri = new Uri(UrlTextBox);
            RestClient client = new RestClient(uri.GetLeftPart(UriPartial.Authority));

            var request = new RestRequest(uri, Method.GET);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            client.ExecuteAsync(request,
                r =>
                {
                    stopWatch.Stop();
                    RawResultsTextBlock = stopWatch.ElapsedMilliseconds.ToString() + "|" + r.StatusCode + "|" + r.Content;
                });
        }

    }
}
