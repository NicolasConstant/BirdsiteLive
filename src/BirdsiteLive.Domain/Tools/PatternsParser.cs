using System;
using System.Linq;

namespace BirdsiteLive.Domain.Tools
{
    public class PatternsParser
    {
        public static string[] Parse(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry)) return new string[0];

            var separationChar = '|';
            if (entry.Contains(";")) separationChar = ';';
            else if (entry.Contains(",")) separationChar = ',';

            var splitEntries = entry
                .Split(new[] {separationChar}, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.ToLowerInvariant().Trim());
            return splitEntries.ToArray();
        }
    }
}