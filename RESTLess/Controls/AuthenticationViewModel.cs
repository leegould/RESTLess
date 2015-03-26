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
        private BindableCollection<string> typesComboBox;

        private string selectedType;

        private string usernameTextBox;

        private string passwordTextBox;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        #region Properties

        public BindableCollection<string> TypesComboBox
        {
            get { return typesComboBox; }
            set
            {
                typesComboBox = value;
                NotifyOfPropertyChange(() => TypesComboBox);
            }
        }

        public string SelectedType
        {
            get { return selectedType; }
            set
            {
                if (selectedType != value)
                {
                    selectedType = value;
                    NotifyOfPropertyChange(() => SelectedType);
                }  
            }
        }

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
            TypesComboBox = new BindableCollection<string>
            {
                "Basic"
            };
        }

        #region Button Actions

        public void SaveButton()
        {
            if (!string.IsNullOrEmpty(SelectedType) && SelectedType == "Basic")
            {
                if (!string.IsNullOrEmpty(UsernameTextBox) && !string.IsNullOrEmpty(PasswordTextBox))
                {
                    var basicvalue = "Basic " +
                                     Convert.ToBase64String(
                                         Encoding.Unicode.GetBytes(UsernameTextBox + ":" + PasswordTextBox));
                    eventAggregator.PublishOnUIThread(new AddHeaderMessage
                    {
                        Header = "Authorization",
                        Value = basicvalue
                    });
                }
            }

            TryClose();
        }

        public void CancelButton()
        {
            TryClose();
        }

        #endregion
    }
}
