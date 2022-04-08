using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using KKBoxCD.Core.Manager;
using KKBoxCD.Core.Support;
using KKBoxCD.Core.Utils;
using KKBoxCD.Properties;
using Leaf.xNet;
using MihaZupan;
using Newtonsoft.Json;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using RestSharp;

namespace KKBoxCD.Core
{
    public class AutoThread
    {
        private readonly string[] PerfectTest;
        private readonly Config mConfig;
        private readonly AccountManager mAccountManager;
        private readonly ProxyManager mProxyManager;
        private readonly ChromeClient mChromeClient;
        private readonly XevilClient mXevilClient;

        #region Properties Variable

        public int ThreadID { get; private set; }

        public Account Account { get; private set; }

        public Proxy Proxy { get; private set; }

        public Browser Browser { get; private set; }

        public Page Page { get; private set; }

        public PageCTL PageCTL { get; private set; }

        #endregion

        public AutoThread(int thread_id)
        {
            if (!File.Exists(Consts.PerfectTestFile))
            {
                File.WriteAllText(Consts.PerfectTestFile, "11design.g@gmail.com");
            }
            PerfectTest = File.ReadAllLines(Consts.PerfectTestFile);
            if (!PerfectTest.Any())
            {
                PerfectTest = new string[]
                {
                    "11design.g@gmail.com"
                };
            }

            ThreadID = thread_id;
            mConfig = Config.Instance;
            mAccountManager = AccountManager.Instance;
            mProxyManager = ProxyManager.Instance;
            mChromeClient = ChromeClient.Instance;
            mXevilClient = XevilClient.Instance;
        }

        #region Operating Function

        public void Start()
        {
            if (mConfig.Mode == Config.RunMode.CheckExist)
            {
                new Thread(CheckExist)
                {
                    Name = "AutoThread"
                }.Start();
            }
            else
            {
                new Thread(GetPlan)
                {
                    Name = "AutoThread"
                }.Start();
            }
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

        private void CheckExist()
        {
            while (true)
            {
                Log(">> Khởi tạo biến nháp");
                RestClient client = null;
                RestRequest request;
                RestResponse response;
                dynamic data = null;

                try
                {
                    Log(">> Lấy Proxy");
                    Proxy = mProxyManager.Random();
                    if (Proxy == null)
                    {
                        Log(">> Đã hết Proxy");
                        break;
                    }

                    Log(">> Khởi tạo RestClient");
                    IWebProxy proxy;
                    if (mConfig.IsSocks)
                    {
                        proxy = new HttpToSocks5Proxy(Proxy.Address, Proxy.Port);
                    }
                    else
                    {
                        proxy = new WebProxy(Proxy.Address, Proxy.Port);
                    }
                    if (!string.IsNullOrEmpty(Proxy.Username) && !string.IsNullOrEmpty(Proxy.Password))
                    {
                        proxy.Credentials = new NetworkCredential(Proxy.Username, Proxy.Password);
                    }
                    client = new RestClient(new RestClientOptions
                    {
                        Proxy = proxy,
                        UserAgent = Addons.RandomUserAgent(),
                        Timeout = 10000
                    });

                    Log(">> Kiểm tra Proxy");
                    try
                    {
                        string perfect_test = PerfectTest[new Random().Next(0, PerfectTest.Length)];
                        request = new RestRequest("https://kkid.kkbox.com/challenge", Method.Post);
                        request.AddParameter("username", perfect_test);
                        request.AddParameter("phone_country_code", "65");
                        request.AddParameter("phone_territory_code", "SG");
                        request.AddParameter("a", Addons.RandomHash(512));
                        response = client.ExecuteAsync(request).Result;
                    }
                    catch
                    {
                        throw new Exception("Kiểm tra Proxy thất bại");
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        States.ProxyBlock++;
                        throw new Exception("Proxy đang bị chặn");
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        Log(">> Lấy Account");
                        Account = mAccountManager.Get();
                        if (Account == null)
                        {
                            break;
                        }

                        Log(">> Yêu cầu Challenge");
                        try
                        {
                            request = new RestRequest("https://kkid.kkbox.com/challenge", Method.Post);
                            request.AddParameter("username", Account.Email);
                            request.AddParameter("phone_country_code", "65");
                            request.AddParameter("phone_territory_code", "SG");
                            request.AddParameter("a", Addons.RandomHash(512));
                            response = client.ExecuteAsync(request).Result;
                            data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                        }
                        catch
                        {
                            throw new Exception("Yêu cầu Challenge thất bại");
                        }

                        Log(">> Kiểm tra Challenge");
                        if (data.error != null)
                        {
                            string error = data.error;
                            string status_code = data.status_code;

                            if (status_code.Equals("404"))
                            {
                                // NotFound or BlockIP
                                States.NotFound++;
                                Account.Status = AccountStatus.NotFound;
                                Account.Data = error;
                                mAccountManager.Write(Account);
                                Account = null;
                                continue;
                            }
                            else if (status_code.Equals("403"))
                            {
                                // Login Without Crpyt Password
                                States.SRPUnsupported++;
                                Account.Status = AccountStatus.SRPUnsupported;
                                Account.Data = error;
                                mAccountManager.Write(Account);
                                Account = null;
                                continue;
                            }
                            else
                            {
                                //Not Verify Mail
                                States.Other++;
                                Account.Status = AccountStatus.Other;
                                Account.Data = error;
                                mAccountManager.Write(Account);
                                Account = null;
                                continue;
                            }
                        }
                        else if (data.g != null && data.s != null && data.q != null)
                        {
                            // Login With Crpyt Password
                            Log(">> Ghi kết quả");
                            States.Perfect++;
                            Account.Status = AccountStatus.Perfect;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                        else
                        {
                            throw new Exception("Yêu cầu Challenge thất bại");
                        }
                    }

                    Log(">> Kiểm tra Account");
                    if (mAccountManager.IsEmpty())
                    {
                        Log(">> Đã hết Account");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (mConfig.ShowExc)
                    {
                        Console.WriteLine($">> Lỗi phát sinh: {ex}");
                    }
                }
                finally
                {
                    Log(">> Dọn dẹp tài nguyên");
                    if (client != null)
                    {
                        client.Dispose();
                    }
                    if (Account != null)
                    {
                        mAccountManager.Push(Account);
                        Account = null;
                    }
                    if (Proxy != null)
                    {
                        if (!mConfig.DuplProxy)
                        {
                            mProxyManager.Push(Proxy);
                        }
                        Proxy = null;
                    }
                }
            }
        }

        private void GetPlan()
        {
            while (true)
            {
                Log(">> Khởi tạo biến gửi");
                string referer = "https://www.kkbox.com/";
                string recaptcha = string.Empty;
                string remember = "1";
                string redirect = string.Empty;
                string phone_country_code = string.Empty;
                string phone_territory_code = string.Empty;
                string friend = string.Empty;
                string ori_username = string.Empty;
                string username = string.Empty;
                string secret = string.Empty;
                string t = string.Empty;

                Log(">> Khởi tạo biến nháp");
                LeafClient client = null;
                dynamic data = null;
                string A = null;
                string g = null;
                string s = null;
                string B = null;

                try
                {
                    Log(">> Lấy Proxy");
                    Proxy = mProxyManager.Random();
                    if (Proxy == null)
                    {
                        Log(">> Đã hết Proxy");
                        break;
                    }

                    Log(">> Lấy Account");
                    Account = mAccountManager.Get();
                    if (Account == null)
                    {
                        Log(">> Đã hết Account");
                        break;
                    }

                    Log(">> Khởi tạo RestClient");
                    IWebProxy proxy;
                    if (mConfig.IsSocks)
                    {
                        proxy = new HttpToSocks5Proxy(Proxy.Address, Proxy.Port);
                    }
                    else
                    {
                        proxy = new WebProxy(Proxy.Address, Proxy.Port);
                    }
                    if (!string.IsNullOrEmpty(Proxy.Username) && !string.IsNullOrEmpty(Proxy.Password))
                    {
                        proxy.Credentials = new NetworkCredential(Proxy.Username, Proxy.Password);
                    }
                    client = new LeafClient()
                    {
                        Timeout = 10000,
                        UserAgent = Addons.RandomUserAgent(),
                    };
                    client.SetProxy("192.168.150.167", 8888, false);

                    Log(">> Yêu cầu Page");
                    bool success = client.GetAsync("https://kkid.kkbox.com/login").Result;
                    if (!success)
                    {
                        throw new Exception("Yêu cầu Page thất bại");
                    }

                    Log(">> Trích xuất CountryCode");
                    try
                    {
                        Regex regex = new Regex("initialCountry: \"(.*?)\",");
                        MatchCollection matched = regex.Matches(client.Response);
                        phone_territory_code = matched[0].Groups[1].Value;
                        phone_country_code = mChromeClient.Page.EvaluateFunctionAsync<string>("(country) => GetDialCode(country)", phone_territory_code).Result;
                    }
                    catch
                    {
                        string[][] temps = new string[][]
                        {
                            new string[] { "TW", "886" },
                            new string[] { "SG", "65" },
                            new string[] { "HK", "852" }
                        };
                        string[] temp = temps[new Random().Next(0, temps.Length)];
                        phone_territory_code = temp[0];
                        phone_country_code = temp[1];
                    }

                    Log(">> Yêu cầu Friend");
                    success = client.GetAsync("https://kkid.kkbox.com/friend").Result;
                    if (!success)
                    {
                        throw new Exception("Yêu cầu Friend thất bại");
                    }
                    data = JsonConvert.DeserializeObject<dynamic>(client.Response);

                    Log(">> Kiểm tra Friend");
                    if (data.q == null)
                    {
                        throw new Exception("Yêu cầu Friend thất bại");
                    }

                    Log(">> Gán Friend, Username, Secret");
                    try
                    {
                        friend = data.q;
                        ori_username = Account.Email;
                        username = Account.Email;
                        secret = Account.Password;
                    }
                    catch
                    {
                        throw new Exception("Gán Friend, Username, Secret thất bại");
                    }

                    Log(">> Khởi tạo SRP, tính A");
                    try
                    {
                        A = mChromeClient.Page.EvaluateFunctionAsync<string>(@"(name) => {
                            RemoveSRP(name);
                            const srp = GetSRP(name);
                            srp.init();
                            srp.computeA();
                            return srp.A.toString(16);
                        }", ori_username).Result;
                    }
                    catch
                    {
                        throw new Exception("Khởi tạo SRP, tính A thất bại");
                    }

                    Log(">> Yêu cầu Challenge");
                    success = client.PostAsync("https://kkid.kkbox.com/challenge", new RequestParams
                    {
                        ["username"] = username,
                        ["phone_country_code"] = phone_country_code,
                        ["phone_territory_code"] = phone_territory_code,
                        ["a"] = A
                    }).Result;
                    if (success)
                    {
                        throw new Exception("Yêu cầu Challenge thất bại");
                    }
                    data = JsonConvert.DeserializeObject<dynamic>(client.Response);

                    Console.WriteLine(client.Response);
                    Console.WriteLine("End");
                    Console.ReadKey();

                    Log(">> Kiểm tra Challenge");
                    if (data.error != null)
                    {
                        string error = data.error;
                        string status_code = data.status_code;

                        if (status_code.Equals("404"))
                        {
                            // NotFound or BlockIP
                            States.NotFound++;
                            Account.Status = AccountStatus.NotFound;
                            Account.Data = error;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                        else if (status_code.Equals("403"))
                        {
                            // Login Without Crpyt Password
                            States.SRPUnsupported++;
                            Account.Status = AccountStatus.SRPUnsupported;
                            Account.Data = error;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                        else
                        {
                            //Not Verify Mail
                            States.Other++;
                            Account.Status = AccountStatus.Other;
                            Account.Data = error;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                    }
                    else if (data.g != null && data.s != null && data.q != null)
                    {
                        g = data.g;
                        s = data.s;
                        username = data.q;
                    }
                    else
                    {
                        throw new Exception("Yêu cầu Challenge thất bại");
                    }

                    Log(">> Tái khởi tạo SRP, tính A");
                    try
                    {
                        A = mChromeClient.Page.EvaluateFunctionAsync<string>(@"async (name, g, s) => {
                        const srp = GetSRP(name);
                        srp.init(g);
                        srp.s = s;
                        srp.computeA();
                        return srp.A.toString(16);
                    }", ori_username, g, s).Result;
                    }
                    catch
                    {
                        throw new Exception("Tái khởi tạo SRP, tính A thất bại");
                    }

                    Log(">> Yêu cầu Challenge Verify");
                    success = client.PostAsync("https://kkid.kkbox.com/challenge_verify", new RequestParams
                    {
                        ["username"] = username,
                        ["a"] = A
                    }).Result;
                    if (!success)
                    {
                        throw new Exception("Yêu cầu Challenge Verify thất bại");
                    }
                    data = JsonConvert.DeserializeObject<dynamic>(client.Response);

                    Log(">> Kiểm tra Challenge Verify");
                    if (data.error != null)
                    {
                        string error = data.error;
                        string status_code = data.status_code;

                        if (status_code.Equals("404"))
                        {
                            States.NotFound++;
                            Account.Status = AccountStatus.NotFound;
                            Account.Data = error;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                        else if (status_code.Equals("403"))
                        {
                            States.LoginFailed++;
                            Account.Status = AccountStatus.LoginFailed;
                            Account.Data = error;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                        else
                        {
                            throw new Exception(string.Concat("Phân loại lỗi thất bại: ", error));
                        }
                    }
                    else if (data.B != null)
                    {
                        B = data.B;
                    }
                    else
                    {
                        throw new Exception("Yêu cầu Challenge Verify thất bại");
                    }

                    Log(">> Gán giá trị SRP");
                    try
                    {
                        mChromeClient.Page.EvaluateFunctionAsync(@"async (name, username, secret, B) => {
                            const srp = GetSRP(name);
                            srp.I = username;
                            srp.p = srp.computeHash(secret);
                            srp.B = new BigInteger(B, 16);
                        }", ori_username, username, secret, B).Wait();
                    }
                    catch
                    {
                        throw new Exception("Gán giá trị SRP thất bại");
                    }

                    Log(">> Kiểm tra Submit");
                    if (!g.Equals("G2048"))
                    {
                        throw new Exception("Không phải Submit G2048");
                    }

                    Log(">> Kiểm tra ReChallenge");
                    bool re_challenge = mChromeClient.Page.EvaluateFunctionAsync<bool>(@"async (name) => {
                        const srp = GetSRP(name);
                        return !srp.verifyB() && srp.verifyHAB();
                    }", ori_username).Result;
                    if (re_challenge)
                    {
                        throw new Exception("Yêu cầu ReChallenge");
                    }

                    Log(">> Mã hóa mật khẩu");
                    try
                    {
                        secret = mChromeClient.Page.EvaluateFunctionAsync<string>(@"async (name) => {
                            const srp = GetSRP(name);
                            srp.computeVerifier();
                            let ck = srp.computeClientK();
                            RemoveSRP(name);
                            return ck;
                        }", ori_username).Result;
                    }
                    catch
                    {
                        throw new Exception("Mã hóa mật khẩu thất bại");
                    }

                    Log(">> Khởi tạo Recaptcha");
                    recaptcha = mXevilClient.Get();
                    if (recaptcha == null)
                    {
                        throw new Exception("Khởi tạo Recaptcha thất bại");
                    }

                    Log(">> Yêu cầu Submit");
                    success = client.PostAsync("https://kkid.kkbox.com/login", new RequestParams
                    {
                        ["referer"] = referer,
                        ["recaptcha"] = recaptcha,
                        ["remember"] = remember,
                        ["redirect"] = redirect,
                        ["phone_country_code"] = phone_country_code,
                        ["phone_territory_code"] = phone_territory_code,
                        ["friend"] = friend,
                        ["username"] = username,
                        ["ori_username"] = ori_username,
                        ["secret"] = secret,
                        ["t"] = t,
                    }).Result;
                    recaptcha = null;
                    if (!success)
                    {
                        throw new Exception("Yêu cầu bại");
                    }

                    Log(">> Kiểm tra Submit");
                    if (client.Response.Contains("passed the reCAPTCHA"))
                    {
                        throw new Exception("Vượt qua reCAPTCHA thất bại");
                    }
                    else if (client.Response.Contains("Login has failed"))
                    {
                        States.LoginFailed++;
                        Account.Status = AccountStatus.LoginFailed;
                        Account.Data = "Login has failed";
                        mAccountManager.Write(Account);
                        Account = null;
                        continue;
                    }

                    Log(">> Yêu cầu Auth");
                    success = client.GetAsync("https://mykkid.kkbox.com/login").Result;
                    if (!success)
                    {
                        throw new Exception("Yêu cầu Auth thất bại");
                    }

                    Log(">> Kiểm tra Auth");
                    if (!client.Response.Contains("member center"))
                    {
                        throw new Exception("Yêu cầu Auth thất bại");
                    }

                    Log(">> Yêu cầu Plan");
                    success = client.GetAsync("https://mykkid.kkbox.com/plan").Result;
                    if (!success)
                    {
                        throw new Exception("Yêu cầu Plan thất bại");
                    }

                    Log(">> Kiểm tra Plan");
                    if (!client.Response.Contains("My Plans"))
                    {
                        throw new Exception("Yêu cầu Plan thất bại");
                    }
                    try
                    {
                        Account.Data = mChromeClient.Page.EvaluateFunctionAsync<string>("async (html) => GetPlanFromHTML(html)", client.Response).Result;
                    }
                    catch
                    {
                        Account.Data = "Get Failed";
                    }

                    Log(">> Ghi kết quả");
                    States.Perfect++;
                    Account.Status = AccountStatus.Perfect;
                    mAccountManager.Write(Account);
                    Account = null;
                }
                catch (Exception ex)
                {
                    if (mConfig.ShowExc)
                    {
                        Console.WriteLine($">> Lỗi phát sinh: {ex}");
                    }
                }
                finally
                {
                    Log(">> Dọn dẹp tài nguyên");
                    if (ori_username != null)
                    {
                        try
                        {
                            mChromeClient.Page.EvaluateFunctionAsync("async (name) => RemoveSRP(name)", ori_username).Wait();
                        }
                        catch { }
                    }
                    if (client != null)
                    {
                        client.Dispose();
                    }
                    if (recaptcha != null)
                    {
                        mXevilClient.Push(recaptcha);
                    }
                    if (Account != null)
                    {
                        mAccountManager.Push(Account);
                        Account = null;
                    }
                    if (Proxy != null)
                    {
                        mProxyManager.Push(Proxy);
                        Proxy = null;
                    }
                }
            }
        }

        private void GetPlanBrowser()
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

                    Log(">> Khởi tạo Browser và Page");
                    PuppeteerExtra extra = new PuppeteerExtra();
                    StealthPlugin stealth = new StealthPlugin();
                    LaunchOptions options = new LaunchOptions()
                    {
                        Headless = !mConfig.IsDebug,
                        ExecutablePath = Consts.ChromeFile,
                        Args = new string[]
                        {
                            //$"--proxy-server=\"{Proxy.Address}:{Proxy.Port}\"",
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
                        },
                    };
                    Browser = extra.LaunchAsync(options).Result;
                    Page = Browser.PagesAsync().Result[0];
                    PageCTL = new PageCTL(Page);

                    string recaptcha = null;
                    bool success = false;
                    int loop = 5;
                    while (loop > 0)
                    {
                        Log(">> Lấy Account");
                        Account = mAccountManager.Get();
                        if (Account == null)
                        {
                            Log("Đã hết Account");
                            throw new Exception("Đã hết Account");
                        }

                        Log(">> Truy cập trang");
                        if (!Page.Url.StartsWith("https://kkid.kkbox.com/login"))
                        {
                            success = PageCTL.GoToAsync("https://kkid.kkbox.com/login", "#recaptcha", new NavigationOptions
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
                        }

                        //Log(">> Đợi Recaptcha sẵn sàng");
                        //success = WaitRecaptcha();
                        //if (!success)
                        //{
                        //    throw new Exception("Đợi Recaptcha thất bại");
                        //}

                        //Log(">> Dừng tải trang");
                        //Thread.Sleep(250);
                        //PageCTL.StopLoadingAsync().Wait();

                        try
                        {
                            Page.ClickAsync("#btn-submit");
                        }
                        catch { }
                        Log("Tiêm trích trang");
                        Thread.Sleep(250);
                        try
                        {
                            Page.EvaluateExpressionAsync(Resources.kkbox_inject).Wait();
                        }
                        catch
                        {
                            throw new Exception("Tiêm trích trang");
                        }

                        Log(">> Khởi tạo Recaptcha");
                        recaptcha = mXevilClient.Get();
                        if (recaptcha == null)
                        {
                            throw new Exception("Khởi tạo Recaptcha thất bại");
                        }

                        Log(">> Gửi lệnh đăng nhập");
                        Thread.Sleep(250);
                        try
                        {
                            Page.EvaluateFunctionAsync(@"
                            (username, password, recaptcha) => {
                                document.querySelector('#recaptcha').value = recaptcha;
                                window.__username = username;
                                window.__password = password;
                                challenge();
                            }", Account.Email, Account.Password, recaptcha).Wait();
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
                        }).Result;
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
                                Account.Data = Page.EvaluateFunctionAsync<string>(@"
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
                                Account.Data = "Get Failed";
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
                                Account.Status = AccountStatus.Other;
                                Account.Data = "not_verified";
                                mAccountManager.Write(Account);
                                Account = null;
                                States.Other++;
                            }
                            else if (error_content.Equals("login_failed"))
                            {
                                Log(">> Ghi kết quả");
                                Account.Status = AccountStatus.LoginFailed;
                                Account.Data = "login_failed";
                                mAccountManager.Write(Account);
                                Account = null;
                                States.LoginFailed++;
                            }
                            else
                            {
                                throw new Exception("Phân loại lỗi ngoại lệ");
                            }
                        }
                        else if (PageCTL.ExistAsync(".server_prompt").Result)
                        {
                            string error = PageCTL.GetElementInnerText(".server_prompt").Result;
                            if (error.Contains("Login has failed"))
                            {
                                States.LoginFailed++;
                                Account.Status = AccountStatus.LoginFailed;
                                Account.Data = "Login has failed";
                                mAccountManager.Write(Account);
                                Account = null;
                                continue;
                            }
                            else if (error.Contains("passed the reCAPTCHA"))
                            {
                                throw new Exception("Vượt qua reCAPTCHA thất bại");
                            }
                            else
                            {
                                throw new Exception("Đăng nhập thất bại");
                            }
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
                    if (mConfig.ShowExc)
                    {
                        Console.WriteLine($">> Lỗi phát sinh: {ex}");
                    }
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
                        mProxyManager.Push(Proxy);
                        Proxy = null;
                    }
                }

                break;
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

        #endregion
    }
}