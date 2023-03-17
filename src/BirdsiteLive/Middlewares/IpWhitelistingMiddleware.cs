using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BirdsiteLive.Middlewares
{
    public class IpWhitelistingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpWhitelistingMiddleware> _logger;
        private readonly InstanceSettings _instanceSettings;
        private readonly byte[][] _safelist;
        private readonly bool _ipWhitelistingSet;

        public IpWhitelistingMiddleware(
            RequestDelegate next,
            ILogger<IpWhitelistingMiddleware> logger,
            InstanceSettings instanceSettings)
        {
            if (!string.IsNullOrWhiteSpace(instanceSettings.IpWhiteListing))
            {
                var ips = PatternsParser.Parse(instanceSettings.IpWhiteListing);
                _safelist = new byte[ips.Length][];
                for (var i = 0; i < ips.Length; i++)
                {
                    _safelist[i] = IPAddress.Parse(ips[i]).GetAddressBytes();
                }
                _ipWhitelistingSet = true;
            }

            _next = next;
            _logger = logger;
            _instanceSettings = instanceSettings;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_ipWhitelistingSet)
            {
                var remoteIp = context.Connection.RemoteIpAddress;

                if (_instanceSettings.EnableXRealIpHeader)
                {
                    var forwardedIp = context.Request.Headers.FirstOrDefault(x => x.Key == "X-Real-IP").Value
                        .ToString();
                    if (!string.IsNullOrWhiteSpace(forwardedIp))
                    {
                        _logger.LogDebug("Redirected IP address detected");
                        remoteIp = IPAddress.Parse(forwardedIp);
                    }
                }

                _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);

                var bytes = remoteIp.GetAddressBytes();
                var badIp = true;
                foreach (var address in _safelist)
                {
                    if (address.SequenceEqual(bytes))
                    {
                        badIp = false;
                        break;
                    }
                }

                if (badIp)
                {
                    _logger.LogWarning("Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}