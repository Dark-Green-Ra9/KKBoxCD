using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using KKBoxCD.Core.Utils;

namespace KKBoxCD.Core.Manager
{
    public class ProxyManager
    {
        #region Singleton

        private static readonly Lazy<ProxyManager> Singleton = new Lazy<ProxyManager>(() => new ProxyManager());

        public static ProxyManager Instance => Singleton.Value;

        #endregion

        private List<string> RawData;

        private Config mConfig;

        protected ProxyManager()
        {
            if (!File.Exists(Consts.ProxyFile))
            {
                File.Create(Consts.ProxyFile).Close();
            }
            mConfig = Config.Instance;
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
            string raw = RawData[index];
            if (!mConfig.DuplProxy)
            {
                RawData.RemoveAt(index);
            }
            try
            {
                return new Proxy(raw);
            }
            catch
            {
                return Random();
            }
        }

        public void Push(Proxy proxy)
        {
            RawData.Add(proxy.Raw);
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

        public string Username { get; private set; }

        public string Password { get; private set; }

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

            if (data.Length > 3)
            {
                Username = data[2].Trim();
                Username = data[3].Trim();
            }
        }
    }
}