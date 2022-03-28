using KKBoxCD.Core.Manager;
using System;
using System.Threading;

namespace KKBoxCD.Core
{
    class Statics
    {
        public static int ThreadSize = 0;

        public static int Perfect = 0;

        public static int Wrong = 0;

        public static int NotExist = 0;

        public static void Start()
        {
            new Thread(() => {
                while(true)
                {
                    AccountManager manager = AccountManager.Instance;
                    int total = Perfect + Wrong + NotExist;

                    //Console.Clear();
                    Console.WriteLine("Thread Size: {0} | Account: {1} | Total: {2} | Perfect: {3} | Wrong: {4} | NotExist: {5}",
                        ThreadSize,
                        manager.Count(),
                        total,
                        Perfect,
                        Wrong,
                        NotExist);
                    Thread.Sleep(1000);
                }
            }).Start();
        }
    }
}
