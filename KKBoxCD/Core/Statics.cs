using KKBoxCD.Core.Manager;
using System;

namespace KKBoxCD.Core
{
    class Statics
    {
        public static int Perfect = 0;

        public static int Wrong = 0;

        public static int NotExist = 0;

        public static void Show()
        {
            AccountManager manager = AccountManager.Instance;
            int total = Perfect + Wrong + NotExist;


            Console.Clear();
            Console.WriteLine("Account: {0}", manager.Count());
            Console.WriteLine("Total: {0}", total);
            Console.WriteLine("Perfect: {0}", Perfect);
            Console.WriteLine("Wrong: {0}", Wrong);
            Console.WriteLine("NotExist: {0}", NotExist);
        }
    }
}
