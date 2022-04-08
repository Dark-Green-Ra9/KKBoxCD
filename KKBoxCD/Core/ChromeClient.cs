using KKBoxCD.Core.Utils;
using KKBoxCD.Properties;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using System;

namespace KKBoxCD.Core
{
    class ChromeClient: IDisposable
    {
        #region Dispose

        private bool _Disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (Page != null)
                {
                    try { Page.CloseAsync().Wait(); } catch { }
                    Page = null;
                }
                if (Browser != null)
                {
                    try { Browser.CloseAsync().Wait(); } catch { }
                    Browser = null;
                }
            }
            _Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ChromeClient()
        {
            Dispose(false);
        }

        #endregion

        #region Singleton

        private static readonly Lazy<ChromeClient> Singleton = new Lazy<ChromeClient>(() => new ChromeClient());

        public static ChromeClient Instance => Singleton.Value;

        #endregion

        private Config mConfig;

        public bool IsReady { get; private set; }

        public Browser Browser { get; private set; }

        public Page Page { get; private set; }

        protected ChromeClient()
        {
            mConfig = Config.Instance;
        }

        public ChromeClient Init()
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

            PuppeteerExtra extra = new PuppeteerExtra();
            LaunchOptions options = new LaunchOptions()
            {
                Headless = true,
                ExecutablePath = Consts.ChromeFile,
                Args = new string[]
                {
                    "--app=\"data:text/html,<title>Chrome Client</title>\"",
                    "--window-size=270,150",
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
                DefaultViewport = null
            };
            Browser = extra.LaunchAsync(options).Result;
            Page = Browser.PagesAsync().Result[0];

            Page.AddScriptTagAsync(new AddTagOptions
            {
                Content = Resources.base64
            }).Wait();
            Page.AddScriptTagAsync(new AddTagOptions
            {
                Content = Resources.hashes_min
            }).Wait();
            Page.AddScriptTagAsync(new AddTagOptions
            {
                Content = Resources.jsbn
            }).Wait();
            Page.AddScriptTagAsync(new AddTagOptions
            {
                Content = Resources.jsbn2
            }).Wait();
            Page.AddScriptTagAsync(new AddTagOptions
            {
                Content = Resources.scrypt
            }).Wait();
            Page.AddScriptTagAsync(new AddTagOptions
            {
                Content = Resources.srp
            }).Wait();
            Page.AddScriptTagAsync(new AddTagOptions
            {
                Content = Resources.chrome_client
            }).Wait();

            IsReady = true;
            return this;
        }
    }
}