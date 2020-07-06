using System;
using System.Linq;

namespace BirdsiteLive.DAL.Tools
{
    public static class RandomGenerator
    {
        private static readonly Random Random = new Random();

        public static string GetString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

    }
}