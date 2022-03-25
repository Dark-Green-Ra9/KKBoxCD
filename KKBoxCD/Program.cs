using KKBoxCD.Core;
using KKBoxCD.Core.Utils;
using System;
using System.Text;
using System.Threading;

namespace KKBoxCD
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Config config = Config.Instance;
            for (int i = 0; i < config.ThreadSize; i++)
            {
                new AutoThread(i).Start();
                Thread.Sleep(2000);
            }
            _ = Console.ReadKey();
        }
    }
}
