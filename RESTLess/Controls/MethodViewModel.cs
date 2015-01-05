using System;

using Caliburn.Micro;
using RestSharp;

using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class MethodViewModel : PropertyChangedBase, IHandle<HistorySelectedMessage>
    {
        private readonly IEventAggregator eventAggregator;
        
        private bool getChecked;
        private bool postChecked;
        private bool putChecked;
        private bool deleteChecked;

        public MethodViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            getChecked = true; // default.
        }

        public bool GetChecked
        {
            get { return getChecked; }
            set
            {
                if (value.Equals(getChecked)) return;
                getChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.GET });
                }
                NotifyOfPropertyChange(() => GetChecked);
            }
        }

        public bool PostChecked
        {
            get { return postChecked; }
            set
            {
                if (value.Equals(postChecked)) return;
                postChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.POST });
                }                
                NotifyOfPropertyChange(() => PostChecked);
            }
        }

        public bool PutChecked
        {
            get { return putChecked; }
            set
            {
                if (value.Equals(putChecked)) return;
                putChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.PUT });
                }
                NotifyOfPropertyChange(() => PutChecked);
            }
        }

        public bool DeleteChecked
        {
            get { return deleteChecked; }
            set
            {
                if (value.Equals(deleteChecked)) return;
                deleteChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.DELETE });
                }
                NotifyOfPropertyChange(() => DeleteChecked);
            }
        }

        public void Handle(HistorySelectedMessage message)
        {
            var method = (Method)Enum.Parse(typeof(Method), message.Request.Method);
            switch (method)
            {
                case Method.GET:
                    GetChecked = true;
                    break;
                case Method.POST:
                    PostChecked = true;
                    break;
                case Method.PUT:
                    PutChecked = true;
                    break;
                case Method.DELETE:
                    DeleteChecked = true;
                    break;
            }
        }
    }
}
