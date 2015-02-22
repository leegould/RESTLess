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
    public class FavouritesViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        public FavouritesViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
        }
    }
}
