using System;
using System.IO;
using Newtonsoft.Json;

namespace KKBoxCD.Core.Utils
{
    public class Config
    {
        #region Singleton

        private static readonly Lazy<Config> Singleton = new Lazy<Config>(() =>
        {
            Config config = new Config();
            try
            {
                if (!File.Exists(Consts.ConfigFile))
                {
                    File.Create(Consts.ConfigFile).Close();
                }
                string json = File.ReadAllText(Consts.ConfigFile);
                config = JsonConvert.DeserializeObject<Config>(json);
            }
            catch { }
            if (config == null)
            {
                config = new Config();
            }
            config.Save();
            return config;
        });

        public static Config Instance => Singleton.Value;

        #endregion

        public int ThreadSize { get; set; } = 1;

        public int PageTimeout { get; set; } = 15000;

        public int TimesPerBrowser { get; set; } = 10;

        public bool IsDebug { get; set; } = false;

        public bool IsSocks { get; set; } = false;

        protected Config() { }

        public void Save()
        {
            try
            {
                File.WriteAllText(Consts.ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch { }
        }
    }
}