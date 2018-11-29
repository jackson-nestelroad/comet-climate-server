using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Scrapers;

namespace Comet.Climate.Server.Controllers
{
    [Route("/twitter")]
    [ApiController]
    public class TwitterController : ControllerBase
    {
        private readonly WebAPIContext _context;

        // Constructor that runs with each request to the API
        public TwitterController(WebAPIContext context)
        {
            // Get the database context
            this._context = context;

            // Get the saved weather
            // .AsNoTracking() so we can create a new Weather object to pass to the database
            // with the same ID number
            var _Twitter = _context.Twitter.AsNoTracking()
                .FromSql("SELECT * FROM \"Twitter\"")
                .ToList();

            // Get the saved error flags
            var _Errors = _context.Errors.AsNoTracking()
                .FromSql("SELECT * FROM \"Errors\" WHERE id = 1")
                .ToList();

            // Errors table is empty, so add the data
            if(_Errors.Count == 0)
            {
                _Errors.Add(new Errors 
                    { 
                        id = 1, 
                        time = DateTime.Now, 
                        weather = false, 
                        twitter = false 
                    });
                this._context.Errors.Add(_Errors[0]);
                this._context.SaveChanges();
            }
                
            // Difference between the last update and now
            double difference = _Twitter.Count != 0 ? 
                DateTime.Now.Subtract(_Twitter[0].last_updated).TotalMilliseconds : -100;

            // If the twitter error flag is not set
            if(!_Errors[0].twitter)
            {
                // If we do not have data or the data is stale (10 minutes old)
                if(difference == -100 || difference > 10*60*1000)
                {
                    // Object to hold old Twitter data
                    // Either make a new list or use the database data
                    List<Twitter> oldTwitter = _Twitter.Count == 0 ? new List<Twitter>() : _Twitter;

                    // We did not find any data, so populate the list with sample data
                    // This should only happen once (first call to server)
                    if(oldTwitter.Count == 0)
                    {
                        for(int tweet = 0; tweet < 10; tweet++)
                            oldTwitter.Add(new Twitter {
                                id = 1,
                                tweet_id = "/",
                                created_at = DateTime.Now,
                                text = "None",
                                user_id = "None",
                                user_name = "None",
                                user_screen_name = "None",
                                user_profile_image = "None",
                                likes = 0,
                                retweets = 0,
                                retweeted = false,
                                last_updated = DateTime.Now
                            });
                    }
                    // Get 10 Tweets
                    List<Twitter> newTwitter = TwitterScraper.scrape(oldTwitter, 10);

                    // We got the exact same data back, so an error must have occurred in scraping
                    if(newTwitter[0].last_updated == oldTwitter[0].last_updated)
                    {
                        // Set error flag
                        _Errors[0].twitter = true;
                        _Errors[0].time = DateTime.Now;

                        this._context.Errors.Update(_Errors[0]);
                        this._context.SaveChanges();
                    }
                    // Update database with new, retrieved data
                    else
                    {
                        // No data previously existed, so add new data
                        if(_Twitter.Count == 0)
                        {
                            newTwitter.ForEach(tweet => {
                                this._context.Twitter.Add(tweet);
                            });
                        }
                        // Replace old data
                        else
                        {
                            newTwitter.ForEach(tweet => {
                                this._context.Twitter.Update(tweet);
                            });
                        }
                        this._context.SaveChanges();
                    }
                }
            }     
        }

        // GET /weather
        [HttpGet]
        public IActionResult Get()
        {
            // Get all tweets
            var tweets = (
                from twitter in _context.Twitter
                select new {
                    id = twitter.id,
                    tweet_id = twitter.tweet_id,
                    created_at = twitter.created_at,
                    text = twitter.text,
                    user_id = twitter.user_id,
                    user_name = twitter.user_name,
                    user_screen_name = twitter.user_screen_name,
                    user_profile_image = twitter.user_profile_image,
                    likes = twitter.likes,
                    retweets = twitter.retweets,
                    retweeted = twitter.retweeted,
                    last_updated = twitter.last_updated
                }
            ).ToArray();
            // Format our JSON object
            var query = (
                from errors in _context.Errors
                select new {
                    success = true,
                    reason = false,
                    code = System.Net.HttpStatusCode.OK,
                    results = tweets,
                    error_flag = errors.twitter 
                }
            ).ToArray();
            return Ok(query[0]);
        }
    }
}