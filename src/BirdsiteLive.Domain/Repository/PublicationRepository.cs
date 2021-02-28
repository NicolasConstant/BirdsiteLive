using System.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain.Tools;

namespace BirdsiteLive.Domain.Repository
{
    public interface IPublicationRepository
    {
        bool IsUnlisted(string twitterAcct);
    }

    public class PublicationRepository : IPublicationRepository
    {
        private readonly string[] _unlistedAccounts;

        #region Ctor
        public PublicationRepository(InstanceSettings settings)
        {
            _unlistedAccounts = PatternsParser.Parse(settings.UnlistedTwitterAccounts);
        }
        #endregion

        public bool IsUnlisted(string twitterAcct)
        {
            if (_unlistedAccounts == null || !_unlistedAccounts.Any()) return false;

            return _unlistedAccounts.Contains(twitterAcct.ToLowerInvariant());
        }
    }
}