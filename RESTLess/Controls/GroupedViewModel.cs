using System;
using System.Collections.Generic;
using System.Linq;

using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public sealed class GroupedViewModel : Screen, ITabItem, IHandle<DeleteAllHistoryMessage>, IHandle<DeleteHistoryBeforeTodayMessage>
    {
        private const string UrlIndexName = "Requests/Grouped/All";

        private const string StatusIndexName = "Requests/Grouped/Status";

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private bool urlGroupedChecked;
        private bool responseGroupedChecked;

        private BindableCollection<RequestGrouped> groupedRequests;

        public BindableCollection<RequestGrouped> GroupedRequests
        {
            get { return groupedRequests; }
            set
            {
                groupedRequests = value;
                NotifyOfPropertyChange(() => GroupedRequests);
            }
        }

        public GroupedViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            DisplayName = "Grouped";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            GroupedRequests = new BindableCollection<RequestGrouped>();
            UrlGroupedChecked = true;
            //LoadUrlGrouped();
        }

        public bool UrlGroupedChecked
        {
            get { return urlGroupedChecked; }
            set
            {
                if (value.Equals(urlGroupedChecked)) return;
                urlGroupedChecked = value;
                LoadUrlGrouped();
                NotifyOfPropertyChange(() => UrlGroupedChecked);
            }
        }

        public bool ResponseGroupedChecked
        {
            get { return responseGroupedChecked; }
            set
            {
                if (value.Equals(responseGroupedChecked)) return;
                responseGroupedChecked = value;
                LoadResponseGrouped();
                NotifyOfPropertyChange(() => ResponseGroupedChecked);
            }
        }


        public void SetSelectedItem(RequestGrouped item)
        {
            if (item != null)
            {
                eventAggregator.PublishOnUIThread(new GroupedSelectedMessage { Request = item });
            }
        }

        private async void LoadResponseGrouped()
        {
            GroupedRequests.Clear();
            GroupedRequests = new BindableCollection<RequestGrouped>();

            using (var conn = documentStore.OpenAsyncSession())
            {
                try
                {
                    var items = await conn.Query<ResponseStatusGrouped>(StatusIndexName).ToListAsync();
                    foreach (var item in items)
                    {
                        if (item != null)
                        {
                            RequestGrouped requestgrouped = GroupedRequests.FirstOrDefault(x => x.Part == item.StatusCode.ToString());

                            if (requestgrouped == null)
                            {
                                requestgrouped = new RequestGrouped { Id = item.RequestId, Part = item.StatusCode.ToString() };
                                GroupedRequests.Add(requestgrouped);
                            }
                            
                            var pathparts = new Queue<string>();

                            pathparts.Enqueue(item.Url);

                            foreach (var p in item.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                pathparts.Enqueue(p);
                            }

                            if (pathparts.Count > 0)
                            {
                                PopulateChildren(requestgrouped, pathparts, item.RequestId);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // TODO : pass exception messages to main window - add to event aggregator
                    // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                }
            }

            // TODO
        }

        private async void LoadUrlGrouped()
        {
            GroupedRequests.Clear();
            GroupedRequests = new BindableCollection<RequestGrouped>();

            using (var conn = documentStore.OpenAsyncSession())
            {
                try
                {
                    var items = await conn.Query<ResultGrouped>(UrlIndexName).ToListAsync();
                    foreach(var item in items)
                    {
                        RequestGrouped requestgrouped = GroupedRequests.FirstOrDefault(x => x.Part == item.Url);
                        if (requestgrouped == null)
                        {
                            requestgrouped = new RequestGrouped {Id = item.Id, Part = item.Url};
                            GroupedRequests.Add(requestgrouped);
                        }

                        if (!string.IsNullOrEmpty(item.Path))
                        {
                            var pathparts = new Queue<string>();
                            foreach (var p in item.Path.Split(new []{ '/' }, StringSplitOptions.RemoveEmptyEntries ))
                            {
                                pathparts.Enqueue(p);
                            }

                            if (pathparts.Count > 0)
                            {
                                PopulateChildren(requestgrouped, pathparts, item.Id);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // TODO : pass exception messages to main window - add to event aggregator
                    // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                }
            }
        }

        private static void PopulateChildren(RequestGrouped requestGrouped, Queue<string> parts, string itemid)
        {
            var part = parts.Dequeue();
            var child = requestGrouped.Children.FirstOrDefault(x => x.Part == part);

            if (child == null)
            {
                child = new RequestGrouped { Id = itemid, Part = part };
                requestGrouped.Children.Add(child);
            }

            if (parts.Count > 0)
            {
                PopulateChildren(child, parts, itemid);
            }
        }

        public void Handle(DeleteAllHistoryMessage message)
        {
            GroupedRequests.Clear();
        }

        public void Handle(DeleteHistoryBeforeTodayMessage message)
        {
            GroupedRequests.Clear();
        }
    }
}
