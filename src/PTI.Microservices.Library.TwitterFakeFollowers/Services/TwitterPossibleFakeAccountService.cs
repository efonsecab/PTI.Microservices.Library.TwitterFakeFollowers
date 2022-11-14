using LinqToTwitter;
using Microsoft.Extensions.Logging;
using PTI.Microservices.Library.Models.TwitterFakeFollowersService.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTI.Microservices.Library.Services.Specialized
{
    /// <summary>
    /// Servie in charge of detecting possible fake accounts in Twitter
    /// </summary>
    public sealed class TwitterPossibleFakeAccountService
    {
        private ILogger<TwitterPossibleFakeAccountService> Logger { get; }
        private TwitterService TwitterService { get; }
        
        /// <summary>
        /// Creates a new instance of <see cref="TwitterPossibleFakeAccountService"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="twitterService"></param>
        public TwitterPossibleFakeAccountService(ILogger<TwitterPossibleFakeAccountService> logger,
            TwitterService twitterService)
        {
            this.Logger = logger;
            this.TwitterService = twitterService;
        }

        /// <summary>
        /// Gets the reasons why a user may be fake
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<PossibleFakeReason>> GetPossibleFakeReasonsAsync(User user, CancellationToken cancellationToken = default)
        {
            this.Logger?.LogInformation($"Looking for possible fake reasons for: {user.ScreenNameResponse}");
            List<PossibleFakeReason> lstPossibleFakeReasons = new List<PossibleFakeReason>();
            if (user.Protected) //we will ignore protected accounts
                return lstPossibleFakeReasons;
            try
            {
                var lastOriginalTweet =
                    (await this.TwitterService.GetTweetsByUsernameAsync(user.ScreenNameResponse, 10, includeRetweets: false,
                    cancellationToken: cancellationToken))
                    .FirstOrDefault();
                bool hasEmptyDescription = String.IsNullOrWhiteSpace(user.Description);
                if (hasEmptyDescription)
                    lstPossibleFakeReasons.Add(PossibleFakeReason.EmptyBioDescription);
                if (lastOriginalTweet != null)
                {
                    var timeSinceOriginalRetweet = DateTimeOffset.UtcNow.Subtract(lastOriginalTweet.CreatedAt);
                    var timeSinceUserCreation = DateTimeOffset.UtcNow.Subtract(user.CreatedAt);
                    if (timeSinceOriginalRetweet.TotalDays > 30 &&
                        //about 8 months
                        timeSinceUserCreation.TotalDays > 240
                        )
                    {
                        lstPossibleFakeReasons.Add(PossibleFakeReason.LongTimeWithoutOriginalTweets);
                    }
                }
                bool hasEmptyProfileImage = String.IsNullOrWhiteSpace(user.ProfileImageUrl) ||
                    String.IsNullOrWhiteSpace(user.ProfileImageUrlHttps);
                if (hasEmptyProfileImage)
                    lstPossibleFakeReasons.Add(PossibleFakeReason.EmptyProfileImage);
                if (user.FollowersCount > user.FriendsCount)
                {

                }
                else
                {
                    double expectedFollowersOffset = user.FriendsCount * 0.4;
                    if (user.FollowersCount < expectedFollowersOffset)
                    {
                        lstPossibleFakeReasons.Add(PossibleFakeReason.InvalidFollowersOffset);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
            }
            return lstPossibleFakeReasons;
        }

        /// <summary>
        /// Gets the reasons why the specified user may be fake
        /// </summary>
        /// <param name="username"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<PossibleFakeReason>> GetPossibleFakeReasonsForUsernameAsync(string username, CancellationToken cancellationToken=default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var user = await this.TwitterService.GetUserInfoByUsernameAsync(username, cancellationToken);
                var result = await this.GetPossibleFakeReasonsAsync(user);
                return result;
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
