using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Abstractions.Extensions;
using Raven.Client;
using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class ResponseViewModel: PropertyChangedBase, IHandle<HistorySelectedMessage>, IHandle<GroupedSelectedMessage>, IHandle<AppSettingsChangedMessage>, IHandle<FavouriteSelectedMessage>, IHandle<SearchSelectedMessage>, IHandle<ResponseReceivedMessage>, IHandle<DeleteAllHistoryMessage>, IHandle<ViewStartMessage>
    {
        private const string DefaultHtmlDisplay = "Controls/Help.html";

        private readonly Brush defaultColor = Brushes.DarkGray;

        private readonly Dictionary<int, Color> resultColors = new Dictionary<int, Color>
        {
            { 200, Color.FromArgb(255, 0, 255, 0) },
            { 401, Color.FromArgb(255, 255, 255, 0) },
            { 404, Color.FromArgb(255, 255, 0, 0) },
            { 500, Color.FromArgb(255, 255, 0, 0) }
        }; 

        private AppSettings appSettings;

        private readonly IEventAggregator eventAggregator;

        public readonly IDocumentStore DocumentStore;

        private string rawResultsTextBox;
        private string headersTextBox;
        private string htmlResultsBox;
        private Brush resultColor;

        private string responseStatusTextBlock;
        private string responseWhenTextBlock;
        private string responseElapsedTextBlock;

        public ResponseViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, AppSettings appsettings)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            DocumentStore = documentStore;
            appSettings = appsettings;
            if (File.Exists(DefaultHtmlDisplay))
            {
                HtmlResultsBox = File.ReadAllText(DefaultHtmlDisplay);
            }
            ResultColor = defaultColor;
        }

        #region Properties

        public Brush ResultColor
        {
            get
            {
                return resultColor;
            }

            set
            {
                resultColor = value;
                NotifyOfPropertyChange(() => ResultColor);
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

        public string HeadersTextBox
        {
            get { return headersTextBox; }
            set
            {
                headersTextBox = value;
                NotifyOfPropertyChange(() => HeadersTextBox);
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

        public void Handle(DeleteAllHistoryMessage message)
        {
            DisplayOrClear(null);
        }

        public void Handle(ViewStartMessage message)
        {
            HtmlResultsBox = File.ReadAllText(DefaultHtmlDisplay);
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
                HeadersTextBox = "No Response";
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
                        HeadersTextBox = string.Join("\n", response.Headers.Select(x => x.Key + ": " + x.Value));
                        HtmlResultsBox = "<pre>" + WebUtility.HtmlEncode(formattedjson) + "</pre>";
                    }
                    catch (JsonReaderException)
                    {
                        RawResultsTextBox = response.Content;
                        HeadersTextBox = response.Headers.Count.ToString();
                        HtmlResultsBox = response.Content;
                    }
                }
                else
                {
                    RawResultsTextBox = string.Empty;
                    HeadersTextBox = string.Empty;
                }

                ResponseElapsedTextBlock = response.Elapsed + " ms.";
                ResponseStatusTextBlock = response.StatusCode + " " + response.StatusCodeDescription;

                if (resultColors.ContainsKey(response.StatusCode))
                {
                    ResultColor = new RadialGradientBrush(resultColors[response.StatusCode], CreateEndColor(resultColors[response.StatusCode]) );
                }
                else
                {
                    ResultColor = defaultColor;
                }

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

        private Color CreateEndColor(Color color)
        {
            var newColor = color;
            newColor.A = newColor.A > 50 ? (Byte)(newColor.A - 50) : (Byte)0;
            newColor.R = newColor.R > 50 ? (Byte)(newColor.R - 50) : (Byte)0;
            newColor.G = newColor.G > 50 ? (Byte)(newColor.G - 50) : (Byte)0;
            newColor.B = newColor.B > 50 ? (Byte)(newColor.B - 50) : (Byte)0;
            return newColor;
        }

        #endregion
    }
}

