using System;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Twitter.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Domain.Tests
{
    [TestClass]
    public class StatusServiceTests
    {
        private readonly InstanceSettings _settings;

        #region Ctor
        public StatusServiceTests()
        {
            _settings = new InstanceSettings
            {
                Domain = "domain.name"
            };
        }
        #endregion

//        [TestMethod]
//        public void ExtractMentionsTest()
//        {
//            #region Stubs
//            var username = "MyUserName";
//            var extractedTweet = new ExtractedTweet
//            {
//                Id = 124L,
//                CreatedAt = DateTime.UtcNow,
//                MessageContent = @"Getting ready for the weekend...have a great one everyone!
//⁠
//Photo by Tim Tronckoe | @timtronckoe 
//⁠
//#archenemy #michaelamott #alissawhitegluz #jeffloomis #danielerlandsson #sharleedangelo⁠"
//            };
//            #endregion

//            var service = new StatusService(_settings);
//            var result = service.GetStatus(username, extractedTweet);

//            #region Validations

//            #endregion
//        }
    }
}
