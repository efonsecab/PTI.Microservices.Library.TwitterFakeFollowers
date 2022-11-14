using System;
using System.Collections.Generic;
using System.Text;

namespace PTI.Microservices.Library.Models.TwitterFakeFollowersService.Enums
{
    /// <summary>
    /// Reasons why an account was marked as a possible fake
    /// </summary>
    public enum PossibleFakeReason
    {
        /// <summary>
        /// User has no bio description set
        /// </summary>
        EmptyBioDescription = 0,
        /// <summary>
        /// User has not had original tweets for a long time
        /// </summary>
        LongTimeWithoutOriginalTweets = 1,
        /// <summary>
        /// User does not have a profile image set
        /// </summary>
        EmptyProfileImage = 2,
        /// <summary>
        /// Following-Followers Offset is invalid
        /// </summary>
        InvalidFollowersOffset = 3,
    }
}
