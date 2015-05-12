using System;
using System.Text;
using Caliburn.Micro;

using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderBasicAuthViewModel : Screen, ITabItem, IHandle<CreateRequestMessage>, IHandle<ClearMessage>
    {
        private const string AuthorizationHeaderString = "Authorization";

        #region Private members

        private readonly IEventAggregator eventAggregator;

        private string usernameTextBox;

        private string passwordTextBox;

        private string actionLabel;

        private Request request;

        #endregion

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

        public RequestBuilderBasicAuthViewModel(IEventAggregator eventAggregator)
        {
            DisplayName = "Basic Auth";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        #region Button Actions

        public void AddButton()
        {
            if (!string.IsNullOrEmpty(UsernameTextBox) && !string.IsNullOrEmpty(PasswordTextBox))
            {
                var basicvalue = "Basic " + Convert.ToBase64String(Encoding.Unicode.GetBytes(UsernameTextBox + ":" + PasswordTextBox));

                if (request != null)
                {
                    if (request.Headers.ContainsKey(AuthorizationHeaderString))
                    {
                        request.Headers.Remove(AuthorizationHeaderString);
                    }

                    request.Headers.Add(AuthorizationHeaderString, basicvalue);
                    ActionLabel = "Added!";
                }
            }
        }

        public void ClearButton()
        {
            UsernameTextBox = string.Empty;
            PasswordTextBox = string.Empty;

            if (request != null && request.Headers.ContainsKey(AuthorizationHeaderString))
            {
                request.Headers.Remove(AuthorizationHeaderString);
                ActionLabel = "Removed!";
            }
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
        }

        #endregion

    }
}
