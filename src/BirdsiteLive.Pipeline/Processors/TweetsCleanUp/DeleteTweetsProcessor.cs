using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Domain;
using BirdsiteLive.Pipeline.Contracts.TweetsCleanUp;
using BirdsiteLive.Pipeline.Models;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Pipeline.Processors.TweetsCleanUp
{
    public class DeleteTweetsProcessor : IDeleteTweetsProcessor
    {
        private readonly IActivityPubService _activityPubService;
        private readonly ILogger<DeleteTweetsProcessor> _logger;

        #region Ctor
        public DeleteTweetsProcessor(IActivityPubService activityPubService, ILogger<DeleteTweetsProcessor> logger)
        {
            _activityPubService = activityPubService;
            _logger = logger;
        }
        #endregion

        public async Task<TweetToDelete> ProcessAsync(TweetToDelete tweet, CancellationToken ct)
        {
            try
            {
                await _activityPubService.DeleteNoteAsync(tweet.Tweet);
                tweet.DeleteSuccessful = true;
            }
            catch (HttpRequestException e)
            {
                //TODO check code under .NET 5
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
            }

            return tweet;
        }
    }
}