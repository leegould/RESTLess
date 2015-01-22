using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using RESTLess.Models;

namespace RESTLess.Controls
{
    public class GroupedViewModel : PropertyChangedBase
    {
        private BindableCollection<Request> groupedRequests;

        public BindableCollection<Request> GroupedRequests
        {
            get { return groupedRequests; }
            set
            {
                groupedRequests = value;
                NotifyOfPropertyChange(() => GroupedRequests);
            }
        }
    }
}
