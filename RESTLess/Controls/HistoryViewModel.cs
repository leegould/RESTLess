using System;
using System.Linq;
using Caliburn.Micro;
using Raven.Client;
using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class HistoryViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;
        
        private Request selectedItem;

        private BindableCollection<Request> historyRequests;

        public BindableCollection<Request> HistoryRequests
        {
            get { return historyRequests; }
            set
            {
                historyRequests = value;
                NotifyOfPropertyChange(() => HistoryRequests);
            }
        }

        public Request SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                eventAggregator.PublishOnUIThread(new HistorySelectedMessage { Request = value });
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        public HistoryViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            this.documentStore = documentStore;
            HistoryRequests = new BindableCollection<Request>();
            LoadHistory();
        }

        private void LoadHistory()
        {
            using (var conn = documentStore.OpenSession())
            {
                try
                {
                    var items = conn.Query<Request>().ToList();
                    HistoryRequests.AddRange(items);
                }
                catch (Exception ex)
                {
                    // TODO : pass exception messages to main window - add to event aggregator
                    // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                }
            }
        }
    }
}
