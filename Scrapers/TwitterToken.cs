using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using WebAPI.Models;

namespace WebAPI.Tokens
{
    // Class to receieve access to Twitter API
    public class TwitterToken
    {
        private string ConsumerKey { get; set; }
        private string ConsumerSecret { get; set; }
        private string BearerToken { get; set; }

        // Constructor to get the API keys
        public TwitterToken()
        {
            // Production environment, keys stored as environment variables
            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                this.ConsumerKey = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY");
                this.ConsumerSecret = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET");    
            }
            // Do something else here in development environment
            else
            {

            }
        }

        // Function to encode a value in Base 64
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        // Function to get bearer token from Twitter
        public string GetBearerToken()
        {
            // Twitter requires us to encode our consumer key and secret before sending as a header
            var EncodedConsumerKey = HttpUtility.UrlEncode(this.ConsumerKey);
            var EncodedConsumerSecret = HttpUtility.UrlEncode(this.ConsumerSecret);
            string SecretHeader = EncodedConsumerKey + ':' + EncodedConsumerSecret;
            SecretHeader = Base64Encode(SecretHeader);

            // Start a request to get our bearer token
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://api.twitter.com/oauth2/token");
            request.Method = "POST";
            request.Headers.Add("Authorization", "Basic " + SecretHeader);
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            
            // Set the body of the request (annoying, right?)
            string body = "grant_type=client_credentials";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bodyBytes = encoding.GetBytes(body);
            request.ContentLength = bodyBytes.Length;

            // Write to the request's body
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bodyBytes, 0, bodyBytes.Length);

            // Get the response from Twitter
            var response = request.GetResponse();

            // Parse the JSON response
            using(Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                string json = reader.ReadToEnd();

                // Parse JSON response
                JObject tokenObject = JObject.Parse(json);
                if(!tokenObject.ContainsKey("access_token"))
                    throw new HttpException((HttpStatusCode) 510);

                // Return bearer token
                return tokenObject["access_token"].ToString();
            }
        }
    }
}