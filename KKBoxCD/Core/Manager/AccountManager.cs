using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KKBoxCD.Core.Utils;

namespace KKBoxCD.Core.Manager
{
    public class AccountManager
    {
        #region Singleton

        private static readonly Lazy<AccountManager> Singleton = new Lazy<AccountManager>(() => new AccountManager());

        public static AccountManager Instance => Singleton.Value;

        #endregion

        private List<string> RawData;

        protected AccountManager()
        {
            if (!Directory.Exists(Consts.DataDir))
            {
                Directory.CreateDirectory(Consts.DataDir);
            }
            if (!File.Exists(Consts.AccountFile))
            {
                File.Create(Consts.AccountFile).Close();
            }
            if (!File.Exists(Consts.RanFile))
            {
                File.Create(Consts.RanFile).Close();
            }

            Load();
        }

        public void Load()
        {
            string[] raw = File.ReadAllLines(Consts.AccountFile);
            string[] ran = File.ReadAllLines(Consts.RanFile);

            EqualityComparer comparer = new EqualityComparer();
            RawData = new List<string>(raw.Except(ran, comparer));
        }

        public Account Get()
        {
            if (RawData == null)
            {
                Load();
            }
            if (IsEmpty())
            {
                return null;
            }

            string raw_data = RawData[0];
            RawData.RemoveAt(0);
            try
            {
                return new Account(raw_data);
            }
            catch
            {
                return Get();
            }
        }

        public void Push(Account account)
        {
            RawData.Add(account.Raw);
        }

        public void Write(Account account)
        {
            try
            {
                if (!Directory.Exists(Consts.OutputDir))
                {
                    Directory.CreateDirectory(Consts.OutputDir);
                }
                string line = $"{account}{Environment.NewLine}";
                string file = Path.Combine(Consts.OutputDir, $"{account.Status}.txt");
                File.AppendAllText(Consts.RanFile, $"{account.Raw}{Environment.NewLine}");
                File.AppendAllText(file, line);
            }
            catch { }
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
            if (RawData == null)
            {
                Load();
            }
            return !RawData.Any();
        }
    }

    public class Account
    {
        public string Raw { get; private set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { set; get; } = string.Empty;

        public AccountStatus Status { set; get; } = AccountStatus.Empty;

        public string Data { set; get; } = string.Empty;

        public Account(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                throw new Exception("Raw data is invalid");
            }

            string[] data = raw.Trim().Split(':');
            if (data.Length < 2)
            {
                throw new Exception("Raw data is invalid");
            }

            Raw = raw;
            Email = data[0].Trim();
            Password = data[1].Trim();
        }

        public override string ToString()
        {
            return $"{Email}:{Password}:{Status}:{Data}";
        }
    }

    public enum AccountStatus
    {
        Empty,
        Other,
        Perfect,
        NotFound,
        SRPUnsupported,
        LoginFailed,
    }
}