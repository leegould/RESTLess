using System;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Raven.Client;
using RESTLess.Models;

namespace RESTLess.Controls
{
    public class HistoryViewModel : PropertyChangedBase
    {
        private Request selectedItem;

        private readonly IDocumentStore documentStore;

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
                NotifyOfPropertyChange(() => SelectedItem);
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
                    // TODO : pass exception messages to main window?
                    //RawResultsTextBox = ex.ToString();
                }
            }
        }
    }
}
