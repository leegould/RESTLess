using System.ComponentModel.Composition;

using Caliburn.Micro;

using Raven.Client;

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
