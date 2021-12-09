using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace BirdsiteLive.Tools
{
    public class HeaderHandler
    {
        public static Dictionary<string, string> RequestHeaders(IHeaderDictionary header)
        {
            return header.ToDictionary<KeyValuePair<string, StringValues>, string, string>(h => h.Key.ToLowerInvariant(), h => h.Value);
        }
    }
}