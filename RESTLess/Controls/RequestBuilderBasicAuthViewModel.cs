using System;
using System.Text;
using Caliburn.Micro;

using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderBasicAuthViewModel : Screen, ITabItem, IHandle<CreateRequestMessage>
    {
        #region Private members

        private readonly IEventAggregator eventAggregator;

        private string usernameTextBox;

        private string passwordTextBox;

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
                //eventAggregator.PublishOnUIThread(new AddHeaderMessage
                //{
                //    Header = "Authorization",
                //    Value = basicvalue
                //});

                if (request != null)
                {
                    request.Headers.Add("Authorization", basicvalue);
                }
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

        #endregion
    }
}
