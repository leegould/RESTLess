using System;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Windows;

using Caliburn.Micro;

using Raven.Client;

using RESTLess.Controls;
using RESTLess.Extensions;
using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : Conductor<ITabItem>.Collection.OneActive, IApp, IHandle<HistorySelectedMessage>, IHandle<GroupedSelectedMessage>, IHandle<FavouriteSelectedMessage>, IHandle<SearchSelectedMessage>, IHandle<ResponseReceivedMessage>
    {
        #region Private members

        private readonly IEventAggregator eventAggregator;

        private readonly IWindowManager windowManager;
        
        public readonly IDocumentStore DocumentStore;

        private string statusBarTextBlock;

        #endregion
        
        public AppViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.windowManager = windowManager;
            DocumentStore = documentStore;

            AppSettings appSettings;
            using (var conn = DocumentStore.OpenSession())
            {
                appSettings = conn.Query<AppSettings>().FirstOrDefault();
                if (appSettings == null)
                {
                    appSettings = AppSettings.CreateDefault();
                    conn.Store(appSettings);
                    conn.SaveChanges();
                }
            }

            RequestBuilderViewModel = new RequestBuilderViewModel(eventAggregator, documentStore, windowManager, appSettings);
            ResponseViewModel = new ResponseViewModel(eventAggregator, documentStore, appSettings);

            // Add tabs. TODO : can add these via bootstrapper;
            Items.Add(new HistoryViewModel(eventAggregator, documentStore));
            Items.Add(new GroupedViewModel(eventAggregator, documentStore));
            Items.Add(new FavouritesViewModel(eventAggregator, documentStore));
            Items.Add(new SearchViewModel(eventAggregator, documentStore));
        }

        //http://caliburnmicro.codeplex.com/discussions/394099
        public override void CanClose(Action<bool> callback)
        {
            using (var conn = DocumentStore.OpenSession())
            {
                var appsettings = conn.Query<AppSettings>().FirstOrDefault();
                if (appsettings == null)
                {
                    appsettings = AppSettings.CreateDefault();
                    conn.Store(appsettings);
                }

                appsettings.Top = Application.Current.MainWindow.Top;
                appsettings.Left = Application.Current.MainWindow.Left;
                appsettings.Width = Application.Current.MainWindow.Width;
                appsettings.Height = Application.Current.MainWindow.Height;
                conn.SaveChanges();
            }

            base.CanClose(callback);
        }
        
        #region Properties

        public RequestBuilderViewModel RequestBuilderViewModel { get; set; }

        public ResponseViewModel ResponseViewModel { get; set; }
        
        public string StatusBarTextBlock
        {
            get { return statusBarTextBlock; }
            set
            {
                statusBarTextBlock = value;
                NotifyOfPropertyChange(() => StatusBarTextBlock);
            }
        }

        #endregion
        
        #region Menu

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        public void Preferences()
        {
            dynamic settings = new ExpandoObject();
            settings.Width = 300;
            settings.Height = 250;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = "Preferences";
            //settings.SizeToContent = "WidthAndHeight";

            windowManager.ShowWindow(new PreferencesViewModel(eventAggregator, DocumentStore), null, settings);
        }

        public void About()
        {
            dynamic settings = new ExpandoObject();
            settings.Width = 300;
            settings.Height = 180;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = "About";

            windowManager.ShowWindow(new AboutViewModel(), null, settings);
        }

        public async void DeleteAllConfirm()
        {
            // TODO : show confirm message? then ConfirmDeleteAllHistory..

            dynamic settings = new ExpandoObject();
            settings.Width = 300;
            settings.Height = 150;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = "Confirm";

            var result = windowManager.ShowDialog(new ConfirmViewModel(), null, settings);

            if (result)
            {
                DeleteAllHistory();
            }
        }

        private async void DeleteAllHistory()
        {
            using (var conn = DocumentStore.OpenAsyncSession())
            {
                try
                {
                    conn.ClearDocumentsAsync<Request>();
                    conn.ClearDocumentsAsync<Response>();
                }
                catch (Exception)
                {
                    // TODO : pass exception messages to main window - add to event aggregator
                    // eventAggregator.PublishOnUIThread(ex); // <- Wrap in a specific exception class
                }
            }
        }

        #endregion

        #region Message Handlers

        public void Handle(HistorySelectedMessage historyRequest)
        {
            StatusBarTextBlock = "Loaded History Item.";
        }

        public void Handle(FavouriteSelectedMessage message)
        {
            StatusBarTextBlock = "Loaded Favourite Item.";
        }

        public void Handle(GroupedSelectedMessage message)
        {
            StatusBarTextBlock = "Loaded Grouped Item.";
        }

        public void Handle(SearchSelectedMessage message)
        {
            StatusBarTextBlock = "Loaded Search Result.";
        }

        public void Handle(ResponseReceivedMessage message)
        {
            StatusBarTextBlock = "Loaded Response.";
        }

        #endregion
    }
}
