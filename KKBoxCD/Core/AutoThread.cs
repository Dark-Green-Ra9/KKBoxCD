using System;
using System.Net;
using System.Threading;
using KKBoxCD.Core.Manager;
using KKBoxCD.Core.Support;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using RestSharp;

namespace KKBoxCD.Core
{
    public class AutoThread
    {
        private readonly AccountManager mAccountManager;
        private readonly ProxyManager mProxyManager;

        #region Properties Variable

        public int ThreadID { get; private set; }

        public Account Account;

        public Proxy Proxy;

        public Browser Browser { get; private set; }

        public Page Page { get; private set; }

        public PageCTL PageCTL { get; private set; }

        #endregion

        public AutoThread(int thread_id)
        {
            ThreadID = thread_id;
            mAccountManager = AccountManager.Instance;
            mProxyManager = ProxyManager.Instance;
        }

        #region Operating Function

        public void Start()
        {
            Thread thread = new Thread(Work)
            {
                Name = "AutoThread"
            };
            thread.Start();
        }

        private void Log(string content)
        {
            Console.WriteLine("[THREAD_{0}] {1}", ThreadID, content);
        }

        #endregion

        #region Work Function

        private void Work()
        {
            while (true)
            {
                try
                {
                    Log(">> Lấy Account");
                    Account = mAccountManager.Get();
                    if (Account == null)
                    {
                        Log("Đã hết Account");
                        break;
                    }

                    Log(">> Lấy Proxy");
                    Proxy = mProxyManager.Random();
                    if (Proxy == null)
                    {
                        Log("Đã hết Proxy");
                        break;
                    }

                    Log(">> Kiểm tra Account");
                    bool success = IsExist();
                    if (!success)
                    {
                        mAccountManager.Write(Account, WriteType.Wrong);
                        Account = null;
                        continue;
                    }

                    Log(">> Khởi tạo Browser và Page");
                    PuppeteerExtra extra = new PuppeteerExtra();
                    LaunchOptions options = new LaunchOptions()
                    {
                        Headless = false,
                        ExecutablePath = Consts.ChromeFile,
                        DefaultViewport = null,
                        Args = new string[]
                        {

                        },
                    };
                    Browser = extra.LaunchAsync(options).Result;
                    Page = Browser.PagesAsync().Result[0];
                    PageCTL = new PageCTL(Page);

                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Log($">> Lỗi phát sinh: {ex}");
                }
                finally
                {
                    Log(">> Dọn dẹp tài nguyên");

                    if (Account != null)
                    {
                        mAccountManager.Push(Account);
                        Account = null;
                    }
                    if (Proxy != null)
                    {
                        Proxy = null;
                    }
                    if (Page != null)
                    {
                        try
                        {
                            Page.CloseAsync().Wait();
                            Page.Dispose();
                        }
                        catch { }
                        Page = null;
                    }
                    if (Browser != null)
                        {
                            try
                            {
                                Browser.CloseAsync().Wait();
                                Browser.Dispose();
                            }
                            catch { }
                            Browser = null;
                        }

                    GC.Collect();
                }
            }
        }

        private bool IsExist()
        {
            try
            {
                WebProxy proxy = new WebProxy(Proxy.Address, Proxy.Port);
                RestClient client = new RestClient("https://kkid.kkbox.com/challenge")
                {
                    Timeout = 30000,
                    //Proxy = proxy,
                };
                RestRequest request = new RestRequest(Method.POST)
                {
                    AlwaysMultipartFormData = true
                };

                request.AddParameter("username", Account.Email);
                request.AddParameter("a", "null");

                IRestResponse response = client.Execute(request);
                HttpStatusCode code = response.StatusCode;

                return code == HttpStatusCode.OK;
            }
            catch
            {
                throw new Exception("Kiểm tra Account thất bại");
            }
        }

        private bool IsExpired()
        {
            try
            {
                return Page.EvaluateFunctionAsync<bool>(@"
                () => {
                    try {
                        const auth = JSON.parse(localStorage['wp:auth:v2']);
                        return auth.user.memberType == 'EXPIRED_PAID';
                    }
                    catch {
                        return false;
                    }
                }").Result;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}