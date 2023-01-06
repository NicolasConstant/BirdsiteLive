using System;
using System.Net;
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
                var code = e.StatusCode;
                if (code is HttpStatusCode.Gone or HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Tweet already deleted");
                    tweet.DeleteSuccessful = true;
                }
                else
                {
                    _logger.LogError(e.Message, e);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
            }

            return tweet;
        }
    }
}