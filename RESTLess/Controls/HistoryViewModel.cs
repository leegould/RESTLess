using System;
using System.Linq;
using Caliburn.Micro;
using Raven.Client;
using RESTLess.Models;

namespace RESTLess.Controls
{
    public class HistoryViewModel : PropertyChangedBase
    {
        private readonly IDocumentStore documentStore;

        private IObservableCollection<Request> historyRequests;

        public IObservableCollection<Request> HistoryRequests
        {
            get { return historyRequests; }
            set
            {
                historyRequests = value;
                NotifyOfPropertyChange(() => HistoryRequests);
            }
        }

        public HistoryViewModel(IDocumentStore documentStore)
        {
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
                    // TODO : event aggregator to pass exception messages to main window?
                    //RawResultsTextBox = ex.ToString();
                }
            }
        }
    }
}
