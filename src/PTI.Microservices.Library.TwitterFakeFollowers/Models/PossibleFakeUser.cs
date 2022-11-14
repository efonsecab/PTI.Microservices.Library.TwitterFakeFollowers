using LinqToTwitter;
using PTI.Microservices.Library.Models.TwitterFakeFollowersService.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PTI.Microservices.Library.Models.TwitterFakeFollowersService
{
    /// <summary>
    /// Combines the User information with the found possible fake reasons
    /// </summary>
    public class PossibleFakeUser
    {
        /// <summary>
        /// User information
        /// </summary>
        public User User { get; set; }
        /// <summary>
        /// Reasons why the user was identified as a possible fake
        /// </summary>
        public List<PossibleFakeReason> PossibleFakeReasons { get; set; }
    }
}
