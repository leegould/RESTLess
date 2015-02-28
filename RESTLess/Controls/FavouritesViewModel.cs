using System;
using System.Windows.Controls;
using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class FavouritesViewModel : PropertyChangedBase, IHandle<FavouriteAddedMessage>
    {
        private const string IndexName = "Requests/Favourite/All";

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private Request selectedItem;

        private BindableCollection<Request> favouriteRequests;

        public BindableCollection<Request> FavouriteRequests
        {
            get { return favouriteRequests; }
            set
            {
                favouriteRequests = value;
                NotifyOfPropertyChange(() => FavouriteRequests);
            }
        }

        public Request SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                if (value != null)
                {
                    eventAggregator.PublishOnUIThread(new FavouriteSelectedMessage {Request = value});
                    NotifyOfPropertyChange(() => SelectedItem);
                }
            }
        }

        public FavouritesViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            FavouriteRequests = new BindableCollection<Request>();
            Load();
        }

        public async void RemoveFavourite(object source)
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
                            dbRequest.Favourite = false;
                            await conn.SaveChangesAsync();
                            eventAggregator.PublishOnUIThread(new FavouriteRemovedMessage() { Request = dbRequest });
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

        private async void Load()
        {
            using (var conn = documentStore.OpenAsyncSession())
            {
                try
                {
                    var items = await conn.Query<Request>(IndexName).ToListAsync();
                    FavouriteRequests.AddRange(items);
                }
                catch (Exception)
                {
                    // TODO : pass exception messages to main window - add to event aggregator
                    // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                }
            }
        }

        public void Handle(FavouriteAddedMessage message)
        {
            FavouriteRequests.Add(message.Request);
        }
    }
}
