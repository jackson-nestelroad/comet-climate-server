using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using WebAPI.Models;
using WebAPI.Tokens;

namespace WebAPI.Scrapers
{
    public class TwitterScraper
    {
        // Function to get data from the Twitter API
        public static List<Twitter> scrape(List<Twitter> oldTwitter, int numTweets)
        {
            try
            {
                // Get bearer token to access the API
                TwitterToken tokenBuilder = new TwitterToken();
                string bearerToken = tokenBuilder.GetBearerToken();

                // Start a new request to the Twitter API
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(
                    "https://api.twitter.com/1.1/statuses/user_timeline.json?user_id=36989808&tweet_mode=extended&exclude_replies=true");
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + bearerToken);

                // Get the response from Twitter
                var response = request.GetResponse();

                // Parse the JSON response
                using(Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string json = reader.ReadToEnd();

                    // Parsing will fail if an error occurred because we are sent a JObject instead of a JArray
                    if(json.StartsWith("{"))
                        return oldTwitter;

                    // Create our new list of tweets
                    List<Twitter> newTwitter = new List<Twitter>();

                    // Parse JSON (array) response
                    JArray tweetObject = JArray.Parse(json);
                    DateTime Now = DateTime.Now;
                    for(int tweet = 0; tweet < numTweets; tweet++)
                    {
                        // Parse Twitter's date format
                        DateTime createdAt = DateTime.ParseExact(tweetObject[tweet]["created_at"].ToString(),
                            "ddd MMM dd HH:mm:ss zzz yyyy", new System.Globalization.CultureInfo("en-US"));

                        // Slightly more complicated to get the number of likes
                        int likes = tweetObject[tweet].SelectToken("retweeted_status") != null ? 
                            tweetObject[tweet]["retweeted_status"]["favorite_count"].ToObject<int>() : 
                            tweetObject[tweet]["favorite_count"].ToObject<int>();

                        // Also complicated to get full retweet text
                        string fullText = tweetObject[tweet].SelectToken("retweeted_status") != null ? 
                            tweetObject[tweet]["retweeted_status"]["full_text"].ToString() : 
                            tweetObject[tweet]["full_text"].ToString();

                        // Add the new data
                        newTwitter.Add(new Twitter {
                            id = tweet + 1,
                            tweet_id = tweetObject[tweet]["id_str"].ToString(),
                            created_at = createdAt,
                            text = fullText,
                            user_id = tweetObject[tweet]["user"]["id_str"].ToString(),
                            user_name = tweetObject[tweet]["user"]["name"].ToString(),
                            user_screen_name = tweetObject[tweet]["user"]["screen_name"].ToString(),
                            user_profile_image = tweetObject[tweet]["user"]["profile_image_url"].ToString(),
                            likes = likes,
                            retweets = tweetObject[tweet]["retweet_count"].ToObject<int>(),
                            retweeted = tweetObject[tweet].SelectToken("retweeted_status") != null,
                            last_updated = Now
                        });
                    }

                    // Return our new list of data
                    return newTwitter;
                }
            }
            // Handle unexpected errors (because we want to send old data, not just a 500 error)
            catch(HttpException)
            {
                return oldTwitter;
            }
        }
    }
}