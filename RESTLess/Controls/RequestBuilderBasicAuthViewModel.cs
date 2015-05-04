using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Raven.Client;
using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderBasicAuthViewModel : Screen, ITabItem
    {
        #region Private members

        private AppSettings appSettings;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        private readonly IWindowManager windowManager;

        private string usernameTextBox;

        private string passwordTextBox;

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

        public RequestBuilderBasicAuthViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, IWindowManager windowManager, AppSettings appsettings)
        {
            DisplayName = "Basic Auth";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.documentStore = documentStore;
            this.windowManager = windowManager;
            appSettings = appsettings;
        }

        #region Button Actions

        public void SaveButton()
        {
            //if (!string.IsNullOrEmpty(SelectedType) && SelectedType == "Basic")
            //{
                if (!string.IsNullOrEmpty(UsernameTextBox) && !string.IsNullOrEmpty(PasswordTextBox))
                {
                    var basicvalue = "Basic " + Convert.ToBase64String(Encoding.Unicode.GetBytes(UsernameTextBox + ":" + PasswordTextBox));
                    eventAggregator.PublishOnUIThread(new AddHeaderMessage
                    {
                        Header = "Authorization",
                        Value = basicvalue
                    });
                }
            //}

            TryClose();
        }

        public void CancelButton()
        {
            TryClose();
        }

        #endregion
    }
}
