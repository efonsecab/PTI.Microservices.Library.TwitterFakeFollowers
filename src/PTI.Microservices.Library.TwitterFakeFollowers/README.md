# PTI.Microservices.Library.TwitterFakeFollowers

This is part of PTI.Microservices.Library set of packages

The purpose of this package is to facilitate recognizing potential Twitter Fake Followers, while maintaining a consistent usage pattern among the different services in the group

**Examples:**

## Get All Possible Fake Followers For Username
    [TestMethod]
    public async Task Test_GetAllPossibleFakeFollowersForUsernameAsync()
    {
        TwitterService twitterService = new TwitterService(null,
            this.TwitterConfiguration, new Microservices.Library.Interceptors.CustomHttpClient(new Microservices.Library.Interceptors.CustomHttpClientHandler(null)));
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TwitterPossibleFakeAccountService twitterPossibleFakeAccountService =
            new TwitterPossibleFakeAccountService(null, twitterService);
        TwitterFakeFollowersService twitterFakeFollowersService =
            new TwitterFakeFollowersService(null, twitterService, twitterPossibleFakeAccountService);
        try
        {
            await GetAllPossibleFakeFollowersByUsernameAsync(cancellationTokenSource, twitterFakeFollowersService,
                twitterService);
        }
        catch (Exception ex)
        {
            Assert.IsInstanceOfType(ex, typeof(OperationCanceledException));
        }
    }

    private async Task GetAllPossibleFakeFollowersByUsernameAsync(CancellationTokenSource cancellationTokenSource,
        TwitterFakeFollowersService twitterFakeFollowersService, TwitterService twitterService)
    {
        ulong listId = FAKE_FOLLOWERS_LIST;
        List<string> fakeAccountsUsernames = new List<string>();
        List<PossibleFakeUser> fakeUsersInfo = new List<PossibleFakeUser>();
        await twitterFakeFollowersService.GetAllPossibleFakeFollowersForUsernameAsync(this.TwitterConfiguration.ScreenName,
            async (PossibleFakeUser possibleFakeUser) =>
            {
                try
                {
                    fakeAccountsUsernames.Add(possibleFakeUser.User.ScreenNameResponse);
                    await twitterService.AddUsersToListAsync(fakeAccountsUsernames, listId);
                    fakeUsersInfo.Add(possibleFakeUser);
                    var jsonData = System.Text.Json.JsonSerializer.Serialize(fakeUsersInfo);
                    await System.IO.File.WriteAllTextAsync(@"E:\Temp\PossibleFakeUsers.json", jsonData);
                    if (fakeAccountsUsernames.Count >= 20)
                    {
                        fakeAccountsUsernames.Clear();
                    }
                    //cancellationTokenSource.Cancel();
                }
                catch (Exception ex)
                {

                }
            }, cancellationToken: cancellationTokenSource.Token);
        if (fakeAccountsUsernames.Count >= 0)
        {
            await twitterService.AddUsersToListAsync(fakeAccountsUsernames, 1362737001099907072);
            fakeAccountsUsernames.Clear();
        }
    }