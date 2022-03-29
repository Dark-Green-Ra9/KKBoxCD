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
    }
}