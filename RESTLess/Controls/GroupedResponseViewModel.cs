using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Raven.Client;
using RESTLess.Models;
using RESTLess.Models.Interface;

namespace RESTLess.Controls
{
    public class GroupedResponseViewModel : Screen, ITabItem
    {
        private const string IndexName = "Requests/GroupedResponse/All";

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private BindableCollection<RequestGroupedByResponse> groupedByResponses;

        public BindableCollection<RequestGroupedByResponse> GroupedByResponses
        {
            get { return groupedByResponses; }
            set
            {
                groupedByResponses = value;
                NotifyOfPropertyChange(() => GroupedByResponses);
            }
        }

        public GroupedResponseViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            DisplayName = "By Response";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            GroupedByResponses = new BindableCollection<RequestGroupedByResponse>();
            //LoadGrouped();
        }
    }
}
