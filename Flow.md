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
                    * Scrapers/WeatherScraper.cs
                        * Send a GET request to the National Weather Service's weather page for Dallas, TX.
                        * Return the old weather data on any errors.
                        * Create a new Weather instance with the same ID (1).
                        * Use XPath nodes to get the temperature, condition, high, and low.
                        * Send a GET request to the National Weather Service's forecast page for Dallas, TX.
                        * Return the old weather data on any errors.
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
    * Navigation to */twitter* - Controllers/TwitterController.cs
        * Get WebAPIContext to connect to database.
        * Get Twitter data.
        * Get errors data.
        * If the Errors table is empty, add the required data to the database.
        * Get the difference in milliseconds between now and the Twitter data's last update.
        * If the Twitter error flag is not set,
            * If difference between in milliseconds is greater than ten minutes or is invalid,
                * Save the old Twitter data in a List, or create sample data to overwrite.
                * Call the **TwitterScraper** class' scrape function to get new Twitter data.
                    * Scrapers/TwitterScraper.cs
                        * Send a POST HTTP request with Consumer Secrets to receieve a Bearer Token to access the API.
                        * Send a GET HTTP request with the Bearer Token to receive the last 10 Tweets from @UT_Dallas.
                        * Return the old Twitter data on any errors.
                        * Parse the response data as a JSON object.
                        * Receieve the necessary data for the 10 Tweets to create a List of 10 Twitter objects.
                        * Return the new List of Twitter objects.
                * If the new Twitter data is the same as the old Twitter data,
                    * Set the Twitter error flag in the Errors table to true.
                    * Update the time column in the Errors table to the current timestamp.
                * If the new Twitter data is not the same as the old Twitter data,
                    * Replace all of the old Twitter data with the new Twitter data in the database.
            * If Twitter data exists and is up to date,
                * Query the Weather and Errors table together.
                * Return a JSON object including if the request was successful, the HTTP code, the full array of Twitter data, and the Twitter error flag.
    * Navigation to an invalid page - Controllers/DefaultController.cs
        * Return a formatted JSON with a 404 error code.
    * Internal Server Errors - Controllers/ErrorController.cs
        * Get the HTTP status code.
        * Get the reason for the error based on the HTTP status code.
        * Return a formatted JSON result with the HTTP status code and reason.