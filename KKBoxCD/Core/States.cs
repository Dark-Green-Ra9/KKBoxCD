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

        public static int Other = 0;

        public static int NotFound = 0;

        public static int SRPUnsupported = 0;

        public static int ProxyBlock = 0;

        public static int LoginFailed = 0;

        public static int RecaptchaFailed = 0;

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
                    //int total = Perfect + NotFound + LoginFailed + RecaptchaFailed;
                    int total = Perfect + Other + NotFound + SRPUnsupported;
                    TimeSpan time = DateTime.Now.Subtract(StartTime);

                    if (mConfig.IsDebug)
                    {
                        //Console.WriteLine("Runtime: {0}d:{1}h:{2}m{3}s | Thread Size: {4} | Account: {5} | Total: {6} | Perfect: {7} | NotFound: {8} | LoginFailed: {9} | RecaptchaFailed: {10}",
                        //    time.Days, time.Hours, time.Minutes, time.Seconds,
                        //    ThreadSize, mManager.Count(), total,
                        //    Perfect, NotFound, LoginFailed, RecaptchaFailed);

                        //Console.WriteLine("Runtime: {0}d:{1}h:{2}m{3}s | Thread Size: {4} | Account: {5} | Total: {6} | Perfect: {7} | NotFound: {8} | SRPUnsupported: {9} | Other: {10} | ProxyBlock: {11}",
                        //    time.Days, time.Hours, time.Minutes, time.Seconds,
                        //    ThreadSize, mManager.Count(), total,
                        //    Perfect, NotFound, SRPUnsupported, Other, ProxyBlock);
                    }
                    else
                    {
                        float speed = (total - LastTotal) / 1.5f;

                        Console.Clear();
                        //Console.WriteLine("Runtime: {0}d:{1}h:{2}m{3}s\nThread Size: {4}\nAccount: {5}\nTotal: {6}\nPerfect: {7}\nNotFound: {8}\nLoginFailed: {9}\nRecaptchaFailed: {10}\nSpeed: {11}/s",
                        //    time.Days, time.Hours, time.Minutes, time.Seconds,
                        //    ThreadSize, mManager.Count(), total,
                        //    Perfect, NotFound, LoginFailed, RecaptchaFailed,
                        //    speed);

                        Console.WriteLine("Runtime: {0}d:{1}h:{2}m{3}s\nThread Size: {4}\nAccount: {5}\nTotal: {6}\nPerfect: {7}\nNotFound: {8}\nSRPUnsupported: {9}\nOther: {10}\nProxyBlock: {11}\nSpeed: {12}/s",
                            time.Days, time.Hours, time.Minutes, time.Seconds,
                            ThreadSize, mManager.Count(), total,
                            Perfect, NotFound, SRPUnsupported, Other, ProxyBlock, speed);
                    }

                    LastTotal = total;
                    Thread.Sleep(1500);
                }
            }).Start();
        }
    }
}
