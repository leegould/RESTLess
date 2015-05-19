using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public sealed class RequestBuilderRawViewModel : Screen, ITabItem, IHandle<CreateRequestMessage>, IHandle<AddHeaderMessage>, IHandle<ClearMessage>
    {
        #region Private members

        private AppSettings appSettings;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private readonly IWindowManager windowManager;

        private string requestRawText;

        #endregion

        public RequestBuilderRawViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, IWindowManager windowManager, AppSettings appsettings)
        {
            DisplayName = "Raw Request";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            this.windowManager = windowManager;
            appSettings = appsettings;
        }

        protected override void OnDeactivate(bool close)
        {
            if (!string.IsNullOrEmpty(RequestRawText))
            {
                var parsedRequest = ParseRequest(RequestRawText);
                if (parsedRequest != null)
                {
                    eventAggregator.BeginPublishOnUIThread(new CreateRequestMessage { Request = parsedRequest });
                }
            }
            base.OnDeactivate(close);
        }

        private Request ParseRequest(string str)
        {
            try
            {
                Request request = new Request();

                if (!string.IsNullOrEmpty(str))
                {
                    // Method + Url
                    var lines = str.Split(new [] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var firstline = lines.FirstOrDefault();
                    lines.Remove(firstline);
                    if (firstline != null)
                    {
                        var flinesplit = firstline.Split(' ');
                        if (flinesplit.Length == 2)
                        {
                            request.Method = flinesplit[0];

                            var uri = new Uri(flinesplit[1]);
                            request.Url = new Uri(uri.Scheme + Uri.SchemeDelimiter + uri.Authority);
                            request.Path = uri.PathAndQuery;
                        }
                    }

                    // Headers
                    const string Headerpattern = @"([-\w]+)\s*:\s*(.+)$";
                    var headerregex = new Regex(Headerpattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    request.Headers = new Dictionary<string, string>();

                    var linesToRemove = new List<string>();

                    foreach (var hline in lines.Where(x => headerregex.IsMatch(x)))
                    {
                        var headerMatches = headerregex.Match(hline);
                        request.Headers.Add(headerMatches.Groups[1].ToString(), headerMatches.Groups[2].ToString());

                        linesToRemove.Add(hline);
                    }

                    if (linesToRemove.Any())
                    {
                        foreach (var rline in linesToRemove)
                        {
                            lines.Remove(rline);
                        }
                    }

                    // Body
                    if (lines.Any())
                    {
                        request.Body = string.Join("\n",lines);
                    }
                }
                return request;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region Properties

        public string RequestRawText
        {
            get { return requestRawText; }
            set
            {
                requestRawText = value;
                NotifyOfPropertyChange(() => RequestRawText);
            }
        }

        #endregion

        public void Handle(CreateRequestMessage message)
        {
            DisplayRequest(message.Request);
        }

        private void DisplayRequest(Request request)
        {
            if (request != null && request.Method != null && request.Url != null)
            {
                RequestRawText = (request.Method ?? string.Empty) + " " + (request.Url != null ? request.Url.AbsoluteUri : string.Empty) +
                                (request.Path.Length > 0 ? request.Path.Substring(1) : string.Empty)
                                + "\n";

                if (request.Headers != null)
                {
                    foreach (var header in request.Headers)
                    {
                        RequestRawText += header.Key + ": " + header.Value + "\n";
                    }
                }

                RequestRawText += "\n" + request.Body + "\n";
            }
        }

        public void Handle(AddHeaderMessage message)
        {
            RequestRawText.Insert(requestRawText.IndexOf('\n') + 1, message.Header + ": " + message.Value); // add after first line.
        }

        public void Handle(ClearMessage message)
        {
            RequestRawText = string.Empty;
        }
    }
}
