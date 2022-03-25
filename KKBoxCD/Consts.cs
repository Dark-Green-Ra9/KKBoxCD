using System.IO;

namespace KKBoxCD
{
    internal class Consts
    {
        public static readonly string BaseDir = Directory.GetCurrentDirectory();
        public static readonly string BinDir = Path.Combine(BaseDir, "bin");
        public static readonly string DataDir = Path.Combine(BaseDir, "data");
        public static readonly string OutputDir = Path.Combine(BaseDir, "output");

        public static readonly string ChromeFile = Path.Combine(BinDir, "chrome\\chrome.exe");
        public static readonly string ConfigFile = Path.Combine(BaseDir, "config.json");
        public static readonly string AccountFile = Path.Combine(DataDir, "account.txt");
        public static readonly string RanFile = Path.Combine(DataDir, "ran.txt");
        public static readonly string ProxyFile = Path.Combine(DataDir, "proxy.txt");
        public static readonly string WrongFile = Path.Combine(OutputDir, "wrong.txt");
        public static readonly string FreeFile = Path.Combine(OutputDir, "free.txt");
        public static readonly string PremFile = Path.Combine(OutputDir, "prem.txt");

    }
}
