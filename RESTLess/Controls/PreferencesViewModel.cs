using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

using Raven.Client;
using Raven.Client.Document;

using RESTLess.Models;

namespace RESTLess.Controls
{
    [Export(typeof(PreferencesViewModel))]
    public class PreferencesViewModel : Screen
    {
        private string timeoutTextBox;

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


        public PreferencesViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
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
                        Timeout = int.Parse(timeoutTextBox)
                    };

                conn.SaveChanges();
            }
        }

        public void CancelButton()
        {
            TryClose();
        }
    }
}
