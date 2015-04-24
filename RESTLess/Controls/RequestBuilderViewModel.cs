using Caliburn.Micro;

using Raven.Client;

using RESTLess.Models;
using RESTLess.Models.Interface;

namespace RESTLess.Controls
{
    public class RequestBuilderViewModel : Conductor<ITabItem>.Collection.OneActive
    {
        #region Private members

        //private AppSettings appSettings;

        //private readonly IEventAggregator eventAggregator;

        //private readonly IDocumentStore documentStore;

        //private readonly IWindowManager windowManager;

        //private IObservableCollection<HttpHeader> headers;

        //private string url;
        //private string body;

        //private Method selectedMethod;

        //private Stopwatch stopWatch;

        //private bool bodyIsVisible;

        //private bool isWaiting;

        private string requestRawText;

        #endregion

        //static RequestBuilderViewModel()
        //{
        //    Mapper.CreateMap<Request, RequestBuilderViewModel>()
        //        .ForMember(d => d.HeadersDataGrid, o => o.MapFrom(s => CreateHeadersFromDict(s.Headers)))
        //        .ForMember(d => d.UrlTextBox, o => o.MapFrom(s => s.Url + s.Path.Substring(1)))
        //        .ForMember(d => d.BodyTextBox, o => o.MapFrom(s => s.Body));
        //}

        public RequestBuilderViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, IWindowManager windowManager, AppSettings appsettings)
        {
            //this.eventAggregator = eventAggregator;
            //eventAggregator.Subscribe(this);
            //this.documentStore = documentStore;
            //this.windowManager = windowManager;
            //appSettings = appsettings;
            Items.Add(new RequestBuilderFormViewModel(eventAggregator, documentStore, windowManager, appsettings));
            //MethodViewModel = new MethodViewModel(eventAggregator);
            //HeadersDataGrid = new BindableCollection<HttpHeader>();

            //selectedMethod = Method.GET;
            //BodyIsVisible = false;
        }

        #region Properties
        
        public string RequestRawText
        {
            get { return requestRawText; }
            set
            {
                requestRawText = value;
                NotifyOfPropertyChange(() => RequestRawText);
            }
        }

        #endregion

        //#region Buttons

        //public void SendButton()
        //{
        //    var uri = new Uri(UrlTextBox);
        //    var client = GetRestClient(uri);
        //    var request = GetRestRequest(uri);

        //    stopWatch = new Stopwatch();
        //    stopWatch.Start();

        //    isWaiting = true;
        //    NotifyOfPropertyChange(() => CanStopButton);

        //    Request req = null;
        //    using (var conn = documentStore.OpenSession())
        //    {
        //        try
        //        {
        //            req = new Request(client.BaseUrl, request, body, appSettings.RequestSettings);
        //            conn.Store(req);
        //            conn.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            // TODO : Create message
        //            StopSending();
        //        }
        //    }
        //    eventAggregator.PublishOnUIThread(new RequestSavedMessage { Request = req });

        //    client.ExecuteAsync(request,
        //        r =>
        //        {
        //            if (isWaiting)
        //            {
        //                stopWatch.Stop();
        //                //StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

        //                Response response = new Response(req != null ? req.Id : "0", r, stopWatch.ElapsedMilliseconds);

        //                StopSending();

        //                //DisplayResponse(response);

        //                eventAggregator.PublishOnUIThread(new ResponseReceivedMessage { Response = response });


        //                using (var conn = documentStore.OpenSession())
        //                {
        //                    try
        //                    {
        //                        conn.Store(response);
        //                        conn.SaveChanges();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        // TODO : Create message
        //                        //StatusBarTextBlock = "Response Error";
        //                        //RawResultsTextBox = ex.ToString();
        //                    }
        //                }
        //            }
        //        });
        //}

        //public void StopButton()
        //{
        //    StopSending();
        //}

        //public void ClearButton()
        //{
        //    UrlTextBox = string.Empty;
        //    BodyTextBox = string.Empty;
        //    HeadersDataGrid.Clear();
        //}

        //public void AddAuth()
        //{
        //    dynamic settings = new ExpandoObject();
        //    settings.Width = 300;
        //    settings.Height = 200;
        //    settings.WindowStartupLocation = WindowStartupLocation.Manual;
        //    settings.Title = "Add Authentication";
        //    //settings.SizeToContent = "WidthAndHeight";

        //    windowManager.ShowWindow(new AuthenticationViewModel(eventAggregator, documentStore), null, settings);
        //}

        //#endregion
    }
}
