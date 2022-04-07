using KKBoxCD.Core.Manager;
using KKBoxCD.Properties;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using System;
using System.Net;

namespace KKBoxCD.Core
{
    class RecaptchaClient
    {
        #region Singleton

        private static readonly Lazy<RecaptchaClient> Singleton = new Lazy<RecaptchaClient>(() => new RecaptchaClient());

        public static RecaptchaClient Instance => Singleton.Value;

        #endregion

        public bool IsReady { get; private set; }

        public Browser Browser { get; private set; }

        public Page Page { get; private set; }

        public RecaptchaClient Init()
        {
            IsReady = false;
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

            Proxy proxy = ProxyManager.Instance.Random();
            StealthPlugin stealth = new StealthPlugin();
            PuppeteerExtra extra = new PuppeteerExtra();
            LaunchOptions options = new LaunchOptions()
            {
                Headless = false,
                ExecutablePath = Consts.ChromeFile,
                DefaultViewport = null,
                Args = new string[]
                {
                    //$"--proxy-server=\"{proxy.Address}:{proxy.Port}\"",
                    "--proxy-server=\"192.168.150.78:10000\"",
                    "--app=\"data:text/html,<title>Recaptcha Client</title>\"",
                    "--window-size=800,600",
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
                }
            };
            Browser = extra.Use(stealth).LaunchAsync(options).Result;
            Page = Browser.PagesAsync().Result[0];
            Page.SetRequestInterceptionAsync(true).Wait();
            Page.Request += OnRequest;
            try
            {
                Page.GoToAsync("https://kkid.kkbox.com/login", new NavigationOptions
            {
                WaitUntil = new WaitUntilNavigation[]
                {
                    WaitUntilNavigation.Networkidle0
                }
            }).Wait();
            }
            catch { }

            IsReady = true;
            return this;
        }

        private async void OnRequest(object sender, RequestEventArgs e)
        {
            Request req = e.Request;
            string url = req.Url;

            if (url.StartsWith("https://kkid.kkbox.com/login"))
            {
                ResponseData res = new ResponseData
                {
                    Body = Resources.login,
                    ContentType = "text/html; charset=utf-8",
                    Status = HttpStatusCode.OK
                };
                await req.RespondAsync(res);
            }
            else
            {
                await req.ContinueAsync();
            }
        }

        public string GetToken()
        {
            try
            {
                return Page.EvaluateFunctionAsync<string>(@"async () => {
                    const token = await grecaptcha.execute('6LcuGcoUAAAAAB8E-zI7hoiQ_fcudMnk9YVZtW4m', {action: 'login'});
                    return token;
                }").Result;
            }
            catch
            {
                return null;
            }
        }
    }
}
