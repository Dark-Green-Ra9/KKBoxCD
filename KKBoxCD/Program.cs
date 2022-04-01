using KKBoxCD.Core;
using KKBoxCD.Core.Support;
using KKBoxCD.Core.Utils;
using KKBoxCD.Properties;
using Newtonsoft.Json;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using RestSharp;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KKBoxCD
{
    public class Program
    {
        public static void Main()
        {
            Demo();
            return;

            Console.OutputEncoding = Encoding.UTF8;

            Addons.CloseChrome();
            States.Start();

            Config config = Config.Instance;
            for (int i = 0; i < config.ThreadSize; i++)
            {
                States.ThreadSize++;
                new AutoThread(i).Start();
                Thread.Sleep(250);
            }
        }

        private static Page Page;

        private static Page PageCap;

        private static void Demo()
        {
            PuppeteerExtra extra = new PuppeteerExtra();
            LaunchOptions options = new LaunchOptions()
            {
                Headless = true,
                ExecutablePath = Consts.ChromeFile
            };
            StealthPlugin stealth = new StealthPlugin();
            Browser browser = extra.Use(stealth).LaunchAsync(options).Result;
            Page = browser.PagesAsync().Result[0];
            //PageCap = browser.NewPageAsync().Result;
            PageCTL ctl = new PageCTL(Page);

            Page.SetRequestInterceptionAsync(true).Wait();
            Page.Request += OnRequest;
            Page.GoToAsync("https://kkid.kkbox.com/login").Wait();
            //PageCap.GoToAsync("https://kkid.kkbox.com/login").Wait();

            string kkidc = "";
            string referer = "";
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

            //Nấy KKIDC và Friend
            WebProxy proxy = new WebProxy("194.31.162.182", 7698);
            RestClient client = new RestClient()
            {
                Proxy = proxy
            };
            RestRequest request = new RestRequest("https://kkid.kkbox.com/friend", Method.GET);
            IRestResponse response = client.Execute(request);
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Content);

            kkidc = response.Cookies[0].Value;
            referer = "https://www.kkbox.com/";
            friend = data.q;
            ori_username = "elsa8011@yahoo.com.tw";
            username = "elsa8011@yahoo.com.tw";
            secret = "teresa09";

            #region Challenge

            //Khởi tạo SRP và tính A
            string A = Page.EvaluateFunctionAsync<string>(@"(name) => {
                RemoveSRP(name);
                const srp = GetSRP(name);
                srp.init();
                srp.computeA();
                return srp.A.toString(16);
            }", ori_username).Result;

            //Tiến hành yêu cầu
            request = new RestRequest("https://kkid.kkbox.com/challenge", Method.POST);
            request.AddCookie("KKIDC", kkidc);
            request.AddParameter("username", ori_username);
            request.AddParameter("phone_country_code", phone_country_code);
            request.AddParameter("phone_territory_code", phone_territory_code);
            request.AddParameter("a", A);
            response = client.Execute(request);
            data = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (data.error != null)
            {
                string error = data.error;
                string error_category = data.error_category;
                string error_code = data.error_code;
                string error_level = data.error_level;
                string status_code = data.status_code;

                throw new Exception(error);
            }

            #endregion

            #region Challenge Reply

            //Gán Username
            username = data.q;

            //Tái khởi tạo SRP và tính A
            string g = data.g;
            string s = data.s;
            A = Page.EvaluateFunctionAsync<string>(@"(name, g, s) => {
                const srp = GetSRP(name);
                srp.init(g);
                srp.s = s;
                srp.computeA();
                return srp.A.toString(16);
            }", ori_username, g, s).Result;

            //Tiến hành yêu cầu
            request = new RestRequest("https://kkid.kkbox.com/challenge_verify", Method.POST);
            request.AddCookie("KKIDC", kkidc);
            request.AddParameter("username", username);
            request.AddParameter("a", A);
            response = client.Execute(request);
            data = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (data.error != null)
            {
                string error = data.error;
                string error_category = data.error_category;
                string error_code = data.error_code;
                string error_level = data.error_level;
                string status_code = data.status_code;

                throw new Exception(error);
            }

            #endregion

            #region Operation User Protection

            //Gán giá trị SRP
            string B = data.B;
            Page.EvaluateFunctionAsync(@"(name, username, secret, B) => {
                const srp = GetSRP(name);
                srp.I = username;
                srp.p = srp.computeHash(secret);
                srp.B = new BigInteger(B, 16);
            }", ori_username, username, secret, B).Wait();

            //Kiểm tra có thể Submit
            if (!g.Equals("G2048"))
            {
                Console.WriteLine("Submit 1");
                return;
            }

            //Kiểm tra ReChallenge
            bool re_challenge = Page.EvaluateFunctionAsync<bool>(@"(name) => {
                const srp = GetSRP(name);
                return !srp.verifyB() && srp.verifyHAB();
            }", ori_username).Result;
            if (re_challenge)
            {
                //ReChallenge
                return;
            }

            //Mã hóa mật khẩu và xóa bỏ SRP
            secret = Page.EvaluateFunctionAsync<string>(@"(name) => {
                const srp = GetSRP(name);
                srp.computeVerifier();
                let ck = srp.computeClientK();
                RemoveSRP(name);
                return ck;
            }", ori_username).Result;

            #endregion

            #region Submit

            //Lấy Recaptcha Token
            recaptcha = GetRecaptchaToken().Result;

            //Tiến hành yêu cầu
            request = new RestRequest("https://kkid.kkbox.com/login", Method.POST);
            request.AddCookie("KKIDC", kkidc);
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
            response = client.Execute(request);
            //data = JsonConvert.DeserializeObject<dynamic>(response.Content);

            #endregion

            Console.WriteLine(response.Content);
            Console.ReadKey();
        }

        private static async void OnRequest(object sender, RequestEventArgs e)
        {
            Request req = e.Request;
            string url = req.Url;
            if (url.EndsWith("base64.js"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    Body = Resources.base64,
                    ContentType = "application/javascript; charset=utf-8"
                });
            }
            else if (url.EndsWith("hashes.min.js"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    Body = Resources.hashes_min,
                    ContentType = "application/javascript; charset=utf-8"
                });
            }
            else if (url.EndsWith("jsbn.js"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    Body = Resources.jsbn,
                    ContentType = "application/javascript; charset=utf-8"
                });
            }
            else if (url.EndsWith("jsbn2.js"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    Body = Resources.jsbn2,
                    ContentType = "application/javascript; charset=utf-8"
                });
            }
            else if (url.EndsWith("scrypt.js"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    Body = Resources.scrypt,
                    ContentType = "application/javascript; charset=utf-8"
                });
            }
            else if (url.EndsWith("srp.js"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    Body = Resources.srp,
                    ContentType = "application/javascript; charset=utf-8"
                });
            }
            else if (url.EndsWith("favicon.ico"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    BodyData = new byte[0],
                    ContentType = "image/avif"
                });
            }
            else if (url.StartsWith("https://kkid.kkbox.com/login"))
            {
                await req.RespondAsync(new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    Body = Resources.login,
                    ContentType = "text/html; charset=utf-8"
                });
            }
            else
            {
                await req.ContinueAsync();
            }
        }

        private static async Task<string> GetRecaptchaToken(int times = 3)
        {
            while (times > 0)
            {
                try
                {
                    string token = await Page.EvaluateFunctionAsync<string>(@"async () => {
                        return await grecaptcha.execute('6LcuGcoUAAAAAB8E-zI7hoiQ_fcudMnk9YVZtW4m', { action: 'login' });
                    }");
                    if (token != null)
                    {
                        return token;
                    }
                }
                catch { }

                await Task.Delay(1000);
                times--;
            }
            return null;
        }
    }
}