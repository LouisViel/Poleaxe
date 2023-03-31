using System;
using System.Linq;

namespace Poleaxe.Helper
{
    public static class RandomHelper
    {
        public static string GuidWithoutNumber()
        {
            string guid = Guid.NewGuid().ToString("N");
            return string.Concat(guid.Select(c => (char)(c + 17)));
        }
    }
}