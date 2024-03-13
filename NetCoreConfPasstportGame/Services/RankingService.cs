using Microsoft.Extensions.Options;
using NetCoreConfPasstportGame.Models;
using NetCoreConfPasstportGame.Options;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.V2;
using Tweetinvi.Parameters.V2;

namespace NetCoreConfPasstportGame.Services
{
    public class RankingService
    {
        private readonly TwitterCredentials _credentials;

        public RankingService()
        {
            _credentials = new TwitterCredentials(TwitterOptions.ConsumerKey, TwitterOptions.ConsumerSecret, TwitterOptions.AccessToken, TwitterOptions.AccessTokenSecret)
            {
                BearerToken = TwitterOptions.BearerToken,
            };
        }

        internal TwitterClient GetClient()
        {
            return new TwitterClient(_credentials);
        }


        public async Task<List<TweetV2>> Search(bool HasMediaFile = true)
        {

            var tweets = new List<TweetV2>();
            var client = GetClient();
            var searchQuery = string.Join(" ", SearchOptions.Hastags) + " has:images";
            var searchParameters = new SearchTweetsV2Parameters(searchQuery)
            {
                Expansions = { "attachments.media_keys" },
                TweetFields = { "attachments" },
                MediaFields = { "url", "preview_image_url", "type" }
            };
            var search = client.SearchV2.GetSearchTweetsV2Iterator(searchParameters);
            if (search == null )
                return tweets;

            
            while(!search.Completed)
            {
                var searchPage = await search.NextPageAsync();
                var searchResponse = searchPage.Content;
                tweets.AddRange(searchResponse.Tweets);
            }

            return tweets;

        }

        public async Task<UserV2> GetUserName(string id)
        {
            var client = GetClient();
            try
            {
                var user = await client.UsersV2.GetUserByIdAsync(id);
                return user.User;
            } catch (Exception ex)
            {
                return new UserV2() { Id = id };
            }

        }




        public async Task<List<RankingModel>> GetRanking()
        {
            var ranking = new List<RankingModel>();
           
            var tweets = await Search();

            var usersInfo = await Task.WhenAll(tweets.Select(tweet => GetUserName(tweet.AuthorId)));


            foreach (var tweet in tweets)
            {                
                var userInfo = usersInfo.FirstOrDefault(user => user.Id == tweet.AuthorId);
                if (userInfo != null)
                {
                    var userName = userInfo.Username + " - " + userInfo.Name; 
                    if(userName == " - ") userName = userInfo.Id.ToString();
                    var userRanking = ranking.FirstOrDefault(r => r.Alias == userName);
                    if (userRanking == null)
                    {                        
                        userRanking = new RankingModel
                        {
                            Alias = userName,
                            Retweets = tweet.PublicMetrics.RetweetCount,
                            Likes = tweet.PublicMetrics.LikeCount,
                            Partners = new Dictionary<string, string>(),
                            Text = tweet.Text,
                            TweetId = tweet.Id
                        };
                        userRanking.Partners =  UpdatePartnersDictionaryFromTweet(tweet.Text, tweet.Id, userRanking.Partners);
                        ranking.Add(userRanking);
                    }
                    else
                    {                        
                        userRanking.Retweets += tweet.PublicMetrics.RetweetCount;
                        userRanking.Likes += tweet.PublicMetrics.LikeCount;
                        userRanking.Partners = UpdatePartnersDictionaryFromTweet(tweet.Text, tweet.Id, userRanking.Partners);
                    }
                }
            }
            
            return ranking.OrderByDescending(r => r.Retweets).ThenByDescending(r => r.Likes).ToList();
        }


        public Dictionary<string,string> UpdatePartnersDictionaryFromTweet(string tweetText, string tweetId, Dictionary<string, string> partnersDict)
        {
            var mentioned = false;
            foreach (var partner in SearchOptions.Partners)
            {
                if (tweetText.Contains(partner, StringComparison.OrdinalIgnoreCase) && !mentioned)
                {                    
                    mentioned = true;
                    partnersDict[partner] = tweetId;
                }
                else if (!partnersDict.ContainsKey(partner))
                {                    
                    partnersDict[partner] = string.Empty;
                }
            }
            
            return partnersDict;
        }
    }
}
