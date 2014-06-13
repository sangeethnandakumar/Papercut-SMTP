﻿// /*  
//  * Papercut
//  *
//  *  Copyright © 2008 - 2012 Ken Robertson
//  *  Copyright © 2013 - 2014 Jaben Cargman
//  *  
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *  
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *  
//  *  Unless required by applicable law or agreed to in writing, software
//  *  distributed under the License is distributed on an "AS IS" BASIS,
//  *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *  See the License for the specific language governing permissions and
//  *  limitations under the License.
//  *  
//  */

namespace Papercut.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Navigation;

    using Caliburn.Micro;

    using MimeKit;

    using Papercut.Core.Helper;
    using Papercut.Core.Message;
    using Papercut.Helpers;
    using Papercut.Views;

    using Serilog;

    public class MessageDetailViewModel : Screen
    {
        readonly MimeMessageLoader _mimeMessageLoader;

        int _attachmentCount;

        string _bcc;

        string _body;

        string _cc;

        string _date;

        string _from;

        string _headers;

        string _htmlFile;

        bool _isHtml;

        bool _isLoading;

        IDisposable _loadingDisposable;

        int _selectedTabIndex;

        string _subject;

        string _textBody;

        string _to;

        public MessageDetailViewModel(
            Func<PartsListViewModel> partsListViewModelFactory,
            Func<MessageViewModel> messagesViewModelFactory,
            MimeMessageLoader mimeMessageLoader)
        {
            _mimeMessageLoader = mimeMessageLoader;
            PartsListViewModel = partsListViewModelFactory();
            MessageViewModel = messagesViewModelFactory();
        }

        public string Subject
        {
            get
            {
                return _subject;
            }
            set
            {
                _subject = value;
                NotifyOfPropertyChange(() => Subject);
            }
        }

        public string To
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
                NotifyOfPropertyChange(() => To);
            }
        }

        public string Bcc
        {
            get
            {
                return _bcc;
            }
            set
            {
                _bcc = value;
                NotifyOfPropertyChange(() => Bcc);
            }
        }

        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                NotifyOfPropertyChange(() => Date);
            }
        }

        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
                NotifyOfPropertyChange(() => From);
            }
        }

        public string CC
        {
            get
            {
                return _cc;
            }
            set
            {
                _cc = value;
                NotifyOfPropertyChange(() => CC);
            }
        }

        public string Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
                NotifyOfPropertyChange(() => Headers);
            }
        }

        public string TextBody
        {
            get
            {
                return _textBody;
            }
            set
            {
                _textBody = value;
                NotifyOfPropertyChange(() => TextBody);
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        public string Body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;
                NotifyOfPropertyChange(() => Body);
            }
        }

        public bool IsHtml
        {
            get
            {
                return _isHtml;
            }
            set
            {
                _isHtml = value;
                NotifyOfPropertyChange(() => IsHtml);
            }
        }

        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                _selectedTabIndex = value;
                NotifyOfPropertyChange(() => SelectedTabIndex);
            }
        }

        public int AttachmentCount
        {
            get
            {
                return _attachmentCount;
            }
            set
            {
                _attachmentCount = value;
                NotifyOfPropertyChange(() => AttachmentCount);
                NotifyOfPropertyChange(() => HasAttachments);
            }
        }

        public bool HasAttachments
        {
            get
            {
                return AttachmentCount > 0;
            }
        }

        public string HtmlFile
        {
            get
            {
                return _htmlFile;
            }
            set
            {
                _htmlFile = value;
                NotifyOfPropertyChange(() => HtmlFile);
            }
        }

        public PartsListViewModel PartsListViewModel { get; private set; }

        public MessageViewModel MessageViewModel { get; private set; }
        
        public void LoadMessageEntry(MessageEntry messageEntry)
        {
            if (_loadingDisposable != null) _loadingDisposable.Dispose();

            if (messageEntry == null)
            {
                // show empty...
                DisplayMimeMessage(null);
                IsLoading = false;
            }
            else
            {
                IsLoading = true;

                // load and show it...
                _loadingDisposable =
                    _mimeMessageLoader.Get(messageEntry)
                        .ObserveOnDispatcher()
                        .Subscribe(
                            m =>
                            {
                                DisplayMimeMessage(m);
                                IsLoading = false;
                            });
            }
        }

        void DisplayMimeMessage(MimeMessage mailMessageEx)
        {
            if (mailMessageEx != null)
            {
                Headers = string.Join("\r\n", mailMessageEx.Headers.Select(h => h.ToString()));

                List<MimePart> parts = mailMessageEx.BodyParts.ToList();
                TextPart mainBody = parts.GetMainBodyTextPart();
                Body = mainBody.Text;

                From = mailMessageEx.From.IfNotNull(s => s.ToString()) ?? string.Empty;
                To = mailMessageEx.To.IfNotNull(s => s.ToString()) ?? string.Empty;
                CC = mailMessageEx.Cc.IfNotNull(s => s.ToString()) ?? string.Empty;
                Bcc = mailMessageEx.Bcc.IfNotNull(s => s.ToString()) ?? string.Empty;
                Date = mailMessageEx.Date.IfNotNull(s => s.ToString()) ?? string.Empty;

                Subject = mailMessageEx.Subject ?? string.Empty;
                IsHtml = mainBody.IsContentHtml();

                MessageViewModel.ShowMessage(mailMessageEx);

                AttachmentCount = parts.GetAttachments().Count();

                PartsListViewModel.MimeMessage = mailMessageEx;

                if (IsHtml)
                {
                    TextPart textPartNotHtml =
                        parts.OfType<TextPart>().Except(new[] { mainBody }).FirstOrDefault();

                    if (textPartNotHtml != null) TextBody = textPartNotHtml.Text;
                }
                else TextBody = null;
            }
            else
            {
                AttachmentCount = 0;
                IsHtml = false;
                HtmlFile = null;
                TextBody = null;
                Body = null;
            }

            SelectedTabIndex = 0;
        }
    }
}