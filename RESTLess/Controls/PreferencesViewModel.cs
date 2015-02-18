using System.ComponentModel.Composition;
using System.Linq;

using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;

namespace RESTLess.Controls
{
    [Export(typeof(PreferencesViewModel))]
    public class PreferencesViewModel : Screen
    {
        private string timeoutTextBox;

        private bool loadResponsesChecked;

        private readonly IEventAggregator eventAggregator;

        private readonly IDocumentStore documentStore;

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


        public PreferencesViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            using (var conn = documentStore.OpenSession())
            {
                var appsettings = conn.Query<AppSettings>().FirstOrDefault();
                if (appsettings != null && appsettings.RequestSettings != null)
                {
                    TimeoutTextBox = appsettings.RequestSettings.Timeout.ToString();
                    LoadResponsesChecked = appsettings.LoadResponses;
                }
                else
                {
                    TimeoutTextBox = "60000"; // Default
                    LoadResponsesChecked = true;
                }
            }
        }
        
        public void SaveButton()
        {
            using (var conn = documentStore.OpenSession())
            {
                var appsettings = conn.Query<AppSettings>().FirstOrDefault();
                if (appsettings == null)
                {
                    appsettings = new AppSettings
                                  {
                                      Height = 600,
                                      Width = 800
                                  };
                    conn.Store(appsettings);
                }

                appsettings.RequestSettings = 
                    new RequestSettings
                    {
                        Timeout = int.Parse(TimeoutTextBox)
                    };

                appsettings.LoadResponses = LoadResponsesChecked;

                conn.SaveChanges();
            }
        }

        public void CancelButton()
        {
            TryClose();
        }
    }
}
