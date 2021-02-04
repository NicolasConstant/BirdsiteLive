using System;
using System.Linq;

namespace BirdsiteLive.Domain.Tools
{
    public class ModerationParser
    {
        public static string[] Parse(string entry)
        {
            var separationChar = '|';
            if (entry.Contains(";")) separationChar = ';';
            else if (entry.Contains(",")) separationChar = ',';

            var splitEntries = entry
                .Split(new[] {separationChar}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLowerInvariant());
            return splitEntries.ToArray();
        }
    }
}