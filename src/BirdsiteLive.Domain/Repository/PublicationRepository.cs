using System.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain.Tools;

namespace BirdsiteLive.Domain.Repository
{
    public interface IPublicationRepository
    {
        bool IsUnlisted(string twitterAcct);
        bool IsSensitive(string twitterAcct);
    }

    public class PublicationRepository : IPublicationRepository
    {
        private readonly string[] _unlistedAccounts;
        private readonly string[] _sensitiveAccounts;

        #region Ctor
        public PublicationRepository(InstanceSettings settings)
        {
            _unlistedAccounts = PatternsParser.Parse(settings.UnlistedTwitterAccounts);
            _sensitiveAccounts = PatternsParser.Parse(settings.SensitiveTwitterAccounts);
        }
        #endregion

        public bool IsUnlisted(string twitterAcct)
        {
            if (_unlistedAccounts == null || !_unlistedAccounts.Any()) return false;

            return _unlistedAccounts.Contains(twitterAcct.ToLowerInvariant());
        }

        public bool IsSensitive(string twitterAcct)
        {
            if (_sensitiveAccounts == null || !_sensitiveAccounts.Any()) return false;

            return _sensitiveAccounts.Contains(twitterAcct.ToLowerInvariant());
        }
    }
}
