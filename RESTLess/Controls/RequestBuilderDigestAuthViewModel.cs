﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;
using RESTLess.Models;
using RESTLess.Models.Interface;
using RESTLess.Models.Messages;

namespace RESTLess.Controls
{
    public class RequestBuilderDigestAuthViewModel : Screen, ITabItem, IHandle<CreateRequestMessage>, IHandle<ClearMessage>
    {
        private const string AuthorizationHeaderString = "Authorization";

        private enum AlgoType
        {
            MD5,
            MD5sess,
        }

        private enum QopType
        {
            none,
            auth,
            authint,
        }

        private readonly IEventAggregator eventAggregator;

        private string usernameTextBox;

        private string passwordTextBox;

        private string realmTextBox;

        private string nonceTextBox;

        private string algoTextBox;

        private string qopTextBox;

        private int nonceCount;

        private string clientNonceTextBox;

        private string opaqueTextBox;

        private string actionLabel;

        private Request request;

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

        public string RealmTextBox
        {
            get { return realmTextBox; }
            set
            {
                realmTextBox = value;
                NotifyOfPropertyChange(() => RealmTextBox);
            }
        }

        public string NonceTextBox
        {
            get { return nonceTextBox; }
            set
            {
                nonceTextBox = value;
                NotifyOfPropertyChange(() => NonceTextBox);
            }
        }

        public string AlgoTextBox
        {
            get { return algoTextBox; }
            set
            {
                algoTextBox = value;
                NotifyOfPropertyChange(() => AlgoTextBox);
            }
        }

        public string QopTextBox
        {
            get { return qopTextBox; }
            set
            {
                qopTextBox = value;
                NotifyOfPropertyChange(() => QopTextBox);
            }
        }

        public int NonceCount
        {
            get { return nonceCount; }
            set
            {
                nonceCount = value;
                NotifyOfPropertyChange(() => NonceCount);
            }
        }

        public string ClientNonceTextBox
        {
            get { return clientNonceTextBox; }
            set
            {
                clientNonceTextBox = value;
                NotifyOfPropertyChange(() => ClientNonceTextBox);
            }
        }

        public string OpaqueTextBox
        {
            get { return opaqueTextBox; }
            set
            {
                opaqueTextBox = value;
                NotifyOfPropertyChange(() => OpaqueTextBox);
            }
        }

        public string ActionLabel
        {
            get { return actionLabel; }
            set
            {
                actionLabel = value;
                NotifyOfPropertyChange(() => ActionLabel);
            }
        }

        #endregion

        protected override void OnDeactivate(bool close)
        {
            if (request != null)
            {
                eventAggregator.BeginPublishOnUIThread(new CreateRequestMessage { Request = request });
            }

            base.OnDeactivate(close);
        }

        public RequestBuilderDigestAuthViewModel(IEventAggregator eventAggregator)
        {
            DisplayName = "Digest Auth";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        #region Button Actions

        // TODO

        public void AddButton()
        {
            // http://en.wikipedia.org/wiki/Digest_access_authentication

            var algoType = AlgoType.MD5;
            if (!string.IsNullOrEmpty(AlgoTextBox))
            {
                Enum.TryParse(AlgoTextBox.Replace("-", string.Empty), out algoType);
            }

            var qopType = QopType.none;
            if (!string.IsNullOrEmpty(QopTextBox))
            {
                Enum.TryParse(QopTextBox.Replace("-", string.Empty), out qopType);
            }

            string ha1 = null;
            if (!string.IsNullOrEmpty(UsernameTextBox) && !string.IsNullOrEmpty(RealmTextBox) && !string.IsNullOrEmpty(PasswordTextBox))
            {
                ha1 = GetMD5HashData(UsernameTextBox + ":" + RealmTextBox + ":" + PasswordTextBox);
                
                if (algoType == AlgoType.MD5sess && !string.IsNullOrEmpty(NonceTextBox) && !string.IsNullOrEmpty(ClientNonceTextBox))
                {
                    ha1 = GetMD5HashData(ha1 + ":" + NonceTextBox + ":" + ClientNonceTextBox);
                }
            }

            string ha2 = null;
            if (qopType == QopType.none || qopType == QopType.auth)
            {
                ha2 = GetMD5HashData(request.Method + request.Url);
            }
            else if (qopType == QopType.authint)
            {
                // TODO : if no body?
                ha2 = GetMD5HashData(request.Method + request.Url + GetMD5HashData(request.Body));
            }

            if (!string.IsNullOrEmpty(ha1) && !string.IsNullOrEmpty(ha2))
            {
                var response = qopType != QopType.none ? GetMD5HashData(ha1 + ":" + NonceTextBox + ":" + NonceCount + ":" + clientNonceTextBox + ":" + QopTextBox + ":" + ha2) : GetMD5HashData(ha1 + ":" + NonceTextBox + ":" + ha2);

                request.Headers.Add(AuthorizationHeaderString, response);
                ActionLabel = "Added!";
            }
        }

        public void ClearButton()
        {
            UsernameTextBox = string.Empty;
            PasswordTextBox = string.Empty;
            RealmTextBox = string.Empty;
            QopTextBox = string.Empty;
            AlgoTextBox = string.Empty;
            NonceTextBox = string.Empty;
            ClientNonceTextBox = string.Empty;
            OpaqueTextBox = string.Empty;
            NonceCount = 0;
            
            if (request != null && request.Headers.ContainsKey(AuthorizationHeaderString))
            {
                request.Headers.Remove(AuthorizationHeaderString);
                ActionLabel = "Removed!";
            }
        }

        #endregion

        private string GetMD5HashData(string data)
        {
            MD5 md5 = MD5.Create();
            byte[] hashData = md5.ComputeHash(Encoding.Default.GetBytes(data));
            StringBuilder returnValue = new StringBuilder();

            for (int i = 0; i < hashData.Length; i++)
            {
                returnValue.Append(i.ToString());
            }

            // return hexadecimal string
            return returnValue.ToString();
        }

        #region Handlers

        public void Handle(CreateRequestMessage message)
        {
            request = message.Request;
        }


        public void Handle(ClearMessage message)
        {
            UsernameTextBox = string.Empty;
            PasswordTextBox = string.Empty;
            ActionLabel = string.Empty;
            request = null;
        }

        #endregion
    }
}
