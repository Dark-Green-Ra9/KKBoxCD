using KKBoxCD.Core;
using KKBoxCD.Core.Utils;
using System;
using System.Text;
using System.Threading;

namespace KKBoxCD
{
    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Addons.CloseChrome();
            Config config = Config.Instance;
            ChromeClient chrome = ChromeClient.Instance;
            XevilClient xevil = XevilClient.Instance;

            chrome.Init();
            xevil.PoolSize = config.RecaptchaPoolSize;
            xevil.ThreadSize = config.RecaptchaThreadSize;
            xevil.Start();

            Console.WriteLine(">> Đợi bể reCaptcha sẵn sàng");
            while (xevil.Size() < config.RecaptchaWaitSize)
            {
                Console.WriteLine("  +  Dung tích: {0}/{1}", xevil.Size(), xevil.PoolSize);
                Thread.Sleep(3000);
            }

            States.Start();
            for (int i = 0; i < config.ThreadSize; i++)
            {
                States.ThreadSize++;
                new AutoThread(i).Start();
                Thread.Sleep(100);
            }
        }
    }
}