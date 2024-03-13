namespace NetCoreConfPasstportGame.Models
{
    public class RankingModel
    {
        public string Alias { get; set; }
        public string Picture { get; set; }
        public int Retweets { get; set; }
        public int Likes { get; set; }
        public string Text { get; set; }    
        public string TweetId { get; set; }
        public Dictionary<string,string> Partners { get; set; }        

    }
}
