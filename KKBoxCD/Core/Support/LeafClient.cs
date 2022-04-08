﻿using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKBoxCD.Core.Support
{
    public class LeafClient : IDisposable
    {
        #region Dispose

        private bool _Disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed) {
                return;
            }

            if (disposing)
            {
                try { HttpRequest.Close(); } catch { }
                try { HttpRequest.Dispose(); } catch { }
                HttpRequest = null;
            }
            _Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LeafClient()
        {
            Dispose(false);
        }

        #endregion

        public int DelayRequest { get; set; } = 25;

        public int Timeout { get; set; } = 60000;

        public HttpRequest HttpRequest { get; set; }

        public string Response
        {
            get => HttpRequest.Response.ToString();
        }

        public bool IsSuccess
        {
            get => HttpRequest.Response.IsOK && HttpRequest.Response.StatusCode.Equals(HttpStatusCode.OK);
        }

        public bool IsHasContent
        {
            get => HttpRequest.Response.MessageBodyLoaded || !string.IsNullOrEmpty(Response);
        }

        public bool IsFailed
        {
            get => !IsSuccess;
        }

        public bool IsForbidden
        {
            get => HttpRequest.Response.StatusCode.Equals(HttpStatusCode.Forbidden);
        }

        public string Accept
        {
            get
            {
                if (HttpRequest.ContainsHeader("Accept"))
                {
                    return HttpRequest["Accept"];
                }
                return null;
            }
            set => HttpRequest["Accept"] = value;
        }

        public string ContentType
        {
            get
            {
                if (HttpRequest.ContainsHeader("Content-Type"))
                {
                    return HttpRequest["Content-Type"];
                }
                return null;
            }
            set => HttpRequest["Content-Type"] = value;
        }

        public string Referer
        {
            get
            {
                if (HttpRequest.ContainsHeader("Referer"))
                {
                    return HttpRequest["Referer"];
                }
                return null;
            }
            set => HttpRequest["Referer"] = value;
        }

        public string Origin
        {
            get
            {
                if (HttpRequest.ContainsHeader("Origin"))
                {
                    return HttpRequest["Origin"];
                }
                return null;
            }
            set => HttpRequest["Origin"] = value;
        }

        public string RequestedWith
        {
            get
            {
                if (HttpRequest.ContainsHeader("X-Requested-With"))
                {
                    return HttpRequest["X-Requested-With"];
                }
                return null;
            }
            set => HttpRequest["X-Requested-With"] = value;
        }

        public string UserAgent
        {
            get => HttpRequest.UserAgent;
            set => HttpRequest.UserAgent = value;
        }

        public LeafClient()
        {
            HttpRequest = new HttpRequest()
            {
                UseCookies = true,
                ConnectTimeout = Timeout,
                ReadWriteTimeout = Timeout,
                Reconnect = false,
                ReconnectLimit = 5,
                ReconnectDelay = 1000,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36",
                KeepAlive = true,
                IgnoreProtocolErrors = false,
                IgnoreInvalidCookie = true,
                AllowAutoRedirect = true,
                MaximumAutomaticRedirections = 10,
                AcceptEncoding = "gzip, deflate, br",
                AllowEmptyHeaderValues = false
            };
        }

        public void SetXMLHttpRequest()
        {
            RequestedWith = "XMLHttpRequest";
        }

        public void AddHeader(string name, string value)
        {
            RemovesHeader(name);
            HttpRequest[name] = value;
        }

        private void RemovesHeader(string name)
        {
            if (HttpRequest.ContainsHeader(name))
            {
                HttpRequest[name] = null;
            }
        }

        public void SetProxy(string host, int port, bool is_socks)
        {
            if (is_socks)
            {
                HttpRequest.Proxy = new Socks5ProxyClient(host, port);
            }
            else
            {
                HttpRequest.Proxy = new HttpProxyClient(host, port);
            }
        }

        public string GetCookie(string address, string name)
        {
            return this.HttpRequest.Cookies.GetCookies(address)[name].Value;
        }

        public void SetNewCookies()
        {
            HttpRequest.Cookies = new CookieStorage();
        }

        public async Task<bool> GetAsync(string url)
        {
            try
            {
                HttpRequest.Get(url);
                await Task.Delay(DelayRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HeadAsync(string url)
        {
            try
            {
                HttpRequest.Head(url);
                await Task.Delay(DelayRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PostAsync(string url)
        {
            try
            {

                HttpRequest.Post(url);
                await Task.Delay(DelayRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PostAsync(string url, RequestParams requestParams)
        {
            try
            {
                HttpRequest.Post(url, requestParams);
                await Task.Delay(DelayRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PostAsync(string url, HttpContent content)
        {
            try
            {
                HttpRequest.Post(url, content);
                await Task.Delay(DelayRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PostAsync(string url, string content, string content_type)
        {
            try
            {
                HttpRequest.Post(url, content, content_type);
                await Task.Delay(DelayRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> PostGetAddressAsync(string url, RequestParams requestParams)
        {
            try
            {
                HttpRequest.Post(url, requestParams);
                await Task.Delay(DelayRequest);
                if (HttpRequest.Response.IsOK)
                {
                    return HttpRequest.Response.Address.OriginalString;
                }
                else {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}