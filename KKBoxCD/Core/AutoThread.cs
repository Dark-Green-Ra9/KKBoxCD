using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using KKBoxCD.Core.Manager;
using KKBoxCD.Core.Support;
using KKBoxCD.Core.Utils;
using KKBoxCD.Properties;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using RestSharp;

namespace KKBoxCD.Core
{
    public class AutoThread
    {
        private readonly Config mConfig;
        private readonly AccountManager mAccountManager;
        private readonly ProxyManager mProxyManager;
        private readonly bool IsDebug = false;

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
            mConfig = Config.Instance;
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
            if (IsDebug)
            {
                Console.WriteLine("[THREAD_{0}] {1}", ThreadID, content);
            }
        }

        #endregion

        #region Work Function

        private void Work()
        {
            while (true)
            {
                try
                {
                    Log(">> Lấy Proxy");
                    Proxy = mProxyManager.Random();
                    if (Proxy == null)
                    {
                        Log("Đã hết Proxy");
                        break;
                    }

                    Log(">> Khởi tạo Browser và Page");
                    PuppeteerExtra extra = new PuppeteerExtra();
                    LaunchOptions options = new LaunchOptions()
                    {
                        Headless = !IsDebug,
                        ExecutablePath = Consts.ChromeFile,
                        Args = new string[]
                        {
                            $"--proxy-server=\"{Proxy.Address}:{Proxy.Port}\"",
                            "--disable-web-security",
                            "--no-sandbox",
                            "--disable-setuid-sandbox",
                            "--disable-dev-shm-usage",
                            "--disable-accelerated-2d-canvas",
                            "--no-first-run",
                            "--no-zygote",
                            "--disable-gpu"
                        },
                    };
                    Browser = extra.LaunchAsync(options).Result;
                    Page = Browser.PagesAsync().Result[0];
                    PageCTL = new PageCTL(Page);

                    Log(">> Ghi đè Request");
                    Page.SetRequestInterceptionAsync(true).Wait();
                    Page.Request += OnRequest;

                    int times = mConfig.TimesPerBrowser;
                    while (times > 0 && !mAccountManager.IsEmpty())
                    {
                        Log(">> Lấy Account");
                        Account = mAccountManager.Get();
                        if (Account == null)
                        {
                            Log("Đã hết Account");
                            break;
                        }

                        Log(">> Truy cập trang");
                        bool success = PageCTL.GoToAsync("https://kkid.kkbox.com/login", "#login_header", new NavigationOptions
                        {
                            WaitUntil = new WaitUntilNavigation[]
                            {
                                WaitUntilNavigation.DOMContentLoaded
                            }
                        }).Result;
                        if (!success)
                        {
                            throw new Exception("Truy cập trang thất bại");
                        }

                        Log(">> Đợi Recaptcha sẵn sàng");
                        Thread.Sleep(250);
                        success = WaitForRecaptcha();
                        if (!success)
                        {
                            throw new Exception("Đợi Recaptcha sẵn sàng thất bại");
                        }

                        Log(">> Tiêm trích trang");
                        Thread.Sleep(250);
                        try
                        {
                            Page.EvaluateExpressionAsync(Resources.KKBoxInject).Wait();
                        }
                        catch
                        {
                            throw new Exception("Tiêm trích trang thất bại");
                        }

                        Log(">> Gửi lệnh đăng nhập");
                        Thread.Sleep(250);
                        try
                        {
                            Page.EvaluateFunctionAsync(@"
                            (username, password) => {
                                window.__username = username;
                                window.__password = password;
                                document.querySelector('#login_header').remove();
                                challenge();
                            }", Account.Email, Account.Password).Wait();
                        }
                        catch
                        {
                            throw new Exception("Gửi lệnh đăng nhập thất bại");
                        }

                        Log(">> Đợi kết quả đăng nhập");
                        success = PageCTL.WaitForExistAnyAsync(new string[]
                        {
                            "#toast-content",
                            "#logout",
                            "#login_header"
                        }, 10000).Result;
                        if (!success)
                        {
                            throw new Exception("Đợi kết quả đăng nhập thất bại");
                        }

                        Log(">> Kiểm tra kết quả đăng nhập");
                        if (PageCTL.ExistAsync("#logout").Result)
                        {
                            Log(">> Tiến hành lấy trạng thái tài khoản");
                            Thread.Sleep(250);
                            try
                            {
                                Account.Plan = Page.EvaluateFunctionAsync<string>(@"
                            () => {
                                try {
                                    const xhr = new XMLHttpRequest();
                                    xhr.open('GET', '/plan', false);
                                    xhr.send();
                                    document.body.outerHTML = xhr.responseText;

                                    const card = document.querySelector('.plan_card');
                                    const data = card.innerText.split('\n');

                                    var plan = '';
                                    if (data.length > 0) {
                                        plan += data[0] + ' | ';
                                    }
                                    if (data.length > 1) {
                                        plan += data[1] + ' | ';
                                    }
                                    if (data.length > 4) {
                                        plan += data[4] + ' | ';
                                    }
                                    if (data.length > 12) {
                                        plan += data[12];
                                    }
                                    return plan;
                                
                                } catch {
                                    return '';
                                }
                            }").Result;
                            }
                            catch { }

                            Log(">> Ghi kết quả");
                            Account.Status = AccountStatus.Perfect.ToString();
                            mAccountManager.Write(Account);
                            Account = null;
                            Statics.Perfect++;
                            break;
                        }
                        else if (PageCTL.ExistAsync("#toast-content").Result)
                        {
                            Log(">> Đọc mã lỗi");
                            string error_content;
                            try
                            {
                                error_content = PageCTL.GetElementInnerText("#toast-content").Result;

                            }
                            catch
                            {
                                error_content = string.Empty;
                            }

                            Log(">> Phân loại lỗi");
                            if (error_content.Equals("not_verified"))
                            {
                                Log(">> Ghi kết quả");
                                Account.Status = AccountStatus.NotExist.ToString();
                                mAccountManager.Write(Account);
                                Account = null;
                                Statics.NotExist++;
                            }
                            else if (error_content.Equals("login_failed"))
                            {
                                Log(">> Ghi kết quả");
                                Account.Status = AccountStatus.Wrong.ToString();
                                mAccountManager.Write(Account);
                                Account = null;
                                Statics.Wrong++;
                            }
                            else
                            {
                                throw new Exception("Phân loại lỗi ngoại lệ");
                            }
                        }
                        else if (PageCTL.ExistAsync("#login_header").Result)
                        {
                            Log(">> Ghi kết quả");
                            Account.Status = AccountStatus.Wrong.ToString();
                            mAccountManager.Write(Account);
                            Account = null;
                            Statics.Wrong++;
                        }
                        times--;
                    }

                    if (mAccountManager.IsEmpty())
                    {
                        break;
                    }
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

        private bool WaitForRecaptcha()
        {
            int times = 30;
            while(times > 0)
            {
                try
                {
                    bool success = Page.EvaluateFunctionAsync<bool>(@"
                    () => {
                        return document.querySelector('#recaptcha').value.length > 0;
                    }").Result;
                    if (success)
                    {
                        return true;
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
                Thread.Sleep(1000);
                times--;
            }
            return false;
        }

        private async void OnRequest(object sender, RequestEventArgs e)
        {
            Request req = e.Request;
            ResourceType type = req.ResourceType;
            if (type == ResourceType.Image || type == ResourceType.Img || type == ResourceType.Font)
            {
                await req.RespondAsync(new ResponseData
                {
                    ContentType = "image/png",
                    BodyData = new byte[0],
                    Status = HttpStatusCode.OK,
                });
            }
            else if (type == ResourceType.StyleSheet)
            {
                await req.RespondAsync(new ResponseData
                {
                    ContentType = "text/css",
                    Body = "",
                    Status = HttpStatusCode.OK,
                });
            }
            else
            {
                await req.ContinueAsync();
            }
        }

        #endregion
    }
}