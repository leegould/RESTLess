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
    public class RequestBuilderDigestAuthViewModel : Screen, ITabItem, IHandle<CreateRequestMessage>, IHandle<ClearMessage>
    {
        private readonly IEventAggregator eventAggregator;

        private string usernameTextBox;

        private string passwordTextBox;

        private string realmTextBox;

        private string nonceTextBox;

        private string algoTextBox;

        private string qopTextBox;

        private int nonceCount;

        private string clientNonceTextBox;

        private string opaqueTextBox;

        private string actionLabel;

        private Request request;

        #region Properties

        public string UsernameTextBox
        {
            get { return usernameTextBox; }
            set
            {
                usernameTextBox = value;
                NotifyOfPropertyChange(() => UsernameTextBox);
            }
        }

        public string PasswordTextBox
        {
            get { return passwordTextBox; }
            set
            {
                passwordTextBox = value;
                NotifyOfPropertyChange(() => PasswordTextBox);
            }
        }

        public string RealmTextBox
        {
            get { return realmTextBox; }
            set
            {
                realmTextBox = value;
                NotifyOfPropertyChange(() => RealmTextBox);
            }
        }

        public string NonceTextBox
        {
            get { return nonceTextBox; }
            set
            {
                nonceTextBox = value;
                NotifyOfPropertyChange(() => NonceTextBox);
            }
        }

        public string AlgoTextBox
        {
            get { return algoTextBox; }
            set
            {
                algoTextBox = value;
                NotifyOfPropertyChange(() => AlgoTextBox);
            }
        }

        public string QopTextBox
        {
            get { return qopTextBox; }
            set
            {
                qopTextBox = value;
                NotifyOfPropertyChange(() => QopTextBox);
            }
        }

        public int NonceCount
        {
            get { return nonceCount; }
            set
            {
                nonceCount = value;
                NotifyOfPropertyChange(() => NonceCount);
            }
        }

        public string ClientNonceTextBox
        {
            get { return clientNonceTextBox; }
            set
            {
                clientNonceTextBox = value;
                NotifyOfPropertyChange(() => ClientNonceTextBox);
            }
        }

        public string OpaqueTextBox
        {
            get { return opaqueTextBox; }
            set
            {
                opaqueTextBox = value;
                NotifyOfPropertyChange(() => OpaqueTextBox);
            }
        }

        public string ActionLabel
        {
            get { return actionLabel; }
            set
            {
                actionLabel = value;
                NotifyOfPropertyChange(() => ActionLabel);
            }
        }

        #endregion

        protected override void OnDeactivate(bool close)
        {
            if (request != null)
            {
                eventAggregator.BeginPublishOnUIThread(new CreateRequestMessage { Request = request });
            }

            base.OnDeactivate(close);
        }

        public RequestBuilderDigestAuthViewModel(IEventAggregator eventAggregator)
        {
            DisplayName = "Digest Auth";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        #region Button Actions

        // TODO

        public void AddButton()
        {
            if (!string.IsNullOrEmpty(UsernameTextBox) && !string.IsNullOrEmpty(PasswordTextBox))
            {
                
            }
        }

        public void ClearButton()
        {
            UsernameTextBox = string.Empty;
            PasswordTextBox = string.Empty;
        }

        #endregion

        #region Handlers

        public void Handle(CreateRequestMessage message)
        {
            request = message.Request;
        }


        public void Handle(ClearMessage message)
        {
            UsernameTextBox = string.Empty;
            PasswordTextBox = string.Empty;
            ActionLabel = string.Empty;
            request = null;
        }

        #endregion
    }
}
