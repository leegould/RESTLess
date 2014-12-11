﻿using System;
using System.Diagnostics;

using Caliburn.Micro;

using RestSharp;

namespace RESTLess
{
    public class AppViewModel : PropertyChangedBase
    {
        #region Private members
        
        private string rawResultsTextBlock;
        private string url;

        private bool getChecked;
        private bool postChecked;
        private bool putChecked;
        private bool deleteChecked;

        #endregion


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

        public bool CanSendButton
        {
            get
            {
                return !string.IsNullOrWhiteSpace(UrlTextBox);
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

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            RawResultsTextBlock = "Loading " + request.Method + " " + client.BaseUrl + request.Resource + "\n";
            
            client.ExecuteAsync(request,
                r =>
                {
                    stopWatch.Stop();
                    RawResultsTextBlock += "Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + "ms | StatusCode: " + r.ResponseStatus + " " + r.StatusCode + " | Content:" + r.Content;
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
