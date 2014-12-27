using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using RESTLess.Models;

namespace RESTLess.Controls
{
    public class HistoryViewModel : PropertyChangedBase
    {
        private IObservableCollection<Request> historyRequests;

        public IObservableCollection<Request> HistoryRequests
        {
            get { return historyRequests; }
            set
            {
                historyRequests = value;
                NotifyOfPropertyChange(() => HistoryRequests);
            }
        }

        public HistoryViewModel()
        {
            HistoryRequests = new BindableCollection<Request>();            
        }
    }
}
