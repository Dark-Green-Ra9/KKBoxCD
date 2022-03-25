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

        public Account Random()
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
            string raw_data = RawData[index];
            RawData.RemoveAt(index);
            try
            {
                return new Account(raw_data);
            }
            catch
            {
                return Random();
            }
        }

        public void Push(Account account)
        {
            RawData.Add(account.Raw);
        }

        public void Write(Account account, WriteType type)
        {
            try
            {
                string line = string.Concat(account.ToString(), Environment.NewLine);

                if (!Directory.Exists(Consts.OutputDir))
                {
                    Directory.CreateDirectory(Consts.OutputDir);
                }
                if (type == WriteType.Wrong)
                {
                    File.AppendAllText(Consts.WrongFile, line);
                }
                else if (type == WriteType.Free)
                {
                    File.AppendAllText(Consts.FreeFile, line);
                }
                else if (type == WriteType.Prem)
                {
                    File.AppendAllText(Consts.PremFile, line);
                }
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
        public string Raw { get; private set; }

        public string Email { get; set; }

        public string Password { set; get; }

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
            return string.Concat(Email, ":", Password);
        }
    }
}