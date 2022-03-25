using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace KKBoxCD.Core.Manager
{
    public class ProxyManager
    {
        #region Singleton

        private static readonly Lazy<ProxyManager> Singleton = new Lazy<ProxyManager>(() => new ProxyManager());

        public static ProxyManager Instance => Singleton.Value;

        #endregion

        private List<string> RawData;

        protected ProxyManager()
        {
            if (!File.Exists(Consts.ProxyFile))
            {
                File.Create(Consts.ProxyFile).Close();
            }

            Load();
        }

        public void Load()
        {
            string[] proxy = File.ReadAllLines(Consts.ProxyFile);
            RawData = new List<string>(proxy);
        }

        public Proxy Random()
        {
            if (RawData == null)
            {
                Load();
            }
            if (IsEmpty())
            {
                return null;
            }
            int index = new Random().Next(0, Count());
            try
            {
                return new Proxy(RawData[index]);
            }
            catch
            {
                RawData.RemoveAt(index);
                return Random();
            }
        }

        public int Count()
        {
            if (RawData == null)
            {
                Load();
            }
            return RawData.Count;
        }

        public bool IsEmpty()
        {
            return !RawData.Any();
        }
    }

    public class Proxy
    {
        public string Raw { get; private set; }

        public string Address { get; private set; }

        public int Port { get; private set; }

        public Proxy(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                throw new Exception("Raw data is invalid");
            }
            string[] data = raw.Trim().Split(':');

            Raw = raw;
            Address = data[0].Trim();
            Port = int.Parse(data[1].Trim());
        }
    }
}