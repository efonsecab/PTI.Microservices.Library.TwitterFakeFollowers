using LinqToTwitter;
using Microsoft.Extensions.Logging;
using PTI.Microservices.Library.Models.TwitterFakeFollowersService;
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
    /// Service in charge of detecting possibel fake followers
    /// </summary>
    public sealed class TwitterFakeFollowersService
    {
        private TwitterService TwitterService { get; }
        private TwitterPossibleFakeAccountService TwitterPossibleFakeAccountService { get; }
        private ILogger<TwitterFakeFollowersService> Logger { get; }
        /// <summary>
        /// Default service constructor
        /// </summary>
        /// <param name="baseTwitterService"></param>
        /// <param name="logger"></param>
        /// <param name="twitterPossibleFakeAccountService"></param>
        public TwitterFakeFollowersService(ILogger<TwitterFakeFollowersService> logger, TwitterService baseTwitterService,
            TwitterPossibleFakeAccountService twitterPossibleFakeAccountService)
        {
            this.Logger = logger;
            this.TwitterService = baseTwitterService;
            this.TwitterPossibleFakeAccountService = twitterPossibleFakeAccountService;
        }

        /// <summary>
        /// Retrieve a list of all possible fake followers for a given username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="onNewPossibleFakeFollowedDetectedAction">Action to be executed when a new possible fake has been found.
        /// Used so that consumer does not have to wait for the whole process to finish</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<PossibleFakeUser>> GetAllPossibleFakeFollowersForUsernameAsync(string username,
            Action<PossibleFakeUser> onNewPossibleFakeFollowedDetectedAction = null, CancellationToken cancellationToken=default)
        {
            List<PossibleFakeUser> lstPossibleFakeFollowers = new List<PossibleFakeUser>();
            var userFollowers = await this.TwitterService.GetUserFollowersByUsernameAsync(username, cancellationToken: cancellationToken);
            long? currentFollowersCursor = null;
            Action<User, List<PossibleFakeReason>> onPossibleFakeFollowerAction = (user, reasons) =>
            {
                PossibleFakeUser possibleFakeUser = new PossibleFakeUser()
                {
                    User = user,
                    PossibleFakeReasons = reasons
                };
                lstPossibleFakeFollowers.Add(possibleFakeUser);
                if (onNewPossibleFakeFollowedDetectedAction != null)
                    onNewPossibleFakeFollowedDetectedAction(possibleFakeUser);
            };
            if (userFollowers != null && userFollowers.Users != null && userFollowers.Users.Count > 0)
            {
                currentFollowersCursor = userFollowers.CursorMovement?.Next;
                foreach (var singleFollower in userFollowers.Users)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await EvaluateIfPossibleFakeFollower(singleFollower, onPossibleFakeFollowerAction, cancellationToken);
                }
                if (currentFollowersCursor != null)
                {
                    do
                    {
                        userFollowers = await this.TwitterService.GetUserFollowersByUsernameAsync(username, cursor: currentFollowersCursor.Value, 
                            cancellationToken: cancellationToken);
                        currentFollowersCursor = userFollowers?.CursorMovement?.Next;
                        foreach (var singleFollower in userFollowers.Users)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await EvaluateIfPossibleFakeFollower(singleFollower, onPossibleFakeFollowerAction, cancellationToken);
                        }
                    }
                    while (userFollowers != null && userFollowers.Users != null && userFollowers.Users.Count > 0);
                }
            }
            return lstPossibleFakeFollowers;
        }


        private async Task EvaluateIfPossibleFakeFollower(LinqToTwitter.User singleFollower,
            Action<User, List<PossibleFakeReason>> onPossibleFakeFollowerAction, CancellationToken cancellationToken=default)
        {
            var lstPossibleFakeReasons = await this.TwitterPossibleFakeAccountService.GetPossibleFakeReasonsAsync(singleFollower, cancellationToken);
            if (lstPossibleFakeReasons != null && lstPossibleFakeReasons.Count() > 0)
            {
                if (onPossibleFakeFollowerAction != null)
                {
                    this.Logger?.LogInformation($"{singleFollower.ScreenNameResponse} detected as a possible fake");
                    onPossibleFakeFollowerAction(singleFollower, lstPossibleFakeReasons);
                }
            }
        }

    }
}
