using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;
using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderDigestAuthViewModel : Screen, ITabItem //, IHandle<CreateRequestMessage>, IHandle<ClearMessage>
    {
        private readonly IEventAggregator eventAggregator;

        public RequestBuilderDigestAuthViewModel(IEventAggregator eventAggregator)
        {
            DisplayName = "Basic Auth";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }
    }
}
