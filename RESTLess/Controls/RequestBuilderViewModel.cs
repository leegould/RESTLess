using System;
using System.Diagnostics;
using Caliburn.Micro;

using Raven.Client;
using RestSharp;
using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderViewModel : Conductor<ITabItem>.Collection.OneActive
    {
        #region Private members

        private readonly AppSettings appsettings;

        private readonly IEventAggregator eventaggregator;

        private readonly IDocumentStore documentstore;

        //private readonly IWindowManager windowManager;

        private Stopwatch stopWatch;

        //private bool bodyIsVisible;

        private bool isWaiting;

        private RequestBuilderFormViewModel rbFormViewModel { get; set; }

        private RequestBuilderRawViewModel rbRawViewModel { get; set; }

        private RequestBuilderBasicAuthViewModel rbBasicAuthVM { get; set; }

        #endregion

        public RequestBuilderViewModel(IEventAggregator eventAggregator, IDocumentStore documentStore, IWindowManager windowManager, AppSettings appSettings)
        {
            documentstore = documentStore;
            eventaggregator = eventAggregator;
            appsettings = appSettings;

            rbFormViewModel = new RequestBuilderFormViewModel(eventAggregator, documentStore, windowManager, appsettings);
            rbFormViewModel.ConductWith(this);
            Items.Add(rbFormViewModel);

            rbRawViewModel = new RequestBuilderRawViewModel(eventAggregator, documentStore, windowManager, appsettings);
            rbRawViewModel.ConductWith(this);
            Items.Add(rbRawViewModel);

            rbBasicAuthVM = new RequestBuilderBasicAuthViewModel(eventaggregator);
            rbBasicAuthVM.ConductWith(this);
            Items.Add(rbBasicAuthVM);
        }

        //#region Buttons

        public void SendButton()
        {
            var uri = new Uri(rbFormViewModel.Url);
            var body = rbFormViewModel.Body;
            
            var client = GetRestClient(uri);
            var request = GetRestRequest(uri, rbFormViewModel.SelectedMethod, body, rbFormViewModel.Headers);

            stopWatch = new Stopwatch();
            stopWatch.Start();

            isWaiting = true;
            NotifyOfPropertyChange(() => rbFormViewModel.CanStopButton);

            Request req = null;
            using (var conn = documentstore.OpenSession())
            {
                try
                {
                    req = new Request(client.BaseUrl, request, body, appsettings.RequestSettings);
                    conn.Store(req);
                    conn.SaveChanges();
                }
                catch (Exception ex)
                {
                    // TODO : Create message
                    StopSending();
                }
            }
            eventaggregator.PublishOnUIThread(new RequestSavedMessage { Request = req });

            client.ExecuteAsync(request,
                r =>
                {
                    if (isWaiting)
                    {
                        stopWatch.Stop();
                        //StatusBarTextBlock = "Status: " + r.ResponseStatus + ". Code:" + r.StatusCode + ". Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms.";

                        Response response = new Response(req != null ? req.Id : "0", r, stopWatch.ElapsedMilliseconds);

                        StopSending();

                        //DisplayResponse(response);

                        eventaggregator.PublishOnUIThread(new ResponseReceivedMessage { Response = response });


                        using (var conn = documentstore.OpenSession())
                        {
                            try
                            {
                                conn.Store(response);
                                conn.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                // TODO : Create message
                                //StatusBarTextBlock = "Response Error";
                                //RawResultsTextBox = ex.ToString();
                            }
                        }
                    }
                });
        }

        private void StopSending()
        {
            if (stopWatch.IsRunning)
            {
                stopWatch.Stop();
                stopWatch.Reset();
            }

            isWaiting = false;
            NotifyOfPropertyChange(() => rbFormViewModel.CanStopButton);
            NotifyOfPropertyChange(() => rbFormViewModel.CanSendButton);
        }

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
        
        //#endregion

        public static RestRequest GetRestRequest(Uri uri, Method selectedMethod, string body, IObservableCollection<HttpHeader> headers)
        {
            var method = selectedMethod;

            var request = new RestRequest(uri, method);

            //request.AddHeader("nocache", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            foreach (var header in headers)
            {
                request.AddHeader(header.Name, header.Value);
            }

            if (RequestBuilderFormViewModel.UseBody(method) && !string.IsNullOrWhiteSpace(body))
            {
                request.AddJsonBody(body);
            }

            return request;
        }

        private RestClient GetRestClient(Uri uri)
        {
            RestClient client = new RestClient(uri.GetLeftPart(UriPartial.Authority))
            {
                Timeout = appsettings.RequestSettings.Timeout
            };
            return client;
        }
    }
}
