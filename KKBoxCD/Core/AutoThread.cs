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
                        Log("Đã hết Proxy");
                        break;
                    }

                    Log(">> Lấy Account");
                    Account = mAccountManager.Get();
                    if (Account == null)
                    {
                        Log("Đã hết Account");
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
                    if (Proxy.Username != null && Proxy.Password != null)
                    {
                        proxy.Credentials = new NetworkCredential(Proxy.Username, Proxy.Password);
                    }
                    RestClient client = new RestClient();

                    Log(">> Yêu cầu Friend");
                    try
                    {
                        request = new RestRequest("https://kkid.kkbox.com/friend", Method.Get);
                        response = client.ExecuteAsync(request).Result;
                        data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    }
                    catch
                    {
                        throw new Exception("Yêu cầu Friend thất bại");
                    }

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
                        string error_category = data.error_category;
                        string error_code = data.error_code;
                        string error_level = data.error_level;
                        string status_code = data.status_code;

                        //Phân loại lỗi

                        throw new Exception(error);
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
                        A = mChromeClient.Page.EvaluateFunctionAsync<string>(@"(name, g, s) => {
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
                        string error_category = data.error_category;
                        string error_code = data.error_code;
                        string error_level = data.error_level;
                        string status_code = data.status_code;

                        // Phân loại lỗi

                        throw new Exception(error);
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
                        mChromeClient.Page.EvaluateFunctionAsync(@"(name, username, secret, B) => {
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
                        Log(">>>> Submit 1");
                        // Sử lý Submit 1
                        return;
                    }

                    Log(">> Kiểm tra ReChallenge");
                    bool re_challenge = mChromeClient.Page.EvaluateFunctionAsync<bool>(@"(name) => {
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
                        secret = mChromeClient.Page.EvaluateFunctionAsync<string>(@"(name) => {
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
                        // Sử lý captcha failed
                        throw new Exception("Vượt qua reCAPTCHA thất bại");
                    }
                    else if (response.Content.Contains("Login has failed"))
                    {
                        // Xử lý login failed
                        throw new Exception("Đăng nhập thất bại");
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
                    // kiểm tra xem có phải trang member center

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
                    //Kiểm tra xem có phải trang plan
                    try
                    {
                        Account.Plan = mChromeClient.Page.EvaluateFunctionAsync<string>("(html) => GetPlanFromHTML(html)", response.Content).Result;
                    }
                    catch
                    {
                        Account.Plan = "Get Failed";
                    }

                    Log(">> Ghi kết quả");
                    Account.Status = AccountStatus.Perfect;
                    mAccountManager.Write(Account);
                    Account = null;
                }
                catch (Exception ex)
                {
                    Log($">> Lỗi phát sinh: {ex}");
                }
                finally
                {
                    Log(">> Dọn dẹp tài nguyên");

                    if (ori_username != null)
                    {
                        try
                        {
                            mChromeClient.Page.EvaluateFunctionAsync("(name) => RemoveSRP(name)", ori_username).Wait();
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
                        Proxy = null;
                    }
                }
            }
        }

        #endregion
    }
}