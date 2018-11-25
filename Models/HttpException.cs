using System;
using System.Net;

namespace WebAPI.Models
{
    // Exception class to send consistent error responses through the API
    public class HttpException : Exception
    {
        public HttpException(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }
        public HttpStatusCode StatusCode { get; private set; }
    }
}