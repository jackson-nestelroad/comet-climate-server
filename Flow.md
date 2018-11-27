### Flow of Program
* Publish the project
    * Restore dependencies according to comet-climate-server.csproj.
    * Create the final .dll files in the /out folder.
* Build Container Environment via Docker
    * Read Dockerfile in the /out folder.
        * Pull base image microsoft/dotnet:2.1-sdk.
        * Copy all files to app directory in container.
        * Run .dll (published) file with `dotnet` command.
* Run Docker Container in Heroku
    * Push container to Heroku registry.
    * Release container in Heroku registry.
* Server Startup
    * Program.cs
        * Build and run web host with the **Startup** class.
    * Startup.cs
        * Adding services to container
            * Add MVC (Model, View Controller) dependency.
            * Get database URI from Heroku's environment variable.
            * Parse the database URI to get the connection string.
            * Add Npgsql dependency to connect to a PostgreSQL database.
            * Add a DBContext using the **WebAPIContext** class.
            * Connect to the DBContext with the parsed connection string.
        * Finish configuration
            * Add global exception handling middleware by routing to */error*.
            * Add Strict-Transport-Security header middleware.
            * Use HTTPS redirecting.
            * Use MVC (Model, View Controller) routing.
* Database Connection
    * WebAPIContext.cs
        * Create a DBContext using the **Weather**, **Twitter**, and **Errors** classes as tables.
* Request Authentication
* Routing - Handling HTTP Requests to the Server
    * Navigation to */weather* - Controllers/WeatherController.cs
        * Get WebAPIContext to connect to database.
        * Get weather data.
        * Get errors data.
        * If the Errors table is empty, add the required data to the database.
        * Get the difference in milliseconds between now and the weather's last update.
        * If the weather error flag is not set,
            * If there is no weather data or the difference in milliseconds is greater than ten minutes,
                * Save the old weather data in a Weather class instance.
                * Call the **WeatherScraper** class' scrape function to get new weather data.
                    * Scrapers/Weather.cs
                        * Send a GET request to the National Weather Service's weather page for Dallas, TX.
                        * Return the oldWeather data on any errors.
                        * Create a new Weather instance with the same ID (1).
                        * Use XPath nodes to get the temperature, condition, high, and low.
                        * Send a GET request to the National Weather Service's forecast page for Dallas, TX.
                        * Return the oldWeather data on any errors.
                        * Use XPath nodes to get the 5-hour forecast, wind chill, wind speed, wind direction, and precipiation potential.
                        * Update the last updated column with the current timestamp.
                        * Return the new Weather object.
                * If the new weather data is the same as the old weather data,
                    * Set the weather error flag in the Errors table to true.
                    * Update the time column in the Errors table to the current timestamp.
                * If the new weather data is not the same as the old weather data,
                    * Replace the old weather data with the new weather data in the database.
            * If weather data exists and is up to date,
                * Query the Weather and Errors table together.
                * Return a JSON object including if the request was successful, the HTTP code, the weather data, and the weather error flag.
    * Navigation to */twitter - Controllers/TwitterController.cs
    * Navigation to an invalid page - Controllers/DefaultController.cs
    * Internal Server Errors - Controllers/ErrorController.cs