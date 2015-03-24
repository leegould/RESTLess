using System.ComponentModel.Composition;
using System.Linq;

using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    [Export(typeof(PreferencesViewModel))]
    public class PreferencesViewModel : Screen
    {
        private string timeoutTextBox;

        private bool loadResponsesChecked;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

        #region Properties

        public string TimeoutTextBox
        {
            get { return timeoutTextBox; }
            set
            {
                timeoutTextBox = value;
                NotifyOfPropertyChange(() => TimeoutTextBox);
            }
        }

        public bool LoadResponsesChecked
        {
            get { return loadResponsesChecked; }
            set
            {
                loadResponsesChecked = value;
                NotifyOfPropertyChange(() => LoadResponsesChecked);
            }
        }

        #endregion

        public PreferencesViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        #region Button Actions

        public void SaveButton()
        {
            using (var conn = documentStore.OpenSession())
            {
                var appsettings = conn.Query<AppSettings>().FirstOrDefault();

                if (appsettings == null)
                {
                    appsettings = AppSettings.CreateDefault();
                    conn.Store(appsettings);
                }

                appsettings.RequestSettings.Timeout = int.Parse(TimeoutTextBox);
                appsettings.LoadResponses = LoadResponsesChecked;
                
                conn.SaveChanges();

                eventAggregator.PublishOnUIThread(new AppSettingsChangedMessage { AppSettings = appsettings });

                TryClose();
            }
        }

        public void CancelButton()
        {
            TryClose();
        }

        #endregion

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            using (var conn = documentStore.OpenSession())
            {
                var appsettings = conn.Query<AppSettings>().FirstOrDefault() ?? AppSettings.CreateDefault();
                LoadValues(appsettings);
            }
        }

        private void LoadValues(AppSettings appsettings)
        {
            LoadResponsesChecked = appsettings.LoadResponses;

            if (appsettings.RequestSettings != null)
            {
                TimeoutTextBox = appsettings.RequestSettings.Timeout.ToString();
            }
        }
    }
}
