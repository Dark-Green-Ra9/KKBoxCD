using System;
using System.Collections.Generic;
using System.IO;

namespace KKBoxCD.Core.Manager
{
    public class SourceManager
    {
        private static Dictionary<string, byte[]> SourceData = new Dictionary<string, byte[]>();

        public static byte[] Get(string name)
        {
            if (SourceData.ContainsKey(name))
            {
                return SourceData[name];
            }
            else
            {
                string file = Path.Combine(Consts.SourceDir, name);
                if (File.Exists(file))
                {
                    try
                    {
                        SourceData[name] = File.ReadAllBytes(file);
                        return SourceData[name];
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
