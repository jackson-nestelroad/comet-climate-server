namespace WebAPI.Models
{
    public class Twitter
    {
        public int id { get; set; }
        public string tweet_id { get; set; }
        public System.DateTime created_at { get; set; }
        public string text { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string user_screen_name { get; set; }
        public string user_background_image { get; set; }
        public bool retweeted { get; set; }
        public System.DateTime last_updated { get; set; }
    }
}