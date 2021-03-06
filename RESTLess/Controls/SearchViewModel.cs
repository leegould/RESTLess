﻿using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class SearchViewModel : Screen, ITabItem, IHandle<DeleteAllHistoryMessage>, IHandle<DeleteHistoryBeforeTodayMessage>
    {
        private const string RequestsIndexName = "Requests/All";

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private string searchTextBox;

        private Request selectedItem;

        private BindableCollection<Request> searchRequests;

        public BindableCollection<Request> SearchRequests
        {
            get { return searchRequests; }
            set
            {
                searchRequests = value;
                NotifyOfPropertyChange(() => SearchRequests);
            }
        }

        public string SearchTextBox
        {
            get { return searchTextBox; }
            set
            {
                searchTextBox = value;
                NotifyOfPropertyChange(SearchTextBox);
            }
        }

        public Request SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                eventAggregator.PublishOnUIThread(new SearchSelectedMessage { Request = value });
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        public SearchViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            DisplayName = "Search";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            SearchRequests = new BindableCollection<Request>();
        }

        public void SearchButton()
        {
            SearchRequests.Clear();

            if (!string.IsNullOrEmpty(SearchTextBox))
            {
                using (var conn = documentStore.OpenSession())
                {
                    var results = conn.Query<Request>(RequestsIndexName).Search(x => x.Path, SearchTextBox);
                    SearchRequests.AddRange(results);
                }
            }
        }

        public void Handle(DeleteAllHistoryMessage message)
        {
            SearchRequests.Clear();
            SearchRequests = new BindableCollection<Request>();
        }

        public void Handle(DeleteHistoryBeforeTodayMessage message)
        {
            SearchRequests.Clear();
            SearchRequests = new BindableCollection<Request>();
        }
    }
}
