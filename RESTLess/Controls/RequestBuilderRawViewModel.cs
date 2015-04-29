using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Windows.Forms.VisualStyles;
using Caliburn.Micro;
using Raven.Client;
using Raven.Database.Server.Controllers;
using RestSharp;
using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderRawViewModel : Screen, ITabItem, IHandle<CreateRequestMessage>
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
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            this.windowManager = windowManager;
            appSettings = appsettings;
        }

        protected override void OnDeactivate(bool close)
        {
            var parsedRequest = ParseRequest(RequestRawText);
            if (parsedRequest != null)
            {
                //var uri = new Uri(Url);
                //var restRequest = RequestBuilderViewModel.GetRestRequest(uri, SelectedMethod, VisualStyleElement.Tab.Body, Headers);
                //var request = new Request(uri, restRequest, VisualStyleElement.Tab.Body, appSettings.RequestSettings);
                //var restRequest = RequestBuilderViewModel.GetRestRequest(request.Url, (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), request.Method), request.Body, new BindableCollection<HttpHeader>(request.Headers.Select(x => new HttpHeader { Name = x.Key, Value = x.Value })));

                eventAggregator.BeginPublishOnUIThread(new CreateRequestMessage { Request = parsedRequest });
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
                    var lines = str.Split('\n').ToList();
                    var firstline = lines.FirstOrDefault();
                    lines.Remove(firstline);
                    if (firstline != null)
                    {
                        var flinesplit = firstline.Split(' ');
                        if (flinesplit.Length == 2)
                        {
                            request.Method = flinesplit[0];
                            request.Url = new Uri(flinesplit[1]);
                        }
                    }

                    // Headers
                    if (lines.Any(x => x.Contains(": ")))
                    {
                        request.Headers = new Dictionary<string, string>();
                        foreach (var headerline in lines.Where(x => x.Contains(": ")))
                        {
                            lines.Remove(headerline);
                            var headersplit = headerline.Split(new[] {": "}, StringSplitOptions.RemoveEmptyEntries);
                            if (headersplit.Length == 2)
                            {
                                request.Headers.Add(headersplit[0], headersplit[1]);
                            }
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
            RequestRawText = request.Method + " " + request.Url + "\n";
            foreach (var header in request.Headers)
            {
                RequestRawText += header.Key + ": " + header.Value + "\n";
            }
            RequestRawText += "\n" + request.Body + "\n";
        }
    }
}
