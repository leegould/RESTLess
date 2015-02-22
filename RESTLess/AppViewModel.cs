using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Windows;

using AutoMapper;
using Caliburn.Micro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client;
using RestSharp;
using RESTLess.Controls;
using RESTLess.Models;
using RESTLess.Models.Messages;

namespace RESTLess
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : Screen, IApp, IHandle<HistorySelectedMessage>, IHandle<MethodSelectedMessage>, IHandle<GroupedSelectedMessage>, IHandle<AppSettingsChangedMessage>
    {
        #region Private members

        private AppSettings appSettings;

        private readonly IEventAggregator eventAggregator;

        private readonly IWindowManager windowManager;
        
        public readonly IDocumentStore DocumentStore;
        
        private string rawResultsTextBox;
        private string htmlResultsBox;
        private string url;
        private string body;

        private string responseStatusTextBlock;
        private string responseWhenTextBlock;
        private string responseElapsedTextBlock;

        private IObservableCollection<HttpHeader> headers;

        private string statusBarTextBlock;

        private bool isWaiting;
        
        private Stopwatch stopWatch;

        private Method selectedMethod;

        private bool bodyIsVisible;

        #endregion

        static AppViewModel()
        {
            Mapper.CreateMap<Request, AppViewModel>()
                .ForMember(d => d.HeadersDataGrid, o => o.MapFrom(s => CreateHeadersFromDict(s.Headers)))
                .ForMember(d => d.UrlTextBox, o => o.MapFrom(s => s.Url + s.Path.Substring(1)))
                .ForMember(d => d.BodyTextBox, o => o.MapFrom(s => s.Body));
        }
        
        public AppViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, IDocumentStore documentStore)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.windowManager = windowManager;
            this.DocumentStore = documentStore;
            HeadersDataGrid = new BindableCollection<HttpHeader>();
            MethodViewModel = new MethodViewModel(eventAggregator);
            HistoryViewModel = new HistoryViewModel(eventAggregator, documentStore);
            GroupedViewModel = new GroupedViewModel(eventAggregator, documentStore);
            FavouritesViewModel = new FavouritesViewModel(eventAggregator, documentStore);
            selectedMethod = Method.GET;
            BodyIsVisible = false;

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

        public MethodViewModel MethodViewModel { get; set; }

        public HistoryViewModel HistoryViewModel { get; set; }

        public GroupedViewModel GroupedViewModel { get; set; }

        public FavouritesViewModel FavouritesViewModel { get; set; }

        public string UrlTextBox
        {
            get { return url; }
            set
            {
                url = value;
                NotifyOfPropertyChange(() => UrlTextBox);
                NotifyOfPropertyChange(() => CanSendButton);
            }
        }

        public string BodyTextBox
        {
            get { return body; }
            set
            {
                body = value;
                NotifyOfPropertyChange(BodyTextBox);
            }
        }

        public string RawResultsTextBox
        {
            get { return rawResultsTextBox; }
            set
            {
                rawResultsTextBox = value;
                NotifyOfPropertyChange(() => RawResultsTextBox);
            }
        }

        public string HtmlResultsBox
        {
            get { return htmlResultsBox; }
            set
            {
                htmlResultsBox = value;
                NotifyOfPropertyChange(() => HtmlResultsBox);
            }
        }

        public string ResponseStatusTextBlock
        {
            get
            {
                return responseStatusTextBlock;
            }
            set
            {
                responseStatusTextBlock = value;
                NotifyOfPropertyChange(() => ResponseStatusTextBlock);
            }
        }

        public string ResponseWhenTextBlock
        {
            get
            {
                return responseWhenTextBlock;
            }
            set
            {
                responseWhenTextBlock = value;
                NotifyOfPropertyChange(() => ResponseWhenTextBlock);
            }
        }

        public string ResponseElapsedTextBlock
        {
            get
            {
                return responseElapsedTextBlock;
            }
            set
            {
                responseElapsedTextBlock = value;
                NotifyOfPropertyChange(() => ResponseElapsedTextBlock);
            }
        }

        public IObservableCollection<HttpHeader> HeadersDataGrid
        {
            get { return headers; }
            set
            {
                headers = value;
                NotifyOfPropertyChange(() => HeadersDataGrid);
            }
        } 

        public bool CanSendButton
        {
            get
            {
                return !string.IsNullOrWhiteSpace(UrlTextBox) && !isWaiting;
            }
        }

        public bool CanStopButton
        {
            get
            {
                return isWaiting;
            }
        }

        public string StatusBarTextBlock
        {
            get { return statusBarTextBlock; }
            set
            {
                statusBarTextBlock = value;
                NotifyOfPropertyChange(() => StatusBarTextBlock);
            }
        }

        public bool BodyIsVisible
        {
            get { return bodyIsVisible; }
            set
            {
                bodyIsVisible = value;
                if (!bodyIsVisible)
                {
                    BodyTextBox = string.Empty;
                }
                NotifyOfPropertyChange(() => BodyIsVisible);
            }
        }

        #endregion


        #region Button Actions

        public void SendButton()
        {
            var uri = new Uri(UrlTextBox);
            RestClient client = new RestClient(uri.GetLeftPart(UriPartial.Authority))
                                {
                                    Timeout = appSettings.RequestSettings.Timeout
                                };
            var method = selectedMethod;

            var request = new RestRequest(uri, method);
            
            //request.AddHeader("nocache", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            foreach (var header in HeadersDataGrid)
            {
                request.AddHeader(header.Name, header.Value);
            }

            if (UseBody(method) && !string.IsNullOrWhiteSpace(BodyTextBox))
            {
                request.AddJsonBody(BodyTextBox);
            }

            stopWatch = new Stopwatch();
            stopWatch.Start();

            StatusBarTextBlock = "Sending " + request.Method + " " + client.BaseUrl.ToString().Substring(0, client.BaseUrl.ToString().Length - 1) + request.Resource;

            isWaiting = true;
            NotifyOfPropertyChange(() => CanStopButton);

            Request req = null;
            using (var conn = DocumentStore.OpenSession())
            {
                try
                {
                    req = new Request(client.BaseUrl, request, body, appSettings.RequestSettings);
                    conn.Store(req);
                    conn.SaveChanges();
                }
                catch (Exception ex)
                {
                    StatusBarTextBlock = "Cancelled request";
                    RawResultsTextBox = ex.ToString();
                    StopSending();
                }
            }
            eventAggregator.PublishOnUIThread(new RequestSavedMessage { Request = req });

            client.ExecuteAsync(request,
                r =>
                {
                    if (isWaiting)
                    {
                        stopWatch.Stop();
                        StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

                        Response response = new Response(req != null ? req.Id : 0, r, stopWatch.ElapsedMilliseconds);

                        DisplayResponse(response);

                        StopSending();

                        using (var conn = DocumentStore.OpenSession())
                        {
                            try
                            {
                                conn.Store(response);
                                conn.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                StatusBarTextBlock = "Response Error";
                                RawResultsTextBox = ex.ToString();
                            }
                        }
                    }
                });
        }

        public void StopButton()
        {
            StopSending();
            StatusBarTextBlock = "Cancelled request";
            RawResultsTextBox = string.Empty;
            ResponseElapsedTextBlock = string.Empty;
            ResponseStatusTextBlock = string.Empty;
            ResponseWhenTextBlock = string.Empty;
        }

        public void ClearButton()
        {
            UrlTextBox = string.Empty;
            BodyTextBox = string.Empty;
            HeadersDataGrid.Clear();
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

        #endregion

        #region Message Handlers

        public void Handle(HistorySelectedMessage historyRequest)
        {
            Mapper.Map(historyRequest.Request, this);

            if (appSettings.LoadResponses)
            {
                using (var docstore = DocumentStore.OpenSession())
                {
                    var response =
                        docstore.Query<Response>().FirstOrDefault(x => x.RequestId == historyRequest.Request.Id);
                    DisplayOrClear(response);
                }
            }
        }

        
        public void Handle(MethodSelectedMessage message)
        {
            selectedMethod = message.Method;
            BodyIsVisible = UseBody(message.Method);
        }

        public void Handle(GroupedSelectedMessage message)
        {
            using (var docstore = DocumentStore.OpenSession())
            {
                var item = docstore.Load<Request>(message.Request.Id);
                Mapper.Map(item, this);

                if (appSettings.LoadResponses)
                {
                    var response = docstore.Query<Response>().FirstOrDefault(x => x.RequestId == item.Id);
                    DisplayOrClear(response);
                }
            }
        }

        public void Handle(AppSettingsChangedMessage message)
        {
            appSettings = message.AppSettings;
            if (!appSettings.LoadResponses)
            {
                DisplayOrClear(null); // Clear
            }
        }

        #endregion

        #region Private Methods

        private void DisplayOrClear(Response response)
        {
            if (response != null)
            {
                DisplayResponse(response);
            }
            else
            {
                ResponseElapsedTextBlock = string.Empty;
                ResponseStatusTextBlock = string.Empty;
                ResponseWhenTextBlock = string.Empty;
                RawResultsTextBox = "No Response";
                HtmlResultsBox = "<p>No Response</p>";
            }
        }

        private void DisplayResponse(Response response)
        {
            try 
            {
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    try
                    {
                        var formattedjson = JObject.Parse(response.Content).ToString(Formatting.Indented);
                        RawResultsTextBox = formattedjson;
                        HtmlResultsBox = "<pre>" + System.Net.WebUtility.HtmlEncode(formattedjson) + "</pre>";
                    }
                    catch(JsonReaderException)
                    {
                        RawResultsTextBox = response.Content;
                        HtmlResultsBox = response.Content;
                    }
                }
                else
                {
                    RawResultsTextBox = string.Empty;
                }

                ResponseElapsedTextBlock =  response.Elapsed + " ms.";
                ResponseStatusTextBlock = response.StatusCode + " " + response.StatusCodeDescription;

                var whentext = response.When.ToString(CultureInfo.InvariantCulture);
                var ago = DateTime.UtcNow.Subtract(response.When);
                if (ago.TotalMinutes > 1)
                {
                    whentext += " (-" + ago.ToString(@"d\.h\:mm\:ss") + ")";
                }

                ResponseWhenTextBlock = whentext;
            }
            catch (Exception ex)
            {
                RawResultsTextBox = ex.ToString();
            }
        }

        private void StopSending()
        {
            if (stopWatch.IsRunning)
            {
                stopWatch.Stop();
                stopWatch.Reset();
            }

            isWaiting = false;
            NotifyOfPropertyChange(() => CanStopButton);
            NotifyOfPropertyChange(() => CanSendButton);
        }

        private static IObservableCollection<HttpHeader> CreateHeadersFromDict(Dictionary<string, string> dictionary)
        {
            var items = new BindableCollection<HttpHeader>();
            foreach (var item in dictionary)
            {
                items.Add(new HttpHeader { Name = item.Key, Value = item.Value });
            }
            return items;
        }

        #endregion

        #region Static Methods

        private static bool UseBody(Method method)
        {
            return method != Method.GET && method != Method.HEAD && method != Method.OPTIONS;
        }

        #endregion
    }
}
