using System;
using System.Linq;
using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Scrapers
{
    public class WeatherScraper
    {
        // Function to scrape the National Weather Service for data
        public static Weather scrape(Weather oldWeather)
        {
            try
            {
                // Load up the first HTML page
                HtmlWeb web = new HtmlWeb();
                HtmlDocument document = web.Load("https://forecast.weather.gov/MapClick.php?lat=32.9607&lon=-96.733");
                
                // Error in getting HTML document, give back the old data
                if(document.ParseErrors == null)
                    return oldWeather;

                // We will now build our new weather object
                Weather newWeather = new Weather();
                newWeather.id = oldWeather.id;
                var currentConditions = document.DocumentNode.SelectNodes("//div[@id='current_conditions-summary']//p");
                
                // We must check every Node Selection for failure
                // Failure means the site might have been reformatted since it is pretty consistent
                if(currentConditions == null)
                    return oldWeather;
                else if(currentConditions.Count != 3)
                    return oldWeather;
                
                // Update condition
                newWeather.condition = currentConditions[0].InnerText;
                
                // This result means the link above is broken somehow
                // This happened the day after I created this scraper
                if(currentConditions[1].InnerText == "N/A")
                    return oldWeather;

                // Update temperature
                newWeather.temperature = int.Parse(
                    new string(currentConditions[1].InnerText.Where(c => char.IsDigit(c)).ToArray())
                );

                // Not exactly sure where highs/lows are on the NWS' site, so this will be OK for now...
                newWeather.high = newWeather.temperature + 3;
                newWeather.low = newWeather.temperature - 5;

                // Load up our second HTML page
                var document2 = web.Load("https://forecast.weather.gov/MapClick.php?lat=32.9607&lon=-96.733&lg=english&&FcstType=digital");
                
                // Error in getting HTML document, give back the old data
                if(document2.ParseErrors == null)
                    return oldWeather;

                newWeather.forecastTemp = new int[5];
                // Update temperature forecast
                for(int row = 3; row < 8; row++)
                {
                    var node = document2.DocumentNode.SelectNodes("//table[6]/tr[4]/td[" + row + "]");
                    if(node == null)
                        return oldWeather;
                    newWeather.forecastTemp[row - 3] = int.Parse(node[0].InnerText);
                }

                newWeather.forecastPrec = new int[5];
                // Update precipitation forecast
                for(int row = 3; row < 8; row++)
                {
                    var node = document2.DocumentNode.SelectNodes("//table[6]/tr[11]/td[" + row + "]");
                    if(node == null)
                        return oldWeather;
                    newWeather.forecastPrec[row - 3] = int.Parse(node[0].InnerText);
                }

                // Update wind chill
                var windChill = document2.DocumentNode.SelectNodes("//table[6]/tr[6]/td[2]");
                if(windChill == null)
                    return oldWeather;
                newWeather.wind_chill = windChill[0].InnerText != "" ? int.Parse(windChill[0].InnerText) : newWeather.temperature;
                
                // Update wind speed
                var windSpeed = document2.DocumentNode.SelectNodes("//table[6]/tr[7]/td[2]");
                if(windSpeed == null)
                    return oldWeather;
                newWeather.wind_speed = int.Parse(windSpeed[0].InnerText);

                // Update wind direction
                var windDirection = document2.DocumentNode.SelectNodes("//table[6]/tr[8]/td[2]");
                if(windDirection == null)
                    return oldWeather;
                newWeather.wind_direction = windDirection[0].InnerText;

                // Update precipitation potential
                var precipitation = document2.DocumentNode.SelectNodes("//table[6]/tr[11]/td[2]");
                if(precipitation == null)
                    return oldWeather;
                newWeather.precipitation = int.Parse(precipitation[0].InnerText);

                // Update time
                newWeather.last_updated = DateTime.Now;

                // Return our new data
                return newWeather;
            }
            // Handle unexpected errors (because we want to send old data, not just a 500 error)
            catch(HttpException)
            {
                return oldWeather;
            }
        }
    }
}