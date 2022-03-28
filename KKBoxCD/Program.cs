﻿using KKBoxCD.Core;
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
            Statics.Start();

            Config config = Config.Instance;
            for (int i = 0; i < config.ThreadSize; i++)
            {
                Statics.ThreadSize++;
                new AutoThread(i).Start();
                Thread.Sleep(250);
            }
        }
    }
}