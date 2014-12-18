using Caliburn.Micro;

using RestSharp;

namespace RESTLess
{
    public class MethodViewModel : PropertyChangedBase
    {
        private bool getChecked;
        private bool postChecked;
        private bool putChecked;
        private bool deleteChecked;

        public MethodViewModel()
        {
            getChecked = true; // default.
        }

        public bool GetChecked
        {
            get { return getChecked; }
            set
            {
                if (value.Equals(getChecked)) return;
                getChecked = value;
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
                NotifyOfPropertyChange(() => DeleteChecked);
            }
        }

        public Method GetMethod()
        {
            if (GetChecked)
            {
                return Method.GET;
            }
            if (PostChecked)
            {
                return Method.POST;
            }
            if (PutChecked)
            {
                return Method.PUT;
            }
            if (DeleteChecked)
            {
                return Method.DELETE;
            }
            return Method.GET;
        }
    }
}
