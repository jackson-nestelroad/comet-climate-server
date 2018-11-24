using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Scrapers;

namespace comet_climate_server.Controllers
{
    [Route("/weather")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly WebAPIContext _context;

        // Constructor that runs with each request to the API
        public WeatherController(WebAPIContext context)
        {             
            // Get the database context
            this._context = context;

            // Get the saved weather
            // .AsNoTracking() so we can create a new Weather object to pass to the database
            // with the same ID number
            var _Weather = _context.Weather.AsNoTracking()
                .FromSql("SELECT * FROM \"Weather\" LIMIT 1")
                .ToList();

            // Get the saved error flags
            var _Errors = _context.Errors.AsNoTracking()
                .FromSql("SELECT * FROM \"Errors\" ORDER BY id DESC LIMIT 1")
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
            double difference = _Weather.Count != 0 ? 
                DateTime.Now.Subtract(_Weather[0].last_updated).TotalMilliseconds : 0;

            // If the weather error flag is not set
            if(!_Errors[0].weather)
            {
                // If we do not have data or the data is stale (10 minutes old)
                if(_Weather.Count == 0 || difference > 10*60*1000)
                {
                    Weather oldWeather = _Weather.Count == 0 ? new Weather {
                        id = 1,
                        temperature = 0,
                        condition = "None",
                        high = 0,
                        low = 0,
                        wind_speed = 0,
                        wind_direction = "None",
                        wind_chill = 0,
                        precipitation = 0,
                        forecast = new int[5],
                        last_updated = DateTime.Now
                    } : _Weather[0];
                    Weather newWeather = WeatherScraper.scrape(oldWeather);

                    // We got the exact same data back, so an error must have occurred in scraping
                    if(newWeather == oldWeather)
                    {
                        // Set error flag
                        _Errors[0].weather = true;
                        _Errors[0].time = DateTime.Now;

                        this._context.Errors.Update(_Errors[0]);
                        this._context.SaveChanges();
                    }
                    // Update database with new, scraped data
                    else
                    {
                        if(_Weather.Count == 0)
                        {
                            this._context.Weather.Add(newWeather);
                        }
                        else
                        {
                            this._context.Weather.Update(newWeather);
                        }
                        this._context.SaveChanges();
                    }
                }
            }     
        }

        // GET /weather
        [HttpGet]
        public ActionResult<IEnumerable<object>> Get()
        {
            // Query and format our data
            var query = (
                from weather in _context.Weather
                join errors in _context.Errors on weather.id equals errors.id
                select new { 
                    success = true,
                    results = new {
                        temperature = weather.temperature,
                        condition = weather.condition,
                        high = weather.high,
                        low = weather.low,
                        wind_speed = weather.wind_speed,
                        wind_direction = weather.wind_direction,
                        wind_chill = weather.wind_chill,
                        precipitation = weather.precipitation,
                        forecast = weather.forecast,
                        last_updated = weather.last_updated
                    },
                    error = errors.weather
                }).ToList();
            return query;
            // return this._context.Weather.ToList();
        }
    }
}