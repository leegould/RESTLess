using System;
using System.Linq;
using Caliburn.Micro;
using Raven.Client;
using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public sealed class HistoryViewModel : Screen, ITabItem, IHandle<RequestSavedMessage>
    {
        private const string RequestsIndexName = "Requests/All";

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
            DisplayName = "History";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            HistoryRequests = new BindableCollection<Request>();
            LoadHistory();
        }

        public async void Favourite(object source)
        {
            var request = source as Request;
            if (request != null)
            {
                using (var conn = documentStore.OpenAsyncSession())
                {
                    try
                    {
                        var requestid = request.Id;
                        var dbRequest = await conn.LoadAsync<Request>(requestid);

                        if (dbRequest != null)
                        {
                            dbRequest.Favourite = true;
                            await conn.SaveChangesAsync();
                            eventAggregator.PublishOnUIThread(new FavouriteSelectedMessage() { Request = dbRequest });
                        }
                    }
                    catch (Exception)
                    {
                        // TODO : pass exception messages to main window - add to event aggregator
                        // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                    }
                }
            }
        }

        public void Handle(RequestSavedMessage message)
        {
            HistoryRequests.Insert(0, message.Request);
        }

        private async void LoadHistory()
        {
            using (var conn = documentStore.OpenAsyncSession())
            {
                try
                {
                    var items = await conn.Query<Request>(RequestsIndexName).Take(50).OrderByDescending(x => x.When).ToListAsync();
                    HistoryRequests.AddRange(items);
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
