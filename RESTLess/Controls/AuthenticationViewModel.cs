using System;
using System.ComponentModel.Composition;
using System.Text;

using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    [Export(typeof(AuthenticationViewModel))]
    class AuthenticationViewModel : Screen
    {
        private string usernameTextBox;

        private string passwordTextBox;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

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

        #region Button Actions

        public void SaveButton()
        {
            if (!string.IsNullOrEmpty(UsernameTextBox) && !string.IsNullOrEmpty(PasswordTextBox))
            {
                var basicvalue = "Basic " + Convert.ToBase64String(Encoding.Unicode.GetBytes(UsernameTextBox + ":" + PasswordTextBox));
                eventAggregator.PublishOnUIThread(new AddHeaderMessage { Header = "Authorization", Value = basicvalue });
            }

            TryClose();
        }

        public void CancelButton()
        {
            TryClose();
        }

        #endregion

        public AuthenticationViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        #region Button Actions

        #endregion

    }
}
