using KKBoxCD.Core;
using KKBoxCD.Core.Utils;
using RestSharp;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace KKBoxCD
{
    public class Program
    {
        public static void Main()
        {
            ChromeClient.Instance.Init();

            RestClient client = new RestClient(new RestClientOptions
            {
                Proxy = new WebProxy("154.92.112.65", 5086)
            });
            RestRequest request = new RestRequest("https://kkid.kkbox.com/login");
            RestResponse response = client.ExecuteAsync(request).Result;

            Regex regex = new Regex("initialCountry: \"(.*?)\",");
            MatchCollection matched = regex.Matches(response.Content);
            string country = matched[0].Groups[1].Value;
            string dial = ChromeClient.Instance.Page.EvaluateFunctionAsync<string>("(country) => GetDialCode(country)", country).Result;
            dial = ChromeClient.Instance.Page.EvaluateFunctionAsync<string>("(country) => GetDialCode(country)", country).Result;
            Console.WriteLine(dial);
            Console.ReadKey();
            return;

            Console.OutputEncoding = Encoding.UTF8;
            Addons.CloseChrome();
            Config config = Config.Instance;

            Console.Write("Run Mode [0.Check Exist | 1.Get Plan]: ");
            char key = Console.ReadKey().KeyChar;
            Console.WriteLine("");
            if (key.Equals('0'))
            {
                config.Mode = Config.RunMode.CheckExist;
            }
            else if (key.Equals('1'))
            {
                ChromeClient.Instance.Init();
                //RecaptchaClient.Instance.Init();
                config.Mode = Config.RunMode.GetPlan;
            }
            else
            {
                Main();
                return;
            }

            States.Start();
            for (int i = 0; i < config.ThreadSize; i++)
            {
                States.ThreadSize++;
                new AutoThread(i).Start();
                Thread.Sleep(100);
            }

            Console.ReadKey();
        }
    }
}