using KKBoxCD.Core;
using KKBoxCD.Core.Utils;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
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
            RecaptchaClient.Instance.Init();
            //States.Start();

            for (int i = 0; i < config.ThreadSize; i++)
            {
                States.ThreadSize++;
                new AutoThread(i).Start();
                Thread.Sleep(250);
            }

            Console.ReadKey();
        }

        private static void Demo()
        {
            RecaptchaClient client = RecaptchaClient.Instance;
            client.Init();
            Thread.Sleep(3000);
            string token = client.GetToken();
            Console.WriteLine(token);
            token = client.GetToken();
            Console.WriteLine(token);
        }
    }
}