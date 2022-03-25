using System;
using System.Collections.Generic;

namespace KKBoxCD.Core.Utils
{
    internal class EqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string a, string b)
        {
            return a.Trim().Equals(b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.Trim().ToLower().GetHashCode();
        }
    }
}
