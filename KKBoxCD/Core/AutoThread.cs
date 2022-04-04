using System;
using System.Net;
using System.Threading;
using KKBoxCD.Core.Manager;
using KKBoxCD.Core.Utils;
using MihaZupan;
using Newtonsoft.Json;
using RestSharp;

namespace KKBoxCD.Core
{
    public class AutoThread
    {
        private readonly Config mConfig;
        private readonly AccountManager mAccountManager;
        private readonly ProxyManager mProxyManager;
        private readonly ChromeClient mChromeClient;
        private readonly RecaptchaClient mRecaptchaClient;

        #region Properties Variable

        public int ThreadID { get; private set; }

        public Account Account { get; private set; }

        public Proxy Proxy { get; private set; }

        #endregion

        public AutoThread(int thread_id)
        {
            ThreadID = thread_id;
            mConfig = Config.Instance;
            mAccountManager = AccountManager.Instance;
            mProxyManager = ProxyManager.Instance;
            mChromeClient = ChromeClient.Instance;
            mRecaptchaClient = RecaptchaClient.Instance;
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
                Log(">> Khởi tạo biến gửi");
                string phone_country_code = "65";
                string phone_territory_code = "SG";
                string username = "";

                Log(">> Khởi tạo biến nháp");
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

                    Log(">> Lấy Account");
                    Account = mAccountManager.Get();
                    if (Account == null)
                    {
                        Log(">> Đã hết Account");
                        break;
                    }
                    username = Account.Email;

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
                    RestClient client = new RestClient(new RestClientOptions
                    {
                        Proxy = proxy,
                        UserAgent = Addons.RandomUserAgent(),
                        Timeout = 10000
                    });

                    Log(">> Khởi tạo SRP, tính A");
                    string A;
                    try
                    {
                        A = mChromeClient.Page.EvaluateFunctionAsync<string>(@"(name) => {
                            RemoveSRP(name);
                            const srp = GetSRP(name);
                            srp.init();
                            srp.computeA();
                            return srp.A.toString(16);
                        }", username).Result;
                    }
                    catch
                    {
                        throw new Exception("Khởi tạo SRP, tính A thất bại");
                    }

                    Log(">> Yêu cầu Challenge");
                    try
                    {
                        request = new RestRequest("https://kkid.kkbox.com/challenge", Method.Post);
                        request.AddParameter("username", username);
                        request.AddParameter("phone_country_code", phone_country_code);
                        request.AddParameter("phone_territory_code", phone_territory_code);
                        request.AddParameter("a", A);
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
                            States.NotFound++;
                            Account.Status = AccountStatus.NotFound;
                            Account.Data = error;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                        else if (status_code.Equals("403"))
                        {
                            States.NotFound++;
                            Account.Status = AccountStatus.NotFound;
                            Account.Data = error;
                            mAccountManager.Write(Account);
                            Account = null;
                            continue;
                        }
                        else
                        {
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
                        Log(">> Ghi kết quả");
                        States.Perfect++;
                        Account.Status = AccountStatus.Perfect;
                        mAccountManager.Write(Account);
                        Account = null;
                    }
                    else
                    {
                        throw new Exception("Yêu cầu Challenge thất bại");
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

                    if (username != null)
                    {
                        try
                        {
                            mChromeClient.Page.EvaluateFunctionAsync("async (name) => RemoveSRP(name)", username).Wait();
                        }
                        catch { }
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

        #endregion

        #region

        private void CheckFull()
        {
            while (true)
            {
                Log(">> Khởi tạo biến gửi");
                string referer = "https://www.kkbox.com/";
                string recaptcha = "";
                string remember = "";
                string redirect = "";
                string phone_country_code = "65";
                string phone_territory_code = "SG";
                string friend = "";
                string ori_username = "";
                string username = "";
                string secret = "";
                string t = "";

                Log(">> Khởi tạo biến nháp");
                RestRequest request;
                RestResponse response;
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
                    RestClient client = new RestClient(new RestClientOptions
                    {
                        Proxy = proxy,
                        UserAgent = Addons.RandomUserAgent(),
                        Timeout = 10000
                    });

                    Log(">> Yêu cầu Friend");
                    try
                    {
                        request = new RestRequest("https://kkid.kkbox.com/friend", Method.Get);
                        response = client.ExecuteAsync(request).Result;
                        data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Concat("Yêu cầu Friend thất bại: ", ex.Message));
                    }

                    Log(">> Kiểm tra Friend");
                    if (data.q == null)
                    {
                        throw new Exception(string.Concat("Yêu cầu Friend thất bại: ", response.Content));
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
                    try
                    {
                        request = new RestRequest("https://kkid.kkbox.com/challenge", Method.Post);
                        request.AddParameter("username", username);
                        request.AddParameter("phone_country_code", phone_country_code);
                        request.AddParameter("phone_territory_code", phone_territory_code);
                        request.AddParameter("a", A);
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
                    try
                    {
                        request = new RestRequest("https://kkid.kkbox.com/challenge_verify", Method.Post);
                        request.AddParameter("username", username);
                        request.AddParameter("a", A);
                        response = client.ExecuteAsync(request).Result;
                        data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    }
                    catch
                    {
                        throw new Exception("Yêu cầu Challenge Verify thất bại");
                    }

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

                    Log(">> Khởi tạo Recaptcha");
                    recaptcha = mRecaptchaClient.GetToken();
                    if (recaptcha == null)
                    {
                        throw new Exception("Khởi tạo Recaptcha thất bại");
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

                    Log(">> Yêu cầu Submit");
                    try
                    {
                        request = new RestRequest("https://kkid.kkbox.com/login", Method.Post);
                        request.AddParameter("referer", referer);
                        request.AddParameter("recaptcha", recaptcha);
                        request.AddParameter("remember", remember);
                        request.AddParameter("redirect", redirect);
                        request.AddParameter("phone_country_code", phone_country_code);
                        request.AddParameter("phone_territory_code", phone_territory_code);
                        request.AddParameter("friend", friend);
                        request.AddParameter("username", username);
                        request.AddParameter("ori_username", ori_username);
                        request.AddParameter("secret", secret);
                        request.AddParameter("t", t);
                        response = client.ExecuteAsync(request).Result;
                    }
                    catch
                    {
                        throw new Exception("Yêu cầu bại");
                    }

                    Log(">> Kiểm tra Submit");
                    if (response.Content.Contains("passed the reCAPTCHA"))
                    {
                        States.RecaptchaFailed++;
                        Account.Status = AccountStatus.RecaptchaFailed;
                        Account.Data = "passed the reCAPTCHA";
                        mAccountManager.Write(Account);
                        Account = null;
                        continue;

                        //throw new Exception("Vượt qua reCAPTCHA thất bại");
                    }
                    else if (response.Content.Contains("Login has failed"))
                    {
                        States.LoginFailed++;
                        Account.Status = AccountStatus.LoginFailed;
                        Account.Data = "Login has failed";
                        mAccountManager.Write(Account);
                        Account = null;
                        continue;
                    }

                    Log(">> Yêu cầu Auth");
                    try
                    {
                        request = new RestRequest("https://mykkid.kkbox.com/login", Method.Get);
                        response = client.ExecuteAsync(request).Result;
                    }
                    catch
                    {
                        throw new Exception("Yêu cầu Auth thất bại");
                    }

                    Log(">> Kiểm tra Auth");
                    if (!response.Content.Contains("member center"))
                    {
                        throw new Exception("Yêu cầu Auth thất bại");
                    }

                    Log(">> Yêu cầu Plan");
                    try
                    {
                        request = new RestRequest("https://mykkid.kkbox.com/plan", Method.Get);
                        response = client.ExecuteAsync(request).Result;
                    }
                    catch
                    {
                        throw new Exception("Yêu cầu Plan thất bại");
                    }

                    Log(">> Kiểm tra Plan");
                    if (!response.Content.Contains("My Plans"))
                    {
                        throw new Exception("Yêu cầu Plan thất bại");
                    }
                    try
                    {
                        Account.Data = mChromeClient.Page.EvaluateFunctionAsync<string>("async (html) => GetPlanFromHTML(html)", response.Content).Result;
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

        #endregion
    }
}