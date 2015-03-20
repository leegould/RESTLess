using System;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Windows;

using Caliburn.Micro;

using Raven.Client;

using RESTLess.Controls;
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
            RequestBuilderViewModel = new RequestBuilderViewModel(eventAggregator, documentStore);
            ResponseViewModel = new ResponseViewModel(eventAggregator, documentStore);

            // Add tabs. TODO : can add these via bootstrapper;
            Items.Add(new HistoryViewModel(eventAggregator, documentStore));
            Items.Add(new GroupedViewModel(eventAggregator, documentStore));
            Items.Add(new FavouritesViewModel(eventAggregator, documentStore));
            Items.Add(new SearchViewModel(eventAggregator, documentStore));

            using (var conn = DocumentStore.OpenSession())
            {
                var appSettings = conn.Query<AppSettings>().FirstOrDefault();
                if (appSettings == null)
                {
                    appSettings = AppSettings.CreateDefault();
                    conn.Store(appSettings);
                    conn.SaveChanges();
                }
            }
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
            settings.Width = 400;
            settings.Height = 300;
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
