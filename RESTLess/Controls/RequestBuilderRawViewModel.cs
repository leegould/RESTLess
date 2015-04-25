using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Raven.Client;
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
