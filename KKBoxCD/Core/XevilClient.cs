using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKBoxCD.Core
{
    public class XevilClient
    {
        #region Singleton

        private static readonly Lazy<XevilClient> Singleton = new Lazy<XevilClient>(() => new XevilClient());

        public static XevilClient Instance => Singleton.Value;

        #endregion

        private readonly int PoolSize = 100;

        public void Start()
        {

        }

        public string GetToken()
        {
            return "";
        }
    }
}
