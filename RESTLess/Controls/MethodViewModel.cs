using Caliburn.Micro;
using RestSharp;

namespace RESTLess.Controls
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

        public Method Method
        {
            get
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
            set
            {
                switch (value)
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
                        deleteChecked = true;
                        break;
                }
            }
        }

        //public Method GetMethod()
        //{
        //    if (GetChecked)
        //    {
        //        return Method.GET;
        //    }
        //    if (PostChecked)
        //    {
        //        return Method.POST;
        //    }
        //    if (PutChecked)
        //    {
        //        return Method.PUT;
        //    }
        //    if (DeleteChecked)
        //    {
        //        return Method.DELETE;
        //    }
        //    return Method.GET;
        //}

        //public void SetMethod(Method method)
        //{
        //    switch (method)
        //    {
        //        case Method.GET:
        //            GetChecked = true;
        //            break;
        //        case Method.POST:
        //            PostChecked = true;
        //            break;
        //        case Method.PUT:
        //            PutChecked = true;
        //            break;
        //        case Method.DELETE:
        //            deleteChecked = true;
        //            break;
        //    }
        //}
    }
}
