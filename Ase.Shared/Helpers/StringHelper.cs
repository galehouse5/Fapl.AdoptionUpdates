using System;
using System.Linq;

namespace Ase.Shared.Helpers
{
    public static class StringHelper
    {
        public static string MultiLineTrim(this string value, bool removeEmptyLines = false)
            => string.Join(Environment.NewLine, value
            .Split(new[] { Environment.NewLine }, removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
            .Select(l => l.Trim()));
    }
}
