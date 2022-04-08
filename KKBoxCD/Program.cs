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
                XevilClient.Instance.Start();
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