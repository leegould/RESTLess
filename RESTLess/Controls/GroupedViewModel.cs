using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;

namespace RESTLess.Controls
{
    public class GroupedViewModel : PropertyChangedBase
    {
        private const string IndexName = "Requests/Grouped/All";

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private Request selectedItem;

        private BindableCollection<ResultGrouped> groupedRequests;

        public BindableCollection<ResultGrouped> GroupedRequests
        {
            get { return groupedRequests; }
            set
            {
                groupedRequests = value;
                NotifyOfPropertyChange(() => GroupedRequests);
            }
        }

        public Request SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                //eventAggregator.PublishOnUIThread(new HistorySelectedMessage { Request = value });
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        public GroupedViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            GroupedRequests = new BindableCollection<ResultGrouped>();
            LoadGrouped();
        }

        private async void LoadGrouped()
        {
            using (var conn = documentStore.OpenAsyncSession())
            {
                try
                {
                    var items = await conn.Query<ResultGrouped>(IndexName).Take(100).ToListAsync();
                    GroupedRequests.AddRange(items);
                }
                catch (Exception)
                {
                    // TODO : pass exception messages to main window - add to event aggregator
                    // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                }
            }
        }
    }
}
