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
        private bool patchChecked;
        private bool headChecked;
        private bool optionsChecked;
        private bool mergeChecked;

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

        public bool PatchChecked
        {
            get { return patchChecked; }
            set
            {
                if (value.Equals(patchChecked)) return;
                patchChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.PATCH });
                }
                NotifyOfPropertyChange(() => PatchChecked);
            }
        }

        public bool HeadChecked
        {
            get { return headChecked; }
            set
            {
                if (value.Equals(headChecked)) return;
                headChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.HEAD });
                }
                NotifyOfPropertyChange(() => HeadChecked);
            }
        }

        public bool OptionsChecked
        {
            get { return optionsChecked; }
            set
            {
                if (value.Equals(optionsChecked)) return;
                optionsChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.OPTIONS });
                }
                NotifyOfPropertyChange(() => OptionsChecked);
            }
        }

        public bool MergeChecked
        {
            get { return mergeChecked; }
            set
            {
                if (value.Equals(mergeChecked)) return;
                mergeChecked = value;
                if (value)
                {
                    eventAggregator.PublishOnUIThread(new MethodSelectedMessage { Method = Method.MERGE });
                }
                NotifyOfPropertyChange(() => MergeChecked);
            }
        }

        public void Handle(HistorySelectedMessage message)
        {
            if (message != null && message.Request != null)
            {
                var method = (Method) Enum.Parse(typeof (Method), message.Request.Method);
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
                    case Method.PATCH:
                        PatchChecked = true;
                        break;
                    case Method.HEAD:
                        HeadChecked = true;
                        break;
                    case Method.OPTIONS:
                        OptionsChecked = true;
                        break;
                    case Method.MERGE:
                        MergeChecked = true;
                        break;
                }
            }
        }
    }
}
