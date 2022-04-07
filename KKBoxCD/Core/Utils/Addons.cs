using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace KKBoxCD.Core.Utils
{
    public class Addons
    {
        public static readonly char[] HexChar = new char[]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static void CloseChrome()
        {
            string chrome_file = Path.GetFullPath(Consts.ChromeFile);
            string query_string = "SELECT ProcessId, ExecutablePath FROM Win32_Process";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query_string))
            using (ManagementObjectCollection results = searcher.Get())
            {
                IEnumerable<ManagementObject> querys = results.Cast<ManagementObject>();
                foreach (ManagementObject query in querys)
                {
                    try
                    {
                        int id = (int)(uint)query["ProcessId"];
                        string path = (string)query["ExecutablePath"];

                        if (chrome_file.Equals(path, StringComparison.OrdinalIgnoreCase))
                        {
                            using (Process prcoces = Process.GetProcessById(id))
                            {
                                if (!prcoces.CloseMainWindow())
                                {
                                    prcoces.Kill();
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        public static string RandomUserAgent()
        {
            Random rand = new Random();
            string chrome_ver = $"{rand.Next(89, 100)}.0.{rand.Next(4389, 4778)}.{rand.Next(93, 212)}";
            return $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{chrome_ver} Safari/537.36";
        }

        public static string RandomHash(int length)
        {
            char[] results = new char[length];
            Random ran = new Random();
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = HexChar[ran.Next(0, HexChar.Length)];
            }

            return string.Join("", results);
        }
    }
}