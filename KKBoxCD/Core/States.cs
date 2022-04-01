using KKBoxCD.Core.Manager;
using KKBoxCD.Core.Utils;
using System;
using System.Threading;

namespace KKBoxCD.Core
{
    class States
    {
        public static int ThreadSize = 0;

        public static int Perfect = 0;

        public static int Wrong = 0;

        public static int NotExist = 0;

        public static int LoginFail = 0;

        private static readonly Config mConfig = Config.Instance;

        private static readonly AccountManager mManager = AccountManager.Instance;

        private static DateTime StartTime = DateTime.MinValue;

        private static int LastTotal = 0;

        public static void Start()
        {
            StartTime = DateTime.Now;
            new Thread(() =>
            {
                while (true)
                {
                    int total = Perfect + Wrong + NotExist + LoginFail;
                    TimeSpan time = DateTime.Now.Subtract(StartTime);

                    if (mConfig.IsDebug)
                    {
                        Console.WriteLine("Runtime: {0}d:{1}h:{2}m{3}s | Thread Size: {4} | Account: {5} | Total: {6} | Perfect: {7} | Wrong: {8} | NotExist: {9} | LoginFail: {10}",
                            time.Days, time.Hours, time.Minutes, time.Seconds,
                            ThreadSize, mManager.Count(), total,
                            Perfect, Wrong, NotExist, LoginFail);
                    }
                    else
                    {
                        float speed = (total - LastTotal) / 1.5f;

                        Console.Clear();
                        Console.WriteLine("Runtime: {0}d:{1}h:{2}m{3}s\nThread Size: {4}\nAccount: {5}\nTotal: {6}\nPerfect: {7}\nWrong: {8}\nNotExist: {9}\nLoginFail: {10}\nSpeed: {11}/s",
                            time.Days, time.Hours, time.Minutes, time.Seconds,
                            ThreadSize, mManager.Count(), total,
                            Perfect, Wrong, NotExist, LoginFail,
                            speed);
                    }

                    LastTotal = total;
                    Thread.Sleep(1500);
                }
            }).Start();
        }
    }
}
