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
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.ToLowerInvariant().Trim());
            return splitEntries.ToArray();
        }
    }
}