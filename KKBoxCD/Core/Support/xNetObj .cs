using Leaf.xNet;
using System;
using System.Threading.Tasks;

namespace KKBoxCD.Core.Support
{
    public class xNetObj : IDisposable
    {
        #region Dispose
        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) { return; }

            if (disposing)
            {
                try { this.httpRequest.Close(); } catch { }
                try { this.httpRequest.Dispose(); } catch { }
                this.httpRequest = null;
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~xNetObj()
        {
            Dispose(false);
        }
        #endregion

        public HttpRequest httpRequest;
        public int timeOut = 60;

        public string Response
        {
            get
            {
                return this.httpRequest.Response.ToString();
            }
        }

        public bool IsSuccess
        {
            get
            {
                return (this.httpRequest.Response.IsOK
                      && this.httpRequest.Response.StatusCode.Equals(Leaf.xNet.HttpStatusCode.OK));
            }
        }

        public bool IsHasContent
        {
            get
            {
                return httpRequest.Response.MessageBodyLoaded || !string.IsNullOrEmpty(Response);
            }
        }

        public bool IsFailed
        {
            get
            {
                return !IsSuccess;
            }
        }

        public bool IsForbidden
        {
            get
            {
                return this.httpRequest.Response.StatusCode.Equals(Leaf.xNet.HttpStatusCode.Forbidden);
            }
        }

        public xNetObj()
        {
            this.httpRequest = new Leaf.xNet.HttpRequest()
            {
                UseCookies = true,

                ConnectTimeout = timeOut * 1000,
                ReadWriteTimeout = timeOut * 1000,

                Reconnect = false,
                ReconnectLimit = 5,
                ReconnectDelay = 1000,

                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36",

                KeepAlive = false,

                IgnoreProtocolErrors = false,
                IgnoreInvalidCookie = true,

                AllowAutoRedirect = true,
                MaximumAutomaticRedirections = 10,

                //SslProtocols = System.Security.Authentication.SslProtocols.Tls12,

                //AcceptEncoding = "gzip",
                //AcceptEncoding = "gzip, deflate",
                AcceptEncoding = "gzip, deflate, br",

                AllowEmptyHeaderValues = false
            };
        }

        public void SetAccept(string value)
        {
            this.httpRequest["Accept"] = value;
        }
        public void SetContentType(string value)
        {
            this.httpRequest["Content-Type"] = value;
        }
        public void SetReferer(string value)
        {
            this.httpRequest["Referer"] = value;
        }
        public void SetOrigin(string value)
        {
            this.httpRequest["Origin"] = value;
        }
        public void SetXMLHttpRequest()
        {
            this.httpRequest["X-Requested-With"] = "XMLHttpRequest";
        }

        public void AddHeader(string name, string value)
        {
            RemovesHeader(name);
            this.httpRequest[name] = value;
        }

        private void RemovesHeader(string name)
        {
            try
            {
                this.httpRequest[name] = null;
            }
            catch { }
        }

        public void SetProxy(string host, int port, bool isSocks5)
        {
            if (isSocks5)
            {
                httpRequest.Proxy = new Leaf.xNet.Socks5ProxyClient(host, port);
            }
            else
            {
                httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(host, port);
            }
        }

        public string GetCookie(string address, string name)
        {
            return this.httpRequest.Cookies.GetCookies(address)[name].Value;
        }

        public void SetNewCookies()
        {
            this.httpRequest.Cookies = new CookieStorage();
        }

        private int delayRequest = 25;
        public async Task<bool> GETAsync(string url)
        {
            try
            {
                this.httpRequest.Get(url);
                await Task.Delay(delayRequest);
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> HEADAsync(string url)
        {
            try
            {
                this.httpRequest.Head(url);
                await Task.Delay(delayRequest);
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> POSTAsync(string url)
        {
            try
            {

                this.httpRequest.Post(url);
                await Task.Delay(delayRequest);
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> POSTAsync(string url, RequestParams requestParams)
        {
            try
            {

                this.httpRequest.Post(url, requestParams);
                await Task.Delay(delayRequest);
            }
            catch
            {
            }
            return true;
        }

        public async Task<bool> POSTAsync(string url, HttpContent httpContent)
        {
            try
            {
                this.httpRequest.Post(url, httpContent);
                await Task.Delay(delayRequest);
            }
            catch { }
            return true;
        }

        public async Task<bool> POSTAsync(string address, string str, string contentType)
        {
            try
            {
                this.httpRequest.Post(address, str, contentType);
                await Task.Delay(delayRequest);
            }
            catch
            {
            }
            return true;
        }

        public async Task<string> POST_GetAddressAsync(string url, RequestParams requestParams)
        {
            try
            {

                this.httpRequest.Post(url, requestParams);
                await Task.Delay(delayRequest);
                if (httpRequest.Response.IsOK)
                {
                    return httpRequest.Response.Address.OriginalString;
                }
                else { return null; }
            }
            catch
            {
                return null;
            }
        }
    }
}
