using System;
using System.Text.RegularExpressions;
using BirdsiteLive.Domain.Repository;
using Org.BouncyCastle.Pkcs;

namespace BirdsiteLive.Domain.Tools
{
    public class ModerationRegexParser
    {
        public static Regex Parse(ModerationEntityTypeEnum type, string data)
        {
            data = data.ToLowerInvariant().Trim();

            if (type == ModerationEntityTypeEnum.Follower)
            {
                if (data.StartsWith("@"))
                    return new Regex($@"^{data}$");

                if (data.StartsWith("*"))
                    data = data.Replace("*", "(.+)");

                return new Regex($@"^@(.+)@{data}$");
            }

            return new Regex($@"^{data}$");
        }
    }
}