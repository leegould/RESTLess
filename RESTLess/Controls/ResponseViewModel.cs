﻿using System;
using System.Globalization;
using System.Linq;
using System.Net;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client;
using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class ResponseViewModel: PropertyChangedBase, IHandle<HistorySelectedMessage>, IHandle<GroupedSelectedMessage>, IHandle<AppSettingsChangedMessage>, IHandle<FavouriteSelectedMessage>, IHandle<SearchSelectedMessage>, IHandle<ResponseReceivedMessage>
    {
        private AppSettings appSettings;

        private readonly IEventAggregator eventAggregator;

        public readonly IDocumentStore DocumentStore;

        private string rawResultsTextBox;
        private string htmlResultsBox;

        private string responseStatusTextBlock;
        private string responseWhenTextBlock;
        private string responseElapsedTextBlock;

        public ResponseViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, AppSettings appsettings)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            DocumentStore = documentStore;
            appSettings = appsettings;
        }

        #region Properties

        public string RawResultsTextBox
        {
            get { return rawResultsTextBox; }
            set
            {
                rawResultsTextBox = value;
                NotifyOfPropertyChange(() => RawResultsTextBox);
            }
        }

        public string HtmlResultsBox
        {
            get { return htmlResultsBox; }
            set
            {
                htmlResultsBox = value;
                NotifyOfPropertyChange(() => HtmlResultsBox);
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

        #endregion

        #region Message Handlers

        public void Handle(HistorySelectedMessage historyRequest)
        {
            var response = LoadResponseFromRequest(historyRequest.Request.Id);
            DisplayOrClear(response);
        }

        public void Handle(AppSettingsChangedMessage message)
        {
            appSettings = message.AppSettings;
            if (!appSettings.LoadResponses)
            {
                DisplayOrClear(null); // Clear
            }
        }

        public void Handle(FavouriteSelectedMessage message)
        {
            var response = LoadResponseFromRequest(message.Request.Id);
            DisplayOrClear(response);
        }

        public void Handle(GroupedSelectedMessage message)
        {
            var response = LoadResponseFromRequest(message.Request.Id);
            DisplayOrClear(response);
        }

        public void Handle(SearchSelectedMessage message)
        {
            var response = LoadResponseFromRequest(message.Request.Id);
            DisplayOrClear(response);
        }

        public void Handle(ResponseReceivedMessage message)
        {
            DisplayOrClear(message.Response);
        }

        #endregion

        #region Private Methods

        private Response LoadResponseFromRequest(string requestid)
        {
            if (appSettings.LoadResponses && !string.IsNullOrEmpty(requestid))
            {
                using (var docstore = DocumentStore.OpenSession())
                {
                    return docstore.Query<Response>().FirstOrDefault(x => x.RequestId == requestid);
                }
            }
            return null;
        }

        private void DisplayOrClear(Response response)
        {
            if (response != null)
            {
                DisplayResponse(response);
            }
            else
            {
                ResponseElapsedTextBlock = string.Empty;
                ResponseStatusTextBlock = string.Empty;
                ResponseWhenTextBlock = string.Empty;
                RawResultsTextBox = "No Response";
                HtmlResultsBox = "<p>No Response</p>";
            }
        }

        private void DisplayResponse(Response response)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    try
                    {
                        var formattedjson = JObject.Parse(response.Content).ToString(Formatting.Indented);
                        RawResultsTextBox = formattedjson;
                        HtmlResultsBox = "<pre>" + WebUtility.HtmlEncode(formattedjson) + "</pre>";
                    }
                    catch (JsonReaderException)
                    {
                        RawResultsTextBox = response.Content;
                        HtmlResultsBox = response.Content;
                    }
                }
                else
                {
                    RawResultsTextBox = string.Empty;
                }

                ResponseElapsedTextBlock = response.Elapsed + " ms.";
                ResponseStatusTextBlock = response.StatusCode + " " + response.StatusCodeDescription;

                var whentext = response.When.ToString(CultureInfo.InvariantCulture);
                var ago = DateTime.UtcNow.Subtract(response.When);
                if (ago.TotalMinutes > 1)
                {
                    whentext += " (-" + ago.ToString(@"d\.h\:mm\:ss") + ")";
                }

                ResponseWhenTextBlock = whentext;
            }
            catch (Exception ex)
            {
                RawResultsTextBox = ex.ToString();
            }
        }

        #endregion
    }
}