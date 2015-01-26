﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Raven.Abstractions.Extensions;
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
            GroupedRequests = new BindableCollection<RequestGrouped>();
            LoadGrouped();
        }

        private async void LoadGrouped()
        {
            using (var conn = documentStore.OpenAsyncSession())
            {
                try
                {
                    var items = await conn.Query<ResultGrouped>(IndexName).Take(100).ToListAsync();
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
                            var pathparts = new Stack<string>();
                            foreach (var p in item.Path.Split(new []{ '/' }, StringSplitOptions.RemoveEmptyEntries ))
                            {
                                pathparts.Push(p);
                            }

                            if (pathparts.Count > 0)
                            {
                                var child = GetChild(requestgrouped.Id, pathparts);
                                requestgrouped.Children.Add(child);
                            }
                        }

                        //GroupedRequests.Add(requestgrouped);
                    }
                }
                catch (Exception)
                {
                    // TODO : pass exception messages to main window - add to event aggregator
                    // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                }
            }
        }

        private static RequestGrouped GetChild(string id, Stack<string> parts)
        {
            var part = parts.Pop();
            var item = new RequestGrouped {Id = id, Part = part };
            if (parts.Count > 0)
            {
                item.Children.Add(GetChild(id, parts));
            }

            return item;
        }
    }
}
