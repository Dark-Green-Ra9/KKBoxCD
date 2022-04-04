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
            Config config = Config.Instance;

            Addons.CloseChrome();
            ChromeClient.Instance.Init();
            //RecaptchaClient.Instance.Init();
            States.Start();

            for (int i = 0; i < config.ThreadSize; i++)
            {
                States.ThreadSize++;
                new AutoThread(i).Start();
                Thread.Sleep(250);
            }

            Console.ReadKey();
        }
    }
}