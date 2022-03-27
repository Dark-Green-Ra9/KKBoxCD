using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using KKBoxCD.Core.Manager;
using KKBoxCD.Core.Support;
using KKBoxCD.Properties;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using RestSharp;

namespace KKBoxCD.Core
{
    public class AutoThread
    {
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
                        Account.Status = AccountStatus.NotExist.ToString();
                        mAccountManager.Write(Account);
                        Account = null;
                        Statics.NotExist++;
                        Statics.Show();
                        continue;
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
                    Page.Response += OnResponse;

                    Log(">> Truy cập trang");
                    success = PageCTL.GoToAsync("https://kkid.kkbox.com/login", "#show-username", new NavigationOptions
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
                        "#logout"
                    }).Result;
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
                        Statics.Show();
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
                            Statics.Show();
                        }
                        else if (error_content.Equals("login_failed"))
                        {
                            Log(">> Ghi kết quả");
                            Account.Status = AccountStatus.Wrong.ToString();
                            mAccountManager.Write(Account);
                            Account = null;
                            Statics.Wrong++;
                            Statics.Show();
                        }
                        else
                        {
                            throw new Exception("Phân loại lỗi ngoại lệ");
                        }
                    }
                    else
                    {
                        throw new Exception("Kiểm tra kết quả đăng nhập thất bại");
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
            else if (type == ResourceType.Script)
            {
                try
                {
                    string name = Path.GetFileName(req.Url);
                    string hash = Hash(name);
                    string file = Path.Combine(Consts.CacheDir, hash);

                    if (File.Exists(file))
                    {
                        string body = File.ReadAllText(file);
                        await req.RespondAsync(new ResponseData
                        {
                            ContentType = "application/javascript",
                            Body = body,
                            Status = HttpStatusCode.OK,
                        });
                    }
                    else
                    {
                        await req.ContinueAsync();
                    }
                }
                catch
                {
                    await req.ContinueAsync();
                }
            }
            else
            {
                await req.ContinueAsync();
            }
        }

        private async void OnResponse(object sender, ResponseCreatedEventArgs e)
        {
            Response res = e.Response;
            ResourceType type = res.Request.ResourceType;
            try
            {
                if (type == ResourceType.Script)
                {
                    string name = Path.GetFileName(res.Request.Url);
                    string hash = Hash(name);
                    string file = Path.Combine(Consts.CacheDir, hash);

                    if (!Directory.Exists(Consts.CacheDir))
                    {
                        Directory.CreateDirectory(Consts.CacheDir);
                    }
                    if (!File.Exists(file))
                    {
                        File.WriteAllText(file, await res.TextAsync());
                    }
                }
            }
            catch { }
        }

        private string Hash(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        private bool IsExist()
        {
            return true;
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

        #endregion
    }
}